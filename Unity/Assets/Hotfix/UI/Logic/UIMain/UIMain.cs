using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using GameFrameX.UI.FairyGUI.Runtime;
#endif
#if ENABLE_UI_UGUI
using GameFrameX.UI.UGUI.Runtime;
#endif
using UnityEngine;
using Hotfix.Manager;
using Hotfix.Proto;

namespace Hotfix.UI
{
    public partial class UIMain
    {
        public override void OnAwake()
        {
            UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Normal.Name);
            base.OnAwake();
        }

        public override async void OnOpen(object userData)
        {
            base.OnOpen(userData);
#if ENABLE_UI_UGUI
            var assetHandle = await GameApp.Asset.LoadAssetAsync<Sprite>(Utility.Asset.Path.GetCategoryFilePath("Sprites", $"avatar/{PlayerManager.Instance.PlayerInfo.Avatar}"));
            m_player_icon.sprite = assetHandle.GetAssetObject<Sprite>();
#elif ENABLE_UI_FAIRYGUI
            m_player_icon.icon = UIPackage.GetItemURL(FUIPackage.UICommonAvatar, PlayerManager.Instance.PlayerInfo.Avatar.ToString());
#endif
            m_player_name.text = PlayerManager.Instance.PlayerInfo.Name;
            m_player_level.text = "当前等级:" + PlayerManager.Instance.PlayerInfo.Level;
            m_bag_button.onClick.Set(OnBagBtnClick);
#if ENABLE_UI_FAIRYGUI
            if (m_room_button != null)
            {
                m_room_button.onClick.Set(OnRoomBtnClick);
            }
#elif ENABLE_UI_UGUI
            m_room_button.onClick.Set(OnRoomBtnClick);
#endif
        }

        private async void OnBagBtnClick()
        {
#if ENABLE_UI_UGUI
            ReqBagInfo reqBagInfo = new ReqBagInfo();
            var respBagInfo = await GameApp.Network.GetNetworkChannel("network").Call<RespBagInfo>(reqBagInfo);
            Log.Debug(respBagInfo);
#elif ENABLE_UI_FAIRYGUI
            await GameApp.UI.OpenAsync<UIBag>(UIGroupConstants.Window);
#endif
        }

        private async void OnRoomBtnClick()
        {
            await GameApp.UI.OpenAsync<UIRoomListPanel>(UIGroupConstants.Window);
        }
    }
}
