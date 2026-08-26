#if ENABLE_UI_FAIRYGUI
using System;
using FairyGUI;
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
            m_self_avatar.icon = ResolveAvatarUrl(avatarId);
        }

        void IRoomBattlePanelBinder.SetOpponentAvatar(string avatarId)
        {
            m_opponent_avatar.icon = ResolveAvatarUrl(avatarId);
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
            SetButtonEnabled(m_start_button.self, enabled);
        }

        void IRoomBattlePanelBinder.SetSyncEnabled(bool enabled)
        {
            SetButtonEnabled(m_sync_button.self, enabled);
        }

        void IRoomBattlePanelBinder.SetGestureButtonsEnabled(bool enabled)
        {
            SetButtonEnabled(m_rock_button.self, enabled);
            SetButtonEnabled(m_scissors_button.self, enabled);
            SetButtonEnabled(m_paper_button.self, enabled);
        }

        void IRoomBattlePanelBinder.RegisterHandlers(
            Action onInvite,
            Action onLeave,
            Action onClose,
            Action onStart,
            Action onSync,
            Action<RockPaperScissorsGesture> onGesture)
        {
            SetupGestureButtons();
            m_invite_button.self.onClick.Set(() => onInvite());
            m_leave_button.self.onClick.Set(() => onLeave());
            m_close_button.self.onClick.Set(() => onClose());
            m_start_button.self.onClick.Set(() => onStart());
            m_sync_button.self.onClick.Set(() => onSync());
            m_rock_button.self.onClick.Set(() => onGesture(RockPaperScissorsGesture.Rock));
            m_scissors_button.self.onClick.Set(() => onGesture(RockPaperScissorsGesture.Scissors));
            m_paper_button.self.onClick.Set(() => onGesture(RockPaperScissorsGesture.Paper));
        }

        void IRoomBattlePanelBinder.ClearHandlers()
        {
            m_invite_button.self.onClick.Clear();
            m_leave_button.self.onClick.Clear();
            m_close_button.self.onClick.Clear();
            m_start_button.self.onClick.Clear();
            m_sync_button.self.onClick.Clear();
            m_rock_button.self.onClick.Clear();
            m_scissors_button.self.onClick.Clear();
            m_paper_button.self.onClick.Clear();
        }

        private static string ResolveAvatarUrl(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return string.Empty;
            }

            return UIPackage.GetItemURL(FUIPackage.UICommonAvatar, avatarId);
        }

        private static void SetButtonEnabled(GButton button, bool enabled)
        {
            if (button == null)
            {
                return;
            }

            button.enabled = enabled;
            button.alpha = enabled ? 1f : 0.45f;
        }

        private void SetupGestureButtons()
        {
            SetGestureButton(m_rock_button, 0, "石头");
            SetGestureButton(m_scissors_button, 1, "剪刀");
            SetGestureButton(m_paper_button, 2, "布");
        }

        private static void SetGestureButton(UIGestureButton button, int gestureIndex, string title)
        {
            if (button == null)
            {
                return;
            }

            button.m_Gesture.selectedIndex = gestureIndex;
            button.self.title = title;
        }
    }
}
#endif
