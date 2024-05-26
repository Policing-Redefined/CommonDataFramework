using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonDataFramework.Engine.Utility.Extensions;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Controls and stores vehicle data.
/// </summary>
public static class VehicleDataController
{
    internal static readonly Dictionary<Vehicle, VehicleData> Database = new();
    private static GameFiber _process;

    /// <summary>
    /// Gets or creates data for the specified vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle to get data for.</param>
    /// <returns>
    /// An object containing the vehicle data.
    /// Can be null if the provided vehicle does not exist and no previous data was stored for it.
    /// </returns>
    /// <seealso cref="VehicleData"/>
    /// <seealso cref="CommonDataFramework.Modules.PedDatabase.PedDataController.GetPedData"/>
    public static VehicleData GetVehicleData(this Vehicle vehicle)
    {
        if (Database.TryGetValue(vehicle, out VehicleData vehData))
        {
            return vehData;
        }

        // Check PedDataController.cs: GetPedData for further info on this method.
        return !vehicle.Exists() ? null : new VehicleData(vehicle);
    }

    internal static void Start()
    {
        _process.AbortSafe();
        _process = GameFiber.StartNew(Process);
        LogDebug("Start: VehicleDataController.");
    }

    internal static void Stop()
    {
        _process.AbortSafe();
        _process = null;
        
        KeyValuePair<Vehicle, VehicleData>[] dbCopy = Database.ToArray();
        Database.Clear(); // Clear the database just in case dismissing throws an exception for whatever reason
        
        // Dismiss temporary peds
        foreach (KeyValuePair<Vehicle, VehicleData> entry in dbCopy)
        {
            if (entry.Value.TempPed.Exists())
            {
                entry.Value.TempPed.Dismiss();
            }
        }
        
        LogDebug("Stop: VehicleDataController.");
    }

    private static void Process()
    {
        try
        {
            while (EntryPoint.OnDuty)
            {
                GameFiber.Sleep(EntryPoint.DatabasePruneInterval);

                int removed = 0;
                foreach (KeyValuePair<Vehicle, VehicleData> entry in Database.ToArray())
                {
                    if (entry.Key.Exists()) continue;
                    Database.Remove(entry.Key);
                    removed++;

                    // Dismiss temporary ped, don't reset the field though
                    if (entry.Value.TempPed.Exists())
                    {
                        entry.Value.TempPed.Dismiss();
                    }
                }
                
                LogDebug($"VehicleDataController: Removed {removed}.");
            }
        }
        catch (ThreadAbortException)
        {
            // ignored
        }
        catch (Exception exception)
        {
            LogException(exception, "VehicleDataController.cs: Process");
            DisplayErrorNotification();
        }
    }
}