using System.Collections.Generic;
using System.Linq;

namespace CommonDataFramework.Engine.Utility.Extensions;

internal static class CollectionExtensions
{
    internal static T Random<T>(this IEnumerable<T> enumerable)
    {
        T[] array = enumerable as T[] ?? enumerable.ToArray();
        return array[Rnd.Next(array.Length)];
    }
}