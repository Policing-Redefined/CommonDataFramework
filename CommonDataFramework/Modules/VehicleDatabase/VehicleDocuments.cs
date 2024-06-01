using System;
using System.Collections.Generic;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Engine.Utility.Resources;
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
    /// <summary>
    /// Gets the VIN string of length 17.
    /// </summary>
    /// <remarks>
    /// Before displaying the VIN you should consider checking <see cref="Status"/>,
    /// because it might be <see cref="EVinStatus.Scratched"/> (if that is something that you want to account for).
    /// </remarks>
    public string Number { get; private set; }

    /// <summary>
    /// Gets or sets the status of the VIN.
    /// </summary>
    public EVinStatus Status { get; set; }
    
    internal VehicleIdentificationNumber(EVinStatus status)
    {
        Status = status;
        Number = GetRandomString(17);
    }
}

/// <summary>
/// Represents the base for a vehicle document.
/// </summary>
public abstract class VehicleDocument
{
    private EDocumentStatus _status;

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
    /// Gets or sets the expiration date of the vehicle document.
    /// Can be null if <see cref="Status"/> is <see cref="EDocumentStatus.None"/>.
    /// </summary>
    /// <seealso cref="Status"/>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Updates the probabilities.
    /// </summary>
    private protected abstract void UpdateWeights();
    
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
    /// <summary>
    /// Holds the probabilities of different document states for this vehicle document.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    private static WeightedList<EDocumentStatus> _weightedStatus;

    internal VehicleRegistration(EDocumentStatus? status)
    {
        status ??= GetRandomStatus();
        Status = (EDocumentStatus) status;
    }
    
    internal static void ResetWeights()
    {
        _weightedStatus = null;
    }
    
    private EDocumentStatus GetRandomStatus()
    {
        if (_weightedStatus == null) UpdateWeights();
        return _weightedStatus.Random();
    }

    /// <summary>
    /// Updates the probabilities.
    /// </summary>
    private protected override void UpdateWeights()
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
public class VehicleInsurance : VehicleDocument
{
    /// <summary>
    /// Holds the probabilities of different document states for this vehicle document.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    private static WeightedList<EDocumentStatus> _weightedStatus;
    
    internal VehicleInsurance(EDocumentStatus? status)
    {
        status ??= GetRandomStatus();
        Status = (EDocumentStatus) status;
    }
    
    internal static void ResetWeights()
    {
        _weightedStatus = null;
    }
    
    private EDocumentStatus GetRandomStatus()
    {
        if (_weightedStatus == null) UpdateWeights();
        return _weightedStatus.Random();
    }

    /// <summary>
    /// Updates the probabilities.
    /// </summary>
    private protected override void UpdateWeights()
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