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

using System;
using System.Collections.Generic;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;
using GameFrameX.Hotfix.Logic.Player.Mail;
using Xunit;

namespace GameFrameX.Hotfix.Tests;

/// <summary>
/// 运营邮件 Campaign 注册表（<see cref="MailCampaignRegistry"/>）总体验收单测（GFX-135 / F2）。
/// 覆盖校验 / 发布（B1 发布后不可修改）/ 撤回（B3 状态字段写入 + 重复撤回 CampaignAlreadyRevoked）/
/// B5（撤回后同 CampaignId 拒绝再发布）/ 端到端 smoke（发布 → 查询 → 撤回 → 查询）/ 查询过滤。
/// 本组用例把 e921348a 移除 SelfCheck 工程时一并删除、且未迁移到 xunit 的 Campaign 侧 self-check 场景移植回归，
/// 与 <see cref="MailAttachmentClaimTests"/>（领取侧 B2/B3/B4/B6）合并满足 GFX-135 验收硬性覆盖。
/// 进程内 registry 为静态字典，每个用例开头 <see cref="MailCampaignRegistry.ResetForTest"/> 隔离全局状态。
/// </summary>
public class MailCampaignRegistryTests
{
    /// <summary>校验：null / 无 title / blank title / 重复 SlotId / MinLevel&gt;MaxLevel 均非法，合法 Campaign 通过。</summary>
    [Fact]
    public void Validate_Returns_InvalidCampaignParameter_For_Null_And_Bad_State()
    {
        MailCampaignRegistry.ResetForTest();

        Assert.Equal(MailCampaignErrorCode.InvalidCampaignParameter, MailCampaignRegistry.Validate(null));

        var noTitle = new MailCampaignState { MailType = MailType.Operation };
        Assert.Equal(MailCampaignErrorCode.InvalidCampaignParameter, MailCampaignRegistry.Validate(noTitle));

        var blankTitle = new MailCampaignState
        {
            MailType = MailType.Operation,
            Titles = new List<MailLocalizedContent> { new MailLocalizedContent { Language = "", Text = "" } },
        };
        Assert.Equal(MailCampaignErrorCode.InvalidCampaignParameter, MailCampaignRegistry.Validate(blankTitle));

        var dupSlot = new MailCampaignState
        {
            MailType = MailType.Operation,
            Titles = new List<MailLocalizedContent> { new MailLocalizedContent { Language = "zh-cn", Text = "title" } },
            Attachments = new List<MailAttachmentState>
            {
                new MailAttachmentState { SlotId = 1, Amount = 10 },
                new MailAttachmentState { SlotId = 1, Amount = 20 },
            },
        };
        Assert.Equal(MailCampaignErrorCode.InvalidCampaignParameter, MailCampaignRegistry.Validate(dupSlot));

        var levelRangeBad = MakeValidCampaign();
        levelRangeBad.MinLevel = 50;
        levelRangeBad.MaxLevel = 10;
        Assert.Equal(MailCampaignErrorCode.InvalidCampaignParameter, MailCampaignRegistry.Validate(levelRangeBad));

        Assert.Equal(MailCampaignErrorCode.Ok, MailCampaignRegistry.Validate(MakeValidCampaign()));
    }

    /// <summary>校验：附件 amount&lt;=0 返回 InvalidCampaignParameter（附件数量非正属 Campaign 参数非法）。</summary>
    [Fact]
    public void Validate_Returns_InvalidCampaignParameter_For_NonPositive_Amount()
    {
        MailCampaignRegistry.ResetForTest();

        var badAttachment = new MailCampaignState
        {
            MailType = MailType.Operation,
            Titles = new List<MailLocalizedContent> { new MailLocalizedContent { Language = "zh-cn", Text = "title" } },
            Attachments = new List<MailAttachmentState> { new MailAttachmentState { SlotId = 1, Amount = 0 } },
        };

        Assert.Equal(MailCampaignErrorCode.InvalidCampaignParameter, MailCampaignRegistry.Validate(badAttachment));
    }

    /// <summary>B1：PublishOrUpdate 分配 CampaignId / PublishVersion=1 / Status=Published / PublishedAt / PublishOperator；
    /// 已发布 Campaign 再次以同 CampaignId 提交应抛 InvalidOperationException（主体字段不可修改）。</summary>
    [Fact]
    public void PublishOrUpdate_Assigns_Fields_And_B1_Blocks_Republish_Of_Published()
    {
        MailCampaignRegistry.ResetForTest();

        var campaign = MakeValidCampaign();
        var published = MailCampaignRegistry.PublishOrUpdate(campaign, "admin-test", 1_700_000_000L);

        Assert.True(published.CampaignId > 0);
        Assert.Equal(1, published.PublishVersion);
        Assert.Equal(MailCampaignStatus.Published, published.Status);
        Assert.Equal(1_700_000_000L, published.PublishedAt);
        Assert.Equal("admin-test", published.PublishOperator);

        // B1：重复发布已发布 Campaign 应抛异常。
        var republish = MakeValidCampaign();
        republish.CampaignId = published.CampaignId;
        Assert.Throws<InvalidOperationException>(() =>
            MailCampaignRegistry.PublishOrUpdate(republish, "admin-test", 1_700_000_100L));
    }

    /// <summary>B3：Revoke 对未知 CampaignId 返回 CampaignNotFound；成功撤回写入 Status=Revoked / RevokedAt / RevokeOperator；
    /// 重复撤回返回 CampaignAlreadyRevoked。</summary>
    [Fact]
    public void Revoke_Returns_NotFound_For_Unknown_Then_Sets_Status_B3()
    {
        MailCampaignRegistry.ResetForTest();

        var published = MailCampaignRegistry.PublishOrUpdate(MakeValidCampaign(), "admin-test", 1_700_000_200L);

        Assert.Equal(MailCampaignErrorCode.CampaignNotFound, MailCampaignRegistry.Revoke(999_999L, "admin-test", 1_700_000_300L));

        Assert.Equal(MailCampaignErrorCode.Ok, MailCampaignRegistry.Revoke(published.CampaignId, "admin-test", 1_700_000_300L));

        Assert.True(MailCampaignRegistry.TryQuery(published.CampaignId, out var revoked));
        Assert.Equal(MailCampaignStatus.Revoked, revoked.Status);
        Assert.Equal(1_700_000_300L, revoked.RevokedAt);
        Assert.Equal("admin-test", revoked.RevokeOperator);

        // B3：重复撤回返回 CampaignAlreadyRevoked（资产回滚由统一发放接口幂等账本保证，不在此校验）。
        Assert.Equal(MailCampaignErrorCode.CampaignAlreadyRevoked,
            MailCampaignRegistry.Revoke(published.CampaignId, "admin-test", 1_700_000_400L));
    }

    /// <summary>B5（Campaign 侧兜底）：撤回后的 Campaign，再次以同 CampaignId 调 PublishOrUpdate 应抛 InvalidOperationException，
    /// 且撤回状态保持不变（不被再发布覆盖），呼应 U1 §4.5 / §4.6「撤回 Campaign 防重实例化」。</summary>
    [Fact]
    public void B5_Revoke_Blocks_Republish_Of_Same_CampaignId()
    {
        MailCampaignRegistry.ResetForTest();

        var published = MailCampaignRegistry.PublishOrUpdate(MakeValidCampaign(), "admin-test", 1_700_010_000L);
        Assert.Equal(MailCampaignErrorCode.Ok, MailCampaignRegistry.Revoke(published.CampaignId, "admin-test", 1_700_010_100L));

        var republish = MakeValidCampaign();
        republish.CampaignId = published.CampaignId;
        Assert.Throws<InvalidOperationException>(() =>
            MailCampaignRegistry.PublishOrUpdate(republish, "admin-test", 1_700_010_200L));

        // 撤回状态应保持不变（不被再发布覆盖）。
        Assert.True(MailCampaignRegistry.TryQuery(published.CampaignId, out var afterAttempt));
        Assert.Equal(MailCampaignStatus.Revoked, afterAttempt.Status);
        Assert.Equal(1_700_010_100L, afterAttempt.RevokedAt);
    }

    /// <summary>端到端 smoke（Campaign 侧）：发布 → TryQuery 命中 Published → 撤回 →
    /// TryQuery 命中 Revoked（RevokedAt / RevokeOperator）→ QueryAll(status=Revoked) 过滤命中 → 重复撤回 CampaignAlreadyRevoked。</summary>
    [Fact]
    public void EndToEnd_Publish_Query_Revoke_Query()
    {
        MailCampaignRegistry.ResetForTest();

        var campaign = MakeValidCampaign();
        campaign.MailType = MailType.Operation;
        campaign.ServerIds = new List<int> { 200 };
        campaign.MinLevel = 20;
        campaign.MaxLevel = 80;

        // 1. 发布 → Server 保存不可变快照（B1）。
        var published = MailCampaignRegistry.PublishOrUpdate(campaign, "admin-flow", 1_700_020_000L);
        Assert.True(published.CampaignId > 0);
        Assert.Equal(MailCampaignStatus.Published, published.Status);
        Assert.Equal("admin-flow", published.PublishOperator);

        // 2. TryQuery 命中 Published。
        Assert.True(MailCampaignRegistry.TryQuery(published.CampaignId, out var q1));
        Assert.Equal(MailCampaignStatus.Published, q1.Status);
        Assert.Equal(0L, q1.RevokedAt);

        // 3. 撤回（B3：仅置状态字段）。
        Assert.Equal(MailCampaignErrorCode.Ok, MailCampaignRegistry.Revoke(published.CampaignId, "admin-revoke", 1_700_020_500L));

        // 4. TryQuery 命中 Revoked + RevokedAt + RevokeOperator。
        Assert.True(MailCampaignRegistry.TryQuery(published.CampaignId, out var q2));
        Assert.Equal(MailCampaignStatus.Revoked, q2.Status);
        Assert.Equal(1_700_020_500L, q2.RevokedAt);
        Assert.Equal("admin-revoke", q2.RevokeOperator);

        // 5. QueryAll(status=Revoked) 过滤命中；重复撤回返回 CampaignAlreadyRevoked。
        var revokedList = MailCampaignRegistry.QueryAll(status: MailCampaignStatus.Revoked);
        Assert.Contains(revokedList, c => c.CampaignId == published.CampaignId);
        Assert.Equal(MailCampaignErrorCode.CampaignAlreadyRevoked,
            MailCampaignRegistry.Revoke(published.CampaignId, "admin-revoke", 1_700_020_600L));
    }

    /// <summary>查询过滤：QueryAll() 返回全部；QueryAll(mailType) / QueryAll(serverId) 命中正确子集。</summary>
    [Fact]
    public void QueryAll_Filters_By_MailType_And_ServerId()
    {
        MailCampaignRegistry.ResetForTest();

        var c1 = MakeValidCampaign();
        c1.MailType = MailType.Operation;
        c1.MinLevel = 10;
        c1.MaxLevel = 50;
        c1.ServerIds = new List<int> { 100 };
        var p1 = MailCampaignRegistry.PublishOrUpdate(c1, "admin-test", 1_700_000_500L);

        var c2 = MakeValidCampaign();
        c2.MailType = MailType.Compensation;
        var p2 = MailCampaignRegistry.PublishOrUpdate(c2, "admin-test", 1_700_000_600L);

        Assert.True(MailCampaignRegistry.QueryAll().Count >= 2);

        var filtered = MailCampaignRegistry.QueryAll(mailType: MailType.Operation);
        Assert.NotEmpty(filtered);
        Assert.All(filtered, c => Assert.Equal(MailType.Operation, c.MailType));

        var byServer = MailCampaignRegistry.QueryAll(serverId: 100);
        Assert.NotEmpty(byServer);
        Assert.All(byServer, c => Assert.Contains(100, c.ServerIds));

        // TryQuery 命中已发布 / 未命中不存在。
        Assert.True(MailCampaignRegistry.TryQuery(p1.CampaignId, out _));
        Assert.False(MailCampaignRegistry.TryQuery(p2.CampaignId + 10_000L, out _));
    }

    // ---------- fixtures ----------

    private static MailCampaignState MakeValidCampaign()
    {
        return new MailCampaignState
        {
            MailType = MailType.Operation,
            Titles = new List<MailLocalizedContent>
            {
                new MailLocalizedContent { Language = "zh-cn", Text = "测试标题" },
                new MailLocalizedContent { Language = "en-us", Text = "Test Title" },
            },
            Contents = new List<MailLocalizedContent>
            {
                new MailLocalizedContent { Language = "zh-cn", Text = "测试正文" },
            },
            Attachments = new List<MailAttachmentState>
            {
                new MailAttachmentState { SlotId = 1, RewardType = 0, ItemId = 1001, Amount = 5 },
            },
            ExpireAttachmentPolicy = ExpireAttachmentPolicy.DiscardUnclaimed,
            ExpireAt = 1_700_999_999L,
        };
    }
}
