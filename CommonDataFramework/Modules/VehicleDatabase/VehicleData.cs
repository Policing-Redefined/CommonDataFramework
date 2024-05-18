﻿using CommonDataFramework.Modules.PedDatabase;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleData
{
    /// <summary>
    /// The vehicle this data belongs do.
    /// </summary>
    public readonly Vehicle Holder;

    private string _licensePlate; // License plate cache
    /// <summary>
    /// Gets the license plate of this vehicle.
    /// </summary>
    public string LicensePlate
    {
        get
        {
            if (Holder.Exists()) // Update license plate cache
            {
                _licensePlate = Holder.LicensePlate;
            }

            return _licensePlate;
        }
    }

    /// <summary>
    /// Gets or sets whether the vehicle is stolen or not.
    /// Points to <see cref="Vehicle.IsStolen"/>.
    /// Always returns false if the <see cref="Rage.Vechicle"/> does not exist.
    /// </summary>
    public bool IsStolen
    {
        get
        {
            if (Holder.Exists())
            {
                return Holder.IsStolen;
            }
            return false;
        }
        set
        {
            if (Holder.Exists())
            {
                Holder.IsStolen = value;
            }
        }
    }
    
    
    /// <summary>
    /// Gets the owner of this vehicle.
    /// </summary>
    /// <seealso cref="VehicleOwner"/>
    public readonly VehicleOwner Owner;

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
    /// Once the vehicle that owns this vehicle data stops existing, the vehicle data is scheduled for deletion.
    /// This field is set to 'true' once the vehicle data is scheduled for deletion.
    /// </summary>
    public bool IsScheduledForDeletion { get; internal set; }
    
    internal VehicleData(Vehicle vehicle)
    {
        Holder = vehicle;
        _licensePlate = vehicle.LicensePlate;
        Owner = new VehicleOwner(vehicle);

        // Create documents
        bool special = vehicle.Model.IsLawEnforcementVehicle;
        Vin = new VehicleIdentificationNumber((!special && GetRandomChance(CDFSettings.VehicleVinScratchedChance)) ? EVinStatus.Scratched : EVinStatus.Valid);
        // If null, a random status will be given.
        Registration = new VehicleRegistration(special ? EDocumentStatus.Valid : null);
        Insurance = new VehicleInsurance(special ? EDocumentStatus.Valid : null);
        
        VehicleDataController.Database.Add(vehicle, this);
    }
}