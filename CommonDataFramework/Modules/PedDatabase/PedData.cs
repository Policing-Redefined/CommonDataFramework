using System;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Modules.PedResidence;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;
namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Ped"/>.
/// </summary>
public class PedData
{
    private readonly Persona _persona;
    
    /// <summary>
    /// The ped this data belongs to.
    /// </summary>
    public readonly Ped Holder;

    /// <summary>
    /// Returns whether this data has an actual ped that exists or has existed.
    /// False if <see cref="Holder"/> is null.
    /// </summary>
    public bool HasRealPed => Holder != null;

    private DateTime? _dlExpiration;
    /// <summary>
    /// The expiration date of the ped's drivers license (if the ped owns one).
    /// </summary>
    public DateTime? DriversLicenseExpiration
    {
        get
        {
            if (_dlState != _persona.ELicenseState) // Compare last cached state to current persona as it might have been changed through external ways
                HandlePersonaUpdate();
            return _dlExpiration;
        }
    }

    /// <summary>
    /// Store the residence of the ped.
    /// </summary>
    /// <seealso cref="PedAddress"/>
    public PedAddress PedAddress { get; set; }

    private ELicenseState _dlState;
    /// <summary>
    /// Gets or sets the current state of the ped's drivers license.
    /// This also affects <see cref="DriversLicenseExpiration"/>.
    /// </summary>
    /// <seealso cref="ELicenseState"/>
    public ELicenseState DriversLicenseState
    {
        get => _persona.ELicenseState;
        set => SetDriversLicenseState(value);
    }
    
    /// <summary>
    /// Gets or sets the firstname of the ped.
    /// </summary>
    public string Firstname
    {
        get => _persona.Forename;
        set => _persona.Forename = value;
    }

    /// <summary>
    /// Gets or sets the lastname of the ped.
    /// </summary>
    public string Lastname
    {
        get => _persona.Surname;
        set => _persona.Surname = value;
    }
    
    /// <summary>
    /// Returns the full name of the ped.
    /// </summary>
    public string FullName => _persona.FullName;

    /// <summary>
    /// Gets or sets the birthday of the ped.
    /// </summary>
    public DateTime Birthday
    {
        get => _persona.Birthday;
        set => _persona.Birthday = value;
    }

    /// <summary>
    /// Gets or sets the amount of citations the ped has received.
    /// </summary>
    public int Citations
    {
        get => _persona.Citations;
        set => _persona.Citations = value;
    }
    
    /// <summary>
    /// Gets or sets the amount of times the ped was stopped.
    /// </summary>
    public int TimesStopped
    {
        get => _persona.TimesStopped;
        set => _persona.TimesStopped = value;
    }

    /// <summary>
    /// Gets or sets the gender of the ped.
    /// </summary>
    public Gender Gender
    {
        get => _persona.Gender;
        set => _persona.Gender = value;
    }

    /// <summary>
    /// Gets or sets whether the ped is wanted.
    /// </summary>
    public bool Wanted
    {
        get => _persona.Wanted;
        set => _persona.Wanted = value;
    }

    /// <summary>
    /// Gets or sets the advisory text for this ped.
    /// </summary>
    public string AdvisoryText
    {
        get => _persona.AdvisoryText;
        set => _persona.AdvisoryText = value;
    }

    /// <summary>
    /// Gets the model age of this ped.
    /// </summary>
    /// <seealso cref="PedModelAge"/>
    public PedModelAge ModelAge => _persona.ModelAge;

    /// <summary>
    /// Gets or sets the runtime information for this ped.
    /// </summary>
    /// <seealso cref="RuntimePersonaInformation"/>
    public RuntimePersonaInformation RuntimeInfo
    {
        get => _persona.RuntimeInfo;
        set => _persona.RuntimeInfo = value;
    }

    /// <summary>
    /// Gets the wanted information for this ped.
    /// </summary>
    /// <seealso cref="WantedInformation"/>
    public WantedInformation WantedInfo => _persona.WantedInformation;
    
    /// <summary>
    /// Gets or sets whether the ped is on parole.
    /// </summary>
    public bool IsOnParole { get; set; }
    
    /// <summary>
    /// Gets or sets whether the ped is on probation.
    /// </summary>
    public bool IsOnProbation { get; set; }
    
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
    /// Empty constructor for creating an instance without providing a persona or Ped right away.
    /// </summary>
    private PedData()
    {
        HuntingPermit = new Permit(null);
        FishingPermit = new Permit(null);
        WeaponPermit = new WeaponPermit(null, null);
        PedAddress = new PedAddress();
        IsOnProbation = GetRandomChance(CDFSettings.PedProbationChance);
        if (!IsOnProbation)
        {
            IsOnParole = GetRandomChance(CDFSettings.PedParoleChance);
        }
    }

    internal PedData(Ped holder) : this()
    {
        Holder = holder;
        _persona = LSPDFRFunctions.GetPersonaForPed(holder);
        HandlePersonaUpdate();
        PedDataController.Database.Add(holder, this);
    }

    internal PedData(Persona persona) : this()
    {
        _persona = persona;
        HandlePersonaUpdate();
    }
    
    /// <summary>
    /// Combines the full name and birthday of this ped data into a string.
    /// </summary>
    /// <returns>The string with fullname and date of birth.</returns>
    public string ToNameAndDOBString() => _persona.ToNameAndDOBString();
    
    private void SetDriversLicenseState(ELicenseState licenseState)
    {
        if (DriversLicenseState == licenseState) return; // Don't update it if it's already that state
        _persona.ELicenseState = licenseState;
        HandlePersonaUpdate();
    }

    private void HandlePersonaUpdate()
    {
        // Update drivers license
        _dlState = _persona.ELicenseState;
        switch (_persona.ELicenseState)
        {
            case ELicenseState.Suspended:
            case ELicenseState.Expired:
                _dlExpiration = GetRandomDateTimeWithinRange(4);
                break;
            case ELicenseState.Valid:
                _dlExpiration = GetRandomDateTimeWithinRange(CurrentDate.AddYears(4));
                break;
            case ELicenseState.None:
            case ELicenseState.Unlicensed:
            default:
                _dlExpiration = null;
                break;
        }
        
        // ...room for more
    }
}