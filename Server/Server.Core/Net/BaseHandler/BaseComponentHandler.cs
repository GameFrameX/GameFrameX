using Server.Core.Actors;
using Server.Core.Hotfix.Agent;

namespace Server.Core.Net.BaseHandler
{
    /// <summary>
    /// 基础组件处理器基类
    /// </summary>
    public abstract class BaseComponentHandler : BaseMessageHandler
    {
        protected long ActorId { get; set; }

        /// <summary>
        /// 组件代理类型
        /// </summary>
        protected abstract Type ComponentAgentType { get; }

        /// <summary>
        /// 缓存组件代理对象
        /// </summary>
        public IComponentAgent CacheComponent { get; set; }

        protected abstract Task InitActor();

        public override async Task Init()
        {
            await InitActor();
            if (CacheComponent == null)
            {
                CacheComponent = await ActorManager.GetComponentAgent(ActorId, ComponentAgentType);
                // LogHelper.Info(CacheComp);
            }
        }

        public override Task InnerAction()
        {
            CacheComponent.Tell(ActionAsync);
            return Task.CompletedTask;
        }

        protected Task<TOtherAgent> GetComponentAgent<TOtherAgent>() where TOtherAgent : IComponentAgent
        {
            return CacheComponent.GetComponentAgent<TOtherAgent>();
        }
    }
}