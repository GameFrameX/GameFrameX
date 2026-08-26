#if ENABLE_UI_FAIRYGUI
using System;
using System.Collections.Generic;
using FairyGUI;
using Hotfix.Manager;
using Hotfix.Proto;

namespace Hotfix.UI
{
    public sealed partial class UIRoomListPanel : IRoomListPanelBinder
    {
        private List<object> _roomItems;
        private Action<long> _joinRoomHandler;

        void IRoomListPanelBinder.SetSummary(string text)
        {
            m_summary_text.text = text;
        }

        void IRoomListPanelBinder.SetTips(string text)
        {
            m_tips_text.text = text;
        }

        void IRoomListPanelBinder.SetRoomList(IReadOnlyList<RoomInfo> rooms, long selectedRoomId)
        {
            EnsureRoomItems();
            _roomItems.Clear();
            foreach (var room in rooms)
            {
                _roomItems.Add(room);
            }

            m_room_list.DataList = _roomItems;
        }

        void IRoomListPanelBinder.RegisterHandlers(Action onRefresh, Action onCreate, Action onClose, Action<long> onJoinRoom)
        {
            _joinRoomHandler = onJoinRoom;
            m_room_list.itemRenderer = RenderRoomItem;
            m_room_list.onClickItem.Set(OnRoomItemClick);
            m_refresh_button.self.onClick.Set(() => onRefresh());
            m_create_button.self.onClick.Set(() => onCreate());
            m_close_button.self.onClick.Set(() => onClose());
            m_mask.onClick.Set(() => onClose());
        }

        void IRoomListPanelBinder.ClearHandlers()
        {
            _joinRoomHandler = null;
            m_refresh_button.self.onClick.Clear();
            m_create_button.self.onClick.Clear();
            m_close_button.self.onClick.Clear();
            m_mask.onClick.Clear();
            m_room_list.onClickItem.Clear();
        }

        private void EnsureRoomItems()
        {
            if (_roomItems == null)
            {
                _roomItems = new List<object>();
            }
        }

        private void RenderRoomItem(int index, GObject item)
        {
            EnsureRoomItems();
            var room = (RoomInfo)_roomItems[index];
            var uiItem = UIRoomListItem.GetFormPool(item);
            uiItem.m_room_name.text = $"#{room.RoomId} {RoomUIFormatter.GetRoomName(room)}";
            uiItem.m_room_status.text = $"{RoomUIFormatter.GetRoomStatusText(room.Status)} {room.PlayerCount}/{room.MaxPlayerCount}";
            uiItem.self.selected = false;
            uiItem.self.data = room;
            var joinEnabled = RoomUIFormatter.CanJoinRoom(room, RoomDataManager.Instance.CurrentRoom != null);
            uiItem.m_join_button.self.enabled = joinEnabled;
            uiItem.m_join_button.self.alpha = joinEnabled ? 1f : 0.45f;
            uiItem.m_join_button.self.onClick.Set(() => OnJoinRoomClick(room.RoomId));
        }

        private void OnRoomItemClick(EventContext context)
        {
            var uiItem = UIRoomListItem.GetFormPool((GObject)context.data);
            if (uiItem.self.data is RoomInfo)
            {
                uiItem.self.selected = true;
            }
        }

        private void OnJoinRoomClick(long roomId)
        {
            if (_joinRoomHandler != null)
            {
                _joinRoomHandler(roomId);
            }
        }
    }
}
#endif
