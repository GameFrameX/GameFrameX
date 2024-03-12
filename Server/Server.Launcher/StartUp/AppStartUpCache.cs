using Server.Utility;

namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏数据缓存
    /// </summary>
    [StartUpTag(ServerType.Cache)]
    internal sealed class AppStartUpCache : AppStartUpBase
    {
        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"开始启动服务器{ServerType}");
                LogHelper.Info($"launch db service start...");

                LogHelper.Info("进入游戏主循环...");
                LogHelper.Info("***进入游戏主循环***");
                GlobalSettings.LaunchTime = DateTime.Now;
                GlobalSettings.IsAppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(5);
                while (!AppExitToken.IsCompleted)
                {
                    await Task.Delay(delay);
                }
            }
            catch (Exception e)
            {
                LogHelper.Info($"服务器执行异常，e:{e}");
                LogHelper.Fatal(e);
            }

            LogHelper.Info($"退出服务器开始");
            LogHelper.Info($"退出服务器成功");
        }
    }
}