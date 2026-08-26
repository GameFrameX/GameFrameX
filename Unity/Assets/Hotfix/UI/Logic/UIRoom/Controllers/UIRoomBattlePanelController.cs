using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Hotfix.Events;
using Hotfix.Manager;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UIRoomBattlePanel 的共享控制器。
    /// Shared controller for UIRoomBattlePanel.
    /// </summary>
    internal sealed class UIRoomBattlePanelController
    {
        private readonly IRoomBattlePanelBinder _binder;
        private readonly Func<bool> _isDisposedGetter;

        private string _shownResultKey;
        private bool _resultPopupOpening;

        public UIRoomBattlePanelController(IRoomBattlePanelBinder binder, Func<bool> isDisposedGetter)
        {
            _binder = binder;
            _isDisposedGetter = isDisposedGetter;
        }

        public void OnOpen()
        {
            GameApp.Event.CheckSubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            GameApp.Event.CheckSubscribe(RockPaperScissorsGameChangedEventArgs.EventId, OnGameChanged);

            _binder.RegisterHandlers(
                CopyInviteText,
                () => LeaveRoomAndReturnList().Forget(),
                CloseSelf,
                () => RoomDataManager.Instance.StartGame().Forget(),
                OnSyncGameInfoClick,
                gesture => RockPaperScissorsDataManager.Instance.SubmitGesture(gesture).Forget());

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
            GameApp.Event.CheckUnsubscribe(RockPaperScissorsGameChangedEventArgs.EventId, OnGameChanged);
            _binder.ClearHandlers();
        }

        private void OnRoomChanged(object sender, GameEventArgs e)
        {
            Refresh();
            TryOpenResultPopup().Forget();
        }

        private void OnGameChanged(object sender, GameEventArgs e)
        {
            Refresh();
            TryOpenResultPopup().Forget();
        }

        private void Refresh()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return;
            }

            var game = RockPaperScissorsDataManager.Instance.CurrentGame;
            var currentRoleId = RoomDataManager.Instance.CurrentRoleId;

            _binder.SetRoomTitle($"房间 #{room.RoomId}  {RoomUIFormatter.GetRoomName(room)}");
            _binder.SetRoomState($"{RoomUIFormatter.GetRoomStatusText(room.Status)}  {room.PlayerCount}/{room.MaxPlayerCount}  局数:{room.Round}  房主:{room.OwnerRoleId}");
            _binder.SetInvite(RoomDataManager.Instance.InviteText);
            _binder.SetArenaResult(GetArenaResultText(room));
            _binder.SetArenaTips(GetArenaTipsText(room));
            _binder.SetTips(GetTipsText(room));

            RefreshPlayer(room.Players.FirstOrDefault(m => m.RoleId == currentRoleId), true, room.Status, game);
            RefreshPlayer(room.Players.FirstOrDefault(m => m.RoleId != currentRoleId), false, room.Status, game);

            _binder.SetStartEnabled(RoomDataManager.Instance.CanStartGame());
            _binder.SetSyncEnabled(RoomUIFormatter.IsGameDataStatus(room.Status));
            _binder.SetGestureButtonsEnabled(RockPaperScissorsDataManager.Instance.CanSubmitGesture());

            if (room.Status == RoomStatus.Playing)
            {
                _shownResultKey = null;
            }
        }

        private void RefreshPlayer(RoomPlayerInfo player, bool isSelf, RoomStatus roomStatus, RockPaperScissorsGameInfo game)
        {
            if (player == null)
            {
                if (isSelf)
                {
                    _binder.SetSelfAvatar(string.Empty);
                    _binder.SetSelfName("我");
                    _binder.SetSelfState("空位", new Color32(174, 184, 199, 255));
                    _binder.SetSelfGesture("-");
                }
                else
                {
                    _binder.SetOpponentAvatar(string.Empty);
                    _binder.SetOpponentName("等待玩家");
                    _binder.SetOpponentState("空位", new Color32(174, 184, 199, 255));
                    _binder.SetOpponentGesture("-");
                }

                return;
            }

            var gesturePlayer = game == null ? null : game.Players.FirstOrDefault(m => m.RoleId == player.RoleId);
            var avatarUrl = player.Avatar > 0 ? player.Avatar.ToString() : string.Empty;
            var stateText = GetPlayerStateText(player, gesturePlayer, roomStatus);
            var stateColor = GetPlayerStateColor(player, gesturePlayer);
            var gestureText = RoomUIFormatter.GetGestureStateText(gesturePlayer, roomStatus);

            if (isSelf)
            {
                _binder.SetSelfAvatar(avatarUrl);
                _binder.SetSelfName(RoomUIFormatter.BuildPlayerName(player, true));
                _binder.SetSelfState(stateText, stateColor);
                _binder.SetSelfGesture(gestureText);
            }
            else
            {
                _binder.SetOpponentAvatar(avatarUrl);
                _binder.SetOpponentName(RoomUIFormatter.BuildPlayerName(player, false));
                _binder.SetOpponentState(stateText, stateColor);
                _binder.SetOpponentGesture(gestureText);
            }
        }

        private async UniTask SyncGameInfoIfNeeded()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null || !RoomUIFormatter.IsGameDataStatus(room.Status))
            {
                return;
            }

            await RockPaperScissorsDataManager.Instance.SynchronizeCurrentGameInfo();
        }

        private async UniTask TryOpenResultPopup()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            var game = RockPaperScissorsDataManager.Instance.CurrentGame;
            if (room == null
                || game == null
                || room.Status != RoomStatus.Settled
                || game.Round != room.Round
                || !RockPaperScissorsDataManager.Instance.HasRevealedResult())
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

        private void OnSyncGameInfoClick()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return;
            }

            RockPaperScissorsDataManager.Instance.RequestGameInfo(room.RoomId).Forget();
        }

        private async UniTask LeaveRoomAndReturnList()
        {
            await RoomDataManager.Instance.LeaveRoom();
            await ReturnList();
        }

        private async UniTask ReturnList()
        {
            GameApp.UI.CloseUIForm<UIRoomBattlePanel>();
            await GameApp.UI.OpenAsync<UIRoomListPanel>(UIGroupConstants.Window);
        }

        private void CloseSelf()
        {
            GameApp.UI.CloseUIForm<UIRoomBattlePanel>();
        }

        private static string GetPlayerStateText(RoomPlayerInfo player, RockPaperScissorsPlayerInfo gesturePlayer, RoomStatus roomStatus)
        {
            if (player.OnlineStatus == RoomPlayerOnlineStatus.Reconnecting)
            {
                return "重连中";
            }

            if (player.OnlineStatus == RoomPlayerOnlineStatus.Offline)
            {
                return "离线";
            }

            if (gesturePlayer != null && roomStatus == RoomStatus.Playing)
            {
                return gesturePlayer.HasGesture ? "已出拳" : "思考中";
            }

            switch (player.PlayerStatus)
            {
                case RoomPlayerStatus.Idle:
                    return "等待中";
                case RoomPlayerStatus.ReadyInRoom:
                    return "可开始";
                case RoomPlayerStatus.InGame:
                    return "游戏中";
                case RoomPlayerStatus.SettlingInRoom:
                    return "结算中";
                case RoomPlayerStatus.SettledInRoom:
                    return "已结算";
                default:
                    return "在线";
            }
        }

        private static Color GetPlayerStateColor(RoomPlayerInfo player, RockPaperScissorsPlayerInfo gesturePlayer)
        {
            if (player.OnlineStatus != RoomPlayerOnlineStatus.Online)
            {
                return new Color32(238, 198, 112, 255);
            }

            return gesturePlayer != null && gesturePlayer.HasGesture
                ? new Color32(126, 220, 160, 255)
                : new Color32(155, 216, 178, 255);
        }

        private static string GetArenaResultText(RoomInfo room)
        {
            if (room.Status == RoomStatus.Settled || room.Status == RoomStatus.Settling)
            {
                return RockPaperScissorsDataManager.Instance.GetResultText();
            }

            if (room.Status == RoomStatus.Playing)
            {
                return "请选择出拳";
            }

            if (room.Status == RoomStatus.Ready)
            {
                return RoomDataManager.Instance.IsCurrentPlayerOwner() ? "可以开始游戏" : "等待房主开始";
            }

            return "等待玩家加入";
        }

        private static string GetArenaTipsText(RoomInfo room)
        {
            if (room.Status == RoomStatus.Playing)
            {
                return RockPaperScissorsDataManager.Instance.HasCurrentPlayerGesture()
                    ? "你已出拳，等待对手。"
                    : "选择石头、剪刀或布。";
            }

            if (room.Status == RoomStatus.Settled)
            {
                return RoomDataManager.Instance.IsCurrentPlayerOwner()
                    ? "结算已完成，可在弹窗中再来一局。"
                    : "结算已完成，等待房主再开。";
            }

            return "房主开始后，双方选择石头、剪刀或布。";
        }

        private static string GetTipsText(RoomInfo room)
        {
            if (room.Status == RoomStatus.Waiting)
            {
                return "可以复制邀请信息，邀请第二名玩家加入。";
            }

            if (room.Status == RoomStatus.Ready)
            {
                return RoomDataManager.Instance.IsCurrentPlayerOwner()
                    ? "玩家已就绪，可以开始游戏。"
                    : "玩家已就绪，等待房主开始。";
            }

            return "房间内状态由服务器推送同步。";
        }

        private static void CopyInviteText()
        {
            GUIUtility.systemCopyBuffer = RoomDataManager.Instance.InviteText;
            Log.Info(RoomDataManager.Instance.InviteText);
        }
    }
}
