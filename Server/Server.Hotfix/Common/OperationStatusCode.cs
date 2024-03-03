namespace Server.Hotfix.Common
{
    /// <summary>
    /// 操作状态码=>服务器错误码
    /// </summary>
    public enum OperationStatusCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 配置表错误
        /// </summary>
        ConfigErr = 400,

        /// <summary>
        /// 客户端传递参数错误
        /// </summary>
        ParamErr,

        /// <summary>
        /// 消耗不足
        /// </summary>
        CostNotEnough,

        /// <summary>
        /// 账号不存在或为空
        /// </summary>
        AccountCannotBeNull,

        /// <summary>
        /// 未知平台
        /// </summary>
        UnknownPlatform,

        /// <summary>
        /// 正常通知
        /// </summary>
        Notice = 100000,

        /// <summary>
        /// 功能未开启，主消息屏蔽
        /// </summary>
        FuncNotOpen,

        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
}