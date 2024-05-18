using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonDataFramework.Engine.Utility.Extensions;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Controls and stores ped data.
/// </summary>
public static class PedDataController
{
    internal static readonly Dictionary<Ped, PedData> Database = new();
    private static readonly Dictionary<Ped, GameFiber> DeletionQueue = new();
    private static GameFiber _process;

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
        // No need for a .Exists check here as we might still have it cached even though it's scheduled for deletion.
        if (Database.TryGetValue(ped, out PedData pedData))
        {
            return pedData;
        }
        
        // Only create new data for a human (!) ped if it exists.
        return (!ped.Exists() || !ped.IsHuman) ? null : new PedData(ped);
    }

    internal static void Start()
    {
        _process.AbortSafe();
        _process = GameFiber.StartNew(Process);
        LogDebug("Start: PedDataController.");
    }

    internal static void Stop()
    {
        _process.AbortSafe();
        _process = null;

        foreach (KeyValuePair<Ped, GameFiber> deletion in DeletionQueue)
        {
            deletion.Value.AbortSafe();
        }
        
        DeletionQueue.Clear();
        Database.Clear();

        LogDebug("Stop: PedDataController.");
    }

    private static void ScheduleForDeletion(PedData pedData, int timeUntil = 60000 * 5) // 5 Minutes
    {
        if (pedData.IsScheduledForDeletion) return;
        pedData.IsScheduledForDeletion = true;
        
        GameFiber deletion = GameFiber.StartNew(() =>
        {
            GameFiber.Sleep(timeUntil);
            Database.Remove(pedData.Holder);
            DeletionQueue.Remove(pedData.Holder);
            LogDebug($"PedDataController: '{pedData.FullName}' was deleted.");
        });

        DeletionQueue[pedData.Holder] = deletion;
    }

    private static void Process()
    {
        try
        {
            while (EntryPoint.OnDuty)
            {
                GameFiber.Yield();
                foreach (KeyValuePair<Ped, PedData> entry in Database.ToArray())
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
            LogException(exception, "PedDataController.cs: Process");
            DisplayErrorNotification();
        }
    }
}