using Server.NetWork.Messages;

namespace Server.NetWork;

public interface IMessageEncoderHandler
{
    byte[] Handler(IMessage message);
}