using Server.Extension;

namespace Server.Utility
{
    /// <summary>
    /// 随机相关的实用函数。
    /// </summary>
    public static class Random
    {
        private static System.Random _random = new System.Random((int)DateTime.UtcNow.Ticks);

        /// <summary>
        /// 设置随机数种子。
        /// </summary>
        /// <param name="seed">随机数种子。</param>
        public static void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <summary>
        /// 返回非负随机数。
        /// </summary>
        /// <returns>大于等于零且小于 System.Int32.MaxValue 的 32 位带符号整数。</returns>
        public static int GetRandom()
        {
            return _random.Next();
        }

        /// <summary>
        /// 返回一个小于所指定最大值的非负随机数。
        /// </summary>
        /// <param name="maxValue">要生成的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于零。</param>
        /// <returns>大于等于零且小于 maxValue 的 32 位带符号整数，即：返回值的范围通常包括零但不包括 maxValue。不过，如果 maxValue 等于零，则返回 maxValue。</returns>
        public static int GetRandom(int maxValue)
        {
            return _random.Next(maxValue);
        }

        /// <summary>
        /// 返回一个指定范围内的随机数。
        /// </summary>
        /// <param name="minValue">返回的随机数的下界（随机数可取该下界值）。</param>
        /// <param name="maxValue">返回的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于 minValue。</param>
        /// <returns>一个大于等于 minValue 且小于 maxValue 的 32 位带符号整数，即：返回的值范围包括 minValue 但不包括 maxValue。如果 minValue 等于 maxValue，则返回 minValue。</returns>
        public static int GetRandom(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        /// <summary>
        /// 返回一个介于 0.0 和 1.0 之间的随机数。
        /// </summary>
        /// <returns>大于等于 0.0 并且小于 1.0 的双精度浮点数。</returns>
        public static double GetRandomDouble()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// 用随机数填充指定字节数组的元素。
        /// </summary>
        /// <param name="buffer">包含随机数的字节数组。</param>
        public static void GetRandomBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }


        /// <summary>
        /// 从1~n中随机选取m个数，m < n
        /// </summary>
        public static HashSet<int> RandomSelect(int m, int n)
        {
            if (m == 0) return new HashSet<int>();

            var s = RandomSelect(m - 1, n - 1);
            var i = ThreadLocalRandom.Current.Next(0, n);
            s.Add(s.Contains(i) ? n - 1 : i);
            return s;
        }

        /// <summary>
        /// 根据权重随机选取，如果需求数量超过权重和（除以权重最大公约数后的），那么按照权重比例加入id，剩余数量再进行随机
        /// 不可重复随机num一定小于等于id数量
        /// </summary>
        private static List<int[]> RandomSelect(string weightStr, int num, int weightIndex, bool canRepeat = true)
        {
            var array = weightStr.SplitTo2IntArray(';', '+');
            return RandomSelect(array, num, weightIndex, canRepeat);
        }

        private static List<int[]> RandomSelect(int[][] array, int num, int weightIndex, bool canRepeat = true)
        {
            var random = ThreadLocalRandom.Current;
            if (canRepeat)
            {
                // 可重复
                return CanRepeatRandom(array, num, weightIndex, random);
            }
            else
            {
                // 不可重复，需求数量不应超过id数量
                if (num > array.Length)
                    throw new ArgumentException($"can't repeat random arg error, num:{num} is great than id count:{array.Length}");

                return NoRepeatRandom(num, weightIndex, random, array);
            }
        }

        private static List<int[]> NoRepeatRandom(int num, int weightIndex, System.Random random, int[][] array)
        {
            var results = new List<int[]>();
            var idxSet = new HashSet<int>();
            for (int i = 0; i < num; i++)
            {
                int totalWeight = 0;
                for (int j = 0; j < array.Length; j++)
                {
                    if (!idxSet.Contains(j))
                        totalWeight += array[j][weightIndex];
                }

                int r = random.Next(totalWeight);
                int temp = 0;
                int idx = 0;
                for (int j = 0; j < array.Length; j++)
                {
                    if (!idxSet.Contains(j))
                    {
                        temp += array[j][weightIndex];
                        if (temp > r)
                        {
                            idx = j;
                            break;
                        }
                    }
                }

                idxSet.Add(idx);
                results.Add(array[idx]);
            }

            return results;
        }

        private static List<int[]> CanRepeatRandom(int[][] array, int num, int weightIndex, System.Random random = null)
        {
            if (random == null)
            {
                random = ThreadLocalRandom.Current;
            }

            int totalWeight = 0;
            foreach (var arr in array)
            {
                totalWeight += arr[weightIndex];
            }

            var results = new List<int[]>(num);
            for (int i = 0; i < num; i++)
            {
                results.Add(SingleRandom(array, totalWeight, weightIndex, random));
            }

            return results;
        }

        /// <summary>
        /// 根据权重独立随机
        /// </summary>
        private static List<int[]> CanRepeatRandom(string weightStr, int num, int weightIndex, System.Random random = null)
        {
            var array = weightStr.SplitTo2IntArray(';', '+');
            return CanRepeatRandom(array, num, weightIndex, random);
        }

        /// <summary>
        /// 单次随机
        /// </summary>
        private static int[] SingleRandom(int[][] array, int totalWeight, int weightIndex, System.Random random)
        {
            int r = random.Next(totalWeight);
            int temp = 0;
            foreach (var arr in array)
            {
                temp += arr[weightIndex];
                if (temp > r)
                {
                    return arr;
                }
            }

            return array[0];
        }

        public static int Idx(int[] weights)
        {
            int totalWight = weights.Sum();
            int r = ThreadLocalRandom.Current.Next(totalWight);
            int temp = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                temp += weights[i];
                if (temp > r)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int Idx(int[][] array, int weightIndex = 1)
        {
            var random = ThreadLocalRandom.Current;
            int totalWeight = 0;
            foreach (var arr in array)
            {
                totalWeight += arr[weightIndex];
            }

            var r = random.Next(totalWeight);
            int temp = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var arr = array[i];
                temp += arr[weightIndex];
                if (temp > r)
                {
                    return i;
                }
            }

            return 0;
        }


        public static List<int> Ids(int[][] array, int num, bool canRepeat = true)
        {
            return RandomSelect(array, num, 1, canRepeat).Select(t => t[0]).ToList();
        }

        public static List<int> Ids(string str, int num, bool canRepeat = true)
        {
            return RandomSelect(str, num, 1, canRepeat).Select(t => t[0]).ToList();
        }


        public static List<int[]> Items(string str, int num, bool canRepeat = true)
        {
            return RandomSelect(str, num, 2, canRepeat);
        }

        public static List<int[]> Items(int[][] array, int num, bool canRepeat = true)
        {
            return RandomSelect(array, num, 2, canRepeat);
        }


        /// <summary>
        /// 求多个数的最大公约数
        /// </summary>
        public static int Gcd(params int[] input)
        {
            if (input.Length == 0)
                return 1;
            if (input.Length == 1)
                return input[0];

            int n = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                n = Gcd(n, input[i]);
            }

            return n;
        }

        /// <summary>
        /// 求两个数的最大公约数
        /// </summary>
        public static int Gcd(int a, int b)
        {
            if (a < b)
            {
                (b, a) = (a, b);
            }

            if (b == 0)
                return a;
            else
                return Gcd(b, a % b);
        }
    }
}