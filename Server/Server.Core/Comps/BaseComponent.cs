using Server.Core.Actors;
using Server.Core.Hotfix;
using Server.Core.Hotfix.Agent;

namespace Server.Core.Comps
{
    /// <summary>
    /// 基础组件基类
    /// </summary>
    public abstract class BaseComponent
    {
        private IComponentAgent cacheAgent = null;
        private readonly object cacheAgentLock = new();

        /// <summary>
        /// 根据组件类型获取对应的IComponentAgent数据
        /// </summary>
        /// <param name="refAssemblyType">引用程序集，如果为null则使用当前程序集引用</param>
        /// <returns></returns>
        public IComponentAgent GetAgent(Type refAssemblyType = null)
        {
            lock (cacheAgentLock)
            {
                if (cacheAgent != null && !HotfixMgr.DoingHotfix)
                {
                    return cacheAgent;
                }

                var agent = HotfixMgr.GetAgent<IComponentAgent>(this, refAssemblyType);
                cacheAgent = agent;
                return agent;
            }
        }

        /// <summary>
        /// 清理缓存代理
        /// </summary>
        public void ClearCacheAgent()
        {
            cacheAgent = null;
        }

        /// <summary>
        /// Actor对象
        /// </summary>
        internal Actor Actor { get; set; }

        /// <summary>
        /// ActorId
        /// </summary>
        internal long ActorId => Actor.Id;

        /// <summary>
        /// 是否是激活状态
        /// </summary>
        public bool IsActive { get; private set; } = false;

        /// <summary>
        /// 激活组件
        /// </summary>
        /// <returns></returns>
        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 反激活组件
        /// </summary>
        public virtual async Task Inactive()
        {
            var agent = GetAgent();
            if (agent != null)
                await agent.Inactive();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <returns></returns>
        internal virtual Task SaveState()
        {
            return Task.CompletedTask;
        }

        internal virtual bool ReadyToInactive => true;
    }
}