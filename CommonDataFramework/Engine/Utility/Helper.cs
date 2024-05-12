using System;
using System.Reflection;

namespace CommonDataFramework.Engine.Utility;

internal static class Helper
{
    internal static long CurrentMillis() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    
    internal static readonly Random Rnd = new(DateTime.Now.Millisecond);
    
    internal static readonly string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 5);
}