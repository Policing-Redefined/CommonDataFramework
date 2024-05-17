using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonDataFramework.Engine.IO;

internal class IniReflector
{
    private readonly Type _iniModel;
    private readonly string _path;
    
    private readonly List<IniReflectorSection> _sections = new();
    private readonly Dictionary<string, object> _defaultValues = new();
    private readonly Dictionary<PropertyInfo, IniReflectorValue> _validProperties = new();
    private readonly Dictionary<FieldInfo, IniReflectorValue> _validFields = new();
    private InitializationFile _iniFile;
    private bool _hasReadBefore;

    internal IniReflector(string path, Type iniModel)
    {
        _iniFile = new InitializationFile(path);
        _iniModel = iniModel;
        _path = path;
    }

    internal void Read(object obj, bool withLogging)
    {
        Type objType = obj.GetType();
        if (objType != _iniModel)
        {
            LogWarn($"Object of type '{objType.Name}' does NOT match ini model '{_iniModel.Name}'.");
            return;
        }
        
        if (_hasReadBefore)
        {
            _sections.Clear();
            _defaultValues.Clear();
            _validProperties.Clear();
            _validFields.Clear();
            _iniFile = new InitializationFile(_path);
            _iniFile.Create();
        }
        else
        {
            _iniFile.Create();
            _hasReadBefore = true;
        }

        if (withLogging) 
            LogDebug($"IniReflector: Reading '{_iniModel.Name}'.");
        _sections.AddRange(_iniModel.GetCustomAttributes<IniReflectorSection>());
        
        PropertyInfo[] properties = _iniModel.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        foreach (PropertyInfo property in properties)
        {
            if (property.GetCustomAttribute<IniReflectorIgnore>() != null) continue;
            
            // Members starting with 'Default' are considered to be storing a default value of a different member
            if (property.Name.StartsWith("Default"))
            {
                if (!property.CanRead) continue; // We must be able to read the default value

                Type propertyType = property.PropertyType;
                object defaultValue = GetDefaultValueOfType(propertyType);
                object value = property.GetValue(obj);
                
                // Ensure that we don't try to set 'null' to a value type
                if (propertyType.IsValueType)
                {
                    if (value == null || value == defaultValue) continue;
                }
                else if (value == defaultValue) // Reference type: defaultValue == null
                {
                    continue;
                }
                
                _defaultValues.Add(property.Name, property.GetValue(obj));
                continue;
            }
            
            IniReflectorValue reflectorValue = property.GetCustomAttribute<IniReflectorValue>();
            // We must be able to write to that member and it must have our custom attribute
            // Only default members are allowed to be static
            if (property.GetMethod.IsStatic || !property.CanWrite)
                continue;
            
            // Check if section name exists
            if (reflectorValue == null)
            {
                // Try to check using the name of the property
                if (!_sections.Any(s => property.Name.StartsWith(s.Name))) continue;
            }
            else if (string.IsNullOrEmpty(reflectorValue.SectionName))
            {
                // We prefer the defined name in the attribute, however if that doesn't exist then we use the name of the property
                string nameToUse = string.IsNullOrEmpty(reflectorValue.Name) ? property.Name : reflectorValue.Name;
                if (!_sections.Any(s => nameToUse.StartsWith(s.Name))) continue;
            }
            
            _validProperties.Add(property, reflectorValue);
        }
        
        FieldInfo[] fields = _iniModel.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute<IniReflectorIgnore>() != null) continue;
            
            // Members starting with 'Default' are considered to be storing a default value of a different member
            if (field.Name.StartsWith("Default"))
            {
                Type fieldType = field.FieldType;
                object defaultValue = GetDefaultValueOfType(fieldType);
                object value = field.GetValue(obj);
                
                // Ensure that we don't try to set 'null' to a value type
                if (fieldType.IsValueType)
                {
                    if (value == null || value == defaultValue) continue;
                }
                else if (value == defaultValue) // Reference type: defaultValue == null
                {
                    continue;
                }
                
                _defaultValues.Add(field.Name, field.GetValue(obj));
                continue;
            }
            
            IniReflectorValue reflectorValue = field.GetCustomAttribute<IniReflectorValue>();
            // We must be able to write to that member (not readonly) and it must have our custom attribute
            // Only default members are allowed to be static
            if (field.IsStatic || field.IsInitOnly) continue;
            
            // Check if section name exists
            if (reflectorValue == null)
            {
                // Try to check using the name of the field
                if (!_sections.Any(s => field.Name.StartsWith(s.Name))) continue;
            }
            else if (string.IsNullOrEmpty(reflectorValue.SectionName))
            {
                // We prefer the defined name in the attribute, however if that doesn't exist then we use the name of the field
                string nameToUse = string.IsNullOrEmpty(reflectorValue.Name) ? field.Name : reflectorValue.Name;
                if (!_sections.Any(s => nameToUse.StartsWith(s.Name))) continue;
            }
            
            _validFields.Add(field, reflectorValue);
        }
        
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Reading {_validProperties.Count} properties.");
        foreach (KeyValuePair<PropertyInfo, IniReflectorValue> value in _validProperties)
        {
            PropertyInfo property = value.Key;
            IniReflectorValue reflectorValue = value.Value;
            Type propertyType = property.PropertyType;

            object defaultValue = GetDefaultValueForMember(propertyType, property.Name, reflectorValue);
            string keyName = reflectorValue?.Name ?? property.Name;
            string sectionName = reflectorValue?.SectionName;
            if (string.IsNullOrEmpty(sectionName))
            {
                sectionName = _sections.Find(s => keyName.StartsWith(s.Name)).Name;
            }
            
            property.SetValue(obj, ReadValue(propertyType, sectionName, keyName, defaultValue));
            LogDebug($"IniReflector '{_iniModel.Name}': [{sectionName}] {property.Name} = {property.GetValue(obj)}");
        }
        
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Reading {_validFields.Count} fields.");
        foreach (KeyValuePair<FieldInfo, IniReflectorValue> value in _validFields)
        {
            FieldInfo field = value.Key;
            IniReflectorValue reflectorValue = value.Value;
            Type fieldType = field.FieldType;

            object defaultValue = GetDefaultValueForMember(fieldType, field.Name, reflectorValue);
            string keyName = reflectorValue?.Name ?? field.Name;
            string sectionName = reflectorValue?.SectionName;
            if (string.IsNullOrEmpty(sectionName))
            {
                sectionName = _sections.Find(s => keyName.StartsWith(s.Name)).Name;
            }
            
            field.SetValue(obj, ReadValue(fieldType, sectionName, keyName, defaultValue));
            LogDebug($"IniReflector '{_iniModel.Name}': [{sectionName}] {field.Name} = {field.GetValue(obj)}");
        }
        
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Finished.");
    }

    private static object GetDefaultValueOfType(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private object GetDefaultValueForMember(Type memberType, string memberName, IniReflectorValue reflectorValue)
    {
        object defaultValue = GetDefaultValueOfType(memberType);
        if (reflectorValue is { DefaultValue: not null } && reflectorValue.DefaultValue != defaultValue)
        {
            defaultValue = reflectorValue.DefaultValue;
        }
        else if (_defaultValues.TryGetValue($"Default{memberName}", out object storedDefaultValue) &&
                 storedDefaultValue != null && storedDefaultValue != defaultValue)
        {
            defaultValue = storedDefaultValue;
        }

        return defaultValue;
    } 
    
    private object ReadValue(Type valueType, string sectionName, string keyName, object defaultValue)
    {
        return !valueType.IsEnum
            ? _iniFile.Read(valueType, sectionName, keyName, defaultValue)
            // RPH has issues reading enums for some reason so we have to read it as a string
            : Enum.Parse(valueType, _iniFile.ReadString(sectionName, keyName, defaultValue.ToString()));
    }
}

internal class IniReflector<T> : IniReflector
{
    internal IniReflector(string path) : base(path, typeof(T)) { }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class IniReflectorIgnore : Attribute
{
    internal IniReflectorIgnore() { }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class IniReflectorValue : Attribute
{
    internal readonly string SectionName;
    internal readonly string Name;
    internal readonly object DefaultValue;
    
    internal IniReflectorValue(string sectionName, string name = null, object defaultValue = null)
    {
        SectionName = sectionName;
        Name = name;
        DefaultValue = defaultValue;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
internal class IniReflectorSection : Attribute
{
    internal readonly string Name;

    internal IniReflectorSection(string name)
    {
        Name = name;
    }
}