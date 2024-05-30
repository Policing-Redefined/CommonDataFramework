using System.Collections.Generic;
using System.Linq;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Controls and stores ped data.
/// </summary>
public static class PedDataController
{
    private static long _lastPrune = CurrentMillis();
    internal static readonly Dictionary<Ped, PedData> Database = new();

    /// <summary>
    /// Gets or creates data for the specified ped.
    /// </summary>
    /// <param name="ped">The ped to get data for.</param>
    /// <returns>
    /// An object containing the ped data.
    /// Can be null if the provided ped does not exist and no previous data was stored for it,
    /// or when the provided ped is not a human.
    /// </returns>
    /// <seealso cref="PedData"/>
    /// <seealso cref="CommonDataFramework.Modules.VehicleDatabase.VehicleDataController.GetVehicleData"/>
    public static PedData GetPedData(this Ped ped)
    {
        // Check if the timer was exceeded
        long now = CurrentMillis();
        if (now - _lastPrune > EntryPoint.DatabasePruneInterval)
        {
            _lastPrune = now;
            Prune();
        }
        
        // No need for a .Exists check here as we might still have it cached even though it's scheduled for deletion.
        if (Database.TryGetValue(ped, out PedData pedData))
        {
            return pedData;
        }
        
        // Only create new data for a human (!) ped if it exists.
        return (!ped.Exists() || !ped.IsHuman) ? null : new PedData(ped);
    }

    internal static void Clear()
    {
        Database.Clear();
        LogDebug("Clear: PedDataController.");
    }

    private static void Prune()
    {
        LogDebug($"PedDataController: Checking {Database.Count}.");
        
        int removed = 0;
        foreach (KeyValuePair<Ped, PedData> entry in Database.ToArray())
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
        }
                
        LogDebug($"PedDataController: Removed {removed}.");
    }
}