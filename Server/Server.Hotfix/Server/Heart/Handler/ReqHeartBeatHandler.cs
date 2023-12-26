using Server.Core.Net.BaseHandler;
using Server.Hotfix.Server.Heart.Agent;
using Server.NetWork.Messages;

namespace Server.Hotfix.Server.Heart.Handler;

/// <summary>
/// 心跳消息处理器
/// </summary>
[MessageMapping(typeof(ReqHeartBeat))]
internal class ReqHeartBeatHandler : GlobalComponentHandler<HeartBeatComponentAgent>
{
    protected override async Task ActionAsync()
    {
        ReqHeartBeat req = this.Message as ReqHeartBeat;
        RespHeartBeat resp = new RespHeartBeat
        {
            Timestamp = TimeHelper.UnixTimeMilliseconds()
        };
        Channel.Write(resp);
        await Task.CompletedTask;
    }
}