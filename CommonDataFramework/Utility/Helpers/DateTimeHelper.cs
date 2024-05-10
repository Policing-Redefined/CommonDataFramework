using System;

namespace CommonDataFramework.Utility.Helpers;

internal static class DateTimeHelper
{
    internal static DateTime CurrentDate => DateTime.Now;
    
    internal static DateTime GetRandomDateTimeWithinRange(int maxYearsBack)
    {
        DateTime currentDate = CurrentDate;
        DateTime startDate = currentDate.AddYears(-maxYearsBack);
        long range = currentDate.Ticks - startDate.Ticks;

        long randomTicks = (long)(range * Rnd.NextDouble()) + startDate.Ticks;
        DateTime randomDate = new(randomTicks);

        return randomDate;
    }
    
    internal static DateTime GetRandomDateTimeWithinRange(DateTime maxYearsAhead)
    {
        DateTime startDate = maxYearsAhead;
        long range = CurrentDate.Ticks - startDate.Ticks;

        long randomTicks = (long)(range * Rnd.NextDouble()) + startDate.Ticks;
        DateTime randomDate = new(randomTicks);

        return randomDate;
    }
}