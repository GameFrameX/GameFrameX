using Geek.Server.Config;
using Microsoft.AspNetCore.Connections;
using NLog;
using Server.App.Common.Net;
using Server.App.Common.Session;
using Server.Core.Actors;
using Server.Core.Comps;
using Server.Core.Hotfix;
using Server.Core.Net.Http;
using Server.Core.Net.Tcp;
using Server.Core.Timer;
using Server.Core.Utility;
using Server.Proto;
using Server.Utility;

namespace Server.Hotfix.Common
{
    internal class HotfixBridge : IHotfixBridge
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
                Log.Debug("MsgType:" + type);
                return ProtoMessageIdHandler.GetRespMessageIdByType(type);
            });
            HotfixMgr.SetMsgGetter((messageId) =>
            {
                Log.Debug("MsgID:" + messageId);
                return ProtoMessageIdHandler.GetReqTypeById(messageId);
            });
            //await TcpServer.Start(Settings.TcpPort);
            await TcpServer.Start(Settings.TcpPort, builder => builder.UseConnectionHandler<AppTcpConnectionHandler>());
            Log.Info("tcp 服务启动完成...");
            await HttpServer.Start(Settings.HttpPort);
            Log.Info("load config data");
            (bool success, string msg) = GameDataManager.ReloadAll();
            if (!success)
                throw new Exception($"载入配置表失败... {msg}");

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