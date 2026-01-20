using System.Collections;

namespace Utils
{
    public static class CollectionUtils
    {
        public static bool IsNullOrEmpty<T>(this T collection)
            where T : ICollection
        {
            return collection is null || collection.Count == 0;
        }
    }
}