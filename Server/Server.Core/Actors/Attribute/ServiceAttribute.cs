namespace Server.Core.Actors
{
    /// <summary>
    /// 此方法会提供给其他Actor访问(对外提供服务)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceAttribute : Attribute
    {
    }
}