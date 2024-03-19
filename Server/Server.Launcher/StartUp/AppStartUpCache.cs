using Server.Apps.Player.Player.Component;
using Server.Apps.Server.Heart.Entity;
using Server.Cache;
using Server.Cache.Memory;
using Server.DBServer.State;
using Server.Launcher.PipelineFilter;
using Server.NetWork.TCPSocket;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;
using Timer = System.Timers.Timer;

namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏数据缓存
    /// </summary>
    [StartUpTag(ServerType.Cache)]
    internal sealed class AppStartUpCache : AppStartUpBase
    {
        private IServer server;
        AsyncTcpSession databaseClient;
        AsyncTcpSession discoveryClient;
        MessageActorDataBaseEncoderHandler messageActorDataBaseEncoderHandler = new MessageActorDataBaseEncoderHandler();
        private ICache cacheService;

        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"开始启动服务器{ServerType}");
                cacheService = new MemoryCacheService();
                await StartServer();
                StartDiscoveryClient();
                StartDataBaseClient();
                Timer saveTimer = new Timer();
                saveTimer.Elapsed += SaveTimerOnElapsed;
                saveTimer.Interval = Setting.SaveDataInterval <= 0 ? 5000 : Setting.SaveDataInterval;
                saveTimer.AutoReset = true;
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

            await Stop();
        }

        private async void SaveTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var cacheState = await cacheService.GetFirstAsync();
            var bytes = messageActorDataBaseEncoderHandler.Handler(cacheState);
            bool isSuccess = databaseClient.TrySend(bytes);
            if (isSuccess)
            {
                cacheService.Remove(cacheState);
            }
        }

        #region DataBaseClient

        private void StartDataBaseClient()
        {
            databaseClient = new AsyncTcpSession();
            databaseClient.Connected += DataBaseDatabaseClientOnConnected;
            databaseClient.Closed += DataBaseDatabaseClientOnClosed;
            databaseClient.Error += DataBaseDatabaseClientOnError;
            databaseClient.DataReceived += DataBaseDatabaseClientOnDataReceived;
            LogHelper.Info("开始链接到DB服务器 ...");
            ConnectToDataBase();
        }

        private void ConnectToDataBase()
        {
            databaseClient.Connect(new DnsEndPoint(Setting.DBUrl, Setting.DbPort));
        }

        private void DataBaseDatabaseClientOnError(object sender, ErrorEventArgs e)
        {
            LogHelper.Error("和DB服务器链接链接失败!.下一次重连时间:" + DateTime.Now.AddMilliseconds(ReconnectionTimer.Interval));
            // 和网关服务器链接失败，开启重连
            ReconnectionTimer.Start();
            ConnectToDataBase();
        }

        private void DataBaseDatabaseClientOnDataReceived(object sender, DataEventArgs e)
        {
            LogHelper.Info("收到DB服务器返回的消息!" + e);
        }

        private void DataBaseDatabaseClientOnClosed(object sender, EventArgs e)
        {
            LogHelper.Info("和DB服务器链接链接断开!");
            // 和网关服务器链接断开，开启重连
            ReconnectionTimer.Start();
            ConnectToDataBase();
        }

        private void DataBaseDatabaseClientOnConnected(object sender, EventArgs e)
        {
            LogHelper.Info("和DB服务器链接链接成功!");
            // 和网关服务器链接成功，关闭重连
            ReconnectionTimer.Stop();
            // 开启和网关服务器的心跳
            HeartBeatTimer.Start();
        }

        protected override void HeartBeatTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            //心跳包
            var heartBeatState = new HeartBeatState();
            var bytes = messageActorDataBaseEncoderHandler.Handler(heartBeatState);
            databaseClient.TrySend(bytes);
        }

        #endregion

        #region Discovery

        private void StartDiscoveryClient()
        {
            discoveryClient = new AsyncTcpSession();
            discoveryClient.Connected += DiscoveryClientOnConnected;
            discoveryClient.Closed += DiscoveryClientOnClosed;
            discoveryClient.Error += DiscoveryClientOnError;
            discoveryClient.DataReceived += DiscoveryClientOnDataReceived;
            LogHelper.Info("开始链接到中心服务器 ...");
            ConnectToDiscovery();
        }

        private void ConnectToDiscovery()
        {
            discoveryClient.Connect(new DnsEndPoint(Setting.CenterUrl, Setting.GrpcPort));
        }

        private void DiscoveryClientOnError(object sender, ErrorEventArgs e)
        {
            LogHelper.Error("和中心服务器链接链接失败!.下一次重连时间:" + DateTime.Now.AddMilliseconds(ReconnectionTimer.Interval));
            // 和网关服务器链接失败，开启重连
            ReconnectionTimer.Start();
            ConnectToDiscovery();
        }

        private void DiscoveryClientOnDataReceived(object sender, DataEventArgs e)
        {
            LogHelper.Info("收到中心服务器返回的消息!" + e);
        }

        private void DiscoveryClientOnClosed(object sender, EventArgs e)
        {
            LogHelper.Info("和中心服务器链接链接断开!");
            // 和网关服务器链接断开，开启重连
            ReconnectionTimer.Start();
            ConnectToDiscovery();
        }

        private void DiscoveryClientOnConnected(object sender, EventArgs e)
        {
            LogHelper.Info("和中心服务器链接链接成功!");
            // 和网关服务器链接成功，关闭重连
            ReconnectionTimer.Stop();
            // 开启和网关服务器的心跳
            HeartBeatTimer.Start();
        }

        #endregion


        #region Server

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


        private async ValueTask CacheServerPackageHandler(IAppSession session, ICacheState cacheState)
        {
            // 这个要对数据进行缓存处理。定时存档和拉取
            if (cacheState is HeartBeatState heartBeatState)
            {
                // 收到了心跳消息。
                return;
            }

            if (cacheState is CacheState saveCacheState)
            {
                // var messageBuffer = messageActorDataBaseEncoderHandler.Handler(saveCacheState);
                // bool isSuccess = databaseClient.TrySend(messageBuffer);
                // 存档到缓存中
                await cacheService.SetAsync(saveCacheState.Id, saveCacheState);
            }

            if (Setting.IsDebug && Setting.IsDebugReceive)
            {
                LogHelper.Debug($"---收到缓存消息， 消息内容:{cacheState}");
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

        #endregion

        public override async Task Stop(string message = "")
        {
            this.discoveryClient.Close();
            ReconnectionTimer.Close();
            HeartBeatTimer.Close();
            databaseClient.Close();
            await server.StopAsync();
            await base.Stop(message);
        }

        protected override void Init()
        {
            if (Setting == null)
            {
                Setting = new AppSetting
                {
                    ServerId = 5500,
                    LocalIp = "127.0.0.1",
                    TcpPort = 25500,
                    ServerType = ServerType.Cache,
                    SaveDataInterval = 5000,
                    // 中心服 配置
                    CenterUrl = "127.0.0.1",
                    GrpcPort = 33300,
                    // DB 配置
                    DBUrl = "127.0.0.1",
                    DbPort = 26000
                };
                if (PlatformRuntimeHelper.IsLinux)
                {
                    Setting.CenterUrl = "discovery";
                    Setting.DBUrl = "database";
                }
            }

            base.Init();
        }
    }
}