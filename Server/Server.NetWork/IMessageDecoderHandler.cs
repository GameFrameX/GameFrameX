using Server.NetWork.Messages;

namespace Server.NetWork;

public interface IMessageDecoderHandler
{
    IMessage Handler(Span<byte> data);
}