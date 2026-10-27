#if ENABLE_UI_UGUI
using System;
using GameFrameX.Runtime;
using Hotfix.Manager;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIConnectFourBattlePanel : IConnectFourBattlePanelBinder
    {
        private static readonly Color EmptyCellColor = new Color32(46, 56, 92, 255);
        private static readonly Color PlayerOneCellColor = new Color32(219, 64, 54, 255);
        private static readonly Color PlayerTwoCellColor = new Color32(242, 191, 38, 255);

        private readonly UnityEngine.UI.Image[] _cellImages = new UnityEngine.UI.Image[ConnectFourDataManager.ColumnCount * ConnectFourDataManager.RowCount];
        private readonly UnityEngine.UI.Button[] _columnButtons = new UnityEngine.UI.Button[ConnectFourDataManager.ColumnCount];

        void IConnectFourBattlePanelBinder.SetRoomTitle(string text)
        {
            m_title.text = text;
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
            var image = GetCellImage(displayIndex);
            if (image == null)
            {
                return;
            }

            image.color = GetCellColor(cell);
        }

        void IConnectFourBattlePanelBinder.SetColumnButtonsEnabled(bool enabled)
        {
            foreach (var button in GetColumnButtons())
            {
                SetButtonEnabled(button, enabled);
            }
        }

        void IConnectFourBattlePanelBinder.SetStartEnabled(bool enabled)
        {
            SetButtonEnabled(m_start_button, enabled);
        }

        void IConnectFourBattlePanelBinder.SetRestartEnabled(bool enabled)
        {
            SetButtonEnabled(m_restart_button, enabled);
        }

        void IConnectFourBattlePanelBinder.RegisterHandlers(
            Action onLeave,
            Action onClose,
            Action onStart,
            Action onRestart,
            Action<int> onColumn)
        {
            m_leave_button.onClick.AddListener(() => onLeave());
            m_close_button.onClick.AddListener(() => onClose());
            m_start_button.onClick.AddListener(() => onStart());
            m_restart_button.onClick.AddListener(() => onRestart());

            var buttons = GetColumnButtons();
            for (var column = 0; column < buttons.Length && column < ConnectFourDataManager.ColumnCount; column++)
            {
                var capturedColumn = column;
                var button = buttons[column];
                if (button == null)
                {
                    continue;
                }

                button.onClick.AddListener(() => onColumn(capturedColumn));
            }

            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.AddListener(() => onClose());
            }
        }

        void IConnectFourBattlePanelBinder.ClearHandlers()
        {
            m_leave_button.onClick.RemoveAllListeners();
            m_close_button.onClick.RemoveAllListeners();
            m_start_button.onClick.RemoveAllListeners();
            m_restart_button.onClick.RemoveAllListeners();

            foreach (var button in GetColumnButtons())
            {
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
            }

            var maskButton = m_mask.GetComponent<UnityEngine.UI.Button>();
            if (maskButton != null)
            {
                maskButton.onClick.RemoveAllListeners();
            }
        }

        private UnityEngine.UI.Image GetCellImage(int displayIndex)
        {
            if (displayIndex < 0 || displayIndex >= _cellImages.Length)
            {
                return null;
            }

            var image = _cellImages[displayIndex];
            if (image == null)
            {
                image = gameObject.transform.FindChildName("cell_" + displayIndex).GetComponent<UnityEngine.UI.Image>();
                _cellImages[displayIndex] = image;
            }

            return image;
        }

        private UnityEngine.UI.Button[] GetColumnButtons()
        {
            var button = _columnButtons[0];
            if (button == null)
            {
                for (var column = 0; column < _columnButtons.Length; column++)
                {
                    _columnButtons[column] = gameObject.transform.FindChildName("column_button_" + column).GetComponent<UnityEngine.UI.Button>();
                }
            }

            return _columnButtons;
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
