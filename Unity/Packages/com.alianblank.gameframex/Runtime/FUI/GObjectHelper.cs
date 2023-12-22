using FairyGUI;
using System.Collections.Generic;

namespace GameFrameX.Runtime
{
    public static class GObjectHelper
    {
        private static readonly Dictionary<GObject, FUI> KeyValuePairs = new Dictionary<GObject, FUI>();

        public static T Get<T>(this GObject self) where T : FUI
        {
            if (self != null && KeyValuePairs.TryGetValue(self, out var pair))
            {
                return pair as T;
            }

            return default(T);
        }

        public static void Add(this GObject self, FUI fui)
        {
            if (self != null && fui != null)
            {
                KeyValuePairs[self] = fui;
            }
        }

        public static FUI Remove(this GObject self)
        {
            if (self != null && KeyValuePairs.ContainsKey(self))
            {
                FUI result = KeyValuePairs[self];
                KeyValuePairs.Remove(self);
                return result;
            }

            return null;
        }
    }
}