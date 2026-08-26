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



using GameFrameX.Apps.Common.Event;
using GameFrameX.Apps.Common.EventData;
using GameFrameX.Apps.Common.Session;
using GameFrameX.Apps.Player.Bag.Entity;
using GameFrameX.Hotfix.Logic.Player.Bag;

namespace GameFrameX.Hotfix.Logic.Http.Bag;

/// <summary>
/// 请求给玩家发送道具
/// </summary>
[HttpMessageMapping(typeof(ReqPlayerSendItemHttpHandler))]
[HttpMessageRequest(typeof(ReqPlayerSendItemRequest))]
[HttpMessageResponse(typeof(ReqPlayerSendItemResponse))]
[Description("请求给玩家发送道具")]
public sealed class ReqPlayerSendItemHttpHandler : BaseHttpHandler
{
    public override async Task<string> Action(string ip, string url, HttpMessageRequestBase requestBase)
    {
        var request = (ReqPlayerSendItemRequest)requestBase;
        var playerSession = SessionManager.GetByRoleId(request.RoleId);
        Dictionary<int, long> itemDic = new Dictionary<int, long>();

        var tbItemConfig = ConfigComponent.Instance.GetConfig<TbItemConfig>();
        if (tbItemConfig != null)
        {
            foreach (var item in request.Items)
            {
                if (!tbItemConfig.TryGet(item.Key, out var itemConfigState))
                {
                    continue;
                }

                itemDic[item.Key] = item.Value;
            }
        }

        // 发送道具事件
        var playerSendItemEventData = new PlayerSendItemEventArgs(request.RoleId, itemDic);
        EventDispatcher.Dispatch(request.RoleId, (int)EventId.PlayerSendItem, playerSendItemEventData);
        if (playerSession.IsNotNull())
        {
            // 玩家在线
            var bagComponentAgent = await ActorManager.GetComponentAgent<BagComponentAgent>(request.RoleId);
            await bagComponentAgent.UpdateChanged(playerSession.WorkChannel, itemDic);
        }
        else
        {
            // 玩家不在线
            var bagState = await GameDb.FindAsync<BagState>(request.RoleId);

            foreach (var item in itemDic)
            {
                if (bagState.List.TryGetValue(item.Key, out var value))
                {
                    value.Count += item.Value;
                    if (value.Count <= 0)
                    {
                        bagState.List.Remove(item.Key);
                    }
                }
                else
                {
                    var bagItem = new BagItemState
                    {
                        Count = item.Value,
                        ItemId = item.Key,
                    };
                    bagState.List[item.Key] = bagItem;
                }
            }

            await GameDb.SaveOneAsync(bagState);
        }

        return HttpJsonResult.SuccessString(new ReqPlayerSendItemResponse { Items = itemDic, });
    }
}

public sealed class ReqPlayerSendItemRequest : HttpMessageRequestBase
{
    [Required]
    [Description("角色ID")]
    [Range(1, long.MaxValue)]
    public long RoleId { get; set; }

    [Required] [Description("道具列表")] public Dictionary<int, long> Items { get; set; } = new Dictionary<int, long>();
}

public sealed class ReqPlayerSendItemResponse : HttpMessageResponseBase
{
    [Description("成功的道具列表")] public Dictionary<int, long> Items { get; set; } = new Dictionary<int, long>();
}