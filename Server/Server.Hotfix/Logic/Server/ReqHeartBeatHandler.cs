using Server.Core.Net.Tcp.Handler;
using Server.Hotfix.Logic.Login;
using Server.Proto;

namespace Server.Hotfix.Logic.Server;

[MsgMapping(typeof(ReqHeartBeat))]
internal class ReqHeartBeatHandler : GlobalCompHandler<HeartBeatCompAgent>
{
    public override async Task ActionAsync()
    {
        Console.WriteLine(Msg);
        await Task.CompletedTask;
        // await Comp.OnLogin(Channel, Msg as ReqHeartBeat);
    }
}