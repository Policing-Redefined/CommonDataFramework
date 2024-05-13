﻿using System;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules;

/// <summary>
/// Represents a LSPDFR persona but with additional features.
/// </summary>
public class PedRecord
{
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

    protected PedRecord()
    {
        
    }
    
    internal PedRecord(Persona persona)
    {
        Persona = persona;
        HandleDriversLicenseExpiration();
    }
    
    /// <summary>
    /// Returns a string containing fullname and date of birth of the ped.
    /// </summary>
    /// <returns>The string with fullname and birthday combined.</returns>
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