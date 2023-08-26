using Server.App.Common;
using Server.Core.Net.Messages;
using Server.Core.Net.Tcp.Codecs;
using Server.Proto;
using Server.Proto.Proto;

namespace Server.Hotfix.Common.Handler;

public static class NetChannelExtensions
{
    public static void WriteAsync(this NetChannel channel, Message msg, int uniId, StateCode code = StateCode.Success, string desc = "")
    {
        if (msg != null)
        {
            msg.UniId = uniId;
            channel.WriteAsync(new NetMessage(msg));
        }

        if (uniId > 0)
        {
            RespErrorCode res = new RespErrorCode
            {
                // UniId = uniId,
                ErrCode = (int) code,
                Desc = desc
            };
            channel.WriteAsync(new NetMessage(res));
        }
    }
}