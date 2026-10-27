#if ENABLE_UI_FAIRYGUI
using System;
using FairyGUI;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIConnectFourBattlePanel : IConnectFourBattlePanelBinder
    {
        private static readonly Color EmptyCellColor = new Color32(46, 56, 92, 255);
        private static readonly Color PlayerOneCellColor = new Color32(219, 64, 54, 255);
        private static readonly Color PlayerTwoCellColor = new Color32(242, 191, 38, 255);

        private GGraph[] _cells;
        private GButton[] _columnButtons;

        void IConnectFourBattlePanelBinder.SetRoomTitle(string text)
        {
            m_room_title_text.text = text;
        }

        void IConnectFourBattlePanelBinder.SetRoomState(string text)
        {
            m_room_state_text.text = text;
        }

        void IConnectFourBattlePanelBinder.SetTurnText(string text)
        {
            m_turn_text.text = text;
        }

        void IConnectFourBattlePanelBinder.SetResultText(string text)
        {
            m_result_text.text = text;
        }

        void IConnectFourBattlePanelBinder.SetCell(int displayIndex, ConnectFourCell cell)
        {
            EnsureCellCache();
            if (_cells == null || displayIndex < 0 || displayIndex >= _cells.Length || _cells[displayIndex] == null)
            {
                return;
            }

            _cells[displayIndex].color = GetCellColor(cell);
        }

        void IConnectFourBattlePanelBinder.SetColumnButtonsEnabled(bool enabled)
        {
            EnsureColumnButtonCache();
            if (_columnButtons == null)
            {
                return;
            }

            foreach (var button in _columnButtons)
            {
                SetButtonEnabled(button, enabled);
            }
        }

        void IConnectFourBattlePanelBinder.SetStartEnabled(bool enabled)
        {
            SetButtonEnabled(m_start_button.self, enabled);
        }

        void IConnectFourBattlePanelBinder.SetRestartEnabled(bool enabled)
        {
            SetButtonEnabled(m_restart_button.self, enabled);
        }

        void IConnectFourBattlePanelBinder.RegisterHandlers(
            Action onLeave,
            Action onClose,
            Action onStart,
            Action onRestart,
            Action<int> onColumn)
        {
            EnsureColumnButtonCache();
            m_leave_button.self.onClick.Set(() => onLeave());
            m_close_button.self.onClick.Set(() => onClose());
            m_start_button.self.onClick.Set(() => onStart());
            m_restart_button.self.onClick.Set(() => onRestart());
            if (_columnButtons == null)
            {
                return;
            }

            for (var column = 0; column < _columnButtons.Length; column++)
            {
                var button = _columnButtons[column];
                if (button == null)
                {
                    continue;
                }

                var capturedColumn = column;
                button.onClick.Set(() => onColumn(capturedColumn));
            }
        }

        void IConnectFourBattlePanelBinder.ClearHandlers()
        {
            EnsureColumnButtonCache();
            m_leave_button.self.onClick.Clear();
            m_close_button.self.onClick.Clear();
            m_start_button.self.onClick.Clear();
            m_restart_button.self.onClick.Clear();
            if (_columnButtons == null)
            {
                return;
            }

            foreach (var button in _columnButtons)
            {
                if (button != null)
                {
                    button.onClick.Clear();
                }
            }
        }

        private void EnsureCellCache()
        {
            if (_cells != null)
            {
                return;
            }

            var cells = new GGraph[42];
            var complete = true;
            for (var displayIndex = 0; displayIndex < cells.Length; displayIndex++)
            {
                cells[displayIndex] = (GGraph)GObject.asCom.GetChild("cell_" + displayIndex);
                if (cells[displayIndex] == null)
                {
                    complete = false;
                }
            }

            // 存在缺失节点时不缓存，下次刷新重试——避免 FUI 包异步预加载时序下缓存出幽灵空棋盘
            if (complete)
            {
                _cells = cells;
            }
        }

        private void EnsureColumnButtonCache()
        {
            if (_columnButtons != null)
            {
                return;
            }

            var buttons = new GButton[7];
            var complete = true;
            for (var column = 0; column < buttons.Length; column++)
            {
                buttons[column] = (GButton)GObject.asCom.GetChild("column_button_" + column);
                if (buttons[column] == null)
                {
                    complete = false;
                }
            }

            if (complete)
            {
                _columnButtons = buttons;
            }
        }

        private static Color GetCellColor(ConnectFourCell cell)
        {
            switch (cell)
            {
                case ConnectFourCell.PlayerOne:
                    return PlayerOneCellColor;
                case ConnectFourCell.PlayerTwo:
                    return PlayerTwoCellColor;
                default:
                    return EmptyCellColor;
            }
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
    }
}
#endif
