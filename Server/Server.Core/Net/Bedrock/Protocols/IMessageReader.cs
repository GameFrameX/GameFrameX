using System.Buffers;

namespace Server.Core.Net.Bedrock.Protocols
{
    public interface IMessageReader<TMessage>
    {
        bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out TMessage message);
    }
}
