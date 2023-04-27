using System.Buffers;

namespace Server.Core.Net.Bedrock.Protocols
{
    public interface IMessageWriter<TMessage>
    {
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}
