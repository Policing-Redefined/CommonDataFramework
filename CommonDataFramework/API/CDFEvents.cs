using CommonDataFramework.Modules.PedDatabase;
using CommonDataFramework.Modules.VehicleDatabase;

namespace CommonDataFramework.API;

/// <summary>
/// The delegate for the <see cref="CDFEvents.OnPluginStateChanged"/> event.
/// </summary>
/// <param name="ready">True, if the plugin is ready now.</param>
public delegate void OnPluginStateChangedDelegate(bool ready);

/// <summary>
/// The delegate for the <see cref="CDFEvents.OnPedDataRemoved"/> event.
/// </summary>
/// <param name="ped">The <see cref="Rage.Ped"/> that held the data.</param>
/// <param name="pedData">The <see cref="PedData"/> that was removed.</param>
public delegate void OnPedDataRemovedDelegate(Ped ped, PedData pedData);

/// <summary>
/// The delegate for the <see cref="CDFEvents.OnVehicleDataRemoved"/> event.
/// </summary>
/// <param name="vehicle">The <see cref="Rage.Vehicle"/> that held the data.</param>
/// <param name="vehicleData">The <see cref="VehicleData"/> that was removed.</param>
public delegate void OnVehicleDataRemovedDelegate(Vehicle vehicle, VehicleData vehicleData);

/// <summary>
/// Holds public events that can be subscribed to.
/// </summary>
public static class CDFEvents
{
    /// <summary>
    /// Invoked when the plugin either finishes loading or unloads.
    /// </summary>
    public static event OnPluginStateChangedDelegate OnPluginStateChanged;

    /// <summary>
    /// Invoked when the plugin removes <see cref="PedData"/> from it's database.
    /// </summary>
    /// <remarks>This is not invoked when the database is cleared.</remarks>
    public static event OnPedDataRemovedDelegate OnPedDataRemoved;

    /// <summary>
    /// Invoked when the plugin removes <see cref="VehicleData"/> from it's database.
    /// </summary>
    /// <remarks>This is not invoked when the database is cleared.</remarks>
    public static event OnVehicleDataRemovedDelegate OnVehicleDataRemoved;

    #region Invocation
    
    internal static void InvokeOnPluginStateChanged(bool ready) => OnPluginStateChanged?.Invoke(ready);

    internal static void InvokeOnPedDataRemoved(Ped ped, PedData pedData) => OnPedDataRemoved?.Invoke(ped, pedData);

    internal static void InvokeOnVehicleDataRemoved(Vehicle vehicle, VehicleData vehicleData) => OnVehicleDataRemoved?.Invoke(vehicle, vehicleData);

    #endregion
}