using System;
using System.Reflection;
using System.Text;
using CommonDataFramework.Engine.Utility.Extensions;

namespace CommonDataFramework.Engine.Utility;

internal static class Helper
{
    internal static Ped MainPlayer => Game.LocalPlayer.Character;
    
    internal static long CurrentMillis() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    
    internal static readonly Random Rnd = new(DateTime.Now.Millisecond);
    
    internal static readonly string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 5);

    internal const string DefaultPluginPath = "plugins/LSPDFR";
    internal const string DefaultPluginFolder = DefaultPluginPath + "/CommonDataFramework";

    internal static Settings CDFSettings => Settings.Instance;

    internal static string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        StringBuilder stringBuilder = new(length);
        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars.Random());
        }

        return stringBuilder.ToString();
    }
    
    internal static int GetRandomChance() => Rnd.Next(0, 101);

    internal static bool GetRandomChance(int chance) => GetRandomChance() <= chance;
}