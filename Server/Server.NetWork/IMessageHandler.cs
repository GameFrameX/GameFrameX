using Server.NetWork.Messages;

namespace Server.NetWork;

public interface IMessageHandler
{
    Task Init();
    Task InnerAction();
    MessageObject Message { get; set; }
    INetChannel Channel { get; set; }
}