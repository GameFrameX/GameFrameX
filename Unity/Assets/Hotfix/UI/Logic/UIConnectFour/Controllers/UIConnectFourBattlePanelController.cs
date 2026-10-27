using System;
using Cysharp.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Hotfix.Events;
using Hotfix.Manager;
using Hotfix.Proto;

namespace Hotfix.UI
{
    /// <summary>
    /// UIConnectFourBattlePanel 的共享控制器。
    /// Shared controller for UIConnectFourBattlePanel.
    /// </summary>
    internal sealed class UIConnectFourBattlePanelController
    {
        private readonly IConnectFourBattlePanelBinder _binder;
        private readonly Func<bool> _isDisposedGetter;

        private string _shownResultKey;
        private bool _resultPopupOpening;

        public UIConnectFourBattlePanelController(IConnectFourBattlePanelBinder binder, Func<bool> isDisposedGetter)
        {
            _binder = binder;
            _isDisposedGetter = isDisposedGetter;
        }
        private bool _moveInFlight;


        public void OnOpen()
        {
            GameApp.Event.CheckSubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            GameApp.Event.CheckSubscribe(ConnectFourGameChangedEventArgs.EventId, OnGameChanged);

            _binder.RegisterHandlers(
                () => LeaveRoomAndReturnList().Forget(),
                CloseSelf,
                () => RoomDataManager.Instance.StartGame().Forget(),
                () => ConnectFourDataManager.Instance.RestartGame().Forget(),
                column => SubmitMove(column));

            if (RoomDataManager.Instance.CurrentRoom == null)
            {
                ReturnList().Forget();
                return;
            }

            Refresh();
            SyncGameInfoIfNeeded().Forget();
        }

        public void OnClose()
        {
            GameApp.Event.CheckUnsubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            GameApp.Event.CheckUnsubscribe(ConnectFourGameChangedEventArgs.EventId, OnGameChanged);
            _binder.ClearHandlers();
        }

        private void OnRoomChanged(object sender, GameEventArgs e)
        {
            _moveInFlight = false;
            Refresh();
            TryOpenResultPopup().Forget();
        }

        private void OnGameChanged(object sender, GameEventArgs e)
        {
            _moveInFlight = false;
            Refresh();
            TryOpenResultPopup().Forget();
        }

        /// <summary>
        /// 提交落子并悲观锁定列按钮：请求往返窗口内禁止重复提交，收到任一玩法数据变化后解锁。
        /// </summary>
        private void SubmitMove(int column)
        {
            if (!ConnectFourDataManager.Instance.CanSubmitMove() || _moveInFlight)
            {
                return;
            }

            _moveInFlight = true;
            _binder.SetColumnButtonsEnabled(false);
            ConnectFourDataManager.Instance.SubmitMove(column).Forget();
        }

        private void Refresh()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return;
            }

            var game = ConnectFourDataManager.Instance.CurrentGame;

            _binder.SetRoomTitle($"房间 #{room.RoomId}  {RoomUIFormatter.GetRoomName(room)}");
            _binder.SetRoomState($"{RoomUIFormatter.GetRoomStatusText(room.Status)}  {room.PlayerCount}/{room.MaxPlayerCount}  局数:{room.Round}  房主:{room.OwnerPlayerId}");
            _binder.SetTurnText(GetTurnText(room));
            _binder.SetResultText(GetResultText(room));
            RefreshBoard(game);

            _binder.SetColumnButtonsEnabled(ConnectFourDataManager.Instance.CanSubmitMove());
            _binder.SetStartEnabled(RoomDataManager.Instance.CanStartGame());
            _binder.SetRestartEnabled(ConnectFourDataManager.Instance.CanRestartGame());

            if (room.Status == RoomStatus.Playing)
            {
                _shownResultKey = null;
            }
        }

        /// <summary>
        /// 刷新棋盘显示。显示行第 0 行为顶行，数据行第 0 行为底行，因此需要翻转行序。
        /// </summary>
        private void RefreshBoard(ConnectFourGameInfo game)
        {
            for (var displayRow = 0; displayRow < ConnectFourDataManager.RowCount; displayRow++)
            {
                var dataRow = ConnectFourDataManager.RowCount - 1 - displayRow;
                for (var column = 0; column < ConnectFourDataManager.ColumnCount; column++)
                {
                    var cell = game == null ? ConnectFourCell.None : ConnectFourDataManager.Instance.GetCell(dataRow, column);
                    _binder.SetCell(displayRow * ConnectFourDataManager.ColumnCount + column, cell);
                }
            }
        }

        private async UniTask SyncGameInfoIfNeeded()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null || !RoomUIFormatter.IsGameDataStatus(room.Status))
            {
                return;
            }

            await ConnectFourDataManager.Instance.SynchronizeCurrentGameInfo();
        }

        private async UniTask TryOpenResultPopup()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null
                || room.Status != RoomStatus.Settled
                || !ConnectFourDataManager.Instance.HasRevealedResult())
            {
                return;
            }

            var key = room.RoomId + ":" + room.Round;
            if (_resultPopupOpening || _shownResultKey == key)
            {
                return;
            }

            _resultPopupOpening = true;
            try
            {
                _shownResultKey = key;
                await GameApp.UI.OpenAsync<UIRoomResultPopup>(UIGroupConstants.Window);
            }
            finally
            {
                _resultPopupOpening = false;
            }
        }

        private async UniTask LeaveRoomAndReturnList()
        {
            await RoomDataManager.Instance.LeaveRoom();
            await ReturnList();
        }

        private async UniTask ReturnList()
        {
            GameApp.UI.CloseUIForm<UIConnectFourBattlePanel>();
            await GameApp.UI.OpenAsync<UIRoomListPanel>(UIGroupConstants.Window);
        }

        private void CloseSelf()
        {
            GameApp.UI.CloseUIForm<UIConnectFourBattlePanel>();
        }

        private static string GetTurnText(RoomInfo room)
        {
            if (room.Status != RoomStatus.Playing)
            {
                return "等待开局";
            }

            var game = ConnectFourDataManager.Instance.CurrentGame;
            if (game == null)
            {
                return "同步对局数据中";
            }

            var seatText = GetSeatName(ConnectFourDataManager.Instance.GetSelfSeatIndex());
            return ConnectFourDataManager.Instance.IsCurrentPlayerTurn()
                ? $"你的回合（{seatText}），点击列落子"
                : $"对手回合（{GetOpponentSeatName()}），请等待";
        }

        private static string GetResultText(RoomInfo room)
        {
            if (room.Status == RoomStatus.Settled || room.Status == RoomStatus.Settling)
            {
                return ConnectFourDataManager.Instance.GetResultText();
            }

            if (room.Status == RoomStatus.Playing)
            {
                return "对局中";
            }

            if (room.Status == RoomStatus.Ready)
            {
                return RoomDataManager.Instance.IsCurrentPlayerOwner() ? "可以开始游戏" : "等待房主开始";
            }

            return "等待玩家加入";
        }

        private static string GetSeatName(int seatIndex)
        {
            return seatIndex == 1 ? "红方" : "黄方";
        }

        private static string GetOpponentSeatName()
        {
            var opponent = ConnectFourDataManager.Instance.GetOpponentPlayer();
            return opponent == null ? "?" : GetSeatName(opponent.SeatIndex);
        }
    }
}
