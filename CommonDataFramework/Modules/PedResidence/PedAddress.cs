using CommonDataFramework.Modules.Postals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// Creates a randomized address.
    /// </summary>
    public PedAddress()
    {
        int index = new Random(DateTime.Today.Millisecond).Next(PostalCodeController.PostalCodeSet.Codes.Count);
        AddressPostal = PostalCodeController.PostalCodeSet.Codes[index];
        StreetName = World.GetStreetName(new Vector3(AddressPostal.X, AddressPostal.Y, 0));
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
    }

    /// <summary>
    /// Creates an address based off a position in the world.
    /// </summary>
    /// <param name="position">Position in world-coordinates</param>
    public PedAddress(Vector3 position)
    {
        AddressPostal = PostalCodeController.GetNearestPostalCode(position).Code;
        StreetName = World.GetStreetName(position);
    }
}
