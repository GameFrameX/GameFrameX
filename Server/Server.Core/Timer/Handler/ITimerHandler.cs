using Server.Core.Hotfix.Agent;
using Server.Utility;

namespace Server.Core.Timer.Handler
{
    public interface ITimerHandler
    {
        Task InnerHandleTimer(ICompAgent actor, Param param);
    }
}