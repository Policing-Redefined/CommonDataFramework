using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CommonDataFramework.Modules.Postals;

/// <summary>
/// A set of postal codes.
/// </summary>
internal class PostalCodeSet
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the codes.
    /// </summary>
    public List<Postal> Codes { get; private set; }

    /// <summary>
    /// Loads a PostalCodeSet object from an XML file.
    /// </summary>
    /// <param name="filename">The XML file.</param>
    /// <returns>A postal code set object.</returns>
    public static PostalCodeSet FromXML(string filename)
    {
        try
        {
            XmlSerializer serializer = new(typeof(List<Postal>));
            string name = Path.GetFileNameWithoutExtension(filename);
            using StreamReader reader = new(filename);
            List<Postal> codes = (List<Postal>)serializer.Deserialize(reader);
            return new PostalCodeSet
            {
                Name = name,
                Codes = codes,
            };
        }
        catch (Exception ex)
        {
            LogDebug(ex.ToString());
        }

        return null;
    }

    /// <summary>
    /// Overrides the ToString method.
    /// </summary>
    /// <returns>The postal code set name.</returns>
    public override string ToString() => Name;
}
