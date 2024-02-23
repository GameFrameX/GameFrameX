using System.Buffers;
using GameFrameX.Runtime;
using ProtoBuf;

namespace GameFrameX.Network
{
    public sealed class DefaultPacketReceiveBodyHandler : IPacketReceiveBodyHandler, IPacketHandler
    {
        public bool Handler<T>(object source, int messageId, out T messageObject) where T : MessageObject
        {
            ReadOnlySequence<byte> sequence = (ReadOnlySequence<byte>)source;
            var reader = new SequenceReader<byte>(sequence);
            var payload = sequence.Slice(reader.Position);
            var messageType = ProtoMessageIdHandler.GetRespTypeById(messageId);
            messageObject = (T)SerializerHelper.Deserialize(payload.First.ToArray(), messageType);
            return true;
        }
    }
}