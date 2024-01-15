namespace Server.Core.Comps
{
    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 准备状态
        /// </summary>
        /// <returns></returns>
        public Task ReadStateAsync();
    }
}