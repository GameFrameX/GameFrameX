using System;

namespace Hotfix.UI
{
    /// <summary>
    /// UIRoomListItem 的视图绑定接口。
    /// View binder interface for UIRoomListItem.
    /// </summary>
    public interface IRoomListItemBinder
    {
        void SetRoomName(string text);

        void SetRoomStatus(string text);

        void SetSelected(bool selected);

        void SetJoinEnabled(bool enabled);

        void RegisterJoin(Action onJoin);

        void ClearHandlers();
    }
}
