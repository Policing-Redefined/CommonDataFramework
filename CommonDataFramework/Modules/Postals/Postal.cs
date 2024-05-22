using System.Xml.Serialization;

namespace CommonDataFramework.Modules.Postals;

/// <summary>
/// A postal code.
/// </summary>
public class Postal
{
    /// <summary>
    /// Gets or sets a value indicating the block number.
    /// </summary>
    [XmlAttribute("Number")]
    public string Number { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the X-coordinate in a Vector3.
    /// </summary>
    [XmlAttribute("X")]
    public float X { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the Y-coordinate in a Vector3.
    /// </summary>
    [XmlAttribute("Y")]
    public float Y { get; set; }
}
