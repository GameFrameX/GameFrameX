using System.Buffers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Server.DBServer.State;
using Server.Extension;
using Server.Utility;
using SuperSocket.ProtoBase;

namespace Server.Launcher.Message;

class MessageActorDataBaseEncoderHandler : IPackageEncoder<ICacheState>
{
    public byte[] Handler(ICacheState message)
    {
        var bytes = Server.Serialize.Serialize.SerializerHelper.Serialize(message);
        var msgId = Hash.XXHash.Hash32(message.GetType().ToString());
        // len +timestamp + msgId + bytes.length
        int len = 8 + 4 + bytes.Length;
        var span = ArrayPool<byte>.Shared.Rent(len);
        int offset = 0;

        span.WriteLong(msgId, ref offset);
        span.WriteInt(bytes.Length, ref offset);
        span.WriteBytesWithoutLength(bytes, ref offset);
        return span;
    }

    public int Encode(IBufferWriter<byte> writer, ICacheState messageObject)
    {
        var bytes = Handler(messageObject);
        LogHelper.Debug($"---发送消息 ==>消息类型:{messageObject.GetType()} 消息内容:{messageObject}");
        writer.Write(bytes);
        ArrayPool<byte>.Shared.Return(bytes);
        return bytes.Length;
    }
}