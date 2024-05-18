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

public abstract class Document
{
    protected EDocumentStatus _status;
    protected static WeightedList<EDocumentStatus> _weightedStatus;

     /// <summary>
    /// Gets or sets the status of the document.
    /// This also alters <see cref="ExpirationDate"/>.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    public EDocumentStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            HandleStatusUpdate();
        }
    }

    /// <summary>
    /// Gets the expiration date of the registration.
    /// Can be null if <see cref="Status"/> is <see cref="EDocumentStatus.None"/>.
    /// </summary>
    /// <seealso cref="Status"/>
    public DateTime? ExpirationDate { get; protected set; }

    protected Document(EDocumentStatus? status)
    {
        if (_weightedStatus == null) UpdateWeights();
        if(status == null) status = GetRandomStatus();
        Status = (EDocumentStatus) status;
    }

    protected void HandleStatusUpdate()
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

    protected abstract void UpdateWeights();

    internal EDocumentStatus GetRandomStatus()
    {
        if (_weightedStatus == null) UpdateWeights();
        return _weightedStatus.Random();
    }

    internal static void ResetWeights()
    {
        _weightedStatus = null;
    }
}


/// <summary>
/// Represents the registration of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleRegistration : Document
{
    public VehicleRegistration(EDocumentStatus? status) : base(status) { }

    protected override void UpdateWeights()
    {
        _weightedStatus = new WeightedList<EDocumentStatus>(new List<WeightedListItem<EDocumentStatus>>
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
public class VehicleInsurance : Document
{
    public VehicleInsurance(EDocumentStatus? status) : base(status) { }
    
    protected override void UpdateWeights()
    {
        _weightedStatus = new WeightedList<EDocumentStatus>(new List<WeightedListItem<EDocumentStatus>>
        {
            new(EDocumentStatus.Valid, CDFSettings.VehicleInsValidChance),
            new(EDocumentStatus.Expired, CDFSettings.VehicleInsExpiredChance),
            new(EDocumentStatus.Revoked, CDFSettings.VehicleInsRevokedChance),
            new(EDocumentStatus.None, CDFSettings.VehicleUninsuredChance)
        });
    }
}