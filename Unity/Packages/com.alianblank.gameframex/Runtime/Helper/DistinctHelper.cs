using System;
using System.Collections.Generic;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 去重。帮助类
    /// </summary>
    public static class DistinctHelper
    {
        /// <summary>
        /// 根据条件去重
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
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