using Server.NetWork;
using Server.NetWork.Messages;

namespace Server.Core.Net.BaseHandler
{
    public abstract class BaseMessageHandler : IMessageHandler
    {
        /// <summary>
        /// 网络频道
        /// </summary>
        public INetChannel Channel { get; set; }

        /// <summary>
        /// 消息对象
        /// </summary>
        public MessageObject Message { get; set; }

        public virtual Task Init()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 动作异步
        /// </summary>
        /// <returns></returns>
        protected abstract Task ActionAsync();

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public virtual Task InnerAction()
        {
            return ActionAsync();
        }
    }
}