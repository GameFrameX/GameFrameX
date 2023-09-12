using System;

namespace GameFramework.Network
{
    /// <summary>
    /// 网络消息处理器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageTypeHandlerAttribute : Attribute
    {
        /// <summary>
        /// 消息ID,不能重复
        /// </summary>
        public int MessageId { get; }

        /// <summary>
        /// 网络消息处理器
        /// </summary>
        /// <param name="messageId">消息ID,不能重复</param>
        public MessageTypeHandlerAttribute(int messageId)
        {
            MessageId = messageId;
        }
    }
}