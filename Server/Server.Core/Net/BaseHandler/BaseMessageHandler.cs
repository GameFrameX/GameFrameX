using Server.Core.Net.Messages;

namespace Server.Core.Net.BaseHandler
{
    public abstract class BaseMessageHandler
    {
        /// <summary>
        /// 网络频道
        /// </summary>
        public NetChannel Channel { get; set; }

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