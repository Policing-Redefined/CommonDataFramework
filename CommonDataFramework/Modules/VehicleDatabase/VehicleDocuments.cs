using System;
using System.Collections.Generic;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Engine.Utility.Resources;
using CommonDataFramework.Modules.PedDatabase;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Different types of VIN states.
/// </summary>
public enum EVinStatus
{
    /// <summary>
    /// VIN is valid.
    /// </summary>
    Valid,
    
    /// <summary>
    /// VIN is scratched.
    /// </summary>
    Scratched
}

/// <summary>
/// Represents the identification number of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleIdentificationNumber
{
    private readonly string _number;

    /// <summary>
    /// Gets the VIN string of length 17.
    /// Null if Status is set to <see cref="EVinStatus.Scratched"/>.
    /// </summary>
    public string Number => Status == EVinStatus.Scratched ? null : _number;

    /// <summary>
    /// Gets or sets the status of the VIN.
    /// This will also alter the value of <see cref="Number"/>.
    /// </summary>
    public EVinStatus Status { get; set; }
    
    internal VehicleIdentificationNumber(EVinStatus status)
    {
        Status = status;
        _number = GetRandomString(17);
    }
}

/// <summary>
/// Represents the registration of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleRegistration
{
    private EDocumentStatus _status;
    private static WeightedList<EDocumentStatus> _weightedRegStatus;

    /// <summary>
    /// Gets or sets the status of the registration.
    /// This also alters <see cref="ExpirationDate"/>.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    public EDocumentStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return; // Don't update it.
            _status = value;
            HandleRegistrationUpdate();
        }
    }
    
    /// <summary>
    /// Gets the expiration date of the registration.
    /// Can be null if <see cref="Status"/> is <see cref="EDocumentStatus.None"/>.
    /// </summary>
    /// <seealso cref="Status"/>
    public DateTime? ExpirationDate { get; private set; }

    internal VehicleRegistration(EDocumentStatus status)
    {
        if (_weightedRegStatus == null) UpdateWeights();
        Status = status;
    }
    
    internal static EDocumentStatus GetRandomStatus()
    {
        if (_weightedRegStatus == null) UpdateWeights();
        return _weightedRegStatus.Random();
    }
    
    internal static void ResetWeights()
    {
        _weightedRegStatus = null;
    }

    private void HandleRegistrationUpdate()
    {
        switch (_status)
        {
            case EDocumentStatus.Revoked:
            case EDocumentStatus.Expired:
                ExpirationDate = GetRandomDateTimeWithinRange(2);
                break;
            case EDocumentStatus.Valid:
                ExpirationDate = GetRandomDateTimeWithinRange(CurrentDate.AddYears(1));
                break;
            case EDocumentStatus.None:
            default:
                ExpirationDate = null;
                break;
        }
    }
    
    private static void UpdateWeights()
    {
        _weightedRegStatus = new WeightedList<EDocumentStatus>(new List<WeightedListItem<EDocumentStatus>>
        {
            new(EDocumentStatus.Valid, CDFSettings.VehicleRegValidChance),
            new(EDocumentStatus.Expired, CDFSettings.VehicleRegExpiredChance),
            new(EDocumentStatus.Revoked, CDFSettings.VehicleRegRevokedChance),
            new(EDocumentStatus.None, CDFSettings.VehicleUnregisteredChance)
        });
    }
}

/// <summary>
/// Represents the insurance of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleInsurance
{
    private EDocumentStatus _status;
    private static WeightedList<EDocumentStatus> _weightedInsStatus;

    /// <summary>
    /// Gets or sets the status of the insurance.
    /// This also alters <see cref="ExpirationDate"/>.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    public EDocumentStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return; // Don't update it.
            _status = value;
            HandleInsuranceUpdate();
        }
    }
    
    /// <summary>
    /// Gets the expiration date of the insurance.
    /// Can be null if <see cref="Status"/> is <see cref="EDocumentStatus.None"/>.
    /// </summary>
    /// <seealso cref="Status"/>
    public DateTime? ExpirationDate { get; private set; }

    internal VehicleInsurance(EDocumentStatus status)
    {
        if (_weightedInsStatus == null) UpdateWeights();
        Status = status;
    }

    internal static EDocumentStatus GetRandomStatus()
    {
        if (_weightedInsStatus == null) UpdateWeights();
        return _weightedInsStatus!.Next(); // Why does this warn of a null ref exception but in vehicle reg it does not?
    }
    
    internal static void ResetWeights()
    {
        _weightedInsStatus = null;
    }

    private void HandleInsuranceUpdate()
    {
        switch (_status)
        {
            case EDocumentStatus.Revoked:
            case EDocumentStatus.Expired:
                ExpirationDate = GetRandomDateTimeWithinRange(2);
                break;
            case EDocumentStatus.Valid:
                ExpirationDate = GetRandomDateTimeWithinRange(CurrentDate.AddYears(1));
                break;
            case EDocumentStatus.None:
            default:
                ExpirationDate = null;
                break;
        }
    }
    
    private static void UpdateWeights()
    {
        _weightedInsStatus = new WeightedList<EDocumentStatus>(new List<WeightedListItem<EDocumentStatus>>
        {
            new(EDocumentStatus.Valid, CDFSettings.VehicleInsValidChance),
            new(EDocumentStatus.Expired, CDFSettings.VehicleInsExpiredChance),
            new(EDocumentStatus.Revoked, CDFSettings.VehicleInsRevokedChance),
            new(EDocumentStatus.None, CDFSettings.VehicleUninsuredChance)
        });
    }
}