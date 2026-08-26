// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

#if ENABLE_UI_FAIRYGUI
using System.Collections.Generic;
using FairyGUI;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Hotfix.Config;
using Hotfix.Config.Tables;
using Hotfix.Events;
using Hotfix.Manager;
using Hotfix.Proto;

namespace Hotfix.UI
{
    public partial class UIBag
    {
        private class ItemTypeData
        {
            /// <summary>
            /// 道具类型
            /// </summary>
            public ItemType Type { get; }

            /// <summary>
            /// 分类名称
            /// </summary>
            public string Name { get; }

            public ItemTypeData(ItemType type, string name)
            {
                Type = type;
                Name = name;
            }
        }

        public override void OnAwake()
        {
            UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Window.Name);
            base.OnAwake();
        }

        private List<ItemTypeData> Tabs = new List<ItemTypeData>();
        List<object> _bagItems = new List<object>();
        private BagItem _selectBagItem = null;

        public override void OnOpen(object userData)
        {
            _bagItems = new List<object>();
            Tabs = new List<ItemTypeData>
            {
                new ItemTypeData(ItemType.Item, "道具"),
                new ItemTypeData(ItemType.Equip, "装备"),
                new ItemTypeData(ItemType.Fragment, "碎片"),
                new ItemTypeData(ItemType.Material, "材料"),
                new ItemTypeData(ItemType.Expendable, "消耗品"),
            };

            GameApp.Event.CheckSubscribe(BagChangedEventArgs.EventId, OnBagChangedEventArgs);

            base.OnOpen(userData);
            m_content.m_list.onClickItem.Set(OnBagItemClick);
            m_content.m_list.itemRenderer = BagItemRenderer;
            m_content.m_type_list.itemRenderer = TypeItemRenderer;
            m_content.m_type_list.onClickItem.Set(OnTabTypeClick);
            m_content.m_type_list.DataList = new List<object>(Tabs);
            m_content.m_type_list.GetChildAt(0).onClick.Call();

            m_content.m_info.m_use_button.onClick.Add(OnUseButtonClick);
            this.m_bg.onClick.Set(OnCloseClick);
        }


        private void OnBagChangedEventArgs(object sender, GameEventArgs e)
        {
            m_content.m_type_list.GetChildAt(m_content.m_type_list.selectedIndex).onClick.Call();
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        private async void OnUseButtonClick()
        {
            if (_selectBagItem.IsNull())
            {
                return;
            }

            await BagManager.Instance.RequestUseItem(_selectBagItem.ItemId, _selectBagItem.Count);
        }

        private void OnBagItemClick(EventContext context)
        {
            var data = UIBagItem.GetFormPool((GObject)context.data);
            var itemTypeData = (BagItem)data.self.dataSource;
            UpdateSelectItem(itemTypeData);
        }


        private void UpdateSelectItem(BagItem bagItem)
        {
            _selectBagItem = bagItem;
            UIGoodItem.GetFormPool(m_content.m_info.m_good_item).SetCount(bagItem.Count).SetIcon(bagItem.ItemId);
            var itemConfig = GameApp.Config.GetConfig<TbItemConfig>().Get(bagItem.ItemId);
            if (itemConfig.CanUse == ItemCanUse.CanNot)
            {
                m_content.m_info.m_IsCanUse.SetSelectedIndex(0);
            }
            else
            {
                m_content.m_info.m_IsCanUse.SetSelectedIndex(1);
            }

            m_content.m_info.m_name_text.text = itemConfig.Name;
            m_content.m_info.m_desc_text.text = itemConfig.Description;
        }


        private void BagItemRenderer(int index, GObject item)
        {
            var bagItemData = (BagItem)item.dataSource;
            var uiBagItem = UIBagItem.GetFormPool(item);
            var uiGoodItem = UIGoodItem.GetFormPool(uiBagItem.m_good_item);
            uiGoodItem.SetCount(bagItemData.Count);
            uiGoodItem.SetIcon(bagItemData.ItemId);
        }

        #region Tab

        private void OnTabTypeClick(EventContext context)
        {
            var data = UIBagTypeItem.GetFormPool((GButton)context.data);
            var itemTypeData = (ItemTypeData)data.self.dataSource;
            _bagItems.Clear();
            _bagItems.AddRange(BagManager.Instance.GetBagItemsByType(itemTypeData.Type));
            m_content.m_list.DataList = _bagItems;
            if (_bagItems.Count > 0)
            {
                m_content.m_list.selectedIndex = 0;
                m_content.m_IsSelectedItem.SetSelectedIndex(1);
                var bagItem = (BagItem)_bagItems[0];
                UpdateSelectItem(bagItem);
            }
            else
            {
                m_content.m_IsSelectedItem.SetSelectedIndex(0);
                _selectBagItem = null;
            }
        }

        private void TypeItemRenderer(int index, GObject item)
        {
            var itemTypeData = (ItemTypeData)item.dataSource;
            item.asButton.title = itemTypeData.Name;
        }

        #endregion

        private void OnCloseClick()
        {
            GameApp.UI.CloseUIForm<UIBag>();
        }
    }
}
#endif