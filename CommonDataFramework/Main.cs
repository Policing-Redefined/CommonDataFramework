using System;
using LSPD_First_Response.Mod.API;

namespace CommonDataFramework;

/// <summary/>
public class EntryPoint : Plugin
{
    /// <summary/>
    public override void Initialize()
    {
        AppDomain.CurrentDomain.DomainUnload += DomainUnload;
    }

    /// <summary/>
    public override void Finally()
    {
        // ignored
    }
    
    private static void UnloadSystems()
    {
        
    }

    private static void DomainUnload(object sender, EventArgs e)
    {
        UnloadSystems();
    }
}