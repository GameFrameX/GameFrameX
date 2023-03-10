namespace Framework
{
    public abstract class GameComponentModule
    {
        /// <summary>
        /// 优先级
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public abstract int Priority { get; }

        /// <summary>
        /// 激活
        /// </summary>
        public abstract void Active();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public abstract void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭
        /// </summary>
        public abstract void Shutdown();
    }
}