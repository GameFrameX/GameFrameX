using Microsoft.AspNetCore.Connections;
using Server.Launcher.Common.Net;
using Server.Launcher.Common.Session;
using Server.NetWork.HTTP;
using Server.NetWork.TCPSocket;
using Server.NetWork.WebSocket;

namespace Server.Hotfix.Common
{
    internal class HotfixBridge : IHotfixBridge
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

        public ServerType BridgeType => ServerType.Game;

        public async Task<bool> OnLoadSuccess(bool reload)
        {
            if (reload)
            {
                ActorManager.ClearAgent();
                return true;
            }

            Log.Info("load config data");
            // PolymorphicTypeMapper.Register(this.GetType().Assembly);
            ProtoMessageIdHandler.Init();
            HotfixMgr.SetMsgGetterByGetId((type) =>
            {
                // Log.Debug("MsgType:" + type);
                return ProtoMessageIdHandler.GetRespMessageIdByType(type);
            });
            HotfixMgr.SetMsgGetter((messageId) =>
            {
                // Log.Debug("MsgID:" + messageId);
                return ProtoMessageIdHandler.GetReqTypeById(messageId);
            });

            var webSocketMessageHelper = new WebSocketMessageHelper(HotfixMgr.GetTcpHandler, HotfixMgr.GetMsgTypeById, HotfixMgr.GetMsgIdByType);
            await WebSocketServer.Start(GlobalSettings.WsPort, GlobalSettings.WssPort, webSocketMessageHelper, new WebSocketChannelHandler());
            Log.Info("WebSocket 服务启动完成...");

            var tcpSocketMessageHelper = new TcpSocketMessageHelper(HotfixMgr.GetTcpHandler, HotfixMgr.GetMsgTypeById, HotfixMgr.GetMsgIdByType);
            await TcpServer.Start(GlobalSettings.TcpPort, tcpSocketMessageHelper, builder => { builder.UseConnectionHandler<AppTcpConnectionHandler>(); });
            Log.Info("tcp 服务启动完成...");
            await HttpServer.Start(GlobalSettings.HttpPort, GlobalSettings.HttpsPort, HotfixMgr.GetHttpHandler);
            Log.Info("load config data");

            GlobalTimer.Start();
            await ComponentRegister.ActiveGlobalComps();
            return true;
        }

        public async Task Stop()
        {
            // 断开所有连接
            await SessionManager.RemoveAll();
            // 取消所有未执行定时器
            await QuartzTimer.Stop();
            // 保证actor之前的任务都执行完毕
            await ActorManager.AllFinish();
            // 关闭网络服务
            await TcpServer.Stop();
            await WebSocketServer.Stop();
            await HttpServer.Stop();
            // 存储所有数据
            await GlobalTimer.Stop();
            await ActorManager.RemoveAll();
        }
    }
}