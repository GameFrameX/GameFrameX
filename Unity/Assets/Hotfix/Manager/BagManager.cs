using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using Hotfix.Config;
using Hotfix.Config.Tables;
using Hotfix.Events;
using Hotfix.Proto;

namespace Hotfix.Manager
{
    /// <summary>
    /// 背包 管理器
    /// </summary>
    public sealed class BagManager : GameFrameworkSingleton<BagManager>, IMessageHandler
    {
        private readonly Dictionary<int, BagItem> _itemDic = new Dictionary<int, BagItem>();

        public List<BagItem> GetItems()
        {
            return _itemDic.Values.ToList();
        }

        /// <summary>
        /// 监听道具变化通知
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(NotifyBagInfoChanged), nameof(NotifyBagInfoChanged))]
        private void NotifyBagInfoChanged(NotifyBagInfoChanged msg)
        {
            foreach (var keyValuePair in msg.ItemDic)
            {
                if (_itemDic.TryGetValue(keyValuePair.Key, out var item))
                {
                    item.Count = keyValuePair.Value.Count;
                    if (_itemDic[keyValuePair.Key].Count <= 0)
                    {
                        _itemDic.Remove(keyValuePair.Key);
                    }
                }
                else
                {
                    _itemDic[keyValuePair.Key] = new BagItem() { ItemId = keyValuePair.Key, Count = keyValuePair.Value.Count };
                }
            }

            GameApp.Event.Fire(this, BagChangedEventArgs.Create());
        }

        /// <summary>
        /// 请求背包信息
        /// </summary>
        public async UniTask RequestGetBagInfo()
        {
            var respBagInfo = await GameApp.Network.GetNetworkChannel("network").Call<RespBagInfo>(new ReqBagInfo());
            if (respBagInfo.ErrorCode != default)
            {
                return;
            }

            foreach (var item in respBagInfo.ItemDic)
            {
                _itemDic[item.Key] = new BagItem() { ItemId = item.Key, Count = item.Value };
            }
        }

        /// <summary>
        /// 请求使用道具
        /// </summary>
        /// <param name="itemId">道具ID</param>
        /// <param name="count">道具数量</param>
        public async UniTask RequestUseItem(int itemId, long count = 1)
        {
            var respUseItem = await GameApp.Network.GetNetworkChannel("network").Call<RespUseItem>(new ReqUseItem() { ItemId = itemId, Count = count });
            if (respUseItem.ErrorCode != default)
            {
                return;
            }

            if (_itemDic.TryGetValue(respUseItem.ItemId, out var value))
            {
                value.Count -= respUseItem.Count;
                if (value.Count <= 0)
                {
                    _itemDic.Remove(respUseItem.ItemId);
                }
            }

            GameApp.Event.Fire(this, BagChangedEventArgs.Create());
        }

        /// <summary>
        /// 获取指定类型的道具
        /// </summary>
        /// <param name="bagType">背包类型</param>
        /// <returns></returns>
        public List<BagItem> GetBagItemsByType(ItemType bagType)
        {
            var result = new List<BagItem>(_itemDic.Count);
            var tbItemConfig = GameApp.Config.GetConfig<TbItemConfig>();
            var itemType = bagType;
            foreach (var bagItem in _itemDic)
            {
                var itemConfig = tbItemConfig.Get(bagItem.Key);
                if (itemConfig.IsNotNull() && itemConfig.Type == itemType)
                {
                    result.Add(bagItem.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// 由于是单例对象。所以在初始化的时候自动调用一次注册消息
        /// </summary>
        public BagManager()
        {
            Register();
        }

        /// <summary>
        /// 注册消息。请勿多次调用
        /// </summary>
        public void Register()
        {
            ProtoMessageHandler.Add(this);
        }
    }
}