#if ENABLE_UI_UGUI
using System;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIRoomResultPopup : IRoomResultPopupBinder
    {
        void IRoomResultPopupBinder.SetResult(string text)
        {
            m_result_text.text = text;
        }

        void IRoomResultPopupBinder.SetRound(string text)
        {
            m_round_text.text = text;
        }

        void IRoomResultPopupBinder.SetSelfName(string text)
        {
            m_self_name_text.text = text;
        }

        void IRoomResultPopupBinder.SetOpponentName(string text)
        {
            m_opponent_name_text.text = text;
        }

        void IRoomResultPopupBinder.SetSelfGesture(string text)
        {
            m_self_gesture_text.text = text;
        }

        void IRoomResultPopupBinder.SetOpponentGesture(string text)
        {
            m_opponent_gesture_text.text = text;
        }

        void IRoomResultPopupBinder.SetRestartEnabled(bool enabled)
        {
            SetResultButtonEnabled(m_restart_button, enabled);
        }

        void IRoomResultPopupBinder.RegisterHandlers(Action onClose, Action onRestart)
        {
            m_close_button.onClick.AddListener(() => onClose());
            m_mask.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => onClose());
            m_restart_button.onClick.AddListener(() => onRestart());
        }

        void IRoomResultPopupBinder.ClearHandlers()
        {
            m_close_button.onClick.RemoveAllListeners();
            m_restart_button.onClick.RemoveAllListeners();
            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.RemoveAllListeners();
            }
        }

        private static void SetResultButtonEnabled(UnityEngine.UI.Button button, bool enabled)
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
