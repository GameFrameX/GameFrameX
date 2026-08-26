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
            _binder.RegisterHandlers(CloseSelf, () => RestartAndClose().Forget());
            Refresh();
        }

        public void OnClose()
        {
            GameApp.Event.CheckUnsubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            GameApp.Event.CheckUnsubscribe(RockPaperScissorsGameChangedEventArgs.EventId, OnGameChanged);
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

        private void Refresh()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            var game = RockPaperScissorsDataManager.Instance.CurrentGame;
            var currentRoleId = RoomDataManager.Instance.CurrentRoleId;

            _binder.SetResult(RockPaperScissorsDataManager.Instance.GetResultText());
            _binder.SetRound(room == null ? "第 0 局" : $"第 {room.Round} 局");

            var selfPlayer = room?.Players.FirstOrDefault(m => m.RoleId == currentRoleId);
            var opponentPlayer = room?.Players.FirstOrDefault(m => m.RoleId != currentRoleId);
            var selfGesture = game?.Players.FirstOrDefault(m => m.RoleId == currentRoleId);
            var opponentGesture = game?.Players.FirstOrDefault(m => m.RoleId != currentRoleId);
            var hasRevealedResult = RockPaperScissorsDataManager.Instance.HasRevealedResult();

            _binder.SetSelfName(selfPlayer == null ? "我" : RoomUIFormatter.GetPlayerName(selfPlayer, true));
            _binder.SetOpponentName(opponentPlayer == null ? "对手" : RoomUIFormatter.GetPlayerName(opponentPlayer, false));
            _binder.SetSelfGesture(hasRevealedResult ? RoomUIFormatter.GetGestureText(selfGesture == null ? RockPaperScissorsGesture.None : selfGesture.Gesture) : "等待结算");
            _binder.SetOpponentGesture(hasRevealedResult ? RoomUIFormatter.GetGestureText(opponentGesture == null ? RockPaperScissorsGesture.None : opponentGesture.Gesture) : "等待结算");
            _binder.SetRestartEnabled(hasRevealedResult && RockPaperScissorsDataManager.Instance.CanRestartGame());
        }

        private async UniTask RestartAndClose()
        {
            if (!RockPaperScissorsDataManager.Instance.CanRestartGame())
            {
                return;
            }

            await RockPaperScissorsDataManager.Instance.RestartGame();
            CloseSelf();
        }

        private void CloseSelf()
        {
            GameApp.UI.CloseUIForm<UIRoomResultPopup>();
        }
    }
}
