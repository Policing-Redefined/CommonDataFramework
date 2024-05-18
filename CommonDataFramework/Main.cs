using System;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using LSPD_First_Response.Mod.API;

namespace CommonDataFramework;

/// <summary/>
public class EntryPoint : Plugin
{
    internal static bool OnDuty { get; private set; }
    
    /// <summary/>
    public override void Initialize()
    {
        LSPDFRFunctions.OnOnDutyStateChanged += LSPDFRFunctions_OnOnDutyStateChanged;
    }

    private static void LSPDFRFunctions_OnOnDutyStateChanged(bool onDuty)
    {
        OnDuty = onDuty;

        if (onDuty)
        {
            LoadSystems();
        }
        else
        {
            UnloadSystems();
        }
    }

    /// <summary/>
    public override void Finally()
    {
        // ignored
    }

    private static void LoadSystems()
    {
        AppDomain.CurrentDomain.DomainUnload += DomainUnload;
        Settings.Load(DefaultPluginFolder + "/CommonDataFramework.ini");
        PedDataController.Start();
        VehicleDataController.Start();
        LogDebug($"Loaded Systems of V{PluginVersion}.");
    }
    
    private static void UnloadSystems()
    {
        AppDomain.CurrentDomain.DomainUnload -= DomainUnload;
        PedDataController.Stop();
        VehicleDataController.Stop();
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