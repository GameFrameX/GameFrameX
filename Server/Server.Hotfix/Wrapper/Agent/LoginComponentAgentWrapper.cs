using Server.Core.Actors.Impl;
using Server.Hotfix.Logic.Login;

namespace Server.Hotfix.Wrapper.Agent;

public class LoginComponentAgentWrapper : LoginComponentAgent
{
    // protected override Task DoSomething3()
    // {
    //     long callChainId = WorkerActor.NextChainId();
    //     base.Actor.WorkerActor.Enqueue(() => base.DoSomething3(), callChainId, true, 12000);
    //     return Task.CompletedTask;
    // }
}