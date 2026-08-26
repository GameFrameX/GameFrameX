using System;
using System.Collections.Generic;
using Hotfix.Proto;

namespace Hotfix.UI
{
    /// <summary>
    /// UIRoomListPanel 的视图绑定接口，由 FairyGUI / UGUI 各自实现。
    /// View binder interface for UIRoomListPanel, implemented by FairyGUI / UGUI separately.
    /// </summary>
    public interface IRoomListPanelBinder
    {
        void SetSummary(string text);

        void SetTips(string text);

        void SetRoomList(IReadOnlyList<RoomInfo> rooms, long selectedRoomId);

        void RegisterHandlers(Action onRefresh, Action onCreate, Action onClose, Action<long> onJoinRoom);

        void ClearHandlers();
    }
}
