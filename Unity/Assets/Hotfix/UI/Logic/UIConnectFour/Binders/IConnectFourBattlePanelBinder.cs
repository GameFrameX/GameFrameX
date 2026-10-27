using System;
using Hotfix.Proto;

namespace Hotfix.UI
{
    /// <summary>
    /// UIConnectFourBattlePanel 的视图绑定接口。
    /// View binder interface for UIConnectFourBattlePanel.
    /// </summary>
    public interface IConnectFourBattlePanelBinder
    {
        void SetRoomTitle(string text);

        void SetRoomState(string text);

        void SetTurnText(string text);

        void SetResultText(string text);

        /// <summary>
        /// 刷新棋盘格子。displayIndex 为显示索引（0-41，第 0 行为顶行），cell 为棋子状态。
        /// </summary>
        void SetCell(int displayIndex, ConnectFourCell cell);

        void SetColumnButtonsEnabled(bool enabled);

        void SetStartEnabled(bool enabled);

        void SetRestartEnabled(bool enabled);

        void RegisterHandlers(
            Action onLeave,
            Action onClose,
            Action onStart,
            Action onRestart,
            Action<int> onColumn);

        void ClearHandlers();
    }
}
