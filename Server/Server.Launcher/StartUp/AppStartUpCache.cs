using Server.Apps.Player.Player.Component;
using Server.DBServer.State;
using Server.Launcher.PipelineFilter;
using Server.NetWork.TCPSocket;

namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏数据缓存
    /// </summary>
    // [StartUpTag(ServerType.Cache)]
    internal sealed class AppStartUpCache : AppStartUpBase
    {
        private IServer server;
        IEasyClient<ICacheState> client;
        MessageActorDataBaseEncoderHandler messageActorDataBaseEncoderHandler = new MessageActorDataBaseEncoderHandler();

        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"开始启动服务器{ServerType}");
                await StartServer();
                await StartClient();
                GlobalSettings.LaunchTime = DateTime.Now;
                GlobalSettings.IsAppRunning = true;
                LogHelper.Info($"启动服务器{ServerType}结束");
                await AppExitToken;
            }
            catch (Exception e)
            {
                LogHelper.Info($"服务器执行异常，e:{e}");
                LogHelper.Fatal(e);
            }

            LogHelper.Info($"退出服务器开始");
            LogHelper.Info($"退出服务器成功");
        }

        private async Task StartClient()
        {
            var messageObjectPipelineFilter = new CacheStatePipelineFilter
            {
                Decoder = new MessageActorDataBaseDecoderHandler()
            };

            client = new EasyClient<ICacheState>(messageObjectPipelineFilter).AsClient();
            client.Closed += ClientOnClosed;
            // client.PackageHandler += PackageHandler;
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
            client.StartReceive();
            // 5 秒发送一次心跳
            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += TimerOnElapsed;
            timer.Enabled = true;
            timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            //心跳包
            var message = new PlayerState();
            var bytes = messageActorDataBaseEncoderHandler.Handler(message);
            client.SendAsync(bytes);
        }

        private async Task StartServer()
        {
            server = SuperSocketHostBuilder.Create<ICacheState, CacheStatePipelineFilter>()
                .ConfigureSuperSocket(ConfigureSuperSocket)
                .UseSessionFactory<GameSessionFactory>()
                .UseClearIdleSession()
                .UsePackageHandler(CacheServerPackageHandler)
                .UseInProcSessionContainer()
                .BuildAsServer();
            await server.StartAsync();
        }

        private void ClientOnClosed(object sender, EventArgs e)
        {
        }

        private ValueTask PackageHandler(EasyClient<IMessage> sender, ICacheState package)
        {
            // if (package is MessageObject msg)
            {
                var messageId = package.Id;
                if (Setting.IsDebug && Setting.IsDebugReceive)
                {
                    LogHelper.Debug($"---收到消息 ==>消息内容:{package}");
                }
            }

            return ValueTask.CompletedTask;
        }

        private async ValueTask CacheServerPackageHandler(IAppSession session, ICacheState cacheState)
        {
            if (Setting.IsDebug && Setting.IsDebugReceive)
            {
                LogHelper.Debug($"---收到存储消息， 消息内容:{cacheState}");
            }

            // 这个要对数据进行缓存处理。定时存档和拉取
            if (cacheState is CacheState saveCacheState)
            {
                // await dbService.AddAsync(saveCacheState);
            }

            await ValueTask.CompletedTask;
        }

        /// <summary>
        /// 配置启动
        /// </summary>
        /// <param name="options"></param>
        private void ConfigureSuperSocket(ServerOptions options)
        {
            options.AddListener(new ListenOptions { Ip = IPAddress.Any.ToString(), Port = Setting.TcpPort });
        }

        public override void Init()
        {
            if (Setting == null)
            {
                Setting = new AppSetting
                {
                    ServerId = 5500,
                    LocalIp = "127.0.0.1",
                    TcpPort = 25500,
                    ServerType = ServerType.Cache,
                    // DB 配置
                    CenterUrl = "127.0.0.1",
                    GrpcPort = 26000
                };
            }

            base.Init();
        }
    }
}