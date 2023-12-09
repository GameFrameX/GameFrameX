namespace Luban.Editor
{
    /// <summary>
    /// 前置执行器
    /// </summary>
    public interface IBeforeGen
    {
        void Process();
    }

    /// <summary>
    /// 后置执行器
    /// </summary>
    public interface IAfterGen
    {
        void Process();
    }
}