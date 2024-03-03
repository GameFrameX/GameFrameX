using Server.Launcher.Common;
using Server.NetWork;
using Server.NetWork.Messages;

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
    public static void WriteAsync(this BaseNetChannel channel, MessageObject msg, int uniId, OperationStatusCode code = OperationStatusCode.Success, string desc = "")
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