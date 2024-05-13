using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonDataFramework.Engine.Utility.Extensions;

namespace CommonDataFramework.Modules.VehicleDatabase;

internal static class VehicleDataController
{
    private static readonly Dictionary<Vehicle, VehicleData> Database = new();
    private static readonly Dictionary<Vehicle, GameFiber> DeletionQueue = new();
    private static GameFiber _process;

    internal static VehicleData GetVehicleData(this Vehicle vehicle)
    {
        if (Database.TryGetValue(vehicle, out VehicleData pedData)) return pedData;
        if (!vehicle.Exists()) return null;

        VehicleData newData = new(vehicle);
        Database[vehicle] = newData;

        return newData;
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

        LogDebug($"VehicleDataController: Scheduled '{vehicleData.LicensePlate}' for deletion (Time: {timeUntil / 1000}s).");
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