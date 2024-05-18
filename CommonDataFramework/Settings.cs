using CommonDataFramework.Engine.IO;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace CommonDataFramework;

[IniReflectorSection("Vehicle")]
internal class Settings
{
    #region DEFAULT
    
    // Default: Vehicle documents
    internal const int DefaultVehicleStolenChance = 20;
    internal const int DefaultVehicleRegValidChance = 65;
    internal const int DefaultVehicleRegExpiredChance = 20;
    internal const int DefaultVehicleRegRevokedChance = 5;
    internal const int DefaultVehicleUnregisteredChance = 10;
    internal const int DefaultVehicleInsValidChance = 55;
    internal const int DefaultVehicleInsExpiredChance = 25;
    internal const int DefaultVehicleInsRevokedChance = 5;
    internal const int DefaultVehicleUninsuredChance = 15;
    internal const int DefaultVehicleVinValidChance = 90;
    internal const int DefaultVehicleVinScratchedChance = 10;
    
    #endregion
    
    // Vehicle
    internal int VehicleStolenChance;
    internal int VehicleRegValidChance;
    internal int VehicleRegExpiredChance;
    internal int VehicleRegRevokedChance;
    internal int VehicleUnregisteredChance;
    internal int VehicleInsValidChance;
    internal int VehicleInsExpiredChance;
    internal int VehicleInsRevokedChance;
    internal int VehicleUninsuredChance;
    internal int VehicleVinValidChance;
    internal int VehicleVinScratchedChance;
    
    #region STATIC
    
    [IniReflectorIgnore]
    internal static Settings Instance { get; private set; }

    internal static void Load(string path)
    {
        Instance = new Settings();
        IniReflector<Settings> iniReflector = new(path);
        iniReflector.Read(Instance, true);
    }
    
    #endregion
}