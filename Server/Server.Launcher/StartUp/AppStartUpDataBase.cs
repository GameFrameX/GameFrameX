using Server.Apps.Server.Heart.Entity;
using Server.DBServer.State;
using Server.Launcher.PipelineFilter;
using Server.NetWork.TCPSocket;

namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏数据库
    /// </summary>
    [StartUpTag(ServerType.DataBase, 50)]
    internal sealed class DataBase : AppStartUpBase
    {
        private IServer server;
        private IGameDbService dbService;

        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"启动服务器{Setting.ServerType}开始");
                dbService = new MongoDbServiceConnection();
                dbService.Open(Setting.DataBaseUrl, Setting.DataBaseName);
                await StartServer();
                LogHelper.Info($"启动服务器{Setting.ServerType}结束");
                await AppExitToken;
            }
            catch (Exception e)
            {
                LogHelper.Info($"服务器执行异常，e:{e}");
                LogHelper.Fatal(e);
            }

            await Stop();
        }

        private async Task StartServer()
        {
            server = SuperSocketHostBuilder.Create<ICacheState>()
                .ConfigureSuperSocket(ConfigureSuperSocket)
                .UseSessionFactory<GameSessionFactory>()
                .UseClearIdleSession() // 清除空闲连接
                .UsePipelineFilter<CacheStatePipelineFilter>()
                .UsePackageDecoder<MessageActorDataBaseDecoderHandler>()
                // .UsePackageEncoder<MessageActorEncoderHandler>()
                // .UseSessionHandler(OnConnected, OnDisconnected)
                .UsePackageHandler(MessagePackageHandler)
                .UseInProcSessionContainer()
                .BuildAsServer();

            await server.StartAsync();
        }

        private async ValueTask MessagePackageHandler(IAppSession session, ICacheState cacheState)
        {
            if (cacheState is HeartBeatState _)
            {
                // 收到了心跳消息。
                return;
            }

            if (Setting.IsDebug && Setting.IsDebugReceive)
            {
                LogHelper.Debug($"---收到存储消息， 消息内容:{cacheState}");
            }

            if (cacheState is CacheState saveCacheState)
            {
                await dbService.AddAsync(saveCacheState);
            }
            // 发送
            // var response = new RespActorHeartBeat()
            // {
            //     Timestamp = TimeHelper.UnixTimeSeconds()
            // };
            // await session.SendAsync((IPackageEncoder<IMessage>)messageEncoderHandler, response);
        }

        /// <summary>
        /// 配置启动
        /// </summary>
        /// <param name="options"></param>
        private void ConfigureSuperSocket(ServerOptions options)
        {
            options.AddListener(new ListenOptions { Ip = IPAddress.Any.ToString(), Port = Setting.TcpPort });
        }

        public override async Task Stop(string message = "")
        {
            LogHelper.Info($"退出服务器开始");
            dbService.Close();
            await server.StopAsync();
            LogHelper.Info($"退出服务器成功");
            await base.Stop(message);
        }

        protected override void Init()
        {
            if (Setting == null)
            {
                Setting = new AppSetting
                {
                    ServerId = 6000,
                    TcpPort = 26000,
                    ServerType = ServerType.DataBase,
                    DataBaseName = "gameframex",
                    DataBaseUrl = "mongodb://127.0.0.1:27017"
                };
            }

            base.Init();
        }
    }
}