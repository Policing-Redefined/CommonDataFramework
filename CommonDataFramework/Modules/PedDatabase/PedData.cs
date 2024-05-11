using System;
using CommonDataFramework.Utility.Extensions;
using LSPD_First_Response.Engine.Scripting.Entities;
using static CommonDataFramework.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Ped"/>.
/// </summary>
public class PedData
{
    internal readonly Ped Holder;
    
    /// <summary>
    /// The persona record of the ped.
    /// </summary>
    /// <seealso cref="Persona"/>
    public readonly Persona Record;

    /// <summary>
    /// The hunting permit of the ped.
    /// </summary>
    /// <seealso cref="Permit"/>
    public readonly Permit HuntingPermit;
    
    /// <summary>
    /// The fishing permit of the ped.
    /// </summary>
    /// <seealso cref="Permit"/>
    public readonly Permit FishingPermit;
    
    /// <summary>
    /// The weapon permit of the ped.
    /// </summary>
    /// <seealso cref="WeaponPermit"/>
    public readonly WeaponPermit WeaponPermit;
    
    /// <summary>
    /// The expiration date of the ped's drivers license (if the ped owns one).
    /// </summary>
    public DateTime? DriversLicenseExpiration { get; private set; }
    // TODO PedAddress

    internal PedData(Ped holder)
    {
        Holder = holder;
        Record = LSPDFRFunctions.GetPersonaForPed(holder).Clone();
        HuntingPermit = new Permit();
        FishingPermit = new Permit();
        WeaponPermit = new WeaponPermit();
        HandleDriversLicenseExpiration();
    }

    /// <summary>
    /// Changes the status of the ped's license and updates the expiration date of the license accordingly.
    /// </summary>
    /// <param name="licenseState">The new status of the ped's license.</param>
    /// <seealso cref="ELicenseState"/>
    /// <seealso cref="DriversLicenseExpiration"/>
    public void SetDriversLicenseState(ELicenseState licenseState)
    {
        Record.ELicenseState = licenseState;
        HandleDriversLicenseExpiration();
    }

    private void HandleDriversLicenseExpiration()
    {
        switch (Record.ELicenseState)
        {
            case ELicenseState.Suspended:
            case ELicenseState.Expired:
                DriversLicenseExpiration = GetRandomDateTimeWithinRange(4);
                break;
            case ELicenseState.Valid:
                DriversLicenseExpiration = GetRandomDateTimeWithinRange(CurrentDate.AddYears(4));
                break;
            case ELicenseState.None:
            case ELicenseState.Unlicensed:
            default:
                DriversLicenseExpiration = null;
                break;
        }
    }
}