using Quartz;
using Server.Utility;

namespace Server.Core.Timer.Handler
{
    /// <summary>
    /// 非热更程序集的计时器处理器，不需要热更时间更新
    /// </summary>
    public abstract class NotHotfixTimerHandler : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var param = context.JobDetail.JobDataMap.Get(QuartzTimer.PARAM_KEY) as Param;
            return HandleTimer(param);
        }

        protected abstract Task HandleTimer(Param param);
    }
}