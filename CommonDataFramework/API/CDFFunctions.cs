namespace CommonDataFramework.API;

/// <summary>
/// Holds public functions that aren't handled otherwise already.
/// </summary>
public static class CDFFunctions
{
    internal static bool SystemsLoaded;

    /// <summary>
    /// Determines whether the plugin has fully loaded and is ready to be used.
    /// </summary>
    /// <returns>True, if the plugin is ready.</returns>
    public static bool IsPluginReady() => SystemsLoaded;
}