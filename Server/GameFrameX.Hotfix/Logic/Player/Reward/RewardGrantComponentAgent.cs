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

using GameFrameX.Apps.Common.Session;
using GameFrameX.Apps.Player.Reward;
using GameFrameX.Apps.Player.Reward.Component;
using GameFrameX.Apps.Player.Reward.Entity;
using GameFrameX.Config;
using GameFrameX.Config.Tables;
using GameFrameX.Core.Abstractions.Attribute;
using GameFrameX.Foundation.Utility;
using GameFrameX.Hotfix.Logic.Player.Bag;

namespace GameFrameX.Hotfix.Logic.Player.Reward;

/// <summary>
/// 统一奖励发放组件代理。提供跨组件可调用的 <see cref="GrantAsync"/>，作为邮件 / 运营补偿 / 兑换码 / 活动奖励等业务发放资产的唯一入口。
/// </summary>
/// <remarks>
/// 发放幂等（B6）以 <c>RoleId:SourceType:SourceId:TraceId</c> 为键，命中账本则短路返回上次结果，不重复写背包、不重复扣附件。
/// 一期仅落地普通道具路由（写 <c>BagState</c>），其余奖励类型为预留路由位，返回 <c>Unsupported</c>，不污染普通背包。
/// </remarks>
public class RewardGrantComponentAgent : StateComponentAgent<RewardGrantComponent, RewardGrantRecordState>
{
    /// <summary>
    /// 统一奖励发放。幂等键 <c>RoleId:SourceType:SourceId:TraceId</c> 重复提交时短路返回上次结果（B6）。
    /// </summary>
    /// <param name="request">发放请求。调用方需保证 <see cref="RewardGrantRequest.SourceType"/> 非 <see cref="RewardSourceType.None"/>，<see cref="RewardGrantRequest.SourceId"/>/<see cref="RewardGrantRequest.TraceId"/> 非空。</param>
    /// <returns>逐项发放结果、整体错误码与是否全部成功。</returns>
    [Service]
    public virtual async Task<RewardGrantResult> GrantAsync(RewardGrantRequest request)
    {
        var traceKey = BuildTraceKey(request);

        // 幂等短路（B6）：命中账本直接返回上次结果，不重复发奖。
        if (State.Records.TryGetValue(traceKey, out var existed))
        {
            return new RewardGrantResult
            {
                Items = existed.Items,
                ErrorCode = existed.ErrorCode,
                AllSuccess = existed.AllSuccess,
            };
        }

        var result = await DispatchAsync(request);

        // 记录幂等账本，持久化。记录写入后不可变。
        State.Records[traceKey] = new RewardGrantRecord
        {
            TraceKey = traceKey,
            SourceType = (int)request.SourceType,
            SourceId = request.SourceId,
            TraceId = request.TraceId,
            GrantedAt = TimerHelper.UnixTimeSeconds(),
            AllSuccess = result.AllSuccess,
            ErrorCode = result.ErrorCode,
            Items = result.Items,
        };
        await OwnerComponent.WriteStateAsync();

        return result;
    }

    /// <summary>
    /// 按 <see cref="RewardType"/> 路由发放各项奖励。
    /// </summary>
    private async Task<RewardGrantResult> DispatchAsync(RewardGrantRequest request)
    {
        var items = new List<RewardGrantItemResult>(request.Rewards.Count);
        var normalItemDic = new Dictionary<int, long>();
        var bagItemResultIndex = new List<int>();

        for (var i = 0; i < request.Rewards.Count; i++)
        {
            var reward = request.Rewards[i];
            var itemResult = new RewardGrantItemResult
            {
                RewardType = reward.RewardType,
                ItemId = reward.ItemId,
                Count = reward.Count,
            };

            if (reward.RewardType == RewardType.NormalItem)
            {
                if (reward.Count <= 0)
                {
                    itemResult.Success = false;
                    itemResult.ErrorCode = (int)OperationStatusCode.InvalidReward;
                }
                else if (!ConfigComponent.Instance.GetConfig<TbItemConfig>().TryGet(reward.ItemId, out _))
                {
                    itemResult.Success = false;
                    itemResult.ErrorCode = (int)OperationStatusCode.NotFound;
                }
                else
                {
                    // 暂记待发放，稍后批量写背包。
                    normalItemDic[reward.ItemId] = normalItemDic.TryGetValue(reward.ItemId, out var sum) ? sum + reward.Count : reward.Count;
                    bagItemResultIndex.Add(i);
                    continue;
                }
            }
            else
            {
                // 预留路由位：隐藏道具 / 月卡 / 终生卡 / VIP 点 / 权益一期不实现。
                itemResult.Success = false;
                itemResult.ErrorCode = (int)OperationStatusCode.Unsupported;
            }

            items.Add(itemResult);
        }

        // 普通道具批量写背包（在线推 notify、离线只持久化）。
        if (normalItemDic.Count > 0)
        {
            var bagAgent = await ActorManager.GetComponentAgent<BagComponentAgent>();
            var channel = SessionManager.GetByRoleId(request.RoleId)?.WorkChannel;
            var bagState = await bagAgent.UpdateChanged(channel, normalItemDic);
            var bagFailed = bagState.IsNull();

            foreach (var idx in bagItemResultIndex)
            {
                var reward = request.Rewards[idx];
                items.Add(new RewardGrantItemResult
                {
                    RewardType = reward.RewardType,
                    ItemId = reward.ItemId,
                    Count = reward.Count,
                    Success = !bagFailed,
                    ErrorCode = bagFailed ? (int)OperationStatusCode.Unprocessable : (int)OperationStatusCode.Ok,
                });
            }
        }

        Aggregate(items, out var errorCode, out var allSuccess);

        return new RewardGrantResult
        {
            Items = items,
            ErrorCode = errorCode,
            AllSuccess = allSuccess,
        };
    }

    /// <summary>
    /// 构造幂等键 <c>RoleId:SourceType:SourceId:TraceId</c>。
    /// </summary>
    private static string BuildTraceKey(RewardGrantRequest request)
    {
        return request.RoleId + ":" + (int)request.SourceType + ":" + request.SourceId + ":" + request.TraceId;
    }

    /// <summary>
    /// 汇总逐项结果为整体错误码与是否全部成功：
    /// 全部成功 <c>Ok</c>；部分成功 <c>PartialSuccess</c>；全部失败取首个失败项错误码。
    /// </summary>
    private static void Aggregate(List<RewardGrantItemResult> items, out int errorCode, out bool allSuccess)
    {
        if (items.Count == 0)
        {
            errorCode = (int)OperationStatusCode.Ok;
            allSuccess = true;
            return;
        }

        var successCount = 0;
        var firstFailureCode = (int)OperationStatusCode.Ok;
        foreach (var item in items)
        {
            if (item.Success)
            {
                successCount++;
            }
            else if (firstFailureCode == (int)OperationStatusCode.Ok)
            {
                firstFailureCode = item.ErrorCode;
            }
        }

        if (successCount == items.Count)
        {
            errorCode = (int)OperationStatusCode.Ok;
            allSuccess = true;
        }
        else if (successCount == 0)
        {
            errorCode = firstFailureCode;
            allSuccess = false;
        }
        else
        {
            errorCode = (int)OperationStatusCode.PartialSuccess;
            allSuccess = false;
        }
    }
}
