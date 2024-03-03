using System.Buffers;
using System.Text;
using Server.Extension;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.Proto;
using Server.Serialize.Serialize;
using Server.Utility;

namespace Server.Launcher.Message;

class MessageEncoderHandler : IMessageEncoderHandler
{
    public byte[] Handler(IMessage message)
    {
        var bytes = SerializerHelper.Serialize(message);

        // len +timestamp + msgId + bytes.length
        int len = 4 + 4 + 8 + 4 + bytes.Length;
        var span = ArrayPool<byte>.Shared.Rent(len);
        int offset = 0;
        span.WriteInt(len, ref offset);
        span.WriteLong(TimeHelper.UnixTimeSeconds(), ref offset);
        var messageType = message.GetType();
        var msgId = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
        span.WriteInt(msgId, ref offset);
        // var uniqueIdBytes = Encoding.UTF8.GetBytes(message.UniqueId);
        // span.WriteInt(uniqueIdBytes.Length, ref offset);
        // span.WriteBytesWithoutLength(uniqueIdBytes, ref offset);
        span.WriteInt(bytes.Length, ref offset);
        span.WriteBytesWithoutLength(bytes, ref offset);
        return span;
    }
}