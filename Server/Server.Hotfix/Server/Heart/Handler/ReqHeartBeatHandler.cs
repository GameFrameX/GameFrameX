using Server.Core.Net.BaseHandler;
using Server.Hotfix.Server.Heart.Agent;

namespace Server.Hotfix.Server.Heart.Handler;

[MsgMapping(typeof(ReqHeartBeat))]
internal class ReqHeartBeatHandler : GlobalComponentHandler<HeartBeatComponentAgent>
{
    protected override async Task ActionAsync()
    {
        ReqHeartBeat req = this.Message as ReqHeartBeat;
        RespHeartBeat resp = new RespHeartBeat
        {
            Timestamp = TimeHelper.UnixTimeSeconds()
        };
        // Channel.HeartBeatStatus.Reset();
        this.Channel.Write(resp);
        Console.WriteLine(resp);
        await Task.CompletedTask;
        // await Comp.OnLogin(Channel, Msg as ReqHeartBeat);
    }
}