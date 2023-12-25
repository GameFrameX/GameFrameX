namespace Server.NetWork.HTTP;

public enum HttpStatusCode
{
    ///<summary>
    /// 成功
    /// </summary>
    Success = 200,

    ///<summary>
    /// 未定义的命令
    /// </summary>
    Undefine = 11,

    ///<summary>
    /// 非法
    /// </summary>
    Illegal = 12,

    ///<summary>
    /// 参数错误
    /// </summary>
    ParamErr = 13,

    ///<summary>
    /// 验证失败
    /// </summary>
    CheckFailed = 14,

    ///<summary>
    /// 操作失败
    /// </summary>
    ActionFailed = 15
}