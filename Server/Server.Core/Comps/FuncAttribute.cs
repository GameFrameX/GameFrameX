namespace Server.Core.Comps
{
    /// <summary>
    /// 有关组件的功能
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FuncAttribute : Attribute
    {
        public readonly short Func;

        public FuncAttribute(short func)
        {
            this.Func = func;
        }
    }
}