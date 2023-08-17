namespace Server.Setting;

public enum ServerType
{
    None = 0,

    ///<summary>
    /// 游戏服
    /// </summary>
    Game,

    ///<summary>
    /// 中心服
    /// </summary>
    Center,

    ///<summary>
    /// 充值服
    /// </summary>
    Recharge,

    ///<summary>
    /// 远程备份
    /// </summary>
    Backup
}