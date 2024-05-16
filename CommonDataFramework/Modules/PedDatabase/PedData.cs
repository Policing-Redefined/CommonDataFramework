﻿using System;
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
    
    internal Persona Persona { get; private set; }
    
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
        get => Persona.ELicenseState;
        set => SetDriversLicenseState(value);
    }
    
    /// <summary>
    /// Gets or sets the firstname of the ped.
    /// </summary>
    public string Firstname
    {
        get => Persona.Forename;
        set => Persona.Forename = value;
    }

    /// <summary>
    /// Gets or sets the lastname of the ped.
    /// </summary>
    public string Lastname
    {
        get => Persona.Surname;
        set => Persona.Surname = value;
    }
    
    /// <summary>
    /// Returns the full name of the ped.
    /// </summary>
    public string FullName => Persona.FullName;

    /// <summary>
    /// Gets or sets the birthday of the ped.
    /// </summary>
    public DateTime Birthday
    {
        get => Persona.Birthday;
        set => Persona.Birthday = value;
    }

    // TODO 'GiveCitationToPed'
    /// <summary>
    /// Gets the amount of citations the ped has received.
    /// </summary>
    public int Citations => Persona.Citations;
    
    /// <summary>
    /// Gets or sets the amount of times the ped was stopped.
    /// </summary>
    public int TimesStopped
    {
        get => Persona.TimesStopped;
        set => Persona.TimesStopped = value;
    }

    /// <summary>
    /// Gets or sets the gender of the ped.
    /// </summary>
    public Gender Gender
    {
        get => Persona.Gender;
        set => Persona.Gender = value;
    }

    /// <summary>
    /// Gets or sets whether the ped is wanted.
    /// </summary>
    public bool Wanted
    {
        get => Persona.Wanted;
        set => Persona.Wanted = value;
    }

    /// <summary>
    /// Gets or sets the advisory text for this ped.
    /// </summary>
    public string AdvisoryText
    {
        get => Persona.AdvisoryText;
        set => Persona.AdvisoryText = value;
    }

    /// <summary>
    /// Gets the model age of this ped.
    /// </summary>
    /// <seealso cref="PedModelAge"/>
    public PedModelAge ModelAge => Persona.ModelAge;

    /// <summary>
    /// Gets or sets the runtime information for this ped.
    /// </summary>
    /// <seealso cref="RuntimePersonaInformation"/>
    public RuntimePersonaInformation RuntimeInfo
    {
        get => Persona.RuntimeInfo;
        set => Persona.RuntimeInfo = value;
    }

    /// <summary>
    /// Gets the wanted information for this ped.
    /// </summary>
    /// <seealso cref="WantedInformation"/>
    public WantedInformation WantedInfo => Persona.WantedInformation;
    
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
    /// Empty constructor for creating an instance without providing a persona or Ped right away.
    /// </summary>
    
    protected PedData()
    {
        HuntingPermit = new Permit();
        FishingPermit = new Permit();
        WeaponPermit = new WeaponPermit();
    }

    internal PedData(Ped holder) : this()
    {
        Holder = holder;
        Persona = LSPDFRFunctions.GetPersonaForPed(holder).Clone();
        HandleDriversLicenseExpiration();
        PedDataController.Database.Add(holder, this);
    }

    internal PedData(Persona persona) : this()
    {
        Holder = null;
        Persona = persona;
        HandleDriversLicenseExpiration();
    }
    
    public string ToNameAndDOBString() => Persona.ToNameAndDOBString();

    internal void ForceSetPersona(Persona persona)
    {
        Persona = persona;
    }
    
    private void SetDriversLicenseState(ELicenseState licenseState)
    {
        Persona.ELicenseState = licenseState;
        HandleDriversLicenseExpiration();
    }

    private void HandleDriversLicenseExpiration()
    {
        switch (Persona.ELicenseState)
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