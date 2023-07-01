using Animancer;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Hotfix
{
    public static class HotfixLauncher
    {
        public static string serverIp = "127.0.0.1";
        public static int serverPort = 8898;

        public static void Main()
        {
            Log.Info("Hello World HybridCLR");
            GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<AnimancerComponent>();
            GameApp.Lua.DoString("CS.UnityEngine.Debug.Log('Hello World Lua')");

            LoadConfig();
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
        static void LoadConfig()
        {
            // var tables = new cfg.Tables(file =>
            //     JSON.Parse(File.ReadAllText(AssetUtility.GetConfigPath(file, "json"))
            //     ));


            var tables = new cfg.Tables(Loader);
            var item = tables.TbItem.Get(1);
            Log.Info(item);
        }

        private static JSONNode Loader(string file)
        {
            var rawFileOperationHandle = GameApp.Asset.LoadRawFileSync(AssetUtility.GetConfigPath(file, ".json"));

            return JSON.Parse(rawFileOperationHandle.GetRawFileText());
        }
    }
}