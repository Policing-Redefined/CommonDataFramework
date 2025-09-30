using System;
using System.ComponentModel;
using CommonDataFramework.Engine.Utility.Extensions;
using CommonDataFramework.Engine.Utility.Helpers;
using CommonDataFramework.Modules.PedDatabase;
using LSPD_First_Response.Engine.Scripting.Entities;
using CommonDataFramework.Modules.VehicleDatabase;

namespace CommonDataFramework.Modules.VehicleDatabase;

/// <summary>
/// Context for owner assignment operations.
/// </summary>
internal class OwnerAssignmentContext
{
    public VehicleData VehicleData { get; }
    public Vehicle Vehicle => VehicleData.Holder;
    public EVehicleOwnerType? RequestedOwnerType { get; }
    public PedData ManualPedData { get; }
    public bool IsStolen => VehicleData.IsStolen;
    public bool IsEmergencyVehicle => Vehicle.Exists() && Vehicle.Model.IsEmergencyVehicle;
    public bool VehicleExists => Vehicle.Exists();
    public bool HasOccupants => VehicleExists && Vehicle.Occupants.Length > 0;
    public bool HasDriver => VehicleExists && Vehicle.Driver != null;
    public bool HasPassengers => VehicleExists && Vehicle.Passengers.Length > 0;

    public OwnerAssignmentContext(VehicleData vehicleData, EVehicleOwnerType? requestedOwnerType, PedData manualPedData)
    {
        VehicleData = vehicleData;
        RequestedOwnerType = requestedOwnerType;
        ManualPedData = manualPedData;
    }
}

/// <summary>
/// Result of an owner assignment operation.
/// </summary>
internal class OwnerAssignmentResult
{
    public bool Assigned { get; }
    public PedData Owner { get; }
    public EVehicleOwnerType OwnerType { get; }

    public OwnerAssignmentResult(bool success, PedData owner, EVehicleOwnerType ownerType)
    {
        Assigned = success;
        Owner = owner;
        OwnerType = ownerType;
    }

    public static OwnerAssignmentResult Failure() => new(false, null, EVehicleOwnerType.RandomPed);
    public static OwnerAssignmentResult Success(PedData owner, EVehicleOwnerType ownerType) => new(true, owner, ownerType);
}

/// <summary>
/// Strategy for assigning vehicle owners.
/// </summary>
internal interface IOwnerAssignmentStrategy
{
    OwnerAssignmentResult AssignOwner(OwnerAssignmentContext context);
}

/// <summary>
/// Factory for creating owner assignment strategies.
/// </summary>
internal static class OwnerAssignmentStrategyFactory
{
    public static IOwnerAssignmentStrategy CreateStrategy(OwnerAssignmentContext context)
    {
        // Priority order: Government > Stolen > Manual > Specific Type > Auto-detect
        
        if (context.IsEmergencyVehicle && context.RequestedOwnerType == null)
        {
            return new GovernmentOwnerStrategy();
        }
        
        if (context.IsStolen || ShouldBeStolen(context))
        {
            return new StolenVehicleStrategy();
        }
        
        if (context.RequestedOwnerType == EVehicleOwnerType.Manual)
        {
            return new ManualOwnerStrategy();
        }
        
        if (context.RequestedOwnerType.HasValue)
        {
            return new SpecificOwnerTypeStrategy();
        }
        
        return new AutoDetectOwnerStrategy();
    }
    
    private static bool ShouldBeStolen(OwnerAssignmentContext context)
    {
        return context.RequestedOwnerType == null && GetRandomChance(CDFSettings.VehicleStolenChance);
    }
}

/// <summary>
/// Strategy for government vehicles.
/// </summary>
internal class GovernmentOwnerStrategy : IOwnerAssignmentStrategy
{
    public OwnerAssignmentResult AssignOwner(OwnerAssignmentContext context)
    {
        var gov = PersonaHelper.GenerateNewPersona();
        gov.Forename = "";
        gov.Surname = "Government";
        gov.ELicenseState = ELicenseState.Valid;
        gov.Wanted = false;

        var owner = new PedData(gov);
        return OwnerAssignmentResult.Success(owner, EVehicleOwnerType.Government);
    }
}

/// <summary>
/// Strategy for stolen vehicles.
/// </summary>
internal class StolenVehicleStrategy : IOwnerAssignmentStrategy
{
    public OwnerAssignmentResult AssignOwner(OwnerAssignmentContext context)
    {
        // Only allow RandomPed or Manual for stolen vehicles
        if (context.RequestedOwnerType.HasValue && 
            context.RequestedOwnerType != EVehicleOwnerType.RandomPed && 
            context.RequestedOwnerType != EVehicleOwnerType.Manual)
        {
            return OwnerAssignmentResult.Failure();
        }
        
        if (context.RequestedOwnerType == EVehicleOwnerType.Manual)
        {
            if (context.ManualPedData == null)
            {
                return OwnerAssignmentResult.Failure();
            }
            return OwnerAssignmentResult.Success(context.ManualPedData, EVehicleOwnerType.Manual);
        }
        
        // Mark as stolen and use random ped
        context.VehicleData.IsStolen = true;
        var owner = new PedData(PersonaHelper.GenerateNewPersona());
        return OwnerAssignmentResult.Success(owner, EVehicleOwnerType.RandomPed);
    }
}

/// <summary>
/// Strategy for manually assigned owners.
/// </summary>
internal class ManualOwnerStrategy : IOwnerAssignmentStrategy
{
    public OwnerAssignmentResult AssignOwner(OwnerAssignmentContext context)
    {
        if (context.ManualPedData == null)
        {
            return OwnerAssignmentResult.Failure();
        }
        
        return OwnerAssignmentResult.Success(context.ManualPedData, EVehicleOwnerType.Manual);
    }
}

/// <summary>
/// Strategy for specific owner types.
/// </summary>
internal class SpecificOwnerTypeStrategy : IOwnerAssignmentStrategy
{
    public OwnerAssignmentResult AssignOwner(OwnerAssignmentContext context)
    {
        var ownerType = context.RequestedOwnerType.Value;
        
        // Check if vehicle exists for occupant-based owner types
        if (!context.VehicleExists && IsOccupantBasedOwnerType(ownerType))
        {
            return OwnerAssignmentResult.Failure();
        }
        
        // Check stolen vehicle restrictions
        if (context.IsStolen && ownerType != EVehicleOwnerType.RandomPed && ownerType != EVehicleOwnerType.Manual)
        {
            return OwnerAssignmentResult.Failure();
        }
        
        return AssignOwnerByType(context, ownerType);
    }
    
    private static bool IsOccupantBasedOwnerType(EVehicleOwnerType ownerType)
    {
        return ownerType is EVehicleOwnerType.Driver or EVehicleOwnerType.Passenger or EVehicleOwnerType.FamilyMember;
    }
    
    private static OwnerAssignmentResult AssignOwnerByType(OwnerAssignmentContext context, EVehicleOwnerType ownerType)
    {
        return ownerType switch
        {
            EVehicleOwnerType.Driver => AssignDriverOwner(context),
            EVehicleOwnerType.Passenger => AssignPassengerOwner(context),
            EVehicleOwnerType.FamilyMember => AssignFamilyMemberOwner(context),
            EVehicleOwnerType.RandomPed => AssignRandomPedOwner(context),
            _ => throw new InvalidEnumArgumentException($"{nameof(EVehicleOwnerType)}: Invalid owner type: {ownerType}.")
        };
    }
    
    private static OwnerAssignmentResult AssignDriverOwner(OwnerAssignmentContext context)
    {
        if (!context.HasDriver)
        {
            return OwnerAssignmentResult.Failure();
        }
        
        var owner = context.Vehicle.Driver.GetPedData();
        return OwnerAssignmentResult.Success(owner, EVehicleOwnerType.Driver);
    }
    
    private static OwnerAssignmentResult AssignPassengerOwner(OwnerAssignmentContext context)
    {
        if (!context.HasPassengers)
        {
            return OwnerAssignmentResult.Failure();
        }
        
        var owner = context.Vehicle.Passengers.Random().GetPedData();
        return OwnerAssignmentResult.Success(owner, EVehicleOwnerType.Passenger);
    }
    
    private static OwnerAssignmentResult AssignFamilyMemberOwner(OwnerAssignmentContext context)
    {
        if (!context.HasDriver)
        {
            return OwnerAssignmentResult.Failure();
        }
        
        // Get driver and potentially passenger data
        var driverData = context.Vehicle.Driver.GetPedData();
        var passengerToUse = (context.HasPassengers && VehicleData.GetRandomOwnerType() == EVehicleOwnerType.Passenger) 
            ? context.Vehicle.Passengers.Random() 
            : null;
        var passengerData = passengerToUse?.GetPedData();
        
        // Generate random family member
        var owner = new PedData(PersonaHelper.GenerateNewPersona());
        
        // Match family names
        driverData.Lastname = owner.Lastname;
        if (passengerData != null)
        {
            passengerData.Lastname = owner.Lastname;
        }
        
        return OwnerAssignmentResult.Success(owner, EVehicleOwnerType.FamilyMember);
    }
    
    private static OwnerAssignmentResult AssignRandomPedOwner(OwnerAssignmentContext context)
    {
        var owner = new PedData(PersonaHelper.GenerateNewPersona());
        return OwnerAssignmentResult.Success(owner, EVehicleOwnerType.RandomPed);
    }
}

/// <summary>
/// Strategy for auto-detecting owner type.
/// </summary>
internal class AutoDetectOwnerStrategy : IOwnerAssignmentStrategy
{
    public OwnerAssignmentResult AssignOwner(OwnerAssignmentContext context)
    {
        var suitableOwnerType = GetSuitableOwnerType(context.Vehicle);
        var specificStrategy = new SpecificOwnerTypeStrategy();
        
        // Create a new context with the detected owner type
        var newContext = new OwnerAssignmentContext(context.VehicleData, suitableOwnerType, context.ManualPedData);
        return specificStrategy.AssignOwner(newContext);
    }
    
    private static EVehicleOwnerType GetSuitableOwnerType(Vehicle vehicle)
    {
        if (!vehicle.Exists() || vehicle.Occupants.Length == 0) 
            return EVehicleOwnerType.RandomPed;
            
        var ownerType = VehicleData.GetRandomOwnerType();
        return ownerType switch
        {
            EVehicleOwnerType.Driver when vehicle.Driver == null => EVehicleOwnerType.RandomPed,
            EVehicleOwnerType.FamilyMember when vehicle.Driver == null => EVehicleOwnerType.RandomPed,
            EVehicleOwnerType.Passenger when vehicle.Passengers.Length == 0 => 
                VehicleData.GetRandomOwnerType() == EVehicleOwnerType.Driver ? EVehicleOwnerType.Driver : EVehicleOwnerType.RandomPed,
            _ => ownerType
        };
    }
}
