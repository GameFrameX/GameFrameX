namespace Server.App.Common.Event
{
    public enum EventId
    {
        #region role event

        /// <summary>
        /// 玩家事件
        /// </summary>
        SessionRemove = 1000,

        /// <summary>
        /// 玩家等级提升
        /// </summary>
        RoleLevelUp = 1001,

        /// <summary>
        /// 玩家vip改变
        /// </summary>
        RoleVipChange,

        /// <summary>
        /// 玩家上线
        /// </summary>
        OnRoleOnline,

        /// <summary>
        /// 玩家下线
        /// </summary>
        OnRoleOffline,

        /// <summary>
        /// 解锁用
        /// </summary>
        GotNewPet,

        #endregion

        /// <summary>
        /// 玩家事件分割点
        /// </summary>
        RoleSeparator = 8000,

        #region server event

        //服务器事件
        /// <summary>
        /// 世界等级改变
        /// </summary>
        WorldLevelChange,

        #endregion
    }
}