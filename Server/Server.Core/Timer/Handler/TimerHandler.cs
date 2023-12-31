﻿using Server.Core.Hotfix.Agent;
using Server.Utility;

namespace Server.Core.Timer.Handler
{
    public abstract class TimerHandler<TAgent> : ITimerHandler where TAgent : ICompAgent
    {
        public Task InnerHandleTimer(ICompAgent agent, Param param)
        {
            return HandleTimer((TAgent)agent, param);
        }

        protected abstract Task HandleTimer(TAgent agent, Param param);
    }
}
