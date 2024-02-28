namespace Server.NetWork.TCPSocket
{
    public sealed class KcpSocketMessageHelper : BaseSocketMessageHelper
    {
        public KcpSocketMessageHelper(Func<int, IMessageHandler> messageHandler, Func<int, Type> typeGetter, Func<Type, int> idGetter) : base(messageHandler, typeGetter, idGetter)
        {
        }
    }
}