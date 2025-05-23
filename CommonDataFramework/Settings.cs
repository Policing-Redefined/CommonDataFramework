using CommonDataFramework.Engine.IO;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace CommonDataFramework;

[IniReflectorSection("Vehicle")]
[IniReflectorSection("Ped")]
[IniReflectorSection("Postals")]
internal class Settings
{
    #region DEFAULT
    
    // Default: Vehicle documents
    internal const int DefaultVehicleStolenChance = 8;
    // -- Registration --
    internal const int DefaultVehicleRegValidChance = 65;
    internal const int DefaultVehicleRegExpiredChance = 20;
    internal const int DefaultVehicleRegRevokedChance = 5;
    internal const int DefaultVehicleUnregisteredChance = 10;
    // -- Insurance --
    internal const int DefaultVehicleInsValidChance = 55;
    internal const int DefaultVehicleInsExpiredChance = 25;
    internal const int DefaultVehicleInsRevokedChance = 5;
    internal const int DefaultVehicleUninsuredChance = 15;
    // -- VIN --
    internal const int DefaultVehicleVinValidChance = 90;
    internal const int DefaultVehicleVinScratchedChance = 10;
    // -- Driver --
    internal const int DefaultVehicleOwnerDriver = 30;
    internal const int DefaultVehicleOwnerPassenger = 25;
    internal const int DefaultVehicleOwnerFamily = 25;
    internal const int DefaultVehicleOwnerRandom = 20;
    
    // Default: Ped
    internal const int DefaultPedProbationChance = 25;
    internal const int DefaultPedParoleChance = 30;
    
    #endregion
    
    // Vehicle
    [IniReflectorValue(description: "The probability for a vehicle to be stolen. (0-100)")]
    // Registration
    internal int VehicleStolenChance;
    [IniReflectorValue(description: "The probability for a vehicle to have a valid registration. (0-100; all registration settings should add up to 100!)")]
    internal int VehicleRegValidChance;
    [IniReflectorValue(description: "The probability for a vehicle to have an expired registration. (0-100)")]
    internal int VehicleRegExpiredChance;
    [IniReflectorValue(description: "The probability for a vehicle to have a revoked registration. (0-100)")]
    internal int VehicleRegRevokedChance;
    [IniReflectorValue(description: "The probability for a vehicle to be unregistered. (0-100)")]
    internal int VehicleUnregisteredChance;
    // Insurance
    [IniReflectorValue(description: "The probability for a vehicle to have a valid insurance. (0-100; all insurance settings should add up to 100!)")]
    internal int VehicleInsValidChance;
    [IniReflectorValue(description: "The probability for a vehicle to have an expired insurance. (0-100)")]
    internal int VehicleInsExpiredChance;
    [IniReflectorValue(description: "The probability for a vehicle to have a revoked insurance. (0-100)")]
    internal int VehicleInsRevokedChance;
    [IniReflectorValue(description: "The probability for a vehicle to be uninsured. (0-100)")]
    internal int VehicleUninsuredChance;
    // VIN
    [IniReflectorValue(description: "The probability for a vehicle to have a valid VIN. (0-100; all VIN settings should add up to 100!)")]
    internal int VehicleVinValidChance;
    [IniReflectorValue(description: "The probability for a vehicle to have a scratched VIN. (0-100)")]
    internal int VehicleVinScratchedChance;
    // Driver
    [IniReflectorValue(description: "The probability for a vehicle's owner to be the current driver. (0-100; all owner settings should add up to 100!)")]
    internal int VehicleOwnerDriver;
    [IniReflectorValue(description: "The probability for a vehicle's owner to be a current passenger. (0-100)")]
    internal int VehicleOwnerPassenger;
    [IniReflectorValue(description: "The probability for a vehicle's owner to be a family member of a current occupant. (0-100)")]
    internal int VehicleOwnerFamily;
    [IniReflectorValue(description: "The probability for a vehicle's owner to be random. (0-100)")]
    internal int VehicleOwnerRandom;
    
    // Ped
    [IniReflectorValue(description: "The probability for a ped to be on probation. (0-100)")]
    internal int PedProbationChance;
    [IniReflectorValue(description: "The probability for a ped to be on parole. (0-100)")]
    internal int PedParoleChance;

    // Postals
    [IniReflectorValue(description: "The postals set to use.", defaultValue: "")]
    internal string PostalsSet;
    
    #region STATIC
    
    internal static Settings Instance { get; private set; }

    internal static void Load(string path)
    {
        Instance = new Settings();
        IniReflector<Settings> iniReflector = new(path);
        iniReflector.Read(Instance, true);
    }
    
    #endregion
}