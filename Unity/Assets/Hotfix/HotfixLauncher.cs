using Animancer;
using UnityEngine;

namespace Hotfix
{
    public static class HotfixLauncher
    {
        public static void Main()
        {
            UnityGameFramework.Runtime.Log.Info("AAAAAAAAA");
            GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<AnimancerComponent>();
        }
    }
}