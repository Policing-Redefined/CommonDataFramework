using CommonDataFramework.Modules.Postals;
using CommonDataFramework.Engine.Utility.Extensions;
using LSPD_First_Response.Engine.Scripting;

namespace CommonDataFramework.Modules.PedResidence;

/// <summary>
/// Represents an address of a ped.
/// </summary>
public class PedAddress
{
    /// <summary>
    /// Gets the postal of the address.
    /// </summary>
    /// <remarks>Can theoretically happen to be <see cref="Postal.Default"/>, but it should not unless someone has some weird postals.</remarks>
    /// <seealso cref="Postal"/>
    public readonly Postal AddressPostal;

    /// <summary>
    /// Gets the street of the address.
    /// </summary>
    public readonly string StreetName;

    /// <summary>
    /// Gets the position of the address.
    /// </summary>
    /// <remarks>Note: Based on the construction of the object (e.g. through <see cref="Postal"/>) the Z-Value can be 0.</remarks>
    public readonly Vector3 Position;

    /// <summary>
    /// Gets the world zone of <see cref="Position"/>.
    /// </summary>
    /// <seealso cref="WorldZone"/>
    public WorldZone Zone => LSPDFRFunctions.GetZoneAtPosition(Position);
    
    /// <summary>
    /// Creates a randomized address.
    /// </summary>
    public PedAddress()
    {
        AddressPostal = PostalCodeController.ActivePostalCodeSet?.Codes.Random() ?? Postal.Default;
        StreetName = World.GetStreetName(AddressPostal);
        Position = AddressPostal;
    }
    
    /// <summary>
    /// Creates an address based off a postal and a street name.
    /// </summary>
    /// <param name="postal">The postal to use.</param>
    /// <param name="streetName">The name of the street to use.</param>
    public PedAddress(Postal postal, string streetName)
    {
        AddressPostal = postal;
        StreetName = streetName;
        Position = postal;
    }

    /// <summary>
    /// Creates an address based off a position in the world.
    /// </summary>
    /// <param name="position">Position in world-coordinates</param>
    public PedAddress(Vector3 position)
    {
        AddressPostal = PostalCodeController.GetNearestPostalCode(position)?.Code ?? Postal.Default;
        StreetName = World.GetStreetName(position);
        Position = position;
    }

    /// <summary>
    /// Returns a string representation of the address.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{AddressPostal.Number} {StreetName}";
    }
}
