using System.Buffers;
using MessagePack;

namespace GameFrameX.Network
{
    public sealed class DefaultPacketReceiveBodyHandler : IPacketReceiveBodyHandler, IPacketHandler
    {
        public bool Handler<T>(object source, out T messageObject) where T : MessageObject
        {
            ReadOnlySequence<byte> sequence = (ReadOnlySequence<byte>)source;
            var reader = new MessagePack.SequenceReader<byte>(sequence);
            var payload = sequence.Slice(reader.Position);
            messageObject = MessagePackSerializer.Deserialize<T>(payload);
            return true;
        }
    }
}