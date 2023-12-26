using Server.Apps.Server.Heart.Component;
using Server.Apps.Server.Heart.Entity;
using Server.Core.Timer.Handler;

namespace Server.Hotfix.Server.Heart.Agent;

public class HeartBeatComponentAgent : StateComponentAgent<HeartBeatComponent, HeartBeatState>
{
    /// <summary>
    /// 心跳 定时器
    /// </summary>
    // class HeartBeatScheduleTimer : TimerHandler<HeartBeatComponentAgent>
    // {
    //     protected override Task HandleTimer(HeartBeatComponentAgent agent, Param param)
    //     {
    //         agent.Log.Debug($"心跳 时间:{TimeHelper.CurrentTimeWithFullString()}");
    //         return Task.CompletedTask;
    //     }
    // }

    // private long _heartBeatScheduleTimerId;
    public override void Active()
    {
        // _heartBeatScheduleTimerId = Schedule<HeartBeatScheduleTimer>(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public override Task Inactive()
    {
        // Unscheduled(_heartBeatScheduleTimerId);
        return Task.CompletedTask;
    }
}