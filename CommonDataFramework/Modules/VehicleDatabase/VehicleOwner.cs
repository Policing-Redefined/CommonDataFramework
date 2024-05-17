using System.Collections.Generic;
using System.ComponentModel;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Engine.Utility.Resources;
using CommonDataFramework.Modules.PedDatabase;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Represents the owner of a <see cref="VehicleData"/> entry.
/// </summary>
public class VehicleOwner : PedData
{
    private static readonly WeightedList<EVehicleOwnerType> VehicleOwnerTypes = new(new List<WeightedListItem<EVehicleOwnerType>>
    {
        new(EVehicleOwnerType.Driver, 30),
        new(EVehicleOwnerType.Passenger, 25),
        new(EVehicleOwnerType.FamilyMember, 25),
        new(EVehicleOwnerType.RandomPed, 20),
    });
    
    /// <summary>
    /// Holds the vehicle that belongs to this owner.
    /// </summary>
    public readonly Vehicle Reference;

    private EVehicleOwnerType _ownerType;
    /// <summary>
    /// Gets or sets the type of owner.
    /// </summary>
    /// <seealso cref="EVehicleOwnerType"/>
    public EVehicleOwnerType OwnerType
    {
        get
        {
            // Check if the vehicle was marked as stolen after this data has been generated and needs to updated.
            if (Reference.Exists() && Reference.IsStolen && !Reference.Model.IsLawEnforcementVehicle // If it is a government vehicle, then don't change the vehicle owner
                && _ownerType != EVehicleOwnerType.RandomPed && _ownerType != EVehicleOwnerType.Manual)
            {
                SetVehicleOwner(null);
            }

            return _ownerType;
        }
    }

    internal VehicleOwner(Vehicle vehicle)
    {
        Reference = vehicle;
        SetVehicleOwner(null);
    }

    /// <summary>
    /// Attempts to set the owner using an owner type.
    /// </summary>
    /// <param name="ownerType">The type of owner to set.</param>
    /// <returns>Whether the attempt was successful.</returns>
    /// <seealso cref="EVehicleOwnerType"/>
    public bool TrySetOwner(EVehicleOwnerType ownerType) => SetVehicleOwner(ownerType);

    /// <summary>
    /// Attempts to manually set the owner.
    /// </summary>
    /// <param name="pedData">The ped record to use.</param>
    /// <returns>Whether the attempt was successful.</returns>
    /// <seealso cref="PedData"/>
    public bool TrySetOwner(PedData pedData) => SetVehicleOwner(EVehicleOwnerType.Manual, pedData.Persona);

    private bool SetVehicleOwner(EVehicleOwnerType? ownerType, Persona persona = null)
    {
        // No owner type was specified, so let's do some base checks first before creating a random one or using the provided one.
        if (ownerType == null && Reference.Exists())
        {
            if (Reference.Model.IsEmergencyVehicle) // This vehicle is owned by the government (priority over the stolen check).
            {
                Persona gov = PersonaHelper.GenerateNewPersona();
                gov.Forename = "";
                gov.Surname = "Government";
                gov.ELicenseState = ELicenseState.Valid;
                gov.Wanted = false;
                
                ForceSetPersona(gov);
                _ownerType = EVehicleOwnerType.Government;
                
                return true;
            }
            
            if (Reference.IsStolen) // This vehicle owner must be a random ped.
            {
                ForceSetPersona(PersonaHelper.GenerateNewPersona());
                _ownerType = EVehicleOwnerType.RandomPed;
                return true;
            }
        }
        
        // Use the provided one or get a random one.
        EVehicleOwnerType owner = ownerType ?? GetSuitableOwnerType(Reference);

        // If the vehicle does not exist, we can't make use of occupants.
        if (!Reference.Exists() && owner is EVehicleOwnerType.Driver or EVehicleOwnerType.Passenger or EVehicleOwnerType.FamilyMember)
        {
            return false;
        }

        // If the vehicle is marked as stolen, only allow 'RandomPed' or 'Manual' type
        if (Reference.Exists() && Reference.IsStolen && owner != EVehicleOwnerType.RandomPed && owner != EVehicleOwnerType.Manual)
        {
            return false;
        }

        // We are manually setting the owner
        if (owner == EVehicleOwnerType.Manual)
        {
            if (persona == null) // A persona must be provided
            {
                return false;
            }
            
            _ownerType = owner;
            ForceSetPersona(persona); // No need to clone as this is only ever through PedData, which already cloned the Persona
            return true;
        }

        Persona final;
        switch (owner)
        {
            case EVehicleOwnerType.Driver:
            {
                final = Reference.Driver.GetPedData().Persona;
                break;
            }
            case EVehicleOwnerType.Passenger:
            {
                final = Reference.Passengers.Random().GetPedData().Persona;
                break;
            }
            case EVehicleOwnerType.FamilyMember:
            {
                // Get driver persona and passenger persona (if applicable)
                PedData driverData = Reference.Driver.GetPedData();
                Ped passengerToUse = (Reference.Passengers.Length != 0 && VehicleOwnerTypes.Next() == EVehicleOwnerType.Passenger) ? Reference.Passengers.Random() : null;
                PedData passengerData = passengerToUse != null ? passengerToUse.GetPedData() : null;
                
                // Generate random persona
                Persona family = PersonaHelper.GenerateNewPersona();
                final = family; // A family member is the owner of the vehicle
                driverData.Lastname = family.Surname; // Match driver lastname with family name

                if (passengerData != null) // A passenger can be a member of the family, but not the owner of the vehicle.
                {
                    passengerData.Lastname = family.Surname;
                }
                
                break;
            }
            case EVehicleOwnerType.RandomPed:
            {
                final = PersonaHelper.GenerateNewPersona();
                break;
            }
            default:
                throw new InvalidEnumArgumentException($"{nameof(EVehicleOwnerType)}: Invalid owner type: {owner}.");
        }

        _ownerType = owner;
        ForceSetPersona(final);
        return true;
    }
    
    private static EVehicleOwnerType GetSuitableOwnerType(Vehicle vehicle)
    {
        if (!vehicle.Exists() || vehicle.Occupants.Length == 0) return EVehicleOwnerType.RandomPed;
        EVehicleOwnerType ownerType = VehicleOwnerTypes.Next();
        return ownerType switch
        {
            EVehicleOwnerType.Driver when vehicle.Driver == null => EVehicleOwnerType.RandomPed,
            EVehicleOwnerType.FamilyMember when vehicle.Driver == null => EVehicleOwnerType.RandomPed,
            EVehicleOwnerType.Passenger when vehicle.Passengers.Length == 0 => VehicleOwnerTypes.Next() == EVehicleOwnerType.Driver
                ? EVehicleOwnerType.Driver
                : EVehicleOwnerType.RandomPed,
            _ => ownerType
        };
    }
}

/// <summary>
/// Different types of vehicle owners.
/// </summary>
public enum EVehicleOwnerType
{
    /// <summary>
    /// The driver is the owner of the vehicle.
    /// </summary>
    Driver,
        
    /// <summary>
    /// The passenger is the owner of the vehicle.
    /// </summary>
    Passenger,
        
    /// <summary>
    /// A family member is the owner of the vehicle.
    /// </summary>
    /// <remarks>A passenger can be a family member too.</remarks>
    FamilyMember,
        
    /// <summary>
    /// A random ped is the owner of the vehicle.
    /// </summary>
    /// <remarks>Is also true when the vehicle is stolen.</remarks>
    RandomPed,
        
    /// <summary>
    /// This vehicle is owned by the government.
    /// </summary>
    Government,
    
    /// <summary>
    /// This vehicle is owner by a company.
    /// </summary>
    /// <example>LSIA, Bus company...</example>
    Company,
    
    /// <summary>
    /// This vehicles owner <see cref="PedData"/> has been set manually.
    /// </summary>
    Manual
}