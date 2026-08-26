#if ENABLE_UI_UGUI
using System;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIRoomListItem : IRoomListItemBinder
    {
        void IRoomListItemBinder.SetRoomName(string text)
        {
            m_room_name_text.text = text;
        }

        void IRoomListItemBinder.SetRoomStatus(string text)
        {
            m_room_status_text.text = text;
        }

        void IRoomListItemBinder.SetSelected(bool selected)
        {
            m_bg.color = selected
                ? new Color32(63, 92, 144, 255)
                : new Color32(42, 47, 58, 255);
        }

        void IRoomListItemBinder.SetJoinEnabled(bool enabled)
        {
            SetButtonEnabled(m_join_button, enabled);
        }

        void IRoomListItemBinder.RegisterJoin(Action onJoin)
        {
            m_join_button.onClick.AddListener(() => onJoin());
        }

        void IRoomListItemBinder.ClearHandlers()
        {
            m_join_button.onClick.RemoveAllListeners();
        }

        private static void SetButtonEnabled(UnityEngine.UI.Button button, bool enabled)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = enabled;
            var canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = enabled ? 1f : 0.45f;
        }
    }
}
#endif
