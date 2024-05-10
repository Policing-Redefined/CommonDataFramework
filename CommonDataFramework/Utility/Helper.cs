using System;

namespace CommonDataFramework.Utility;

internal static class Helper
{
    internal static readonly Random Rnd = new(DateTime.Now.Millisecond);
}