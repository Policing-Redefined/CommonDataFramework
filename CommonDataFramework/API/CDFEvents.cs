namespace CommonDataFramework.API;

/// <summary>
/// The delegate for the <see cref="CDFEvents.OnPluginStateChanged"/> event.
/// </summary>
/// <param name="ready">True, if the plugin is ready now.</param>
public delegate void OnPluginStateChangedDelegate(bool ready);

/// <summary>
/// Holds public events that can be subscribed to.
/// </summary>
public static class CDFEvents
{
    /// <summary>
    /// Invoked when the plugin either finishes loading or unloads.
    /// </summary>
    public static event OnPluginStateChangedDelegate OnPluginStateChanged;

    internal static void InvokeOnPluginStateChanged(bool ready) => OnPluginStateChanged?.Invoke(ready);
}