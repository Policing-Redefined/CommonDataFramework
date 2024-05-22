using CommonDataFramework.Modules.Postals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Engine.Scripting;

namespace CommonDataFramework.Modules.PedResidence;

/// <summary>
/// Represents an address of a ped.
/// </summary>
public class PedAddress
{
    /// <summary>
    /// Gets or sets the Postal of the address.
    /// </summary>
    /// <seealso cref="Postal"/>
    public Postal AddressPostal { get; set; }

    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    public string StreetName { get; set; }
    
    /// <summary>
    /// Gets the position of the address.
    /// </summary>
    /// <remarks>Note: Based on the construction of the object (e.g. through <see cref="Postal"/>) the Z-Value can be 0.</remarks>
    public Vector3 Position { get; private set; }

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
        int index = new Random(DateTime.Today.Millisecond).Next(PostalCodeController.PostalCodeSet.Codes.Count);
        AddressPostal = PostalCodeController.PostalCodeSet.Codes[index];
        StreetName = World.GetStreetName(AddressPostal);
        Position = AddressPostal;
    }
    
    /// <summary>
    /// Creates an address based off a postal and a street name.
    /// </summary>
    /// <param name="postal"></param>
    /// <param name="address"></param>
    public PedAddress(Postal postal, string address)
    {
        AddressPostal = postal;
        StreetName = address;
        Position = postal;
    }

    /// <summary>
    /// Creates an address based off a position in the world.
    /// </summary>
    /// <param name="position">Position in world-coordinates</param>
    public PedAddress(Vector3 position)
    {
        AddressPostal = PostalCodeController.GetNearestPostalCode(position).Code;
        StreetName = World.GetStreetName(position);
        Position = position;
    }
}
