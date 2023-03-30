using System;
using System.Collections.Generic;

namespace UnityGameFramework.Runtime
{
    public static class DistinctHelper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var identifiedKeys = new HashSet<TKey>();

            foreach (var item in source)
            {
                if (identifiedKeys.Add(keySelector(item)))
                {
                    yield return item;
                }
            }
        }
    }
}