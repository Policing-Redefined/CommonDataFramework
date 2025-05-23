using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonDataFramework.Engine.IO;

internal class IniReflector<T> : IniReflector
{
    internal IniReflector(string path) : base(path, typeof(T)) { }
}

internal class IniReflector
{
    #region Const
    
    private const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
    
    #endregion
    
    private readonly Type _iniModel;
    private readonly string _path;
    
    private readonly List<IniReflectorSection> _sections = new();
    private readonly List<Tuple<PropertyInfo, IniReflectorValue>> _validProperties = new();
    private readonly List<Tuple<FieldInfo, IniReflectorValue>> _validFields = new();
    private readonly Dictionary<string, object> _defaultValues = new();
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
        
        // Parse object
        ReflectObject(obj, withLogging);
        
        // Read properties
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Reading {_validProperties.Count} properties.");
        foreach ((PropertyInfo property, IniReflectorValue reflectorValue) in _validProperties)
        {
            // Get default value and ini key/section
            object defaultValue = GetDefaultValueForMember(property.Name, reflectorValue, property.PropertyType);
            GetIniValues(reflectorValue, property.Name, out string keyName, out string sectionName);
            
            // Deserialize to property
            property.SetValue(obj, ReadValue(property.PropertyType, sectionName, keyName, defaultValue, reflectorValue.Description));
            if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': [{sectionName}] {property.Name} = {property.GetValue(obj)}");
        }
        
        // Read fields
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Reading {_validFields.Count} fields.");
        foreach ((FieldInfo field, IniReflectorValue reflectorValue) in _validFields)
        {
            // Get default value and ini key/section
            object defaultValue = GetDefaultValueForMember(field.Name, reflectorValue, field.FieldType);
            GetIniValues(reflectorValue, field.Name, out string keyName, out string sectionName);
            
            // Deserialize to field
            field.SetValue(obj, ReadValue(field.FieldType, sectionName, keyName, defaultValue, reflectorValue.Description));
            if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': [{sectionName}] {field.Name} = {field.GetValue(obj)}");
        }
        
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Finished.");
    }

    internal void Write(object obj, bool withLogging)
    {
        Type objType = obj.GetType();
        if (objType != _iniModel)
        {
            LogWarn($"Object of type '{objType.Name}' does NOT match ini model '{_iniModel.Name}'.");
            return;
        }
        
        // Parse object
        ReflectObject(obj, withLogging);

        // Write properties
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Writing {_validProperties.Count} properties.");
        foreach ((PropertyInfo property, IniReflectorValue reflectorValue) in _validProperties)
        {
            // Get new value and ini key/section
            object newValue = property.GetValue(obj) ?? GetDefaultValueForMember(property.Name, reflectorValue, property.PropertyType);
            GetIniValues(reflectorValue, property.Name, out string keyName, out string sectionName);
            
            // Serialize property
            WriteValue(sectionName, keyName, newValue, reflectorValue.Description);
            if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': [{sectionName}] {property.Name} = {newValue}");
        }
        
        // Write fields
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Writing {_validFields.Count} fields.");
        foreach ((FieldInfo field, IniReflectorValue reflectorValue) in _validFields)
        {
            // Get new value and ini key/section
            object newValue = field.GetValue(obj) ?? GetDefaultValueForMember(field.Name, reflectorValue, field.FieldType);
            GetIniValues(reflectorValue, field.Name, out string keyName, out string sectionName);
            
            // Serialize field
            WriteValue(sectionName, keyName, newValue, reflectorValue.Description);
            if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': [{sectionName}] {field.Name} = {newValue}");
        }
        
        if (withLogging) LogDebug($"IniReflector '{_iniModel.Name}': Finished.");
    }
    
    internal bool WriteSingle(string memberName, object newValue)
    {
        if (!_hasReadBefore) return false; // We do not have any information about the object yet

        IniReflectorValue reflectorValue;
        // Try to find property
        Tuple<PropertyInfo, IniReflectorValue> pMember = _validProperties.Find(p => p.Item1.Name == memberName);
        if (pMember != null)
        {
            if (pMember.Item1.PropertyType != newValue.GetType()) return false; // Verify property type
            reflectorValue = pMember.Item2;
        }
        else
        {
            // Try to find field
            Tuple<FieldInfo, IniReflectorValue> fMember = _validFields.Find(f => f.Item1.Name == memberName);
            if (fMember == null || fMember.Item1.FieldType != newValue.GetType()) return false; // We couldn't find the member or the field type mismatched
            reflectorValue = fMember.Item2;
        }

        GetIniValues(reflectorValue, memberName, out string keyName, out string sectionName);
        WriteValue(sectionName, keyName, newValue, reflectorValue.Description);
        return true;
    }

    private object GetDefaultValueForMember(string memberName, IniReflectorValue reflectorValue, Type memberType)
    {
        if (reflectorValue.DefaultValue != null) return reflectorValue.DefaultValue;
        return _defaultValues.TryGetValue(memberName, out object defaultValue) ? defaultValue : GetDefaultValueOfType(memberType);
    }

    private void GetIniValues(IniReflectorValue reflectorValue, string memberName, out string keyName, out string sectionName)
    {
        string initKeyName = reflectorValue.Name ?? memberName;
        sectionName = reflectorValue.SectionName;
        if (string.IsNullOrEmpty(sectionName))
        {
            sectionName = _sections.Find(s => initKeyName.StartsWith(s.Name)).Name;
        }

        keyName = initKeyName;
    }
    
    private object ReadValue(Type valueType, string sectionName, string keyName, object defaultValue, string description)
    {
        // Write default value to .ini
        if (!_iniFile.DoesKeyExist(sectionName, keyName))
        {
            WriteValue(sectionName, keyName, defaultValue, description);
            return defaultValue;
        }
        
        // Parse value (enums need extra treatment)
        return !valueType.IsEnum
            ? _iniFile.Read(valueType, sectionName, keyName, defaultValue)
            // RPH has issues reading enums for some reason so we have to read it as a string
            : Enum.Parse(valueType, _iniFile.ReadString(sectionName, keyName, defaultValue.ToString()).Trim());
    }

    private void WriteValue(string sectionName, string keyName, object value, string description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            keyName = $"//{description}\n{keyName}";
        }
        
        _iniFile.Write(sectionName, keyName, value);
    }

    private void ReflectObject(object obj, bool withLogging)
    {
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
        
        if (withLogging) LogDebug($"IniReflector: Reflecting '{_iniModel.Name}'.");
        _sections.AddRange(_iniModel.GetCustomAttributes<IniReflectorSection>());
        
        PropertyInfo[] properties = _iniModel.GetProperties(MemberFlags);
        foreach (PropertyInfo property in properties)
        {
            // Members starting with 'Default' are considered to be storing a default value of a different member
            if (property.Name.StartsWith("Default"))
            {
                if (!property.CanRead) continue; // We must be able to read the property value
                _defaultValues.Add(property.Name.Substring(7), property.GetValue(obj));
                continue;
            }
            
            // We must be able to write to that member
            // Only default members are allowed to be static
            if (property.GetMethod.IsStatic || !property.CanWrite)
                continue;
            
            // It must have our custom attribute
            IniReflectorValue reflectorValue = property.GetCustomAttribute<IniReflectorValue>();
            if (reflectorValue == null) continue;
            
            // Check if section name exists
            if (string.IsNullOrEmpty(reflectorValue.SectionName))
            {
                // We prefer the defined name in the attribute, however if that doesn't exist then we use the name of the property
                string nameToUse = string.IsNullOrEmpty(reflectorValue.Name) ? property.Name : reflectorValue.Name;
                if (!_sections.Any(s => nameToUse.StartsWith(s.Name))) continue;
            }
            
            _validProperties.Add(new Tuple<PropertyInfo, IniReflectorValue>(property, reflectorValue));
        }
        
        FieldInfo[] fields = _iniModel.GetFields(MemberFlags);
        foreach (FieldInfo field in fields)
        {
            // Members starting with 'Default' are considered to be storing a default value of a different member
            if (field.Name.StartsWith("Default"))
            {
                _defaultValues.Add(field.Name.Substring(7), field.GetValue(obj));
                continue;
            }
            
            // We must be able to write to that member (not readonly)
            // Only default members are allowed to be static
            if (field.IsStatic || field.IsInitOnly) continue;
            
            // It must have our custom attribute
            IniReflectorValue reflectorValue = field.GetCustomAttribute<IniReflectorValue>();
            if (reflectorValue == null) continue;
            
            // Check if section name exists
            if (string.IsNullOrEmpty(reflectorValue.SectionName))
            {
                // We prefer the defined name in the attribute, however if that doesn't exist then we use the name of the field
                string nameToUse = string.IsNullOrEmpty(reflectorValue.Name) ? field.Name : reflectorValue.Name;
                if (!_sections.Any(s => nameToUse.StartsWith(s.Name))) continue;
            }
            
            _validFields.Add(new Tuple<FieldInfo, IniReflectorValue>(field, reflectorValue));
        }
    }
    
    private static object GetDefaultValueOfType(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class IniReflectorSection : Attribute
{
    internal readonly string Name;

    internal IniReflectorSection(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class IniReflectorValue : Attribute
{
    internal readonly string SectionName;
    internal readonly string Name;
    internal readonly object DefaultValue;
    internal readonly string Description;
    
    internal IniReflectorValue(string sectionName = null, string name = null, object defaultValue = null, string description = null)
    {
        SectionName = sectionName;
        Name = name;
        DefaultValue = defaultValue;
        Description = description;
    }
}