namespace Server.Core.Actors;

///<summary>
/// 此方法线程安全
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ThreadSafeAttribute : Attribute
{
}