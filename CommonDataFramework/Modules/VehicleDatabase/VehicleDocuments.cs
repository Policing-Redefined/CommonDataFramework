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
/// Represents the base for a vehicle document.
/// </summary>
public abstract class VehicleDocument
{
    private EDocumentStatus _status;
    
    /// <summary>
    /// Holds the probabilities of different document states for this vehicle document.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    private protected static WeightedList<EDocumentStatus> WeightedStatus;

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

    /// <summary>
    /// Creates an instance.
    /// </summary>
    /// <param name="status">Status for this document. 'Null' will randomize the status.</param>
    protected VehicleDocument(EDocumentStatus? status)
    {
        status ??= GetRandomStatus();
        Status = (EDocumentStatus) status;
    }

    /// <summary>
    /// Updates the probabilities.
    /// </summary>
    /// <seealso cref="WeightedStatus"/>
    private protected abstract void UpdateWeights();

    internal static void ResetWeights()
    {
        WeightedStatus = null;
    }
    
    private EDocumentStatus GetRandomStatus()
    {
        if (WeightedStatus == null) UpdateWeights();
        return WeightedStatus.Random();
    }
    
    private void HandleStatusUpdate()
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
}


/// <summary>
/// Represents the registration of a <see cref="Rage.Vehicle"/>.
/// </summary>
public class VehicleRegistration : VehicleDocument
{
    internal VehicleRegistration(EDocumentStatus? status) : base(status) { }

    /// <summary>
    /// Updates the probabilities.
    /// </summary>
    /// <seealso cref="VehicleDocument.WeightedStatus"/>
    private protected override void UpdateWeights()
    {
        WeightedStatus = new WeightedList<EDocumentStatus>(new List<WeightedListItem<EDocumentStatus>>
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
public class VehicleInsurance : VehicleDocument
{
    internal VehicleInsurance(EDocumentStatus? status) : base(status) { }
    
    /// <summary>
    /// Updates the probabilities.
    /// </summary>
    /// <seealso cref="VehicleDocument.WeightedStatus"/>
    private protected override void UpdateWeights()
    {
        WeightedStatus = new WeightedList<EDocumentStatus>(new List<WeightedListItem<EDocumentStatus>>
        {
            new(EDocumentStatus.Valid, CDFSettings.VehicleInsValidChance),
            new(EDocumentStatus.Expired, CDFSettings.VehicleInsExpiredChance),
            new(EDocumentStatus.Revoked, CDFSettings.VehicleInsRevokedChance),
            new(EDocumentStatus.None, CDFSettings.VehicleUninsuredChance)
        });
    }
}