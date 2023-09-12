using Animancer;
using Cysharp.Threading.Tasks;
using Framework.Asset;
using GameFramework.Network;
using Hotfix.Proto.Proto;
using MessagePack;
using PolymorphicMessagePack;
using Resolvers;
using SimpleJSON;
using UnityEngine;
using UnityGameFramework.Network.Pipelines;
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

            GameApp.Lua.DoString("CS.UnityEngine.Debug.Log('Hello World Lua')");

            RegisterMessagePack();

            GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<AnimancerComponent>();

            ProtoMessageIdHandler.Init(typeof(HotfixProtoHandler).Assembly);
            LoadConfig();

            NetManager.Singleton.Init();
            NetManager.Singleton.Connect(serverIp, serverPort);

            NetTest();
        }

        static void RegisterMessagePack()
        {
            PolymorphicResolver.AddInnerResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
            PolymorphicResolver.AddInnerResolver(MessagePack.Resolvers.GeneratedResolver.Instance);
            PolymorphicResolver.AddInnerResolver(MessageResolver.Instance);
            Server.Proto.Formatter.PolymorphicRegister.Register();
            PolymorphicRegister.Load();
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