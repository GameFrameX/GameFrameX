using System;
using System.Collections.Concurrent;
using System.Text;

namespace BestHTTP.Logger
{
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
    public sealed class ThreadedLogger : BestHTTP.Logger.ILogger, IDisposable
    {
        public Loglevels Level { get; set; }
        public ILogOutput Output { get { return this._output; }
            set
            {
                if (this._output != value)
                {
                    if (this._output != null)
                        this._output.Dispose();
                    this._output = value;
                }
            }
        }
        private ILogOutput _output;

        public int InitialStringBufferCapacity = 256;

#if !UNITY_WEBGL || UNITY_EDITOR
        public TimeSpan ExitThreadAfterInactivity = TimeSpan.FromMinutes(1);

        private ConcurrentQueue<LogJob> jobs = new ConcurrentQueue<LogJob>();
        private System.Threading.AutoResetEvent newJobEvent = new System.Threading.AutoResetEvent(false);

        private volatile int threadCreated;

        private volatile bool isDisposed;
#endif

        private StringBuilder sb = new StringBuilder(0);

        public ThreadedLogger()
        {
            this.Level = UnityEngine.Debug.isDebugBuild ? Loglevels.Warning : Loglevels.Error;
            this.Output = new UnityOutput();
        }

        public void Verbose(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null) {
            AddJob(Loglevels.All, division, msg, null, context1, context2, context3);
        }

        public void Information(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null) {
            AddJob(Loglevels.Information, division, msg, null, context1, context2, context3);
        }

        public void Warning(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null) {
            AddJob(Loglevels.Warning, division, msg, null, context1, context2, context3);
        }

        public void Error(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null) {
            AddJob(Loglevels.Error, division, msg, null, context1, context2, context3);
        }

        public void Exception(string division, string msg, Exception ex, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null) {
            AddJob(Loglevels.Exception, division, msg, ex, context1, context2, context3);
        }

        private void AddJob(Loglevels level, string div, string msg, Exception ex, LoggingContext context1, LoggingContext context2, LoggingContext context3)
        {
            if (this.Level > level)
                return;

            sb.EnsureCapacity(InitialStringBufferCapacity);

#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.isDisposed)
                return;
#endif

            var job = new LogJob
            {
                level = level,
                division = div,
                msg = msg,
                ex = ex,
                time = DateTime.Now,
                threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                stackTrace = System.Environment.StackTrace,
                context1 = context1 != null ? context1.Clone() : null,
                context2 = context2 != null ? context2.Clone() : null,
                context3 = context3 != null ? context3.Clone() : null
            };

#if !UNITY_WEBGL || UNITY_EDITOR
            // Start the consumer thread before enqueuing to get up and running sooner
            if (System.Threading.Interlocked.CompareExchange(ref this.threadCreated, 1, 0) == 0)
                BestHTTP.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ThreadFunc);

            this.jobs.Enqueue(job);
            try
            {
                this.newJobEvent.Set();
            }
            catch
            {
                try
                {
                    this.Output.Write(job.level, job.ToJson(sb));
                }
                catch
                { }
                return;
            }

            // newJobEvent might timed out between the previous threadCreated check and newJobEvent.Set() calls closing the thread.
            // So, here we check threadCreated again and create a new thread if needed.
            if (System.Threading.Interlocked.CompareExchange(ref this.threadCreated, 1, 0) == 0)
                BestHTTP.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ThreadFunc);
#else
            this.Output.Write(job.level, job.ToJson(sb));
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void ThreadFunc()
        {
            System.Threading.Thread.CurrentThread.Name = "BestHTTP.Logger";
            try
            {
                do
                {
                    // Waiting for a new log-job timed out
                    if (!this.newJobEvent.WaitOne(this.ExitThreadAfterInactivity))
                    {
                        // clear StringBuilder's inner cache and exit the thread
                        sb.Length = 0;
                        sb.Capacity = 0;
                        System.Threading.Interlocked.Exchange(ref this.threadCreated, 0);
                        return;
                    }

                    LogJob job;
                    while (this.jobs.TryDequeue(out job))
                    {
                        try
                        {
                            this.Output.Write(job.level, job.ToJson(sb));
                        }
                        catch
                        { }
                    }

                } while (!HTTPManager.IsQuitting);
                System.Threading.Interlocked.Exchange(ref this.threadCreated, 0);

                // When HTTPManager.IsQuitting is true, there is still logging that will create a new thread after the last one quit
                //  and always writing a new entry about the exiting thread would be too much overhead.
                // It would also hard to know what's the last log entry because some are generated on another thread non-deterministically.

                //var lastLog = new LogJob
                //{
                //    level = Loglevels.All,
                //    division = "ThreadedLogger",
                //    msg = "Log Processing Thread Quitting!",
                //    time = DateTime.Now,
                //    threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                //};
                //
                //this.Output.WriteVerbose(lastLog.ToJson(sb));
            }
            catch
            {
                System.Threading.Interlocked.Exchange(ref this.threadCreated, 0);
            }
        }

#endif

        public void Dispose()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            this.isDisposed = true;

            if (this.newJobEvent != null)
            {
                this.newJobEvent.Close();
                this.newJobEvent = null;
            }
#endif

            if (this.Output != null)
            {
                this.Output.Dispose();
                this.Output = new UnityOutput();
            }

            GC.SuppressFinalize(this);
        }
    }

    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    struct LogJob
    {
        private static string[] LevelStrings = new string[] { "Verbose", "Information", "Warning", "Error", "Exception" };
        public Loglevels level;
        public string division;
        public string msg;
        public Exception ex;

        public DateTime time;
        public int threadId;
        public string stackTrace;

        public LoggingContext context1;
        public LoggingContext context2;
        public LoggingContext context3;

        private static string WrapInColor(string str, string color)
        {
#if UNITY_EDITOR
            return string.Format("<b><color={1}>{0}</color></b>", str, color);
#else
            return str;
#endif
        }

        public string ToJson(StringBuilder sb)
        {
            sb.Length = 0;

            sb.AppendFormat("{{\"tid\":{0},\"div\":\"{1}\",\"msg\":\"{2}\"",
                WrapInColor(this.threadId.ToString(), "yellow"),
                WrapInColor(this.division, "yellow"),
                WrapInColor(LoggingContext.Escape(this.msg), "yellow"));

            if (ex != null)
            {
                sb.Append(",\"ex\": [");

                Exception exception = this.ex;

                while (exception != null)
                {
                    sb.Append("{\"msg\": \"");
                    sb.Append(LoggingContext.Escape(exception.Message));
                    sb.Append("\", \"stack\": \"");
                    sb.Append(LoggingContext.Escape(exception.StackTrace));
                    sb.Append("\"}");

                    exception = exception.InnerException;

                    if (exception != null)
                        sb.Append(",");
                }

                sb.Append("]");
            }

            if (this.stackTrace != null)
            {
                sb.Append(",\"stack\":\"");
                ProcessStackTrace(sb);
                sb.Append("\"");
            }
            else
                sb.Append(",\"stack\":\"\"");

            if (this.context1 != null || this.context2 != null || this.context3 != null)
            {
                sb.Append(",\"ctxs\":[");

                if (this.context1 != null)
                    this.context1.ToJson(sb);

                if (this.context2 != null)
                {
                    if (this.context1 != null)
                        sb.Append(",");

                    this.context2.ToJson(sb);
                }

                if (this.context3 != null)
                {
                    if (this.context1 != null || this.context2 != null)
                        sb.Append(",");

                    this.context3.ToJson(sb);
                }

                sb.Append("]");
            }
            else
                sb.Append(",\"ctxs\":[]");

            sb.AppendFormat(",\"t\":{0},\"ll\":\"{1}\",",
                this.time.Ticks.ToString(),
                LevelStrings[(int)this.level]);

            sb.Append("\"bh\":1}");

            return sb.ToString();
        }

        private void ProcessStackTrace(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(this.stackTrace))
                return;

            var lines = this.stackTrace.Split('\n');

            // skip top 4 lines that would show the logger.
            for (int i = 3; i < lines.Length; ++i)
                sb.Append(LoggingContext.Escape(lines[i].Replace("BestHTTP.", "")));
        }
    }
}
