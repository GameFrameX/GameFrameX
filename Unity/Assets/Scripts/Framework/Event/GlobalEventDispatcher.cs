using Geek.Client;

public enum BaseEventID
{
    ServerListLoaded = -1000,
    NoticeLoaded,
    MainCityDollyCmp,
    LoginHistory,
}

/// <summary>
/// Global Event Dispatcher
/// </summary>
public static class GlobalEventDispatcher
{
    /// <summary>
    /// 网络
    /// </summary>
    public static readonly EventDispatcher NetDispatcher = new EventDispatcher();

    /// <summary>
    /// 游戏
    /// </summary>
    public static EventDispatcher GameDispatcher = new EventDispatcher();
}