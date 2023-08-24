using Animancer;
using Base.Net;
using Cysharp.Threading.Tasks;
using Framework.Asset;
using Hotfix.Proto.Proto;
using Net;
using SimpleJSON;
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

            ProtoMessageIdHandler.Init(typeof(HotfixLauncher).Assembly);
            LoadConfig();

            NetManager.Singleton.Init();
            NetManager.Singleton.Connect(serverIp, serverPort);

            NetTest();

            // var gameNetworkComponent = GameEntry.GetComponent<GameNetworkComponent>();
            // gameNetworkComponent.ConnectedToServer(serverIp, serverPort);


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

        private static async void NetTest()
        {
            await UniTask.Delay(3000);


            var req = new ReqLogin
            {
                SdkType = 0,
                SdkToken = "",
                UserName = "Blank",
                Password = "123456",
                Device = SystemInfo.deviceUniqueIdentifier
            };
            req.Platform = PathHelper.GetPlatformName;
            NetManager.Singleton.Send(req);

            // NetManager.Singleton.Send(new ReqHeartBeat() {Timestamp = 2222});
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