using System.Linq;
using Cysharp.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using Hotfix.Events;
using Hotfix.Proto;

namespace Hotfix.Manager
{
    /// <summary>
    /// 石头剪刀布玩法数据管理器，负责出拳、结算与查询。
    /// 通过订阅 <see cref="RoomChangedEventArgs"/> 自动同步玩法数据。
    /// Rock-paper-scissors game data manager. Subscribes to RoomChangedEventArgs to sync game data.
    /// </summary>
    public sealed class RockPaperScissorsDataManager : GameFrameworkSingleton<RockPaperScissorsDataManager>, IMessageHandler
    {
        public RockPaperScissorsGameInfo CurrentGame { get; private set; }

        public RockPaperScissorsDataManager()
        {
            Register();
            GameApp.Event.CheckSubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
        }

        public void Register()
        {
            ProtoMessageHandler.Add(this);
        }

        [MessageHandler(typeof(NotifyRockPaperScissorsGameChanged), nameof(NotifyRockPaperScissorsGameChanged))]
        private void NotifyRockPaperScissorsGameChanged(NotifyRockPaperScissorsGameChanged msg)
        {
            CurrentGame = msg.GameInfo;
            FireChanged();
        }

        private void OnRoomChanged(object sender, GameEventArgs e)
        {
            SynchronizeCurrentGameInfo().Forget();
        }

        public async UniTask SubmitGesture(RockPaperScissorsGesture gesture)
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return;
            }

            var response = await NetworkCall<RespSubmitRockPaperScissorsGesture>(new ReqSubmitRockPaperScissorsGesture
            {
                RoomId = room.RoomId,
                Gesture = gesture
            });
            if (response.ErrorCode != default)
            {
                Log.Warning($"出拳失败:{response.ErrorCode}");
                return;
            }

            CurrentGame = response.GameInfo;
            FireChanged();
        }

        public async UniTask RestartGame()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return;
            }

            var response = await NetworkCall<RespRestartRockPaperScissorsGame>(new ReqRestartRockPaperScissorsGame { RoomId = room.RoomId });
            if (response.ErrorCode != default)
            {
                Log.Warning($"再来一局失败:{response.ErrorCode}");
                return;
            }

            CurrentGame = response.GameInfo;
            FireChanged();
        }

        public async UniTask RequestGameInfo(long roomId)
        {
            var response = await NetworkCall<RespRockPaperScissorsGameInfo>(new ReqRockPaperScissorsGameInfo { RoomId = roomId });
            if (response.ErrorCode != default)
            {
                Log.Warning($"请求猜拳信息失败:{response.ErrorCode}");
                return;
            }

            CurrentGame = response.GameInfo;
            FireChanged();
        }

        /// <summary>
        /// 根据当前房间状态自动同步玩法数据。房间为空或非玩法阶段时清空当前玩法。
        /// Sync game data based on current room status. Clears game data when room is null or not in game phase.
        /// </summary>
        public async UniTask SynchronizeCurrentGameInfo()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null || !IsGameDataStatus(room.Status))
            {
                var changed = CurrentGame != null;
                CurrentGame = null;
                if (changed)
                {
                    FireChanged();
                }

                return;
            }

            await RequestGameInfo(room.RoomId);
        }

        public bool HasGesture(long roleId)
        {
            if (CurrentGame == null)
            {
                return false;
            }

            var playerInfo = CurrentGame.Players.FirstOrDefault(m => m.RoleId == roleId);
            return playerInfo != null && playerInfo.HasGesture;
        }

        public bool HasCurrentPlayerGesture()
        {
            return HasGesture(RoomDataManager.Instance.CurrentRoleId);
        }

        public RockPaperScissorsPlayerInfo GetPlayer(long roleId)
        {
            return CurrentGame == null ? null : CurrentGame.Players.FirstOrDefault(m => m.RoleId == roleId);
        }

        public RockPaperScissorsPlayerInfo GetCurrentPlayer()
        {
            return GetPlayer(RoomDataManager.Instance.CurrentRoleId);
        }

        public RockPaperScissorsPlayerInfo GetOpponentPlayer()
        {
            if (CurrentGame == null)
            {
                return null;
            }

            var roleId = RoomDataManager.Instance.CurrentRoleId;
            return CurrentGame.Players.FirstOrDefault(m => m.RoleId != roleId);
        }

        public bool CanSubmitGesture()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            return room != null && room.Status == RoomStatus.Playing && !HasCurrentPlayerGesture();
        }

        public bool CanRestartGame()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            return room != null
                && RoomDataManager.Instance.IsCurrentPlayerOwner()
                && room.Status == RoomStatus.Settled
                && HasRevealedResult();
        }

        public bool HasRevealedResult()
        {
            return HasRevealedResult(RoomDataManager.Instance.CurrentRoom, CurrentGame);
        }

        public string GetResultText()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return "未进入房间";
            }

            if (room.Status != RoomStatus.Settling && room.Status != RoomStatus.Settled)
            {
                return "等待双方出拳";
            }

            if (!HasRevealedResult(room, CurrentGame))
            {
                return "等待结算数据";
            }

            if (CurrentGame.WinnerRoleId == 0)
            {
                return "平局";
            }

            return CurrentGame.WinnerRoleId == RoomDataManager.Instance.CurrentRoleId ? "你赢了" : "你输了";
        }

        private static bool IsGameDataStatus(RoomStatus status)
        {
            return status == RoomStatus.Playing || status == RoomStatus.Settling || status == RoomStatus.Settled;
        }

        private static bool HasRevealedResult(RoomInfo room, RockPaperScissorsGameInfo game)
        {
            if (room == null || game == null)
            {
                return false;
            }

            if (room.Status != RoomStatus.Settling && room.Status != RoomStatus.Settled)
            {
                return false;
            }

            if (game.RoomId != room.RoomId || game.Round != room.Round)
            {
                return false;
            }

            if (room.Players == null || room.Players.Count == 0 || game.Players == null)
            {
                return false;
            }

            return room.Players.All(player =>
            {
                var gesturePlayer = game.Players.FirstOrDefault(m => m.RoleId == player.RoleId);
                return gesturePlayer != null
                       && gesturePlayer.HasGesture
                       && gesturePlayer.Gesture != RockPaperScissorsGesture.None;
            });
        }

        private static async UniTask<TResponse> NetworkCall<TResponse>(MessageObject request)
            where TResponse : MessageObject, IResponseMessage
        {
            return await GameApp.Network.GetNetworkChannel("network").Call<TResponse>(request);
        }

        private static void FireChanged()
        {
            GameApp.Event.Fire(Instance, RockPaperScissorsGameChangedEventArgs.Create());
        }
    }
}
