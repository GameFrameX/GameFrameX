using System;
using System.Net;
using Cysharp.Threading.Tasks;
using Framework.Asset;
using GameFrameX;
using GameFrameX.Event;
using GameFrameX.Network;
using Hotfix.Proto.Proto;
using Resolvers;
using SimpleJSON;
using UnityEngine;
using GameFrameX.Runtime;
using GameMain;
using Hotfix.Network;
using Hotfix.UI;

namespace Hotfix
{
    public static class HotfixLauncher
    {
        public static string serverIp = "127.0.0.1";
        public static int serverPort = 8898;
        private static INetworkChannel networkChannel;

        public static void Main()
        {
            Log.Info("Hello World HybridCLR");

            GameApp.Lua.DoString("CS.UnityEngine.Debug.Log('Hello World Lua')");

            RegisterMessagePack();

            ProtoMessageIdHandler.Init(HotfixProtoHandler.CurrentAssembly);
            LoadConfig();


            var uiLogin = GameApp.UI.Add<UILogin>(UILogin.CreateInstance, AssetUtility.GetUIPackagePath(FUIPackage.UILogin), UILayer.Floor);
            uiLogin.m_enter.onClick.Add(() =>
            {
                if (networkChannel != null && networkChannel.Connected)
                {
                    NetTest();
                    return;
                }

                if (networkChannel != null && GameApp.Network.HasNetworkChannel("network") && !networkChannel.Connected)
                {
                    GameApp.Network.DestroyNetworkChannel("network");
                }

                networkChannel = GameApp.Network.CreateNetworkChannel("network", new DefaultNetworkChannelHelper());
                // NetManager.Singleton.Init();
                // 注册心跳消息
                DefaultPacketHeartBeatHandler packetSendHeaderHandler = new DefaultPacketHeartBeatHandler();
                networkChannel.RegisterHandler(packetSendHeaderHandler);
                networkChannel.Connect(IPAddress.Parse(serverIp), serverPort);
                networkChannel.SetDefaultHandler((sender, e) =>
                {
                    Log.Info("Receive: " + e);
                });
                GameApp.Event.Subscribe(NetworkConnectedEventArgs.EventId, OnNetworkConnected);
                GameApp.Event.Subscribe(NetworkClosedEventArgs.EventId, OnNetworkClosed);
            });
        }

        private static void OnNetworkClosed(object sender, GameEventArgs e)
        {
            Log.Info(nameof(OnNetworkClosed));
        }

        private static void OnNetworkConnected(object sender, GameEventArgs e)
        {
            Log.Info(nameof(OnNetworkConnected));
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
            await UniTask.Delay(1);


            var req = new ReqLogin
            {
                SdkType = 0,
                SdkToken = "",
                UserName = "Blank",
                Password = "123456",
                Device = SystemInfo.deviceUniqueIdentifier
            };
            req.Platform = PathHelper.GetPlatformName;
            networkChannel.Send(req);

            // NetManager.Singleton.Send(new ReqHeartBeat() {Timestamp = 2222});
        }

        static void LoadConfig()
        {
            // var tables = new cfg.Tables(file =>
            //     JSON.Parse(File.ReadAllText(AssetUtility.GetConfigPath(file, "json"))
            //     ));


            var tables = new cfg.Tables(Loader, null);
            var item = tables.TbItem.Get(1);
            Log.Info(item);
        }

        private static JSONNode Loader(string file)
        {
            string assetPath = AssetUtility.GetConfigPath(file, Utility.Const.FileNameSuffix.Json);
            Log.Info(assetPath);
            var rawFileOperationHandle = GameApp.Asset.LoadRawFileSync(assetPath);

            return JSON.Parse(rawFileOperationHandle.GetRawFileText());
        }
    }
}