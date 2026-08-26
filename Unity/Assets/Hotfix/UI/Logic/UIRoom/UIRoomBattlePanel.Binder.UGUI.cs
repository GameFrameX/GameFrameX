#if ENABLE_UI_UGUI
using System;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIRoomBattlePanel : IRoomBattlePanelBinder
    {
        void IRoomBattlePanelBinder.SetRoomTitle(string text)
        {
            m_room_title_text.text = text;
        }

        void IRoomBattlePanelBinder.SetRoomState(string text)
        {
            m_room_state_text.text = text;
        }

        void IRoomBattlePanelBinder.SetInvite(string text)
        {
            m_invite_text.text = text;
        }

        void IRoomBattlePanelBinder.SetArenaResult(string text)
        {
            m_result_text.text = text;
        }

        void IRoomBattlePanelBinder.SetArenaTips(string text)
        {
            m_arena_tips_text.text = text;
        }

        void IRoomBattlePanelBinder.SetTips(string text)
        {
            m_tips_text.text = text;
        }

        void IRoomBattlePanelBinder.SetSelfAvatar(string avatarId)
        {
            SetAvatar(m_self_avatar, avatarId);
        }

        void IRoomBattlePanelBinder.SetOpponentAvatar(string avatarId)
        {
            SetAvatar(m_opponent_avatar, avatarId);
        }

        void IRoomBattlePanelBinder.SetSelfName(string text)
        {
            m_self_name_text.text = text;
        }

        void IRoomBattlePanelBinder.SetOpponentName(string text)
        {
            m_opponent_name_text.text = text;
        }

        void IRoomBattlePanelBinder.SetSelfState(string text, Color color)
        {
            m_self_state_text.text = text;
            m_self_state_text.color = color;
        }

        void IRoomBattlePanelBinder.SetOpponentState(string text, Color color)
        {
            m_opponent_state_text.text = text;
            m_opponent_state_text.color = color;
        }

        void IRoomBattlePanelBinder.SetSelfGesture(string text)
        {
            m_self_gesture_text.text = text;
        }

        void IRoomBattlePanelBinder.SetOpponentGesture(string text)
        {
            m_opponent_gesture_text.text = text;
        }

        void IRoomBattlePanelBinder.SetStartEnabled(bool enabled)
        {
            SetButtonEnabled(m_start_button, enabled);
        }

        void IRoomBattlePanelBinder.SetSyncEnabled(bool enabled)
        {
            SetButtonEnabled(m_sync_button, enabled);
        }

        void IRoomBattlePanelBinder.SetGestureButtonsEnabled(bool enabled)
        {
            SetButtonEnabled(m_rock_button, enabled);
            SetButtonEnabled(m_scissors_button, enabled);
            SetButtonEnabled(m_paper_button, enabled);
        }

        void IRoomBattlePanelBinder.RegisterHandlers(
            Action onInvite,
            Action onLeave,
            Action onClose,
            Action onStart,
            Action onSync,
            Action<RockPaperScissorsGesture> onGesture)
        {
            m_invite_button.onClick.AddListener(() => onInvite());
            m_leave_button.onClick.AddListener(() => onLeave());
            m_close_button.onClick.AddListener(() => onClose());
            m_start_button.onClick.AddListener(() => onStart());
            m_sync_button.onClick.AddListener(() => onSync());
            m_rock_button.onClick.AddListener(() => onGesture(RockPaperScissorsGesture.Rock));
            m_scissors_button.onClick.AddListener(() => onGesture(RockPaperScissorsGesture.Scissors));
            m_paper_button.onClick.AddListener(() => onGesture(RockPaperScissorsGesture.Paper));

            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.AddListener(() => onClose());
            }
        }

        void IRoomBattlePanelBinder.ClearHandlers()
        {
            m_invite_button.onClick.RemoveAllListeners();
            m_leave_button.onClick.RemoveAllListeners();
            m_close_button.onClick.RemoveAllListeners();
            m_start_button.onClick.RemoveAllListeners();
            m_sync_button.onClick.RemoveAllListeners();
            m_rock_button.onClick.RemoveAllListeners();
            m_scissors_button.onClick.RemoveAllListeners();
            m_paper_button.onClick.RemoveAllListeners();

            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.RemoveAllListeners();
            }
        }

        private static void SetAvatar(GameFrameX.UI.UGUI.Runtime.UIImage avatar, string avatarId)
        {
            if (avatar == null || string.IsNullOrEmpty(avatarId))
            {
                return;
            }

            avatar.icon = GameFrameX.Runtime.Utility.Asset.Path.GetCategoryFilePath("Sprites", "avatar/" + avatarId);
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
