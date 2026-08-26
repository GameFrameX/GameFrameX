using System;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UIRoomBattlePanel 的视图绑定接口。
    /// View binder interface for UIRoomBattlePanel.
    /// </summary>
    public interface IRoomBattlePanelBinder
    {
        void SetRoomTitle(string text);

        void SetRoomState(string text);

        void SetInvite(string text);

        void SetArenaResult(string text);

        void SetArenaTips(string text);

        void SetTips(string text);

        void SetSelfAvatar(string url);

        void SetOpponentAvatar(string url);

        void SetSelfName(string text);

        void SetOpponentName(string text);

        void SetSelfState(string text, Color color);

        void SetOpponentState(string text, Color color);

        void SetSelfGesture(string text);

        void SetOpponentGesture(string text);

        void SetStartEnabled(bool enabled);

        void SetSyncEnabled(bool enabled);

        void SetGestureButtonsEnabled(bool enabled);

        void RegisterHandlers(
            Action onInvite,
            Action onLeave,
            Action onClose,
            Action onStart,
            Action onSync,
            Action<RockPaperScissorsGesture> onGesture);

        void ClearHandlers();
    }
}
