using System;
using System.Collections.Generic;
using CommonDataFramework.Engine.Utility.Resources;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules.PedDatabase;

// Weapon specific
/// <summary>
/// Specifies the different types of weapon permits.
/// </summary>
public enum EWeaponPermitType
{
    /// <summary>
    /// Carrying a concealed weapon
    /// </summary>
    CcwPermit = 2,
    
    /// <summary>
    /// Federal firearms license
    /// </summary>
    FflPermit = 4
}

/// <summary>
/// Represents a permit.
/// </summary>
public class Permit
{
    private static readonly WeightedList<EDocumentStatus> DocumentStatuses = new(new List<WeightedListItem<EDocumentStatus>>
    {
        new(EDocumentStatus.None, 35),
        new(EDocumentStatus.Revoked, 15),
        new(EDocumentStatus.Expired, 20),
        new(EDocumentStatus.Valid, 23)
    });

    private EDocumentStatus _status;
    
    /// <summary>
    /// Gets or sets status of the permit.
    /// This also affects <see cref="ExpirationDate"/>.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    public EDocumentStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            UpdateValues();
        }
    }
    
    /// <summary>
    /// Gets or sets expiration date of the permit.
    /// Can be null if <see cref="Status"/> is <see cref="EDocumentStatus.None"/>.
    /// </summary>
    /// <seealso cref="Status"/>
    public DateTime? ExpirationDate { get; set; }

    internal Permit(EDocumentStatus? status)
    {
        Status = status ?? DocumentStatuses.Next();
    }

    /// <summary>
    /// Updates expiration date for the current instance.
    /// </summary>
    private void UpdateValues()
    {
        ExpirationDate = Status switch
        {
            EDocumentStatus.Revoked => GetRandomDateTimeWithinRange(4),
            EDocumentStatus.Expired => GetRandomDateTimeWithinRange(4),
            EDocumentStatus.Valid => GetRandomDateTimeWithinRange(CurrentDate.AddYears(4)),
            EDocumentStatus.None => null,
            _ => null
        };
    }
}

/// <summary>
/// Represents a weapon permit.
/// </summary>
public class WeaponPermit : Permit
{
    private static readonly WeightedList<EWeaponPermitType> WeaponPermitTypes = new(new List<WeightedListItem<EWeaponPermitType>>
    {
        new(EWeaponPermitType.CcwPermit, 30),
        new(EWeaponPermitType.FflPermit, 7)
    });

    /// <summary>
    /// Gets or sets type of the weapon permit.
    /// </summary>
    /// <seealso cref="EWeaponPermitType"/>
    public EWeaponPermitType PermitType { get; set; }

    internal WeaponPermit(EDocumentStatus? status, EWeaponPermitType? permit) : base(status)
    {
        PermitType = permit ?? WeaponPermitTypes.Next();
    }
}