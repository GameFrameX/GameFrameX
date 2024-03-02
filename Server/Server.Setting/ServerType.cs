namespace Server.Setting;

/// <summary>
/// 服务器类型
/// </summary>
public enum ServerType
{
    /// <summary>
    /// 全部
    /// </summary>
    All = 0,

    /// <summary>
    /// 登录服务器
    /// </summary>
    Login,

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
    Backup,

    /// <summary>
    /// 服务发现服
    /// </summary>
    Discovery,

    /// <summary>
    /// 网关服
    /// </summary>
    Gate,

    /// <summary>
    /// 逻辑服
    /// </summary>
    Logic,

    /// <summary>
    /// 聊天服
    /// </summary>
    Chat,

    /// <summary>
    /// 邮件服
    /// </summary>
    Mail,

    /// <summary>
    /// 公会服
    /// </summary>
    Guild,

    /// <summary>
    /// 账号服
    /// </summary>
    Account,

    /// <summary>
    /// 房间服
    /// </summary>
    Room,

    /// <summary>
    /// 日志服
    /// </summary>
    Log,

    /// <summary>
    /// 数据库
    /// </summary>
    DataBase,

    /// <summary>
    /// 缓存服
    /// </summary>
    Cache
}