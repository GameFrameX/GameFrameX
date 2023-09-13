using Server.Hotfix.Logic.Heart.Agent;

namespace Server.Hotfix.Logic.Heart.Handler;

[MsgMapping(typeof(ReqHeartBeat))]
internal class ReqHeartBeatHandler : GlobalComponentHandler<HeartBeatComponentAgent>
{
    public override async Task ActionAsync()
    {
        ReqHeartBeat req = this.Msg as ReqHeartBeat;
        RespHeartBeat resp = new RespHeartBeat
        {
            Timestamp = TimeHelper.UnixTimeSeconds()
        };
        Channel.HeartBeatStatus.Reset();
        this.Channel.WriteAsync(resp);
        Console.WriteLine(resp);
        await Task.CompletedTask;
        // await Comp.OnLogin(Channel, Msg as ReqHeartBeat);
    }
}