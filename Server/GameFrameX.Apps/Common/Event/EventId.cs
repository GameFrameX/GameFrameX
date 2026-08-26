// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//  
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//  
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//  
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

namespace GameFrameX.Apps.Common.Event;

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

    /// <summary>
    /// 玩家发送道具
    /// </summary>
    PlayerSendItem,

    /// <summary>
    /// 玩家最终属性变化
    /// </summary>
    AttributeChanged,

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
