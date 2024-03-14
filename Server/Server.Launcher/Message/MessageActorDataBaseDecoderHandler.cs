using System.Buffers;
using Server.DBServer.State;
using Server.Extension;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.Proto;
using Server.Serialize.Serialize;
using SuperSocket.ProtoBase;

namespace Server.Launcher.Message;

internal class MessageActorDataBaseDecoderHandler : IPackageDecoder<ICacheState>
{
    public ICacheState Handler(Span<byte> data)
    {
        int readOffset = 0;
        var messageTypeId = data.ReadLong(ref readOffset);
        var messageData = data.ReadBytes(ref readOffset);
        var messageTypeType = CacheStateTypeManager.GetType(messageTypeId);
        var messageObject = (ICacheState)SerializerHelper.Deserialize(messageData, messageTypeType);
        return messageObject;
    }

    public ICacheState Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var data = buffer.ToArray();
        var messageObject = Handler(data);
        return messageObject;
    }
}