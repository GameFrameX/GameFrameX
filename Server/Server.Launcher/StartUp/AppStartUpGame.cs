namespace Server.Launcher.StartUp
{
    /// <summary>
    /// 游戏服务器
    /// </summary>
    // [StartUpTag(ServerType.All)]
    internal sealed class AppStartUpGame : AppStartUpBase
    {
        public override async Task EnterAsync()
        {
            try
            {
                LogHelper.Info($"开始启动服务器{ServerType}");
                var hotfixPath = Directory.GetCurrentDirectory() + "/hotfix";
                if (!Directory.Exists(hotfixPath))
                {
                    Directory.CreateDirectory(hotfixPath);
                }


                var flag = LoggerHandler.Start();
                if (!flag)
                {
                    return; //启动服务器失败
                }

                LogHelper.Info($"Load Config Start...");
                ConfigManager.Instance.LoadConfig();

                LogHelper.Info($"Load Config End...");

                LogHelper.Info($"launch db service start...");
                ActorLimit.Init(ActorLimit.RuleType.None);
                LogHelper.Info($"launch db service end...");

                LogHelper.Info($"regist comps start...");
                await ComponentRegister.Init(typeof(AppsHandler).Assembly);
                LogHelper.Info($"regist comps end...");

                LogHelper.Info($"load hotfix module start");
                await HotfixMgr.LoadHotfixModule();
                LogHelper.Info($"load hotfix module end");

                LogHelper.Info("进入游戏主循环...");
                LogHelper.Info("***进入游戏主循环***");
                GlobalSettings.LaunchTime = DateTime.Now;
                GlobalSettings.IsAppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(1);
                await AppExitToken;
            }
            catch (Exception e)
            {
                LogHelper.Info($"服务器执行异常，e:{e}");
                LogHelper.Fatal(e);
            }

            LogHelper.Info($"退出服务器开始");
            await HotfixMgr.Stop();
            LogHelper.Info($"退出服务器成功");
        }
    }
}