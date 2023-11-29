using Server.Core.Net;
using Server.Launcher.Common;
using Server.Core.Net.Messages;
using Server.Core.Net.Tcp.Codecs;
using Server.Proto;
using Server.Proto.Proto;

namespace Server.Hotfix.Common.Handler;

public static class NetChannelExtensions
{
    /// <summary>
    /// 将消息对象异步写入网络通道。
    /// </summary>
    /// <param name="channel">网络通道。</param>
    /// <param name="msg">消息对象。</param>
    /// <param name="uniId">唯一ID。</param>
    /// <param name="code">状态码。</param>
    /// <param name="desc">描述。</param>
    public static void WriteAsync(this NetChannel channel, MessageObject msg, int uniId, StateCode code = StateCode.Success, string desc = "")
    {
        if (msg != null)
        {
            msg.UniId = uniId;
            channel.Write(msg);
        }

        if (uniId > 0)
        {
            RespErrorCode res = new RespErrorCode
            {
                // UniId = uniId,
                ErrCode = (int)code,
                Desc = desc
            };
            channel.Write(res);
        }
    }
}