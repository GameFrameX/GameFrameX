using System.Buffers;
using Server.Extension;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.Proto;
using Server.Serialize.Serialize;
using SuperSocket.ProtoBase;

namespace Server.Launcher.Message;

class MessageActorDiscoveryDecoderHandler :  IPackageDecoder<IMessage>
{
    public IMessage Handler(Span<byte> data)
    {
        int readOffset = 0;
        var length = data.ReadInt(ref readOffset);
        var timestamp = data.ReadLong(ref readOffset);
        var messageId = data.ReadInt(ref readOffset);
        // var messageUniqueData = data.ReadBytes(ref readOffset);
        var messageData = data.ReadBytes(ref readOffset);
        var messageType = ProtoMessageIdHandler.GetRequestActorTypeById(messageId);
        if (messageType != null)
        {
            var messageObject = (MessageObject)SerializerHelper.Deserialize(messageData, messageType);
            messageObject.MessageId = messageId;
            return messageObject;
        }
        else
        {
            LogHelper.Fatal("未知消息类型");
            return null;
        }
    }

    public IMessage Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var data = buffer.ToArray();
        var messageObject = Handler(data);
        return messageObject;
    }
}