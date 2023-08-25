using System.Diagnostics;
using System.Text;
using NLog;
using Server.App.Common;
using Server.Core.Utility;
using Server.Setting;
using Server.Utility;

namespace Server.App
{
    internal static class Program
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

        private static volatile bool ExitCalled = false;
        private static volatile Task GameLoopTask = null;
        private static volatile Task ShutDownTask = null;

        static async Task Main(string[] args)
        {
            try
            {
                AppExitHandler.Init(HandleExit);
                GameLoopTask = AppStartUp.Enter();
                await GameLoopTask;
                if (ShutDownTask != null)
                {
                    await ShutDownTask;
                }
            }
            catch (Exception e)
            {
                string error;
                if (GlobalSettings.IsAppRunning)
                {
                    error = $"服务器运行时异常 e:{e}";
                    Console.WriteLine(error);
                }
                else
                {
                    error = $"启动服务器失败 e:{e}";
                    Console.WriteLine(error);
                }

                await File.WriteAllTextAsync("server_error.txt", $"{error}", Encoding.UTF8);
            }
        }

        private static void HandleExit()
        {
            if (ExitCalled)
            {
                return;
            }

            ExitCalled = true;
            Log.Info($"监听到退出程序消息");
            ShutDownTask = Task.Run(() =>
            {
                GlobalSettings.IsAppRunning = false;
                GameLoopTask?.Wait();
                LogManager.Shutdown();
                Console.WriteLine($"退出程序");
                Process.GetCurrentProcess().Kill();
            });
            ShutDownTask.Wait();
        }
    }
}