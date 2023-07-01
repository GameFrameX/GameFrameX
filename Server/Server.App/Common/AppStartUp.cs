using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using Server.Config;
using Server.Core.Actors.Impl;
using Server.Core.Comps;
using Server.Core.Hotfix;
using Server.Core.Utility;
using Server.DBServer;
using Server.Utility;

namespace Server.App.Common
{
    internal static class AppStartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag)
                {
                    return; //启动服务器失败
                }

                Log.Info($"Load Config Start...");
                ConfigManager.Instance.LoadConfig();
                // var it = ConfigManager.Instance.Tables.TbItem.Get(1);
                // Console.WriteLine(it);
                Log.Info($"Load Config End...");

                Log.Info($"launch db service start...");
                ActorLimit.Init(ActorLimit.RuleType.None);
                GameDb.Init();
                GameDb.Open(Settings.MongoUrl, Settings.MongoDBName);
                Log.Info($"launch db service end...");

                Log.Info($"regist comps start...");
                await ComponentRegister.Init();
                Log.Info($"regist comps end...");

                Log.Info($"load hotfix module start");
                await HotfixMgr.LoadHotfixModule();
                Log.Info($"load hotfix module end");

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(1);
                while (Settings.AppRunning)
                {
                    await Task.Delay(delay);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"服务器执行异常，e:{e}");
                Log.Fatal(e);
            }

            Console.WriteLine($"退出服务器开始");
            await HotfixMgr.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Settings.Load<AppSetting>("Configs/app_config.json", ServerType.Game);
                Console.WriteLine("init NLog config...");
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/app_log.config");
                LogManager.AutoShutdown = false;
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"启动服务器失败,异常:{e}");
                return false;
            }
        }
    }
}