using Animancer;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Hotfix
{
    public static class HotfixLauncher
    {
        public static void Main()
        {
            Log.Info("Hello World hybridCLR");
            GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<AnimancerComponent>();
            GameApp.Lua.DoString("CS.UnityEngine.Debug.Log('Hello World lua')");
            // MemoryStream memoryStream = new MemoryStream();
            // var userInfo = new CSHeartBeat();
            // userInfo.Timestamp = 11111111;
            // RuntimeTypeModel.Default.Serialize(memoryStream, userInfo);
            // var buffer = memoryStream.ToArray();
            //
            // using (MemoryStream desMemoryStream = new MemoryStream(buffer))
            // {
            //     var deserializedData = RuntimeTypeModel.Default.Deserialize(desMemoryStream, null, typeof(CSHeartBeat));
            //     Log.Warning(Utility.Json.ToJson(deserializedData));
            // }
        }
    }
}