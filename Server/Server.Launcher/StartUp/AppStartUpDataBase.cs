namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏数据库
    /// </summary>
    [StartUpTag(ServerType.DataBase)]
    internal sealed class DataBase : AppStartUpBase
    {
        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"开始启动服务器{ServerType}");
                LogHelper.Info($"launch db service start...");
                GameDb.Init(new MongoDbServiceConnection());
                GameDb.Open(Setting.DataBaseUrl, Setting.DataBaseName);
                LogHelper.Info($"launch db service end...");
                LogHelper.Info("进入游戏主循环...");
                LogHelper.Info("***进入游戏主循环***");
                GlobalSettings.LaunchTime = DateTime.Now;
                GlobalSettings.IsAppRunning = true;
                await Setting.AppExitToken;
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