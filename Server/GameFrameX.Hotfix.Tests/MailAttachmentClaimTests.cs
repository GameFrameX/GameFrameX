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

using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;
using GameFrameX.Hotfix.Logic.Player.Mail;
using Xunit;

namespace GameFrameX.Hotfix.Tests;

/// <summary>
/// 邮件附件领取闭环（GFX-136）单元测试。覆盖领取成功 / 重复领取幂等 / 不可领 / 不存在 / 删除门禁 / 撤回作废 / 过期作废 / 一键领取等关键状态流转，
/// 通过对纯函数 <see cref="MailAttachmentClaim"/> 直接断言，避免引入 Actor 运行时。
/// </summary>
public class MailAttachmentClaimTests
{
    /// <summary>GFX-136 闭环：单附件领取成功 → Claimable 可发放 → 命中邮件 / 附件。</summary>
    [Fact]
    public void Prepare_Claimable_Returns_Ok()
    {
        var state = NewMailBox(MakeMail(out var mailId, out var slotId));

        var prepare = MailAttachmentClaim.Prepare(state, mailId, slotId);

        Assert.NotNull(prepare.Mail);
        Assert.NotNull(prepare.Attachment);
        Assert.Equal(ClaimStatus.Claimable, prepare.Attachment.ClaimStatus);
    }

    /// <summary>B6 幂等：重复领取已 Claimed 槽位，命中附件仍为 Claimed（调用方据此短路，不重复发奖）。</summary>
    [Fact]
    public void Prepare_AlreadyClaimed_Returns_Idempotent()
    {
        var state = NewMailBox(MakeMail(out var mailId, out var slotId));
        var prepare0 = MailAttachmentClaim.Prepare(state, mailId, slotId);
        MailAttachmentClaim.MarkClaimed(prepare0.Attachment, 1_700_000_000L);

        var prepare = MailAttachmentClaim.Prepare(state, mailId, slotId);

        Assert.NotNull(prepare.Attachment);
        Assert.Equal(ClaimStatus.Claimed, prepare.Attachment.ClaimStatus);
    }

    /// <summary>B3 / B4：撤回 / 过期作废后的附件命中状态为 Discarded。</summary>
    [Fact]
    public void Prepare_Discarded_Returns_Unclaimable()
    {
        var state = NewMailBox(MakeMail(out var mailId, out var slotId));
        MailAttachmentClaim.MarkDiscarded(MailAttachmentClaim.Prepare(state, mailId, slotId).Attachment);

        var prepare = MailAttachmentClaim.Prepare(state, mailId, slotId);

        Assert.NotNull(prepare.Attachment);
        Assert.Equal(ClaimStatus.Discarded, prepare.Attachment.ClaimStatus);
    }

    /// <summary>邮件不存在 → 未命中（Mail 为 null）。</summary>
    [Fact]
    public void Prepare_MissingMail_Returns_MailNotFound()
    {
        var state = NewMailBox();

        var prepare = MailAttachmentClaim.Prepare(state, 9999L, 1);

        Assert.Null(prepare.Mail);
    }

    /// <summary>B2 删除终态：已删除邮件按不存在处理（Mail 为 null）。</summary>
    [Fact]
    public void Prepare_DeletedMail_Returns_MailNotFound()
    {
        var state = NewMailBox(MakeMail(out var mailId, out var slotId));
        state.List[0].MailStatus = MailStatus.Deleted;

        var prepare = MailAttachmentClaim.Prepare(state, mailId, slotId);

        Assert.Null(prepare.Mail);
    }

    /// <summary>槽位不存在 → 邮件命中但附件未命中（Attachment 为 null）。</summary>
    [Fact]
    public void Prepare_MissingSlot_Returns_AttachmentNotFound()
    {
        var state = NewMailBox(MakeMail(out var mailId, out _));

        var prepare = MailAttachmentClaim.Prepare(state, mailId, 999);

        Assert.Null(prepare.Attachment);
    }

    /// <summary>MarkClaimed 写入终态 Claimed + ClaimTime（不可回退）。</summary>
    [Fact]
    public void MarkClaimed_Sets_Terminal()
    {
        var state = NewMailBox(MakeMail(out var mailId, out var slotId));
        var att = MailAttachmentClaim.Prepare(state, mailId, slotId).Attachment;

        MailAttachmentClaim.MarkClaimed(att, 1_700_000_100L);

        Assert.Equal(ClaimStatus.Claimed, att.ClaimStatus);
        Assert.Equal(1_700_000_100L, att.ClaimTime);
    }

    /// <summary>B3 不回滚已领：MarkDiscarded 对 Claimed 附件为 no-op。</summary>
    [Fact]
    public void MarkDiscarded_DoesNotRollback_Claimed()
    {
        var state = NewMailBox(MakeMail(out var mailId, out var slotId));
        var att = MailAttachmentClaim.Prepare(state, mailId, slotId).Attachment;
        MailAttachmentClaim.MarkClaimed(att, 1_700_000_000L);

        MailAttachmentClaim.MarkDiscarded(att);

        Assert.Equal(ClaimStatus.Claimed, att.ClaimStatus);
    }

    /// <summary>B3 撤回作废：未领附件作废、已领不动，邮件整体维度变 Unclaimable。</summary>
    [Fact]
    public void ComputeStatus_AfterRevoke_UnclaimedDiscarded_ClaimedStays()
    {
        var mail = MakeMailWithTwoSlots(out var mailId, out var slot1, out var slot2);
        var att1 = mail.Attachments[0];
        var att2 = mail.Attachments[1];
        MailAttachmentClaim.MarkClaimed(att1, 1_700_000_000L); // slot1 已领

        // 撤回作废未领附件
        MailAttachmentClaim.MarkDiscarded(att2);
        mail.MailStatus = MailStatus.Revoked;

        Assert.Equal(ClaimStatus.Claimed, att1.ClaimStatus);   // 已领不动（B3）
        Assert.Equal(ClaimStatus.Discarded, att2.ClaimStatus); // 未领作废
        Assert.Equal(AttachmentStatus.Unclaimable, MailAttachmentClaim.ComputeAttachmentStatus(mail));
    }

    /// <summary>B4 过期作废（DiscardUnclaimed 策略）：未领作废、邮件 Expired、整体 Unclaimable。</summary>
    [Fact]
    public void ComputeStatus_AfterExpire_UnclaimedDiscarded()
    {
        var mail = MakeMail(out var mailId, out var slotId);

        MailAttachmentClaim.MarkDiscarded(mail.Attachments[0]);
        mail.MailStatus = MailStatus.Expired;

        Assert.Equal(ClaimStatus.Discarded, mail.Attachments[0].ClaimStatus);
        Assert.Equal(AttachmentStatus.Unclaimable, MailAttachmentClaim.ComputeAttachmentStatus(mail));
    }

    /// <summary>B2 删除门禁：存在未领附件 → PartialClaimed → 删除应被拒（UnclaimedAttachment）。</summary>
    [Fact]
    public void ComputeStatus_UnclaimedAttachment_Blocks_Delete()
    {
        var mail = MakeMail(out _, out _);

        // 全部 Claimable，无一已领 → PartialClaimed（B2 门禁：非 NoAttachment/AllClaimed/Unclaimable 即拒绝删除）。
        Assert.Equal(AttachmentStatus.PartialClaimed, MailAttachmentClaim.ComputeAttachmentStatus(mail));
    }

    /// <summary>全领后删除放行：AttachmentStatus == AllClaimed。</summary>
    [Fact]
    public void ComputeStatus_AllClaimed_Allows_Delete()
    {
        var mail = MakeMail(out var mailId, out var slotId);
        MailAttachmentClaim.MarkClaimed(MailAttachmentClaim.Prepare(NewMailBox(mail), mailId, slotId).Attachment, 1L);

        Assert.Equal(AttachmentStatus.AllClaimed, MailAttachmentClaim.ComputeAttachmentStatus(mail));
    }

    /// <summary>无附件邮件 → NoAttachment（B2 放行删除）。</summary>
    [Fact]
    public void ComputeStatus_NoAttachment()
    {
        var mail = new MailState { MailId = 1, MailStatus = MailStatus.Read };

        Assert.Equal(AttachmentStatus.NoAttachment, MailAttachmentClaim.ComputeAttachmentStatus(mail));
    }

    /// <summary>一键领取模拟：多邮件多槽位，逐项 Claimable→Claimed，已领 / 作废跳过。</summary>
    [Fact]
    public void ClaimAll_Skips_NonClaimable_Advances_Claimable()
    {
        var box = NewMailBox();
        var mailA = MakeMailWithTwoSlots(out _, out var a1, out var a2);
        var mailB = MakeMail(out _, out var b1);
        box.List.Add(mailA);
        box.List.Add(mailB);

        // 预置：a1 已领，a2 作废（撤回），b1 可领
        MailAttachmentClaim.MarkClaimed(mailA.Attachments[0], 1L);
        MailAttachmentClaim.MarkDiscarded(mailA.Attachments[1]);

        var claimed = 0;
        foreach (var mail in box.List)
        {
            foreach (var att in mail.Attachments)
            {
                if (att.ClaimStatus != ClaimStatus.Claimable)
                {
                    continue;
                }

                MailAttachmentClaim.MarkClaimed(att, 2L);
                claimed++;
            }
        }

        Assert.Single(new[] { b1 });           // 仅 b1 一开始可领
        Assert.Equal(1, claimed);              // 只领了 b1
        Assert.Equal(ClaimStatus.Claimed, mailA.Attachments[0].ClaimStatus);
        Assert.Equal(ClaimStatus.Discarded, mailA.Attachments[1].ClaimStatus);
        Assert.Equal(ClaimStatus.Claimed, mailB.Attachments[0].ClaimStatus);
    }

    /// <summary>trace 四元组格式：CampaignId:Version:MailId:SlotId。</summary>
    [Fact]
    public void BuildTrace_Contains_All_Four_Parts()
    {
        var trace = MailAttachmentClaim.BuildTrace(100L, 3, 55L, 7);

        Assert.Equal("100:3:55:7", trace);
    }

    // ---------- fixtures ----------

    private static MailBoxState NewMailBox(MailState seed = null)
    {
        var box = new MailBoxState();
        if (seed != null)
        {
            box.List.Add(seed);
        }

        return box;
    }

    private static MailState MakeMail(out long mailId, out int slotId)
    {
        mailId = 1L;
        slotId = 1;
        return new MailState
        {
            MailId = mailId,
            CampaignId = 100L,
            CampaignVersion = 1,
            MailStatus = MailStatus.Read,
            Attachments = new List<MailAttachmentInstance>
            {
                new MailAttachmentInstance { SlotId = slotId, RewardType = 0, ItemId = 1001, Amount = 5, ClaimStatus = ClaimStatus.Claimable },
            },
        };
    }

    private static MailState MakeMailWithTwoSlots(out long mailId, out int slot1, out int slot2)
    {
        mailId = 1L;
        slot1 = 1;
        slot2 = 2;
        return new MailState
        {
            MailId = mailId,
            CampaignId = 100L,
            CampaignVersion = 1,
            MailStatus = MailStatus.Read,
            Attachments = new List<MailAttachmentInstance>
            {
                new MailAttachmentInstance { SlotId = slot1, RewardType = 0, ItemId = 1001, Amount = 5, ClaimStatus = ClaimStatus.Claimable },
                new MailAttachmentInstance { SlotId = slot2, RewardType = 0, ItemId = 1002, Amount = 1, ClaimStatus = ClaimStatus.Claimable },
            },
        };
    }
}
