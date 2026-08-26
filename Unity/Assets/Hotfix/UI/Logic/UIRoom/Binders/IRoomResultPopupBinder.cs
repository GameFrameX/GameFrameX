using System;

namespace Hotfix.UI
{
    /// <summary>
    /// UIRoomResultPopup 的视图绑定接口。
    /// View binder interface for UIRoomResultPopup.
    /// </summary>
    public interface IRoomResultPopupBinder
    {
        void SetResult(string text);

        void SetRound(string text);

        void SetSelfName(string text);

        void SetOpponentName(string text);

        void SetSelfGesture(string text);

        void SetOpponentGesture(string text);

        void SetRestartEnabled(bool enabled);

        void RegisterHandlers(Action onClose, Action onRestart);

        void ClearHandlers();
    }
}
