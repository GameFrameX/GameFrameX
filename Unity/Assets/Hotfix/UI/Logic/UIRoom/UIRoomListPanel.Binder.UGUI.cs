#if ENABLE_UI_UGUI
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrameX.Runtime;
using Hotfix.Manager;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIRoomListPanel : IRoomListPanelBinder
    {
        private readonly List<UIRoomListItem> _itemInstances = new List<UIRoomListItem>();
        private GameObject _itemPrefab;
        private Action<long> _joinRoomHandler;

        private async UniTask EnsureItemPrefabLoaded()
        {
            if (_itemPrefab != null)
            {
                return;
            }

            var handle = await GameApp.Asset.LoadAssetAsync<GameObject>(Utility.Asset.Path.GetUIPath("UIRoom/UIRoomListItem"));
            _itemPrefab = handle.GetAssetObject<GameObject>();
        }

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
            ClearItemInstances();
            if (_itemPrefab == null || rooms == null)
            {
                return;
            }

            foreach (var room in rooms)
            {
                var go = Instantiate(_itemPrefab, m_room_list_content);
                go.SetActive(true);
                EnsureItemLayout(go);
                var item = UIRoomListItem.Create(go);
                var itemBinder = (IRoomListItemBinder)item;
                itemBinder.SetRoomName($"#{room.RoomId} {RoomUIFormatter.GetRoomName(room)}");
                itemBinder.SetRoomStatus($"{RoomUIFormatter.GetRoomStatusText(room.Status)} {room.PlayerCount}/{room.MaxPlayerCount}");
                itemBinder.SetSelected(room.RoomId == selectedRoomId);
                var joinEnabled = RoomUIFormatter.CanJoinRoom(room, RoomDataManager.Instance.CurrentRoom != null);
                itemBinder.SetJoinEnabled(joinEnabled);
                var roomId = room.RoomId;
                itemBinder.ClearHandlers();
                itemBinder.RegisterJoin(() => OnJoinRoomClick(roomId));
                _itemInstances.Add(item);
            }
        }

        void IRoomListPanelBinder.RegisterHandlers(Action onRefresh, Action onCreate, Action onClose, Action<long> onJoinRoom)
        {
            _joinRoomHandler = onJoinRoom;
            m_refresh_button.onClick.AddListener(() => onRefresh());
            m_create_button.onClick.AddListener(() => onCreate());
            m_close_button.onClick.AddListener(() => onClose());

            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.AddListener(() => onClose());
            }
        }

        void IRoomListPanelBinder.ClearHandlers()
        {
            _joinRoomHandler = null;
            m_refresh_button.onClick.RemoveAllListeners();
            m_create_button.onClick.RemoveAllListeners();
            m_close_button.onClick.RemoveAllListeners();
            ClearItemInstances();

            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.RemoveAllListeners();
            }
        }

        private void ClearItemInstances()
        {
            foreach (var item in _itemInstances)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }

            _itemInstances.Clear();
        }

        private static void EnsureItemLayout(GameObject go)
        {
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(520f, 92f);
                rectTransform.localScale = Vector3.one;
            }

            var layoutElement = go.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = go.AddComponent<UnityEngine.UI.LayoutElement>();
            }

            layoutElement.preferredWidth = 520f;
            layoutElement.preferredHeight = 92f;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
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
