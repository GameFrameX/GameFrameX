namespace Server.Setting;

/// <summary>
/// 服务器类型
/// </summary>
public enum ServerType
{
    /// <summary>
    /// 空值
    /// </summary>
    None = 0,

    ///<summary>
    /// 游戏服
    /// </summary>
    Game = 1,

    ///<summary>
    /// 中心服
    /// </summary>
    Center = 2,

    ///<summary>
    /// 充值服
    /// </summary>
    Recharge = 3,

    ///<summary>
    /// 远程备份
    /// </summary>
    Backup = 4
}