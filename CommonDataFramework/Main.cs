using System;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.Postals;
using CommonDataFramework.Modules.VehicleDatabase;
using LSPD_First_Response.Mod.API;

namespace CommonDataFramework;

/// <summary/>
public class EntryPoint : Plugin
{
    internal const int DatabasePruneInterval = 15 * 60 * 1000; // 15 Minutes
    internal static bool OnDuty { get; private set; }
    internal static bool PluginReady { get; private set; }

    /// <summary/>
    public override void Initialize()
    {
        AppDomain.CurrentDomain.DomainUnload += DomainUnload;
        LSPDFRFunctions.OnOnDutyStateChanged += LSPDFRFunctions_OnOnDutyStateChanged;
    }

    private static void LSPDFRFunctions_OnOnDutyStateChanged(bool onDuty)
    {
        OnDuty = onDuty;

        if (onDuty)
        {
            GameFiber.StartNew(LoadSystems);
        }
        else
        {
            GameFiber.StartNew(UnloadSystems);
        }
    }

    /// <summary/>
    public override void Finally()
    {
        // ignored
    }

    private static void LoadSystems()
    {
        Settings.Load(DefaultPluginFolder + "/Settings.ini");
        PostalCodeController.Load();
        LogDebug($"Loaded Systems of V{PluginVersion}.");
        PluginReady = true;
    }
    
    private static void UnloadSystems()
    {
        PluginReady = false;
        PedDataController.Clear();
        VehicleDataController.Clear();
        InvalidateCache();
        LogDebug($"Unloaded Systems of V{PluginVersion}.");
    }

    private static void DomainUnload(object sender, EventArgs e)
    {
        UnloadSystems();
    }

    private static void InvalidateCache()
    {
        VehicleRegistration.ResetWeights();
        VehicleInsurance.ResetWeights();
    }
}