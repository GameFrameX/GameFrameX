using Server.Launcher.Common.Session;
using Server.NetWork.HTTP;
using Server.NetWork.WebSocket;

namespace Server.Hotfix.Common
{
    internal class HotfixBridge : IHotfixBridge
    {
        public ServerType BridgeType => ServerType.Game;

        public async Task<bool> OnLoadSuccess(bool reload)
        {
            if (reload)
            {
                ActorManager.ClearAgent();
                return true;
            }

            LogHelper.Info("load config data");
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
            var appSetting = GlobalSettings.GetSetting<AppSetting>(ServerType.Game);
            var webSocketMessageHelper = new WebSocketMessageHelper(HotfixMgr.GetTcpHandler, HotfixMgr.GetMsgTypeById, HotfixMgr.GetMsgIdByType);
            await WebSocketServer.Start(appSetting.WsPort, appSetting.WssPort, appSetting.WssCertFilePath, appSetting, webSocketMessageHelper, new WebSocketChannelHandler());
            LogHelper.Info("WebSocket 服务启动完成...");

            // var tcpSocketMessageHelper = new TcpSocketMessageHelper(HotfixMgr.GetTcpHandler, HotfixMgr.GetMsgTypeById, HotfixMgr.GetMsgIdByType);
            // await TcpServer.Start(appSetting.TcpPort, appSetting, tcpSocketMessageHelper, builder => { builder.UseConnectionHandler<AppTcpConnectionHandler>(); });
            LogHelper.Info("tcp 服务启动完成...");
            await HttpServer.Start(appSetting.HttpPort, appSetting.HttpsPort, HotfixMgr.GetHttpHandler);
            LogHelper.Info("load config data");

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
            // await TcpServer.Stop();
            await WebSocketServer.Stop();
            await HttpServer.Stop();
            // 存储所有数据
            await GlobalTimer.Stop();
            await ActorManager.RemoveAll();
        }
    }
}