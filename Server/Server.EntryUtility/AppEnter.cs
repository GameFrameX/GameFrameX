using System.Diagnostics;
using System.Text;
using Server.Log;
using Server.Setting;
using Server.Utility;

namespace Server.EntryUtility
{
    /// <summary>
    /// App入口
    /// </summary>
    public static class AppEnter
    {

        private static volatile bool _exitCalled = false;
        private static volatile Task? _gameLoopTask = null;
        private static volatile Task? _shutDownTask = null;

        public static async Task Entry(Func<Task> entry )
        {
            try
            {
                AppExitHandler.Init(HandleExit);
                _gameLoopTask = entry();
                await _gameLoopTask;
                if (_shutDownTask != null)
                {
                    await _shutDownTask;
                }
            }
            catch (Exception e)
            {
                string error;
                if (GlobalSettings.IsAppRunning)
                {
                    error = $"服务器运行时异常 e:{e}";
                    LogHelper.Info(error);
                }
                else
                {
                    error = $"启动服务器失败 e:{e}";
                    LogHelper.Info(error);
                }

                await File.WriteAllTextAsync("server_error.txt", $"{error}", Encoding.UTF8);
            }
        }

        private static void HandleExit()
        {
            if (_exitCalled)
            {
                return;
            }

            _exitCalled = true;
            LogHelper.Info($"监听到退出程序消息");
            _shutDownTask = Task.Run(() =>
            {
                GlobalSettings.IsAppRunning = false;
                _gameLoopTask?.Wait();
                LogHelper.Info($"退出程序");
                Process.GetCurrentProcess().Kill();
            });
            _shutDownTask.Wait();
        }
    }
}