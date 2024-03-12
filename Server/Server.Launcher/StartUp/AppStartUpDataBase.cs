namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏数据库
    /// </summary>
    [StartUpTag(ServerType.DataBase)]
    internal sealed class DataBase : AppStartUpBase
    {
        public override void Init()
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

        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"启动服务器{ServerType}开始");
                GameDb.Init(new MongoDbServiceConnection());
                GameDb.Open(Setting.DataBaseUrl, Setting.DataBaseName);
                LogHelper.Info($"启动服务器{ServerType}结束");
                GlobalSettings.LaunchTime = DateTime.Now;
                GlobalSettings.IsAppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(5);
                // while (!AppExitToken.IsCompleted)
                // {
                //     await Task.Delay(delay);
                // }

                await AppExitToken;
            }
            catch (Exception e)
            {
                LogHelper.Info($"服务器执行异常，e:{e}");
                LogHelper.Fatal(e);
            }
            finally
            {
                await GameDb.SaveAll();
            }

            LogHelper.Info($"退出服务器开始");
            await HotfixMgr.Stop();
            LogHelper.Info($"退出服务器成功");
        }
    }
}