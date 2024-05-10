using System;

namespace CommonDataFramework.Utility;

// Rohit said he wanted credit. So credit to Rohit for the code https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/Logger.cs
internal static class Logger
{
    private const string DefaultInfo = "[{0}] Common Data Framework: {1}";

    // consider calling 'DisplayErrorNotification' along with this
    internal static void LogException(Exception ex, string location)
    {
        Game.LogTrivial(string.Format(DefaultInfo, $"ERROR - {location}", ex));
    }

    internal static void LogDebug(string msg)
    {
        Game.LogTrivial(string.Format(DefaultInfo, "DEBUG", msg));
    }

    internal static void LogWarn(string msg)
    {
        Game.LogTrivial(string.Format(DefaultInfo, "WARN", msg));
    }
}