using System;
using System.Collections.Generic;
using CommonDataFramework.Engine.Utility.Resources;
using static CommonDataFramework.Engine.Utility.Helpers.DateTimeHelper;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Specifies the different states of documents.
/// </summary>
public enum EDocumentStatus
{
    /// <summary>
    /// Document has no status.
    /// </summary>
    None,
    
    /// <summary>
    /// Document has been revoked.
    /// </summary>
    Revoked,
    
    /// <summary>
    /// Document is expired.
    /// </summary>
    Expired,
    
    /// <summary>
    /// Document is valid. 
    /// </summary>
    Valid
}

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
    
    /// <summary>
    /// The status of the permit.
    /// </summary>
    /// <seealso cref="EDocumentStatus"/>
    /// <seealso cref="SetStatus"/>
    public EDocumentStatus Status { get; private set; }
    
    /// <summary>
    /// The expiration date of the permit.
    /// Can only be changed through <see cref="SetStatus"/>.
    /// </summary>
    public DateTime ExpirationDate { get; private set; }

    /// <summary>
    /// Changes the status of the permit.
    /// </summary>
    /// <param name="status">The new status.</param>
    /// <seealso cref="EDocumentStatus"/>
    /// <seealso cref="Status"/>
    public void SetStatus(EDocumentStatus status)
    {
        Status = status;
        GenerateValues(false);
    }

    /// <summary>
    /// Generates permit status and expiration date for the current instance.
    /// </summary>
    protected virtual void GenerateValues(bool randomStatus = true)
    {
        if (randomStatus)
        {
            Status = DocumentStatuses.Next();
        }
        
        ExpirationDate = Status switch
        {
            EDocumentStatus.Revoked => GetRandomDateTimeWithinRange(4),
            EDocumentStatus.Expired => GetRandomDateTimeWithinRange(4),
            EDocumentStatus.None => GetRandomDateTimeWithinRange(4),
            EDocumentStatus.Valid => GetRandomDateTimeWithinRange(CurrentDate.AddYears(4)),
            _ => ExpirationDate
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
        new(EWeaponPermitType.FflPermit, 7),
    });
    
    /// <summary>
    /// The type of the weapon permit.
    /// </summary>
    /// <seealso cref="EWeaponPermitType"/>
    /// <seealso cref="SetPermitType"/>
    public EWeaponPermitType PermitType { get; private set; }

    internal WeaponPermit()
    {
        GenerateValues();
    }

    /// <summary>
    /// Sets the type of the weapon permit.
    /// </summary>
    /// <param name="permitType">The new type.</param>
    /// <seealso cref="EWeaponPermitType"/>
    /// <seealso cref="PermitType"/>
    public void SetPermitType(EWeaponPermitType permitType)
    {
        PermitType = permitType;
    }

    /// <summary>
    /// Generates permit type (permit status, expiration date inherited from <see cref="Permit"/>) for the current instance.
    /// </summary>
    protected sealed override void GenerateValues(bool randomStatus = true)
    {
        if (randomStatus)
        {
            PermitType = WeaponPermitTypes.Next();
        }
        
        base.GenerateValues(randomStatus);
    }
}