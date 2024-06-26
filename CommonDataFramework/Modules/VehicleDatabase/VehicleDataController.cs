﻿using System.Collections.Generic;
using System.Linq;
using CommonDataFramework.API;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Controls and stores vehicle data.
/// </summary>
public static class VehicleDataController
{
    private static long _lastPrune = CurrentMillis();
    internal static readonly Dictionary<Vehicle, VehicleData> Database = new();

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
        // Check if the timer was exceeded
        long now = CurrentMillis();
        if (now - _lastPrune > EntryPoint.DatabasePruneInterval)
        {
            _lastPrune = now;
            Prune();
        }
        
        if (Database.TryGetValue(vehicle, out VehicleData vehData))
        {
            return vehData;
        }

        // Check PedDataController.cs: GetPedData for further info on this method.
        return !vehicle.Exists() ? null : new VehicleData(vehicle);
    }

    internal static void Clear()
    {
        Database.Clear(); // Clear the database just in case dismissing throws an exception for whatever reason
        LogDebug("Clear: VehicleDataController.");
    }

    private static void Prune()
    {
        LogDebug($"VehicleDataController: Checking {Database.Count}.");
        
        int removed = 0;
        foreach (KeyValuePair<Vehicle, VehicleData> entry in Database.ToArray())
        {
            if (entry.Key.Exists()) continue;
            
            // Let the data live until the next prune.
            if (!entry.Value.RemoveDuringNextPrune)
            {
                entry.Value.RemoveDuringNextPrune = true;
                continue;
            }
            
            Database.Remove(entry.Key);
            removed++;
            CDFEvents.InvokeOnVehicleDataRemoved(entry.Key, entry.Value);
        }
                
        LogDebug($"VehicleDataController: Removed {removed}.");
    }
}