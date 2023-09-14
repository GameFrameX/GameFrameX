using Microsoft.AspNetCore.Connections;
using Server.Luncher.Common.Net;
using Server.Luncher.Common.Session;
using Server.Core.Net.Tcp;

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
                ActorMgr.ClearAgent();
                return true;
            }

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
            //await TcpServer.Start(Settings.TcpPort);
            await TcpServer.Start(GlobalSettings.TcpPort, builder => builder.UseConnectionHandler<AppTcpConnectionHandler>());
            Log.Info("tcp 服务启动完成...");
            await HttpServer.Start(GlobalSettings.HttpPort);
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
            await ActorMgr.AllFinish();
            // 关闭网络服务
            await HttpServer.Stop();
            await TcpServer.Stop();
            // 存储所有数据
            await GlobalTimer.Stop();
            await ActorMgr.RemoveAll();
        }
    }
}