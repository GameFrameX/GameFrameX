
using Server.Core.Actors;
using Server.Core.Comps;
using Server.Core.Timer;
using Server.Core.Timer.Handler;
using Server.Utility;

namespace Server.Core.Hotfix.Agent
{
    public abstract class BaseComponentAgent<TComponent> : IComponentAgent where TComponent : BaseComponent
    {
        public BaseComponent Owner { get; set; }
        public TComponent Comp => (TComponent)Owner;
        public Actor Actor => Owner.Actor;
        public long ActorId => Actor.Id;
        public ActorType OwnerType => Actor.Type;

        public HashSet<long> ScheduleIdSet => Actor.ScheduleIdSet;

        public virtual void Active()
        {
        }

        protected void SetAutoRecycle(bool autoRecycle)
        {
            Actor.SetAutoRecycle(autoRecycle);
        }

        public virtual Task Inactive()
        {
            return Task.CompletedTask;
        }

        public Task ActorCrossDay(int serverDay)
        {
            return Actor.CrossDay(serverDay);
        }

        public Task<IComponentAgent> GetComponentAgent(Type agentType)
        {
            return Actor.GetComponentAgent(agentType);
        }

        public Task<T> GetComponentAgent<T>() where T : IComponentAgent
        {
            return Actor.GetComponentAgent<T>();
        }

        public void Tell(Action work, int timeout = Actor.TIME_OUT)
        {
            Actor.Tell(work, timeout);
        }

        public void Tell(Func<Task> work, int timeout = Actor.TIME_OUT)
        {
            Actor.Tell(work, timeout);
        }

        public Task SendAsync(Action work, int timeout = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeout);
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeout = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeout);
        }

        public Task SendAsync(Func<Task> work, int timeout = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeout);
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeOut);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="id">Id</param>
        public void Unscheduled(long id)
        {
            QuartzTimer.UnSchedule(id);
            ScheduleIdSet.Remove(id);
        }

        /// <summary>
        /// 延迟 定时器
        /// </summary>
        /// <param name="time">延迟的具体时间</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅的ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Delay<T>(DateTime time, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Delay<T>(ActorId, time - DateTime.Now, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 延迟 定时器
        /// </summary>
        /// <param name="time">延迟的时间</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅的ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Delay<T>(long time, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Delay<T>(ActorId, new DateTime(time) - DateTime.Now, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 延迟 定时器
        /// </summary>
        /// <param name="delay">延迟多久的时间差</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅的ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Delay<T>(TimeSpan delay, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Delay<T>(ActorId, delay, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 订阅定时器
        /// </summary>
        /// <param name="delay">延迟时间差</param>
        /// <param name="interval">间隔时间差</param>
        /// <param name="param">参数</param>
        /// <param name="repeatCount">调用次数，如果小于0， 则一直循环 </param>
        /// <param name="unScheduleId">取消订阅的ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Schedule<T>(TimeSpan delay, TimeSpan interval, Param param = null, int repeatCount = -1, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Schedule<T>(ActorId, delay, interval, param, repeatCount);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 基于 每天 的定时器
        /// </summary>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅的ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Daily<T>(int hour = 0, int minute = 0, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Daily<T>(ActorId, hour, minute, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 基于 每周某天 的定时器
        /// </summary>
        /// <param name="dayOfWeek">具体是哪天</param>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅的ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Weekly<T>(DayOfWeek dayOfWeek, int hour = 0, int minute = 0, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Weekly<T>(ActorId, dayOfWeek, hour, minute, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 基于 每周某天 的定时器
        /// </summary>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="param">参数</param>
        /// <param name="dayOfWeeks">某天的参数列表</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long WithDayOfWeeks<T>(int hour, int minute, Param param, params DayOfWeek[] dayOfWeeks) where T : ITimerHandler
        {
            long scheduleId = QuartzTimer.WithDayOfWeeks<T>(ActorId, hour, minute, param, dayOfWeeks);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 基于 每月某天 的定时器
        /// </summary>
        /// <param name="dayOfMonth">具体哪天</param>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Monthly<T>(int dayOfMonth, int hour = 0, int minute = 0, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.Monthly<T>(ActorId, dayOfMonth, hour, minute, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        /// <summary>
        /// 基于Cron 表达式的定时器
        /// </summary>
        /// <param name="cronExpression">cron 表达式</param>
        /// <param name="param">参数</param>
        /// <param name="unScheduleId">取消订阅ID</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long WithCronExpression<T>(string cronExpression, Param param = null, long unScheduleId = 0) where T : ITimerHandler
        {
            Unscheduled(unScheduleId);
            long scheduleId = QuartzTimer.WithCronExpression<T>(ActorId, cronExpression, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }
    }
}