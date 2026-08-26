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


using GameFrameX.Apps.Player.Bag.Component;
using GameFrameX.Apps.Player.Bag.Entity;
using GameFrameX.Config;
using GameFrameX.Config.Tables;

namespace GameFrameX.Hotfix.Logic.Player.Bag;

public class BagComponentAgent : StateComponentAgent<BagComponent, BagState>
{
    /// <summary>
    /// 增加背包物品
    /// </summary>
    /// <param name="netWorkChannel"></param>
    /// <param name="message"></param>
    /// <param name="response"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task OnAddBagItem(INetWorkChannel netWorkChannel, ReqAddItem message, RespAddItem response)
    {
        // 校验物品是否存在
        foreach (var item in message.ItemDic)
        {
            if (!ConfigComponent.Instance.GetConfig<TbItemConfig>().TryGet(item.Key, out var itemConfigState))
            {
                response.ErrorCode = (int)OperationStatusCode.NotFound;
                return;
            }
        }

        var result = await UpdateChanged(netWorkChannel, message.ItemDic);
        if (result.IsNull())
        {
            response.ErrorCode = (int)OperationStatusCode.Unprocessable;
        }
    }

    /// <summary>
    /// 增加背包物品
    /// </summary>
    /// <param name="netWorkChannel"></param>
    /// <param name="itemDic"></param>
    /// <returns></returns>
    public async Task<BagState> UpdateChanged(INetWorkChannel netWorkChannel, Dictionary<int, long> itemDic)
    {
        //将物品添加到背包
        var bagState = OwnerComponent.State;
        var notifyBagInfoChanged = new NotifyBagInfoChanged();
        foreach (var item in itemDic)
        {
            var notifyBagItem = new NotifyBagItem() { ItemId = item.Key };
            if (bagState.List.TryGetValue(item.Key, out var value))
            {
                value.Count += item.Value;
                notifyBagItem.Count = value.Count;
                notifyBagItem.Value = item.Value;
            }
            else
            {
                bagState.List[item.Key] = new BagItemState() { Count = item.Value, ItemId = item.Key };
                notifyBagItem.Count = item.Value;
                notifyBagItem.Value = item.Value;
            }

            notifyBagInfoChanged.ItemDic[item.Key] = notifyBagItem;
        }


        // netWorkChannel 为空表示离线发放（统一奖励发放接口）：只持久化背包，不推送客户端通知。
        if (netWorkChannel != null)
        {
            await netWorkChannel.WriteAsync(notifyBagInfoChanged);
        }

        await OwnerComponent.WriteStateAsync();
        return bagState;
    }

    /// <summary>
    /// 减少背包物品
    /// </summary>
    /// <param name="netWorkChannel"></param>
    /// <param name="message"></param>
    /// <param name="response"></param>
    public async Task OnRemoveBagItem(INetWorkChannel netWorkChannel, ReqRemoveItem message, RespRemoveItem response)
    {
        // 校验物品是否存在
        foreach (var item in message.ItemDic)
        {
            if (!ConfigComponent.Instance.GetConfig<TbItemConfig>().TryGet(item.Key, out var itemConfigState))
            {
                response.ErrorCode = (int)OperationStatusCode.NotFound;
                return;
            }
        }

        //将物品添加到背包
        var bagState = OwnerComponent.State;
        if (bagState.IsNotNull())
        {
            foreach (var item in message.ItemDic)
            {
                if (bagState.List.TryGetValue((int)item.Key, out var value))
                {
                    value.Count -= item.Value;
                    if (value.Count < 0)
                    {
                        response.ErrorCode = (int)OperationStatusCode.Unprocessable;
                        return;
                    }
                }
                else
                {
                    response.ErrorCode = (int)OperationStatusCode.NotFound;
                    return;
                }
            }
        }

        await OwnerComponent.WriteStateAsync();
    }

    /// <summary>
    /// 减少背包物品
    /// </summary>
    /// <param name="netWorkChannel"></param>
    /// <param name="message"></param>
    /// <param name="response"></param>
    public async Task OnUseBagItem(INetWorkChannel netWorkChannel, ReqUseItem message, RespUseItem response)
    {
        // 校验物品是否存在
        if (!ConfigComponent.Instance.GetConfig<TbItemConfig>().TryGet(message.ItemId, out var itemConfigState))
        {
            response.ErrorCode = (int)OperationStatusCode.NotFound;
            return;
        }

        //将物品从背包中删除
        var bagState = OwnerComponent.State;
        if (bagState.List.TryGetValue(message.ItemId, out var value))
        {
            response.ItemId = message.ItemId;

            value.Count -= message.Count;
            if (value.Count < 0)
            {
                value.Count = 0;
                bagState.List.Remove(message.ItemId); // 移除
            }

            response.Count = value.Count;
            var notifyBagInfoChanged = new NotifyBagInfoChanged
            {
                ItemDic =
                {
                    [message.ItemId] = new NotifyBagItem() { Count = value.Count, ItemId = message.ItemId, Value = message.Count },
                },
            };
            await netWorkChannel.WriteAsync(notifyBagInfoChanged);
        }
        else
        {
            response.ErrorCode = (int)OperationStatusCode.NotFound;
            return;
        }

        await OwnerComponent.WriteStateAsync();
    }

    /// <summary>
    /// 异步请求背包数据
    /// </summary>
    /// <param name="netWorkChannel"></param>
    /// <param name="message"></param>
    /// <param name="response"></param>
    public async Task OnReqBagInfoAsync(INetWorkChannel netWorkChannel, ReqBagInfo message, RespBagInfo response)
    {
        var bagState = OwnerComponent.State;
        if (bagState.IsNotNull())
        {
            response.ItemDic = bagState.List.ToDictionary(x => x.Key, x => x.Value.Count);
        }

        await Task.CompletedTask;
    }
}