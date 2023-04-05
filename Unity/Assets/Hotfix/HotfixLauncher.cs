using Animancer;
using UnityEngine;

namespace Hotfix
{
    public static class HotfixLauncher
    {
        public static void Main()
        {
            UnityGameFramework.Runtime.Log.Info("Hello World");
            GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<AnimancerComponent>();
            GameApp.Lua.DoString("CS.UnityEngine.Debug.Log('Hello World')");
        }
    }
}