using CommonDataFramework.Engine.Utility.Extensions;

namespace CommonDataFramework.Modules.PedDatabase;

/// <summary>
/// Represents a data record of a <see cref="Rage.Ped"/>.
/// </summary>
public class PedData : PedRecord
{
    internal readonly Ped Holder;
    
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

    internal PedData(Ped holder) : base(LSPDFRFunctions.GetPersonaForPed(holder).Clone())
    {
        Holder = holder;
        HuntingPermit = new Permit();
        FishingPermit = new Permit();
        WeaponPermit = new WeaponPermit();
    }
}