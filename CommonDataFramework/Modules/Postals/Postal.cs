using System.Xml.Serialization;
using LSPD_First_Response.Engine.Scripting;

namespace CommonDataFramework.Modules.Postals;

/// <summary>
/// A postal code.
/// </summary>
public class Postal
{
    internal static Postal Default => new()
    {
        Number = "-1",
        X = 0f,
        Y = 0f
    };
    
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

    /// <summary>
    /// Returns the world zone of this postal.
    /// </summary>
    /// <seealso cref="WorldZone"/>
    public WorldZone Zone => LSPDFRFunctions.GetZoneAtPosition(this);

    /// <summary>
    /// Returns a <see cref="Rage.Vector3"/> based off the <see cref="X"/> and <see cref="Y"/> values.
    /// </summary>
    /// <param name="value">Postal to use.</param>
    /// <returns>A <see cref="Rage.Vector3"/> representation of the Postal object.</returns>
    public static implicit operator Vector3(Postal value) => new(value.X, value.Y, 0f);
}
