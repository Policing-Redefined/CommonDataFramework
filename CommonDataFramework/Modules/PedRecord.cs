using System;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules;

/// <summary>
/// Represents a LSPDFR persona but with additional features.
/// </summary>
public class PedRecord
{
    private readonly Persona _persona;
    
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

    // TODO 'GiveCitationToPed'
    /// <summary>
    /// Gets the amount of citations the ped has received.
    /// </summary>
    public int Citations => _persona.Citations;
    
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
    
    internal PedRecord(Persona persona)
    {
        _persona = persona;
        HandleDriversLicenseExpiration();
    }
    
    /// <summary>
    /// Returns a string containing fullname and date of birth of the ped.
    /// </summary>
    /// <returns>The string with fullname and birthday combined.</returns>
    public string ToNameAndDOBString() => _persona.ToNameAndDOBString();

    /// <summary>
    /// Changes the status of the ped's license and updates the expiration date of the license accordingly.
    /// </summary>
    /// <param name="licenseState">The new status of the ped's license.</param>
    /// <seealso cref="ELicenseState"/>
    /// <seealso cref="DriversLicenseExpiration"/>
    private void SetDriversLicenseState(ELicenseState licenseState)
    {
        _persona.ELicenseState = licenseState;
        HandleDriversLicenseExpiration();
    }

    private void HandleDriversLicenseExpiration()
    {
        switch (_persona.ELicenseState)
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