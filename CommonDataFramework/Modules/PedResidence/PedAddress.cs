using CommonDataFramework.Modules.Postals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataFramework.Modules.PedResidence;

public class PedAddress
{

    public static Postal AddressPostal { get; set; }

    public static string StreetName { get; set; }

    public PedAddress(Postal postal, string address)
    {
        AddressPostal = postal;
        StreetName = address;
    }

    public PedAddress(Vector3 position)
    {
        AddressPostal = PostalCodeHandler.GetNearestPostalCode(position).Code;
        StreetName = World.GetStreetName(position);
    }
}
