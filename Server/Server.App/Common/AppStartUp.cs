using Microsoft.Extensions.Http.Logging;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using PolymorphicMessagePack;
using Server.Config;
using Server.Core.Actors.Impl;
using Server.Core.Comps;
using Server.Core.Hotfix;
using Server.Core.Net.Messages;
using Server.Core.Utility;
using Server.DBServer;
using Server.DBServer.DbService.MongoDB;
using Server.Log;
using Server.Proto.Formatter;
using Server.Setting;
using Server.Utility;

namespace Server.App.Common
{
    internal static class AppStartUp
    {
        static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Enter()
        {
            try
            {
                GlobalSettings.Load<AppSetting>("Configs/app_config.json", ServerType.Game);
                RegisterMessagePack();
                var flag = LoggerHandler.Start();
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
                GameDb.Init(new MongoDbServiceConnection());
                GameDb.Open(GlobalSettings.DataBaseUrl, GlobalSettings.DataBaseName);
                Log.Info($"launch db service end...");

                Log.Info($"regist comps start...");
                await ComponentRegister.Init();
                Log.Info($"regist comps end...");

                Log.Info($"load hotfix module start");
                await HotfixMgr.LoadHotfixModule();
                Log.Info($"load hotfix module end");

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                GlobalSettings.LaunchTime = DateTime.Now;
                GlobalSettings.IsAppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(1);
                while (GlobalSettings.IsAppRunning)
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

        private static void RegisterMessagePack()
        {
            // PolymorphicTypeMapper.Register(typeof(AppStartUp).Assembly); //app
            PolymorphicTypeMapper.Register<Message>();
            PolymorphicRegister.Load();
            PolymorphicResolver.Instance.Init();
        }
    }
}