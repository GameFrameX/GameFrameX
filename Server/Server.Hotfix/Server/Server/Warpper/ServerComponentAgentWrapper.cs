using Server.Core.Actors.Impl;
using Server.Hotfix.Server.Server.Agent;

namespace Server.Hotfix.Server.Server.Warpper;

public class ServerComponentAgentWrapper : ServerComponentAgent
{
    // Token: 0x06000007 RID: 7 RVA: 0x00002090 File Offset: 0x00000290
    public override ValueTask AddOnlineRole(long actorId)
    {
        long callChainId = WorkerActor.NextChainId();
        // base.Actor.WorkerActor.Enqueue(() => base. (actorId), callChainId, true, int.MaxValue);
        return ValueTask.CompletedTask;
    }

    // Token: 0x06000008 RID: 8 RVA: 0x000020E8 File Offset: 0x000002E8
    public override ValueTask RemoveOnlineRole(long actorId)
    {
        long callChainId = WorkerActor.NextChainId();
        // base.Actor.WorkerActor.Enqueue<ValueTask>(() => this. <>n__1(actorId), callChainId, true, int.MaxValue);
        return ValueTask.CompletedTask;
    }

    // Token: 0x06000009 RID: 9 RVA: 0x00002140 File Offset: 0x00000340
    public override Task<bool> IsOnline(long roleId)
    {
        ValueTuple<bool, long> valueTuple = base.Actor.WorkerActor.IsNeedEnqueue();
        bool needEnqueue = valueTuple.Item1;
        long chainId = valueTuple.Item2;
        bool flag = !needEnqueue;
        Task<bool> result = Task.FromResult(false);
        if (flag)
        {
            result = base.IsOnline(roleId);
        }
        else
        {
            // result = base.Actor.WorkerActor.Enqueue<bool>(() => this. <>n__2(roleId), chainId, false, int.MaxValue);
        }

        return result;
    }

    // Token: 0x0600000A RID: 10 RVA: 0x000021C4 File Offset: 0x000003C4
    protected override Task DoSomething1()
    {
        base.DoSomething1();
        return Task.CompletedTask;
    }

    // Token: 0x0600000B RID: 11 RVA: 0x000021E4 File Offset: 0x000003E4
    protected override Task DoSomething3()
    {
        long callChainId = WorkerActor.NextChainId();
        base.Actor.WorkerActor.Enqueue(() => base.DoSomething3(), callChainId, true, 12000);
        return Task.CompletedTask;
    }
}