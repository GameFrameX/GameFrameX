using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Server.Core.Actors;
using Server.Core.Hotfix;
using Server.Core.Timer.Handler;
using Server.Core.Utility;
using Server.Log;
using Server.Utility;

namespace Server.Core.Timer
{
    public static class QuartzTimer
    {
        private static readonly StatisticsTool StatisticsTool = new();

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="id"></param>
        public static void UnSchedule(long id)
        {
            if (id <= 0)
                return;
            _scheduler.DeleteJob(JobKey.Create(id + ""));
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="set"></param>
        public static void UnSchedule(IEnumerable<long> set)
        {
            foreach (var id in set)
            {
                if (id > 0)
                    _scheduler.DeleteJob(JobKey.Create(id + ""));
            }
        }

        #region 热更定时器

        /// <summary>
        /// 每隔一段时间执行一次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actorId"></param>
        /// <param name="delay">延迟</param>
        /// <param name="interval">间隔</param>
        /// <param name="param"></param>
        /// <param name="repeatCount"> 循环次数, -1 表示永远 </param>
        /// <returns></returns>
        public static long Schedule<T>(long actorId, TimeSpan delay, TimeSpan interval, Param param = null, int repeatCount = -1) where T : ITimerHandler
        {
            var nextId = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            TriggerBuilder builder;
            if (repeatCount < 0)
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever());
            }
            else
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).WithRepeatCount(repeatCount));
            }

            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), builder.Build());
            return nextId;
        }

        /// <summary>
        /// 基于时间delay
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actorId"></param>
        /// <param name="delay">延迟</param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Delay<T>(long actorId, TimeSpan delay, Param param = null) where T : ITimerHandler
        {
            var nextId = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            var trigger = TriggerBuilder.Create().StartAt(firstTimeOffset).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 基于cron表达式
        /// </summary>
        public static long WithCronExpression<T>(long actorId, string cronExpression, Param param = null) where T : ITimerHandler
        {
            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(cronExpression).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每日
        /// </summary>
        public static long Daily<T>(long actorId, int hour, int minute, Param param = null) where T : ITimerHandler
        {
            if (hour < 0 || hour >= 24 || minute < 0 || minute >= 60)
            {
                throw new ArgumentOutOfRangeException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(hour)}:{hour} {nameof(minute)}:{minute}");
            }

            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每周某些天
        /// </summary>
        public static long WithDayOfWeeks<T>(long actorId, int hour, int minute, Param param, params DayOfWeek[] dayOfWeeks) where T : ITimerHandler
        {
            if (dayOfWeeks == null || dayOfWeeks.Length <= 0)
            {
                throw new ArgumentNullException($"定时每周执行 参数为空：{nameof(dayOfWeeks)} TimerHandler:{typeof(T).FullName} actorId:{actorId} actorType:{IdGenerator.GetActorType(actorId)}");
            }

            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(hour, minute, dayOfWeeks)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每周某天
        /// </summary>
        public static long Weekly<T>(long actorId, DayOfWeek dayOfWeek, int hour, int minute, Param param = null) where T : ITimerHandler
        {
            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每月某天
        /// </summary>
        public static long Monthly<T>(long actorId, int dayOfMonth, int hour, int minute, Param param = null) where T : ITimerHandler
        {
            if (dayOfMonth is < 0 or > 31)
            {
                throw new ArgumentException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(dayOfMonth)}:{dayOfMonth} actorId:{actorId} actorType:{IdGenerator.GetActorType(actorId)}");
            }

            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(dayOfMonth, hour, minute)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, actorId, param), trigger);
            return nextId;
        }

        #endregion

        #region 非热更定时器

        /// <summary>
        /// 每隔一段时间执行一次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delay"></param>
        /// <param name="interval"></param>
        /// <param name="param"></param>
        /// <param name="repeatCount"> -1 表示永远 </param>
        /// <returns></returns>
        public static long Schedule<T>(TimeSpan delay, TimeSpan interval, Param param = null, int repeatCount = -1) where T : NotHotfixTimerHandler
        {
            var nextId = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            TriggerBuilder builder;
            if (repeatCount < 0)
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever());
            }
            else
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).WithRepeatCount(repeatCount));
            }

            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), builder.Build());
            return nextId;
        }

        /// <summary>
        /// 基于时间delay
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delay"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Delay<T>(TimeSpan delay, Param param = null) where T : NotHotfixTimerHandler
        {
            var nextId = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            var trigger = TriggerBuilder.Create().StartAt(firstTimeOffset).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 基于cron表达式
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static long WithCronExpression<T>(string cronExpression, Param param = null) where T : NotHotfixTimerHandler
        {
            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(cronExpression).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每日
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static long Daily<T>(int hour, int minute, Param param = null) where T : NotHotfixTimerHandler
        {
            if (hour < 0 || hour >= 24 || minute < 0 || minute >= 60)
            {
                throw new ArgumentOutOfRangeException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(hour)}:{hour} {nameof(minute)}:{minute}");
            }

            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每周某些天
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="param"></param>
        /// <param name="dayOfWeeks"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static long WithDayOfWeeks<T>(int hour, int minute, Param param, params DayOfWeek[] dayOfWeeks) where T : NotHotfixTimerHandler
        {
            if (dayOfWeeks == null || dayOfWeeks.Length <= 0)
            {
                throw new ArgumentNullException($"定时每周执行 参数为空：{nameof(dayOfWeeks)} TimerHandler:{typeof(T).FullName}");
            }

            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(hour, minute, dayOfWeeks)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每周某天
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static long Weekly<T>(DayOfWeek dayOfWeek, int hour, int minute, Param param = null) where T : NotHotfixTimerHandler
        {
            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), trigger);
            return nextId;
        }

        /// <summary>
        /// 每月某天
        /// </summary>
        /// <param name="dayOfMonth"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static long Monthly<T>(int dayOfMonth, int hour, int minute, Param param = null) where T : NotHotfixTimerHandler
        {
            if (dayOfMonth is < 0 or > 31)
            {
                throw new ArgumentException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(dayOfMonth)}:{dayOfMonth}");
            }

            var nextId = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(dayOfMonth, hour, minute)).Build();
            _scheduler.ScheduleJob(GetJobDetail<T>(nextId, param), trigger);
            return nextId;
        }

        #endregion

        #region 调度

        private static IScheduler _scheduler = null;

        /// <summary>
        /// 可防止反复初始化
        /// </summary>
        static QuartzTimer()
        {
            Init().Wait();
            Start().Wait();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        static async Task Init()
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            var factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        public static async Task Start()
        {
            await _scheduler.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        public static Task Stop()
        {
            return _scheduler.Shutdown();
        }

        private static long _id = DateTime.Now.Ticks;

        private static long NextId()
        {
            return Interlocked.Increment(ref _id);
        }

        private sealed class TimerJobHelper : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                var handlerType = context.JobDetail.JobDataMap.GetString(TIMER_KEY);
                try
                {
                    var param = context.JobDetail.JobDataMap.Get(PARAM_KEY) as Param;
                    var handler = HotfixMgr.GetInstance<ITimerHandler>(handlerType);
                    if (handler != null)
                    {
                        var actorId = context.JobDetail.JobDataMap.GetLong(ACTOR_ID_KEY);
                        var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                        var comp = await ActorManager.GetComponentAgent(actorId, agentType);
                        comp.Tell(() => handler.InnerHandleTimer(comp, param));
                    }
                    else
                    {
                        LogHelper.Error($"错误的ITimer类型，回调失败 type:{handlerType}");
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e.ToString());
                }
            }
        }

        public const string PARAM_KEY = "param";
        const string ACTOR_ID_KEY = "actor_id";
        const string TIMER_KEY = "timer";

        private static IJobDetail GetJobDetail<T>(long id, long actorId, Param param) where T : ITimerHandler
        {
            var handlerType = typeof(T);
            StatisticsTool.Count(handlerType.FullName);
            if (handlerType.Assembly != HotfixMgr.HotfixAssembly)
            {
                throw new Exception("定时器代码需要在热更项目里");
            }

            var job = JobBuilder.Create<TimerJobHelper>().WithIdentity(id + string.Empty).Build();
            job.JobDataMap.Add(PARAM_KEY, param);
            job.JobDataMap.Add(ACTOR_ID_KEY, actorId);
            job.JobDataMap.Add(TIMER_KEY, handlerType.FullName);
            return job;
        }

        private static IJobDetail GetJobDetail<T>(long id, Param param) where T : NotHotfixTimerHandler
        {
            StatisticsTool.Count(typeof(T).FullName);
            var job = JobBuilder.Create<T>().WithIdentity(id + string.Empty).Build();
            job.JobDataMap.Add(PARAM_KEY, param);
            return job;
        }

        private class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (func != null)
                    {
                        if (level < LogLevel.Warn)
                        {
                        }
                        else if (level == LogLevel.Warn)
                        {
                            LogHelper.Warn(func(), parameters);
                        }
                        else
                        {
                            LogHelper.Error(func(), parameters);
                        }
                    }

                    return true;
                };
            }

            public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}