namespace Server.NetWork.TCPSocket
{
    public class BaseSocketMessageHelper : IMessageHelper
    {
        /// <summary>
        /// 消息处理器。根据消息Id查找消息的处理器
        /// </summary>
        public Func<int, IMessageHandler> MessageHandler { get;protected set; }

        /// <summary>
        /// 根据消息Id查找消息类型
        /// </summary>
        public Func<int, Type> MessageTypeByIdGetter { get; protected set; }

        /// <summary>
        /// 根据消息类型查找消息ID
        /// </summary>
        public Func<Type, int> MessageIdByTypeGetter { get; protected set; }

        /// <summary>
        /// 构建基础的消息帮助器
        /// </summary>
        /// <param name="messageHandler">消息处理器</param>
        /// <param name="typeGetter">根据消息ID类型查找消息类型</param>
        /// <param name="idGetter">根据消息类型类型查找消息ID</param>
        public BaseSocketMessageHelper(Func<int, IMessageHandler> messageHandler, Func<int, Type> typeGetter, Func<Type, int> idGetter)
        {
            MessageHandler = messageHandler;
            MessageTypeByIdGetter = typeGetter;
            MessageIdByTypeGetter = idGetter;
        }
    }
}