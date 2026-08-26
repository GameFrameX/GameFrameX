#if ENABLE_UI_FAIRYGUI
using System;
using FairyGUI;
using Hotfix.Proto;

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
            SetResultButtonEnabled(m_restart_button.self, enabled);
        }

        void IRoomResultPopupBinder.RegisterHandlers(Action onClose, Action onRestart)
        {
            m_close_button.self.onClick.Set(() => onClose());
            m_mask.onClick.Set(() => onClose());
            m_restart_button.self.onClick.Set(() => onRestart());
        }

        void IRoomResultPopupBinder.ClearHandlers()
        {
            m_close_button.self.onClick.Clear();
            m_mask.onClick.Clear();
            m_restart_button.self.onClick.Clear();
        }

        private static void SetResultButtonEnabled(GButton button, bool enabled)
        {
            if (button == null)
            {
                return;
            }

            button.enabled = enabled;
            button.alpha = enabled ? 1f : 0.45f;
        }
    }
}
#endif
