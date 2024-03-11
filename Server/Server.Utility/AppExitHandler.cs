using System.Collections;
using Server.Log;


namespace Server.Utility
{
    public static class AppExitHandler
    {
        private static Action _existCallBack;

        public static void Init(Action existCallBack)
        {
            _existCallBack = existCallBack;
            //退出监听
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { _existCallBack?.Invoke(); };
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { HandleFetalException(e.ExceptionObject); };
            //ctrl+c
            Console.CancelKeyPress += (s, e) => { _existCallBack?.Invoke(); };
        }

        private static void HandleFetalException(object e)
        {
            //这里可以发送短信或者钉钉消息通知到运维
            LogHelper.Error("get unhandled exception");
            if (e is IEnumerable arr)
            {
                foreach (var ex in arr)
                {
                    LogHelper.Error($"Unhandled Exception:{ex}");
                }
            }
            else
            {
                LogHelper.Error($"Unhandled Exception:{e}");
            }

            _existCallBack?.Invoke();
        }
    }
}