namespace Server.NetWork.TCPSocket
{
    public sealed class TcpSocketMessageHelper : BaseSocketMessageHelper
    {
        public TcpSocketMessageHelper(Func<int, IMessageHandler> messageHandler, Func<int, Type> typeGetter, Func<Type, int> idGetter) : base(messageHandler, typeGetter, idGetter)
        {
        }
    }
}