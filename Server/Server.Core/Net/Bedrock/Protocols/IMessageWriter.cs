using System.Buffers;

namespace Server.Core.Net.Bedrock.Protocols
{
    public interface IMessageWriter<in TMessage>
    {
        void WriteMessage(TMessage netMessage, IBufferWriter<byte> output);
    }
}