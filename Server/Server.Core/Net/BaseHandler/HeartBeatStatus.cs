namespace Server.Core.Net.BaseHandler;

/// <summary>
/// 心跳状态
/// </summary>
public sealed class HeartBeatStatus
{
    private float m_HeartBeatElapseSeconds;
    private int m_MissHeartBeatCount;

    public HeartBeatStatus()
    {
        m_HeartBeatElapseSeconds = 0f;
        m_MissHeartBeatCount = 0;
    }

    /// <summary>
    /// 心跳间隔时长
    /// </summary>
    public float HeartBeatElapseSeconds
    {
        get => m_HeartBeatElapseSeconds;
        set => m_HeartBeatElapseSeconds = value;
    }

    /// <summary>
    /// 心跳丢失次数
    /// </summary>
    public int MissHeartBeatCount
    {
        get => m_MissHeartBeatCount;
        set => m_MissHeartBeatCount = value;
    }

    /// <summary>
    /// 重置心跳数据=>保活
    /// </summary>
    /// <param name="resetHeartBeatElapseSeconds">是否重置心跳流逝时长</param>
    public void Reset(bool resetHeartBeatElapseSeconds = true)
    {
        if (resetHeartBeatElapseSeconds)
        {
            m_HeartBeatElapseSeconds = 0f;
        }

        m_MissHeartBeatCount = 0;
    }
}