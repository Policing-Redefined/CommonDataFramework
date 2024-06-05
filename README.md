# Common Data Framework
Common Data Framework 'CDF' is an open-source LSPDFR plugin that offers an extended vehicle and ped record API for developers. It's main goal is to replace missing features of the LSPDFR API while providing synchronization of that data across all plugins who use it. It is used by the upcoming 'Policing Redefined' plugin and is already planned to be implement in current and future plugins.
<br><br/>

## How to install
### User
- Download and install latest [LSPDFR and RagePluginHook](https://www.lcpdfr.com/downloads/gta5mods/g17media/7792-lspd-first-response/)
- Download latest [Common Data Framework](https://github.com/Policing-Redefined/CommonDataFramework/releases/)
- Extract the .zip and drag-and-drop everything into your main GTA 5 directory (except the .xml)
- *CDF does NOT do anything on itself, you must be using a plugin that has CDF as a dependency*

### Developer
*...all steps above*
- Reference `CommonDataFramework.dll` in your assembly
- [Recommended] If you want to make use of intellisense, the `CommonDataFramework.xml` must be in the same directory of your referenced `.dll`
- Jump to the [example code](https://github.com/Policing-Redefined/CommonDataFramework/tree/main?tab=readme-ov-file#example-for-developers)
- *You can also have your .dll etc. somewhere else but if you are running your code, then CDF **must** be present in the LSPDFR directory.*
<br><br/>

## Features
### General
- Lightweight and easy-to-use API: `ped.GetPedData()` and `vehicle.GetVehicleData()`, aswell as events
- Even when the entity stopped existing, CDF stores the data in the database for an extra period of time to ensure longer accessibility
- Very customizeable objects to alter almost any property to your needs
- End-user can customize probabilities through a simple `.ini` to enhance their gameplay experience without you having to write extra code

### Pedestrian Data
- Drivers license expiration
- Home address
- Permits (weapon, fishing, hunting)
- ...and more
- Syncs with LSPDFR persona, all persona fields can be directly accessed through the object too

### Vehicle Data
- VIN
- Registration
- Insurance
- Vehicle owner (of type `PedData`) aswell as different owner "types"

### Example images by PR
![Example of PR using PedData](https://i.ibb.co/NSLw5F1/Example-Ped-Data.png)
![Example of PR using VehicleData](https://i.ibb.co/1f5LttR/Example-Vehicle-Data.png)
<br><br/>

## Example for developers
```cs
using CommonDataFramework.API;
using CommonDataFramework.Modules;
using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Attributes;

public class EntryPoint : Plugin
{
    internal static bool OnDutyState;
    
    public override void Initialize()
    {
        LSPDFRFunctions.OnOnDutyStateChanged += OnOnDutyStateChanged;
        Game.AddConsoleCommands();
    }

    public override void Finally()
    {
        LSPDFRFunctions.OnOnDutyStateChanged -= OnOnDutyStateChanged;
    }

    // Example of getting ped data:
    [ConsoleCommand]
    internal static void Command_LogClosestPed(bool changeHuntingPermit)
    {
        Ped ped = Game.LocalPlayer.Character.GetNearbyPeds(1)[0];
        if (ped == null)
        {
            Game.LogTrivial("Could not find any nearby ped.");
            return;
        }

        PedData pedData = ped.GetPedData();
        Game.LogTrivial($"Ped name: {pedData.FullName}.");
        
        // Example of giving the ped a hunting permit:
        if (changeHuntingPermit && pedData.HuntingPermit.Status != EDocumentStatus.Valid)
        {
            pedData.HuntingPermit.Status = EDocumentStatus.Valid;
            Game.LogTrivial($"Hunting permit expiration: {pedData.HuntingPermit.ExpirationDate:MM/dd/yyyy}");
        }
    }

    // Example of getting vehicle data:
    [ConsoleCommand]
    internal static void Command_LogClosestVehicle(bool changeRegistration)
    {
        Vehicle vehicle = Game.LocalPlayer.Character.GetNearbyVehicles(1)[0];
        if (vehicle == null)
        {
            Game.LogTrivial("Could not find any nearby vehicle.");
            return;
        }

        VehicleData vehicleData = vehicle.GetVehicleData();
        Game.LogTrivial($"Vehicle VIN: {vehicleData.Vin}.");
        Game.LogTrivial($"Vehicle Owner: {vehicleData.Owner.FullName} (Type: {vehicleData.OwnerType})."); // <VehicleData>.Owner -> PedData
        
        // Example of setting a vehicle's registration as expired:
        if (changeRegistration && vehicleData.Registration.Status != EDocumentStatus.Expired)
        {
            vehicleData.Registration.Status = EDocumentStatus.Expired;
            Game.LogTrivial($"Vehicle registration expired on: {vehicleData.Registration.ExpirationDate:MM/dd/yyyy}");
        }
    }

    private static void OnOnDutyStateChanged(bool onDuty)
    {
        OnDutyState = onDuty;
        
        // CDF might take some time until it fully read the users .xml and .ini file on startup.
        // This means that if you try to access CDF stuff before it was marked as ready, it will have to fall back to default values.
        // One way of solving this:
        
        GameFiber.WaitUntil(CDFFunctions.IsPluginReady, 30000); // The fiber will wait until CDF loaded fully.
        // ...rest of your startup code that includes CDF usage.
        
        // Alternatively, if you don't care about the default values or you are not using CDF right on startup, you can simply skip this.
    }
```
