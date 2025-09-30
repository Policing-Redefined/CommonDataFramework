using System.Collections.Generic;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Engine.Utility.Resources;
using CommonDataFramework.Modules.PedDatabase;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace CommonDataFramework.Modules.VehicleDatabase;

public partial class VehicleData
{
	private bool AssignGovernmentOwner()
	{
		var gov = PersonaHelper.GenerateNewPersona();
		gov.Forename = "";
		gov.Surname = "Government";
		gov.ELicenseState = ELicenseState.Valid;
		gov.Wanted = false;

		Owner = new PedData(gov);
		OwnerType = EVehicleOwnerType.Government;
		return true;
	}

	private bool AssignDriverOwner()
	{
		if (!Holder.Exists() || Holder.Driver == null) return false;
		Owner = Holder.Driver.GetPedData();
		OwnerType = EVehicleOwnerType.Driver;
		return true;
	}

	private bool AssignPassengerOwner()
	{
		if (!Holder.Exists() || Holder.Passengers.Length == 0) return false;
		Owner = Holder.Passengers.Random().GetPedData();
		OwnerType = EVehicleOwnerType.Passenger;
		return true;
	}

	private bool AssignFamilyMemberOwner()
	{
		if (!Holder.Exists() || Holder.Driver == null) return false;

		var driverData = Holder.Driver.GetPedData();
		var usePassenger = Holder.Passengers.Length > 0 && GetRandomOwnerType() == EVehicleOwnerType.Passenger;
		var passengerData = usePassenger ? Holder.Passengers.Random().GetPedData() : null;

		var owner = new PedData(PersonaHelper.GenerateNewPersona());
		driverData.Lastname = owner.Lastname;
		if (passengerData != null)
		{
			passengerData.Lastname = owner.Lastname;
		}

		Owner = owner;
		OwnerType = EVehicleOwnerType.FamilyMember;
		return true;
	}

	private bool AssignRandomPedOwner()
	{
		Owner = new PedData(PersonaHelper.GenerateNewPersona());
		OwnerType = EVehicleOwnerType.RandomPed;
		return true;
	}

	private static EVehicleOwnerType GetAutoDetectedOwnerType(Vehicle vehicle)
	{
		if (!vehicle.Exists() || vehicle.Occupants.Length == 0)
			return EVehicleOwnerType.RandomPed;
        WeightedList<EVehicleOwnerType> _filteredOwnerTypes = new WeightedList<EVehicleOwnerType>(new List<WeightedListItem<EVehicleOwnerType>>
        {
            new(EVehicleOwnerType.Driver, CDFSettings.VehicleOwnerDriver),
            new(EVehicleOwnerType.FamilyMember, CDFSettings.VehicleOwnerFamily),
        });
        if (vehicle.Occupants.Length > 1)
        {
            _filteredOwnerTypes.Add(EVehicleOwnerType.Passenger, CDFSettings.VehicleOwnerPassenger);
        }
		var ownerType = _filteredOwnerTypes.Next();
		return ownerType;
	}
}


