using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonDataFramework.Engine.Utility.Extensions;

namespace CommonDataFramework.Modules.PedDatabase;

internal static class PedDataController
{
    private static readonly Dictionary<Ped, PedData> Database = new();
    private static readonly Dictionary<Ped, GameFiber> DeletionQueue = new();
    private static GameFiber _process;

    internal static PedData GetPedData(this Ped ped)
    {
        if (Database.TryGetValue(ped, out PedData pedData)) return pedData;
        if (!ped.Exists()) return null;

        PedData newData = new(ped);
        Database[ped] = newData;

        return newData;
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

        LogDebug($"PedDataController: Scheduled '{pedData.FullName}' for deletion (Time: {timeUntil / 1000}s).");
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