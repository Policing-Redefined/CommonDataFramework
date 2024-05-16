namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleData
{
    internal readonly Vehicle Holder;
    
    /// <summary>
    /// Once the vehicle that owns this vehicle data stops existing, the vehicle data is scheduled for deletion.
    /// This field is set to 'true' once the vehicle data is scheduled for deletion.
    /// </summary>
    public bool IsScheduledForDeletion { get; internal set; }

    /// <summary>
    /// Gets the owner of this vehicle.
    /// </summary>
    /// <seealso cref="VehicleOwner"/>
    public readonly VehicleOwner Owner;

    private string _licensePlate = "UNKNOWN"; // License plate cache
    
    
    
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
    
    internal VehicleData(Vehicle vehicle)
    {
        Holder = vehicle;
        Owner = new VehicleOwner(vehicle);
        VehicleDataController.Database.Add(vehicle, this);
    }
}