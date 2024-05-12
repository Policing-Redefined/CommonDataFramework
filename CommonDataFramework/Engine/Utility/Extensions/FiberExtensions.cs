using System;

namespace CommonDataFramework.Engine.Utility.Extensions;

internal static class FiberExtensions
{
    internal static void AbortSafe(this GameFiber fiber)
    {
        GameFiber.StartNew(() =>
        {
            try
            {
                fiber?.Abort();
            }
            catch (Exception)
            {
                // ignored
            }
        });
    }
}