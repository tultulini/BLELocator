using System.Collections.Generic;

namespace BLELocator.Core.Utils
{
    public  static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
        public static bool HasSomething<T>(this ICollection<T> collection)
        {
            return !collection.IsNullOrEmpty();
        }
    }
}