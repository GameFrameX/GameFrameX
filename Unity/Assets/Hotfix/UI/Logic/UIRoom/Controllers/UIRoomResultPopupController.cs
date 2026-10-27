using System;
using System.Linq;
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
    /// UIRoomResultPopup 的共享控制器。
    /// Shared controller for UIRoomResultPopup.
    /// </summary>
    internal sealed class UIRoomResultPopupController
    {
        private readonly IRoomResultPopupBinder _binder;

        public UIRoomResultPopupController(IRoomResultPopupBinder binder)
        {
            _binder = binder;
        }

        public void OnOpen()
        {
            GameApp.Event.CheckSubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            GameApp.Event.CheckSubscribe(RockPaperScissorsGameChangedEventArgs.EventId, OnGameChanged);
            GameApp.Event.CheckSubscribe(ConnectFourGameChangedEventArgs.EventId, OnConnectFourGameChanged);
            _binder.RegisterHandlers(CloseSelf, () => RestartAndClose().Forget());
            Refresh();
        }

        public void OnClose()
        {
            GameApp.Event.CheckUnsubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            GameApp.Event.CheckUnsubscribe(RockPaperScissorsGameChangedEventArgs.EventId, OnGameChanged);
            GameApp.Event.CheckUnsubscribe(ConnectFourGameChangedEventArgs.EventId, OnConnectFourGameChanged);
            _binder.ClearHandlers();
        }


        private void OnRoomChanged(object sender, GameEventArgs e)
        {
            Refresh();
        }

        private void OnGameChanged(object sender, GameEventArgs e)
        {
            Refresh();
        }

        private void OnConnectFourGameChanged(object sender, GameEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room != null && room.GameType == GameType.ConnectFour)
            {
                RefreshConnectFour(room);
                return;
            }

            var game = RockPaperScissorsDataManager.Instance.CurrentGame;
            var currentRoleId = RoomDataManager.Instance.CurrentRoleId;

            _binder.SetResult(RockPaperScissorsDataManager.Instance.GetResultText());
            _binder.SetRound(room == null ? "第 0 局" : $"第 {room.Round} 局");

            var selfPlayer = room?.Players.FirstOrDefault(m => m.PlayerId == currentRoleId);
            var opponentPlayer = room?.Players.FirstOrDefault(m => m.PlayerId != currentRoleId);
            var selfGesture = game?.Players.FirstOrDefault(m => m.PlayerId == currentRoleId);
            var opponentGesture = game?.Players.FirstOrDefault(m => m.PlayerId != currentRoleId);
            var hasRevealedResult = RockPaperScissorsDataManager.Instance.HasRevealedResult();

            _binder.SetSelfName(selfPlayer == null ? "我" : RoomUIFormatter.GetPlayerName(selfPlayer, true));
            _binder.SetOpponentName(opponentPlayer == null ? "对手" : RoomUIFormatter.GetPlayerName(opponentPlayer, false));
            _binder.SetSelfGesture(hasRevealedResult ? RoomUIFormatter.GetGestureText(selfGesture == null ? RockPaperScissorsGesture.None : selfGesture.Gesture) : "等待结算");
            _binder.SetOpponentGesture(hasRevealedResult ? RoomUIFormatter.GetGestureText(opponentGesture == null ? RockPaperScissorsGesture.None : opponentGesture.Gesture) : "等待结算");
            _binder.SetRestartEnabled(hasRevealedResult && RockPaperScissorsDataManager.Instance.CanRestartGame());
        }

        /// <summary>
        /// 四子棋结算展示：复用弹窗，仅替换数据来源与文案。
        /// </summary>
        private void RefreshConnectFour(RoomInfo room)
        {
            var currentRoleId = RoomDataManager.Instance.CurrentRoleId;
            var game = ConnectFourDataManager.Instance.CurrentGame;
            var hasRevealedResult = ConnectFourDataManager.Instance.HasRevealedResult();

            _binder.SetResult(ConnectFourDataManager.Instance.GetResultText());
            _binder.SetRound($"第 {room.Round} 局");

            var selfPlayer = room.Players.FirstOrDefault(m => m.PlayerId == currentRoleId);
            var opponentPlayer = room.Players.FirstOrDefault(m => m.PlayerId != currentRoleId);

            _binder.SetSelfName(selfPlayer == null ? "我" : RoomUIFormatter.GetPlayerName(selfPlayer, true));
            _binder.SetOpponentName(opponentPlayer == null ? "对手" : RoomUIFormatter.GetPlayerName(opponentPlayer, false));
            _binder.SetSelfGesture(hasRevealedResult ? GetConnectFourSeatText(game == null ? null : game.Players.FirstOrDefault(m => m.PlayerId == currentRoleId)) : "等待结算");
            _binder.SetOpponentGesture(hasRevealedResult ? GetConnectFourSeatText(game == null ? null : game.Players.FirstOrDefault(m => m.PlayerId != currentRoleId)) : "等待结算");
            _binder.SetRestartEnabled(ConnectFourDataManager.Instance.CanRestartGame());
        }

        private async UniTask RestartAndClose()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room != null && room.GameType == GameType.ConnectFour)
            {
                if (!ConnectFourDataManager.Instance.CanRestartGame())
                {
                    return;
                }

                await ConnectFourDataManager.Instance.RestartGame();
                CloseSelf();
                return;
            }

            if (!RockPaperScissorsDataManager.Instance.CanRestartGame())
            {
                return;
            }

            await RockPaperScissorsDataManager.Instance.RestartGame();
            CloseSelf();
        }

        private static string GetConnectFourSeatText(ConnectFourPlayerInfo player)
        {
            if (player == null)
            {
                return "-";
            }

            return player.SeatIndex == 1 ? "红方" : "黄方";
        }

        private void CloseSelf()
        {
            GameApp.UI.CloseUIForm<UIRoomResultPopup>();
        }
    }
}
