namespace Server.Extension
{
    public static class CollectionExtensions
    {
        #region DictionaryExtensions

        /// <summary>
        /// 合并字典中的键值对。如果字典中已存在指定的键，则使用指定的函数对原有值和新值进行合并；否则直接添加键值对。
        /// </summary>
        /// <typeparam name="TKey">键的类型。</typeparam>
        /// <typeparam name="TValue">值的类型。</typeparam>
        /// <param name="self">要合并的字典。</param>
        /// <param name="k">要添加或合并的键。</param>
        /// <param name="v">要添加或合并的值。</param>
        /// <param name="func">用于合并值的函数。</param>
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey k, TValue v, Func<TValue, TValue, TValue> func)
        {
            self[k] = self.TryGetValue(k, out var value) ? func(value, v) : v;
        }

        /// <summary>
        /// 获取指定键的值，如果字典中不存在该键，则使用指定的函数获取值并添加到字典中。
        /// </summary>
        /// <typeparam name="TKey">键的类型。</typeparam>
        /// <typeparam name="TValue">值的类型。</typeparam>
        /// <param name="self">要操作的字典。</param>
        /// <param name="key">要获取值的键。</param>
        /// <param name="valueGetter">用于获取值的函数。</param>
        /// <returns>指定键的值。</returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> valueGetter)
        {
            if (!self.TryGetValue(key, out var value))
            {
                value = valueGetter(key);
                self[key] = value;
            }

            return value;
        }

        /// <summary>
        /// 获取指定键的值，如果字典中不存在该键，则使用无参构造函数创建一个新的值并添加到字典中。
        /// </summary>
        /// <typeparam name="TKey">键的类型。</typeparam>
        /// <typeparam name="TValue">值的类型。</typeparam>
        /// <param name="self">要操作的字典。</param>
        /// <param name="key">要获取值的键。</param>
        /// <returns>指定键的值。</returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key) where TValue : new()
        {
            return GetOrAdd(self, key, k => new TValue());
        }

        /// <summary>
        /// 根据指定条件从字典中移除键值对。
        /// </summary>
        /// <typeparam name="TKey">键的类型。</typeparam>
        /// <typeparam name="TValue">值的类型。</typeparam>
        /// <param name="self">要操作的字典。</param>
        /// <param name="predict">判断是否移除键值对的条件。</param>
        /// <returns>移除的键值对数量。</returns>
        public static int RemoveIf<TKey, TValue>(this Dictionary<TKey, TValue> self, Func<TKey, TValue, bool> predict)
        {
            int count = 0;
            foreach (var kv in self)
            {
                if (predict(kv.Key, kv.Value))
                {
                    self.Remove(kv.Key);
                    count++;
                }
            }

            return count;
        }

        #endregion

        #region ICollectionExtensions

        /// <summary>
        /// 检查集合是否为 null 或空。
        /// </summary>
        /// <typeparam name="T">集合元素的类型。</typeparam>
        /// <param name="self">要检查的集合。</param>
        /// <returns>如果集合为 null 或空，则为 true；否则为 false。</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> self)
        {
            return self == null || self.Count <= 0;
        }

        #endregion

        #region List<T>

        /// <summary>
        /// 打乱列表中的元素顺序。
        /// </summary>
        /// <typeparam name="T">列表元素的类型。</typeparam>
        /// <param name="list">要打乱顺序的列表。</param>
        public static void Shuffer<T>(this List<T> list)
        {
            int n = list.Count;
            var r = ThreadLocalRandom.Current;
            for (int i = 0; i < n; i++)
            {
                int rand = r.Next(i, n);
                (list[i], list[rand]) = (list[rand], list[i]);
            }
        }

        /// <summary>
        /// 从列表中移除满足条件的元素。
        /// </summary>
        /// <typeparam name="T">列表元素的类型。</typeparam>
        /// <param name="list">要操作的列表。</param>
        /// <param name="condition">用于判断元素是否满足移除条件的委托。</param>
        public static void RemoveIf<T>(this List<T> list, Predicate<T> condition)
        {
            var idx = list.FindIndex(condition);
            while (idx >= 0)
            {
                list.RemoveAt(idx);
                idx = list.FindIndex(condition);
            }
        }

        #endregion

        /// <summary>
        /// 将一个可枚举集合的元素添加到哈希集合中。
        /// </summary>
        /// <typeparam name="T">哈希集合元素的类型。</typeparam>
        /// <param name="c">要添加元素的哈希集合。</param>
        /// <param name="e">要添加的元素的可枚举集合。</param>
        public static void AddRange<T>(this HashSet<T> c, IEnumerable<T> e)
        {
            foreach (var item in e)
            {
                c.Add(item);
            }
        }
    }
}