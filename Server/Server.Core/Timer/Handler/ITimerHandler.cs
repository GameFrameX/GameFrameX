using Server.Core.Hotfix.Agent;
using Server.Utility;

namespace Server.Core.Timer.Handler
{
    public interface ITimerHandler
    {
        Task InnerHandleTimer(IComponentAgent actor, Param param);
    }
}