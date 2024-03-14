/*using System.Buffers;
using Server.Extension;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.Proto;
using Server.Serialize.Serialize;
using Server.Utility;
using SuperSocket.ProtoBase;

namespace Server.Launcher.Message;

class MessageActorEncoderHandler : IMessageEncoderHandler, IPackageEncoder<IMessage>
{
    public byte[] Handler(IMessage message)
    {
        var bytes = SerializerHelper.Serialize(message);

        // len +timestamp + msgId + bytes.length
        int len = 4 + 8 + 4 + 4 + 4 + bytes.Length;
        var span = ArrayPool<byte>.Shared.Rent(len);
        int offset = 0;
        span.WriteInt(len, ref offset);
        span.WriteLong(TimeHelper.UnixTimeSeconds(), ref offset);
        var messageType = message.GetType();
        var msgId = ProtoMessageIdHandler.GetResponseActorMessageIdByType(messageType);
        span.WriteInt(msgId, ref offset);
        span.WriteInt(bytes.Length, ref offset);
        span.WriteBytesWithoutLength(bytes, ref offset);
        return span;
    }

    public int Encode(IBufferWriter<byte> writer, IMessage messageObject)
    {
        var bytes = Handler(messageObject);
        LogHelper.Debug($"---发送消息 ==>消息类型:{messageObject.GetType()} 消息内容:{messageObject}");
        writer.Write(bytes);
        ArrayPool<byte>.Shared.Return(bytes);
        return bytes.Length;
    }
}*/