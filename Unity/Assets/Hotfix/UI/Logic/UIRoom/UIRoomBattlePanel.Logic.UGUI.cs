#if ENABLE_UI_UGUI
using GameFrameX.UI.Runtime;

namespace Hotfix.UI
{
    public sealed partial class UIRoomBattlePanel
    {
        private UIRoomBattlePanelController _controller;

        public override void OnAwake()
        {
            UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Window.Name);
            base.OnAwake();
        }

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _controller = new UIRoomBattlePanelController(this, () => IsDisposed);
            _controller.OnOpen();
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            if (_controller != null)
            {
                _controller.OnClose();
                _controller = null;
            }

            base.OnClose(isShutdown, userData);
        }
    }
}
#endif
