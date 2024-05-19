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
    private static readonly Dictionary<Vehicle, GameFiber> DeletionQueue = new();
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

        foreach (KeyValuePair<Vehicle, GameFiber> deletion in DeletionQueue)
        {
            deletion.Value.AbortSafe();
        }
        
        DeletionQueue.Clear();
        Database.Clear();

        LogDebug("Stop: VehicleDataController.");
    }

    private static void ScheduleForDeletion(VehicleData vehicleData, int timeUntil = 60000 * 5) // 5 Minutes
    {
        if (vehicleData.IsScheduledForDeletion) return;
        vehicleData.IsScheduledForDeletion = true;
        
        GameFiber deletion = GameFiber.StartNew(() =>
        {
            GameFiber.Sleep(timeUntil);
            Database.Remove(vehicleData.Holder);
            DeletionQueue.Remove(vehicleData.Holder);
            LogDebug($"VehicleDataController: '{vehicleData.LicensePlate}' was deleted.");
        });

        DeletionQueue[vehicleData.Holder] = deletion;
    }

    private static void Process()
    {
        try
        {
            while (EntryPoint.OnDuty)
            {
                GameFiber.Yield();
                foreach (KeyValuePair<Vehicle, VehicleData> entry in Database.ToArray())
                {
                    if (entry.Key.Exists() || entry.Value.IsScheduledForDeletion) continue;
                    ScheduleForDeletion(entry.Value);
                }
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