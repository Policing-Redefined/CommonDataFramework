using System;
using CommonDataFramework.Engine.Utility.Extensions;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Ped"/>.
/// </summary>
public class PedData
{
    internal readonly Ped Holder;
    internal readonly Persona Record;
    
    /// <summary>
    /// Once the ped that owns this ped data stops existing, the ped data is scheduled for deletion.
    /// This field is set to 'true' once the ped data is scheduled for deletion.
    /// </summary>
    public bool IsScheduledForDeletion { get; internal set; }

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

    /// <summary>
    /// Gets or sets the current state of the ped's drivers license.
    /// This also affects <see cref="DriversLicenseExpiration"/>.
    /// </summary>
    /// <seealso cref="ELicenseState"/>
    public ELicenseState DriversLicenseState
    {
        get => Record.ELicenseState;
        set => SetDriversLicenseState(value);
    }
    
    /// <summary>
    /// Gets or sets the firstname of the ped.
    /// </summary>
    public string Firstname
    {
        get => Record.Forename;
        set => Record.Forename = value;
    }

    /// <summary>
    /// Gets or sets the lastname of the ped.
    /// </summary>
    public string Lastname
    {
        get => Record.Surname;
        set => Record.Surname = value;
    }
    
    /// <summary>
    /// Returns the full name of the ped.
    /// </summary>
    public string FullName => Record.FullName;

    /// <summary>
    /// Gets or sets the birthday of the ped.
    /// </summary>
    public DateTime Birthday
    {
        get => Record.Birthday;
        set => Record.Birthday = value;
    }

    // TODO 'GiveCitationToPed'
    /// <summary>
    /// Gets the amount of citations the ped has received.
    /// </summary>
    public int Citations => Record.Citations;
    
    /// <summary>
    /// Gets or sets the amount of times the ped was stopped.
    /// </summary>
    public int TimesStopped
    {
        get => Record.TimesStopped;
        set => Record.TimesStopped = value;
    }

    /// <summary>
    /// Gets or sets the gender of the ped.
    /// </summary>
    public Gender Gender
    {
        get => Record.Gender;
        set => Record.Gender = value;
    }

    /// <summary>
    /// Gets or sets whether the ped is wanted.
    /// </summary>
    public bool Wanted
    {
        get => Record.Wanted;
        set => Record.Wanted = value;
    }

    /// <summary>
    /// Gets or sets the advisory text for this ped.
    /// </summary>
    public string AdvisoryText
    {
        get => Record.AdvisoryText;
        set => Record.AdvisoryText = value;
    }

    /// <summary>
    /// Gets the model age of this ped.
    /// </summary>
    /// <seealso cref="PedModelAge"/>
    public PedModelAge ModelAge => Record.ModelAge;

    /// <summary>
    /// Gets or sets the runtime information for this ped.
    /// </summary>
    /// <seealso cref="RuntimePersonaInformation"/>
    public RuntimePersonaInformation RuntimeInfo
    {
        get => Record.RuntimeInfo;
        set => Record.RuntimeInfo = value;
    }

    /// <summary>
    /// Gets the wanted information for this ped.
    /// </summary>
    /// <seealso cref="WantedInformation"/>
    public WantedInformation WantedInfo => Record.WantedInformation;

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
    /// Returns a string containing fullname and date of birth of the ped.
    /// </summary>
    /// <returns>The string with fullname and birthday combined.</returns>
    public string ToNameAndDOBString() => Record.ToNameAndDOBString();

    /// <summary>
    /// Changes the status of the ped's license and updates the expiration date of the license accordingly.
    /// </summary>
    /// <param name="licenseState">The new status of the ped's license.</param>
    /// <seealso cref="ELicenseState"/>
    /// <seealso cref="DriversLicenseExpiration"/>
    private void SetDriversLicenseState(ELicenseState licenseState)
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