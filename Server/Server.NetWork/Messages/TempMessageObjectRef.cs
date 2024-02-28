using Server.Extension;

namespace Server.NetWork.Messages;

/// <summary>
/// 临时网络消息包
/// </summary>
public readonly ref struct TempNetPackage
{
    /// <summary>
    /// 头长度
    /// </summary>
    public const int HeadLength = 13;

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// 消息标签
    /// </summary>
    public byte Flag { get; }

    /// <summary>
    /// 网络ID
    /// </summary>
    public long NetId { get; }

    /// <summary>
    /// 内部服务ID
    /// </summary>
    public int InnerServerId { get; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public ReadOnlySpan<byte> Body { get; }

    public TempNetPackage(byte flag, long netId, int targetServerId = 0)
    {
        IsOk = true;
        this.Flag = flag;
        this.NetId = netId;
        InnerServerId = targetServerId;
    }

    public TempNetPackage(byte flag, long netId, int targetServerId, ReadOnlySpan<byte> data)
    {
        IsOk = true;
        this.Flag = flag;
        this.NetId = netId;
        InnerServerId = targetServerId;
        Body = data;
    }

    public TempNetPackage(Span<byte> data)
    {
        if (data.Length < HeadLength)
        {
            IsOk = false;
            return;
        }

        IsOk = true;
        Flag = data[0];
        NetId = data.ReadLong(1);
        InnerServerId = data.ReadInt(9);
        Body = data[HeadLength..];
    }

    /// <summary>
    /// 数据总长度
    /// </summary>
    public readonly int Length => HeadLength + Body.Length;

    public override string ToString()
    {
        return $"{{flag:{Flag},netId:{NetId},innerServerId:{InnerServerId},bodyLen:{Body.Length}}}";
    }
}