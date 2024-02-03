using FairyGUI;
using System.Collections.Generic;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// GObject 帮助类
    /// </summary>
    public static class GObjectHelper
    {
        private static readonly Dictionary<GObject, FUI> KeyValuePairs = new Dictionary<GObject, FUI>();

        /// <summary>
        /// 从组件池中获取UI对象
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>UI对象</returns>
        public static T Get<T>(this GObject self) where T : FUI
        {
            if (self != null && KeyValuePairs.TryGetValue(self, out var pair))
            {
                return pair as T;
            }

            return default(T);
        }

        /// <summary>
        /// 添加UI对象到组件池
        /// </summary>
        /// <param name="self"></param>
        /// <param name="fui">UI对象</param>
        public static void Add(this GObject self, FUI fui)
        {
            if (self != null && fui != null)
            {
                KeyValuePairs[self] = fui;
            }
        }

        /// <summary>
        /// 从组件池中删除UI对象。返回删除的UI对象
        /// </summary>
        /// <param name="self"></param>
        /// <returns>UI对象</returns>
        public static FUI Remove(this GObject self)
        {
            if (self != null && KeyValuePairs.TryGetValue(self, out var value))
            {
                KeyValuePairs.Remove(self);
                return value;
            }

            return default;
        }
    }
}