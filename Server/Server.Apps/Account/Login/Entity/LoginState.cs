using Server.DBServer.State;

namespace Server.Apps.Account.Login.Entity;

public class LoginState : CacheState
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 用户状态
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// //是否是重连
    /// </summary>
    public bool isReconnect { get; set; }

    // public long roleId { get; set; }
    public int serverId { get; set; }

    // public string playerId { get; set; }
    // public string newName { get; set; }
    public int uniId { get; set; }

    public int sdkType { get; set; }

    public string sdkChannel { get; set; }
}