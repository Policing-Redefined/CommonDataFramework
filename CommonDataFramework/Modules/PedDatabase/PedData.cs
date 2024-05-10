using System;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Ped"/>.
/// </summary>
public class PedData
{
    internal Ped Holder { get; private set; }
    internal Persona Persona { get; private set; }
    
    /// <summary>
    /// The hunting permit of the ped.
    /// </summary>
    /// <seealso cref="Permit"/>
    public Permit HuntingPermit { get; private set; }
    
    /// <summary>
    /// The fishing permit of the ped.
    /// </summary>
    /// <seealso cref="Permit"/>
    public Permit FishingPermit { get; private set; }
    
    /// <summary>
    /// The weapon permit of the ped.
    /// </summary>
    /// <seealso cref="WeaponPermit"/>
    public WeaponPermit WeaponPermit { get; private set; }
    
    /// <summary>
    /// The expiration date of the ped's drivers license (if the ped owns one).
    /// </summary>
    public DateTime? DriversLicenseExpiration { get; private set; }
    // TODO PedAddress

    internal PedData(Ped holder)
    {
        Holder = holder;
        Persona = LSPDFRFunctions.GetPersonaForPed(holder); // TODO clone?
        HuntingPermit = new Permit();
        FishingPermit = new Permit();
        WeaponPermit = new WeaponPermit();
        // TODO DriversLicenseExpiration
    }
}