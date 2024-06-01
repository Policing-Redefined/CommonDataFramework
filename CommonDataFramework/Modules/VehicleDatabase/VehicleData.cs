﻿using System.Collections.Generic;
using System.ComponentModel;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Engine.Utility.Resources;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.PedResidence;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleData
{
    private static readonly WeightedList<EVehicleOwnerType> VehicleOwnerTypes = new(new List<WeightedListItem<EVehicleOwnerType>>
    {
        new(EVehicleOwnerType.Driver, 30),
        new(EVehicleOwnerType.Passenger, 25),
        new(EVehicleOwnerType.FamilyMember, 25),
        new(EVehicleOwnerType.RandomPed, 20),
    });
    
    /// <summary>
    /// The vehicle this data belongs do.
    /// </summary>
    public readonly Vehicle Holder;

    private bool _isStolen; // Stolen cache
    /// <summary>
    /// Gets or sets whether the vehicle is stolen or not.
    /// Points to <see cref="Rage.Vehicle.IsStolen"/>.
    /// If the vehicle does no longer exist, it gets and sets the last cached state, meaning you can still mark a vehicle
    /// as stolen within CDF using this property.
    /// Changing this might change <see cref="Owner"/> and <see cref="OwnerType"/>.
    /// </summary>
    public bool IsStolen
    {
        get
        {
            if (Holder.Exists()) // Update stolen cache
            {
                _isStolen = Holder.IsStolen;
            }
            
            return _isStolen;
        }
        set
        {
            if (IsStolen == value) return;
            
            if (Holder.Exists())
            {
                Holder.IsStolen = value;
            }
            
            _isStolen = value;
            if (_ownerType != EVehicleOwnerType.RandomPed && _ownerType != EVehicleOwnerType.Manual && _ownerType != EVehicleOwnerType.Government)
            {
                SetVehicleOwner(null); // Change owner because stolen state changed
            }
        }
    }
    
    /// <summary>
    /// Gets the owner of this vehicle.
    /// Make sure to check <see cref="PedData.HasRealPed"/> before making use of the <see cref="PedData.Holder"/> property,
    /// as it might not have an actual ped in use (e.g. for government/stolen/random ped vehicles...).
    /// If you are a e.g. a computer plugin and need a ped mugshot, just grab a random ped from the world and generate
    /// the mugshot based on that.
    /// </summary>
    /// <seealso cref="PedData"/>
    public PedData Owner { get; private set; }

    // When the owner type is set to 'RandomPed' or 'FamilyMember', a random temporary ped is created, that is dismissed
    // when the vehicle stops existing or the owner type is changed.
    internal Ped TempPed { get; private set; }
    
    private EVehicleOwnerType _ownerType;
    /// <summary>
    /// Gets the type of owner.
    /// </summary>
    /// <seealso cref="EVehicleOwnerType"/>
    public EVehicleOwnerType OwnerType
    {
        get
        {
            // Check if the vehicle was marked as stolen after this data has been generated and needs to updated.
            if (IsStolen && (!Holder.Exists() || !Holder.Model.IsEmergencyVehicle) // If it is a government vehicle, then don't change the vehicle owner (even though it's stolen)
                && _ownerType != EVehicleOwnerType.RandomPed && _ownerType != EVehicleOwnerType.Manual)
            {
                SetVehicleOwner(null);
            }

            return _ownerType;
        }
        private set
        {
            _ownerType = value;
            if (value != EVehicleOwnerType.RandomPed && value != EVehicleOwnerType.FamilyMember && TempPed.Exists())
            {
                TempPed.Dismiss();
                TempPed = null; // Reset the field as the ped is no longer needed
            }

            if (Holder.Exists()) // Set the owner within LSPDFR's API
            {
                LSPDFRFunctions.SetVehicleOwnerName(Holder, value == EVehicleOwnerType.Government ? "Government" : Owner.FullName);
            }
        }
    }

    /// <summary>
    /// Gets the vehicle identification number of this vehicle.
    /// </summary>
    /// <seealso cref="VehicleIdentificationNumber"/>
    public readonly VehicleIdentificationNumber Vin;
    
    /// <summary>
    /// Gets the registration of this vehicle.
    /// </summary>
    /// <seealso cref="VehicleRegistration"/>
    public readonly VehicleRegistration Registration;
    
    /// <summary>
    /// Gets the insurance of this vehicle.
    /// </summary>
    /// <seealso cref="VehicleInsurance"/>
    public readonly VehicleInsurance Insurance;
    
    /// <summary>
    /// Determines whether the data should be removed from the database during the next prune,
    /// after the vehicle stopped existing.
    /// </summary>
    internal bool RemoveDuringNextPrune;
    
    internal VehicleData(Vehicle vehicle)
    {
        Holder = vehicle;
        _isStolen = vehicle.IsStolen;
        SetVehicleOwner(null);

        // Create documents
        bool special = vehicle.Model.IsEmergencyVehicle;
        Vin = new VehicleIdentificationNumber((!special && GetRandomChance(CDFSettings.VehicleVinScratchedChance)) ? EVinStatus.Scratched : EVinStatus.Valid);
        // If null, a random status will be given.
        Registration = new VehicleRegistration(special ? EDocumentStatus.Valid : null);
        Insurance = new VehicleInsurance(special ? EDocumentStatus.Valid : null);
        
        VehicleDataController.Database.Add(vehicle, this);
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
    public bool TrySetOwner(PedData pedData) => SetVehicleOwner(EVehicleOwnerType.Manual, pedData);

    private bool SetVehicleOwner(EVehicleOwnerType? ownerType, PedData pedData = null)
    {
        // No owner type was specified, so let's do some basic checks
        if (ownerType == null)
        {
            if (Holder.Exists() && Holder.Model.IsEmergencyVehicle) // This vehicle is owned by the government (priority over the stolen check)
            {
                Persona gov = PersonaHelper.GenerateNewPersona();
                gov.Forename = "";
                gov.Surname = "Government";
                gov.ELicenseState = ELicenseState.Valid;
                gov.Wanted = false;

                Owner = new PedData(gov);
                OwnerType = EVehicleOwnerType.Government;
                
                return true;
            }
            
            if (IsStolen || GetRandomChance(CDFSettings.VehicleStolenChance)) // This vehicle owner must be a random ped if the vehicle is marked as stolen
            {
                OwnerType = EVehicleOwnerType.RandomPed; // Setting it first will prevent 'SetVehicleOwner' being called by the setter of 'IsStolen'
                UseTemporaryPed();
                IsStolen = true;
                return true;
            }
        }
        
        // Use the provided one or get a random one.
        EVehicleOwnerType owner = ownerType ?? GetSuitableOwnerType(Holder);

        // If the vehicle does not exist, we can't make use of occupants.
        if (!Holder.Exists() && owner is EVehicleOwnerType.Driver or EVehicleOwnerType.Passenger or EVehicleOwnerType.FamilyMember)
        {
            return false;
        }

        // If the vehicle is marked as stolen, only allow 'RandomPed' or 'Manual' type
        if (IsStolen && owner != EVehicleOwnerType.RandomPed && owner != EVehicleOwnerType.Manual)
        {
            return false;
        }

        // We are manually setting the owner
        if (owner == EVehicleOwnerType.Manual)
        {
            if (pedData == null) // Data must be provided
            {
                return false;
            }
            
            OwnerType = owner;
            Owner = pedData;
            
            return true;
        }
        
        switch (owner)
        {
            case EVehicleOwnerType.Driver:
            {
                Owner = Holder.Driver.GetPedData();
                break;
            }
            case EVehicleOwnerType.Passenger:
            {
                Owner = Holder.Passengers.Random().GetPedData();
                break;
            }
            case EVehicleOwnerType.FamilyMember:
            {
                // Get driver persona and passenger persona (if applicable)
                PedData driverData = Holder.Driver.GetPedData();
                Ped passengerToUse = (Holder.Passengers.Length != 0 && VehicleOwnerTypes.Next() == EVehicleOwnerType.Passenger) ? Holder.Passengers.Random() : null;
                PedData passengerData = passengerToUse != null ? passengerToUse.GetPedData() : null;
                
                // Generate random ped
                UseTemporaryPed();
                
                driverData.Lastname = Owner.Lastname; // Match driver lastname with family name
                if (passengerData != null) // A passenger can be a member of the family, but not the owner of the vehicle.
                {
                    passengerData.Lastname = Owner.Lastname;
                }
                
                break;
            }
            case EVehicleOwnerType.RandomPed:
            {
                UseTemporaryPed();
                break;
            }
            default:
                throw new InvalidEnumArgumentException($"{nameof(EVehicleOwnerType)}: Invalid owner type: {owner}.");
        }

        OwnerType = owner;
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

    private void UseTemporaryPed()
    {
        // Spawn ped at address position
        PedAddress address = new();
        TempPed = new Ped(address.Position)
        {
            IsPersistent = true
        };
        
        // Set owner
        Owner = new PedData(TempPed, address);
        
        // Teleport them above the ground
        TempPed.SetPositionWithSnap(address.Position);
        TempPed.Tasks.Wander();
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
    /// A passenger is the owner of the vehicle.
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
    /// <remarks><see cref="VehicleData.Owner"/> does not have a valid ped (<see cref="PedData.HasRealPed"/> is false).</remarks>
    /// <example>Generally emergency vehicles</example>
    Government,
    
    /*
    /// <summary>
    /// This vehicle is owner by a company.
    /// </summary>
    /// <example>LSIA, Bus company...</example>
    Company,
    */
    
    /// <summary>
    /// This vehicles owner <see cref="PedData"/> has been set manually.
    /// </summary>
    Manual
}