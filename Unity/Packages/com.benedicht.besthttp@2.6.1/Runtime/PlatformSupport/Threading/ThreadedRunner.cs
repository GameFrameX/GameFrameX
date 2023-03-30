using System;
using System.Threading;

#if NET_STANDARD_2_0
using System.Threading.Tasks;
#endif

namespace BestHTTP.PlatformSupport.Threading
{
    public static class ThreadedRunner
    {
        public static void RunShortLiving<T>(Action<T> job, T param)
        {
#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync(_ => job(param));
#pragma warning restore 4014
#elif NET_STANDARD_2_0
            var _task = new Task(() => job(param));
            _task.ConfigureAwait(false);
            _task.Start();
#else
            ThreadPool.QueueUserWorkItem(new WaitCallback(_ => job(param)));
#endif
        }

        public static void RunShortLiving<T1, T2>(Action<T1, T2> job, T1 param1, T2 param2)
        {
#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync((param) => job(param1, param2));
#pragma warning restore 4014
#elif NET_STANDARD_2_0
            var _task = new Task(() => job(param1, param2));
            _task.ConfigureAwait(false);
            _task.Start();
#else
            ThreadPool.QueueUserWorkItem(new WaitCallback(_ => job(param1, param2)));
#endif
        }

        public static void RunShortLiving<T1, T2, T3>(Action<T1, T2, T3> job, T1 param1, T2 param2, T3 param3)
        {            
#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync((param) => job(param1, param2, param3));
#pragma warning restore 4014
#elif NET_STANDARD_2_0
            var _task = new Task(() => job(param1, param2, param3));
            _task.ConfigureAwait(false);
            _task.Start();
#else
            ThreadPool.QueueUserWorkItem(new WaitCallback(_ => job(param1, param2, param3)));
#endif
        }

        public static void RunShortLiving<T1, T2, T3, T4>(Action<T1, T2, T3, T4> job, T1 param1, T2 param2, T3 param3, T4 param4)
        {
#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync((param) => job(param1, param2, param3, param4));
#pragma warning restore 4014
#elif NET_STANDARD_2_0
            var _task = new Task(() => job(param1, param2, param3, param4));
            _task.ConfigureAwait(false);
            _task.Start();
#else
            ThreadPool.QueueUserWorkItem(new WaitCallback(_ => job(param1, param2, param3, param4)));
#endif
        }

        public static void RunShortLiving(Action job)
        {
#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync((param) => job());
#pragma warning restore 4014
#elif NET_STANDARD_2_0
            var _task = new Task(() => job());
            _task.ConfigureAwait(false);
            _task.Start();
#else
            ThreadPool.QueueUserWorkItem(new WaitCallback((param) => job()));
#endif
        }

        public static void RunLongLiving(Action job)
        {
#if NETFX_CORE
#pragma warning disable 4014
            Windows.System.Threading.ThreadPool.RunAsync((param) => job());
#pragma warning restore 4014
#elif NET_STANDARD_2_0
            var _task = new Task(() => job(), TaskCreationOptions.LongRunning);
            _task.ConfigureAwait(false);
            _task.Start();
#else
            var thread = new Thread(new ParameterizedThreadStart((param) => job()));
            thread.IsBackground = true;
            thread.Start();
#endif
        }
    }
}
