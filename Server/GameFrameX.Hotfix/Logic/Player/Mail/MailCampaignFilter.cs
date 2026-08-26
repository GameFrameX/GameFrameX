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

using GameFrameX.Apps.Player.Mail.Entity;

namespace GameFrameX.Hotfix.Logic.Player.Mail;

/// <summary>
/// 邮件 Campaign 命中筛选（U1 §4.5 <c>MatchFilter</c>）。
/// </summary>
/// <remarks>
/// 命中规则：<c>ServerIds</c> 空 or 含当前服 <c>Setting.ServerId</c>；<c>MinLevel/MaxLevel</c> 为 0 时表示不限。
/// 渠道（<c>ChannelIds</c>）命中由调用方在调用本筛选前单独判定（未补齐前不实例化也不记 key）。
/// </remarks>
internal static class MailCampaignFilter
{
    /// <summary>
    /// 判断 Campaign 是否命中当前玩家。任一非空条件不满足即返回 false。
    /// </summary>
    /// <param name="campaign">Campaign 不可变快照。</param>
    /// <param name="serverId">当前服 <c>Setting.ServerId</c>（业务 ServerInstanceId）。</param>
    /// <param name="playerLevel">玩家等级。</param>
    /// <param name="playerCreatedTime">玩家创建时间（unix 秒，<c>CacheState.CreatedTime</c>）。</param>
    /// <returns>命中返回 true；否则 false。</returns>
    public static bool Match(MailCampaignState campaign, int serverId, long playerLevel, long playerCreatedTime)
    {
        if (campaign.ServerIds != null && campaign.ServerIds.Count > 0 && !campaign.ServerIds.Contains(serverId))
        {
            return false;
        }

        if (campaign.MinLevel > 0 && playerLevel < campaign.MinLevel)
        {
            return false;
        }

        if (campaign.MaxLevel > 0 && playerLevel > campaign.MaxLevel)
        {
            return false;
        }

        return true;
    }
}
