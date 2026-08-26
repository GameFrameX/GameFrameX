#if ENABLE_UI_FAIRYGUI
using GameFrameX.UI.Runtime;

namespace Hotfix.UI
{
    /// <summary>
    /// 旧版整合面板，已被 UIRoomListPanel + UIRoomBattlePanel + UIRoomResultPopup 取代，仅保留占位以兼容 FairyGUI 包定义。
    /// Legacy combined panel, superseded by UIRoomListPanel + UIRoomBattlePanel + UIRoomResultPopup. Kept only to match the FairyGUI package definition.
    /// </summary>
    public sealed partial class UIRoomPanel
    {
        public override void OnAwake()
        {
            UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Window.Name);
            base.OnAwake();
        }

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GameApp.UI.CloseUIForm<UIRoomPanel>();
        }
    }
}
#endif
