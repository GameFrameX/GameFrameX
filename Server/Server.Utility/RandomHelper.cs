using Server.Extension;

namespace Server.Utility
{
    public static class RandomHelper
    {
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

        private static List<int[]> NoRepeatRandom(int num, int weightIndex, Random random, int[][] array)
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

        private static List<int[]> CanRepeatRandom(int[][] array, int num, int weightIndex, Random random = null)
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
        private static List<int[]> CanRepeatRandom(string weightStr, int num, int weightIndex, Random random = null)
        {
            var array = weightStr.SplitTo2IntArray(';', '+');
            return CanRepeatRandom(array, num, weightIndex, random);
        }

        /// <summary>
        /// 单次随机
        /// </summary>
        private static int[] SingleRandom(int[][] array, int totalWeight, int weightIndex, Random random)
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

        public static int Idx(string str, int weightIndex = 1)
        {
            var array = str.SplitTo2IntArray(';', '+');
            return Idx(array, weightIndex);
        }


        public static List<int> Ids(int[][] array, int num, bool canRepeat = true)
        {
            return RandomSelect(array, num, 1, canRepeat).Select(t => t[0]).ToList();
        }

        public static List<int> Ids(string str, int num, bool canRepeat = true)
        {
            return RandomSelect(str, num, 1, canRepeat).Select(t => t[0]).ToList();
        }

        public static int Id(Dictionary<int, int> dic)
        {
            return Id(dic.Select(kv => new int[2] {kv.Key, kv.Value}).ToArray());
        }

        public static int Id(string str)
        {
            return SingleWeightRandom(str, weightIndex: 1)[0];
        }

        public static int Id(int[][] array)
        {
            return SingleWeightRandom(array, weightIndex: 1)[0];
        }

        public static int[] Item(string str)
        {
            return SingleWeightRandom(str, weightIndex: 2);
        }

        public static List<int[]> Items(string str, int num, bool canRepeat = true)
        {
            return RandomSelect(str, num, 2, canRepeat);
        }

        public static List<int[]> Items(int[][] array, int num, bool canRepeat = true)
        {
            return RandomSelect(array, num, 2, canRepeat);
        }

        public static int[] Item(int[][] array)
        {
            return SingleWeightRandom(array, weightIndex: 2);
        }

        private static int[] SingleWeightRandom(int[][] array, int weightIndex = 2, Random random = null)
        {
            if (random == null)
            {
                random = ThreadLocalRandom.Current;
            }

            int totalWeight = 0;
            foreach (var item in array)
            {
                totalWeight += item[weightIndex];
            }

            return SingleRandom(array, totalWeight, weightIndex, random);
        }

        private static int[] SingleWeightRandom(string str, int weightIndex = 2, Random random = null)
        {
            var array = str.SplitTo2IntArray(';', '+');
            return SingleWeightRandom(array, weightIndex, random);
        }


        /// <summary>
        /// 求多个数的最大公约数
        /// </summary>
        public static int Gcd(params int[] input)
        {
            if (input == null || input.Length == 0)
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