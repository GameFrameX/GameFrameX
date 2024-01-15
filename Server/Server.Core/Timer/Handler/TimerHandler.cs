using Server.Core.Hotfix.Agent;
using Server.Utility;

namespace Server.Core.Timer.Handler
{
    /// <summary>
    /// 热更新程序集的计时器处理器
    /// </summary>
    /// <typeparam name="TAgent">组件类型</typeparam>
    public abstract class TimerHandler<TAgent> : ITimerHandler where TAgent : IComponentAgent
    {
        /// <summary>
        /// 内部计时器处理器调用。由Quartz调用
        /// </summary>
        /// <param name="agent">组件代理对象</param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task InnerHandleTimer(IComponentAgent agent, Param param)
        {
            return HandleTimer((TAgent)agent, param);
        }

        /// <summary>
        /// 计时器调用
        /// </summary>
        /// <param name="agent">调用对象</param>
        /// <param name="param">参数对象</param>
        /// <returns></returns>
        protected abstract Task HandleTimer(TAgent agent, Param param);
    }
}