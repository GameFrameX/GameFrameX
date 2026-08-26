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

using System.Collections.Generic;

namespace GameFrameX.Apps.Player.Mail.Entity
{
    /// <summary>
    /// 运营邮件附件元信息（Campaign 侧）。描述一个附件槽位的类型、物品、数量，
    /// 实际发放走统一奖励发放接口（<c>RewardGrantComponentAgent.GrantAsync</c>），
    /// 玩家领取时再生成具体 <c>RewardItem</c> 并写入幂等账本。
    /// </summary>
    public sealed class MailAttachmentState
    {
        /// <summary>
        /// 附件槽位 ID，Campaign 内唯一。用于领取幂等与展示排序。
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// 奖励类型（普通道具 / 隐藏道具 / 月卡 / VIP 点 等）。一期仅 <c>NormalItem</c> 可发放，其它返回 <c>Unsupported</c>。
        /// </summary>
        public int RewardType { get; set; }

        /// <summary>
        /// 物品 ID（普通道具对应 <c>TbItemConfig</c> 配置 ID）。
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// 数量。必须大于 0，否则发布校验返回 <c>InvalidReward</c>。
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 展示用图标 ID（可选，缺省时由客户端按 ItemId 兜底）。
        /// </summary>
        public int IconId { get; set; }
    }
}
