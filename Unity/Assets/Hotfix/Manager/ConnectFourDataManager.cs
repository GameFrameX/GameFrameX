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
    /// 四子棋玩法数据管理器，负责落子、结算与查询。
    /// 通过订阅 <see cref="RoomChangedEventArgs"/> 自动同步玩法数据。
    /// Connect-four game data manager. Subscribes to RoomChangedEventArgs to sync game data.
    /// </summary>
    public sealed class ConnectFourDataManager : GameFrameworkSingleton<ConnectFourDataManager>, IMessageHandler
    {
        /// <summary>
        /// 棋盘列数。
        /// </summary>
        public const int ColumnCount = 7;

        /// <summary>
        /// 棋盘行数（第 0 行为底行）。
        /// </summary>
        public const int RowCount = 6;

        public ConnectFourGameInfo CurrentGame { get; private set; }

        public ConnectFourDataManager()
        {
            Register();
            GameApp.Event.CheckSubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
        }

        public void Register()
        {
            ProtoMessageHandler.Add(this);
        }

        [MessageHandler(typeof(NotifyConnectFourGameChanged), nameof(NotifyConnectFourGameChanged))]
        private void NotifyConnectFourGameChanged(NotifyConnectFourGameChanged msg)
        {
            CurrentGame = msg.GameInfo;
            FireChanged();
        }

        private void OnRoomChanged(object sender, GameEventArgs e)
        {
            SynchronizeCurrentGameInfo().Forget();
        }

        public async UniTask SubmitMove(int column)
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null)
            {
                return;
            }

            var response = await NetworkCall<RespSubmitConnectFourMove>(new ReqSubmitConnectFourMove
            {
                RoomId = room.RoomId,
                Column = column
            });
            if (response.ErrorCode != default)
            {
                Log.Warning($"落子失败:{response.ErrorCode}");
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

            var response = await NetworkCall<RespRestartConnectFourGame>(new ReqRestartConnectFourGame { RoomId = room.RoomId });
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
            var response = await NetworkCall<RespConnectFourGameInfo>(new ReqConnectFourGameInfo { RoomId = roomId });
            if (response.ErrorCode != default)
            {
                Log.Warning($"请求四子棋信息失败:{response.ErrorCode}");
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
            if (room == null || room.GameType != GameType.ConnectFour || !IsGameDataStatus(room.Status))
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

        public ConnectFourPlayerInfo GetPlayer(long roleId)
        {
            return CurrentGame == null ? null : CurrentGame.Players.FirstOrDefault(m => m.PlayerId == roleId);
        }

        public ConnectFourPlayerInfo GetCurrentPlayer()
        {
            return GetPlayer(RoomDataManager.Instance.CurrentRoleId);
        }

        public ConnectFourPlayerInfo GetOpponentPlayer()
        {
            if (CurrentGame == null)
            {
                return null;
            }

            var roleId = RoomDataManager.Instance.CurrentRoleId;
            return CurrentGame.Players.FirstOrDefault(m => m.PlayerId != roleId);
        }

        /// <summary>
        /// 获取当前玩家座位索引（1 或 2），未知时为 0。
        /// </summary>
        public int GetSelfSeatIndex()
        {
            var player = GetCurrentPlayer();
            return player == null ? 0 : player.SeatIndex;
        }

        /// <summary>
        /// 读取棋盘格子。row 0 为底行，column 0 为最左列。
        /// </summary>
        public ConnectFourCell GetCell(int row, int column)
        {
            var game = CurrentGame;
            if (game == null || game.Board == null)
            {
                return ConnectFourCell.None;
            }

            var index = row * ColumnCount + column;
            if (index < 0 || index >= game.Board.Count)
            {
                return ConnectFourCell.None;
            }

            return (ConnectFourCell)game.Board[index];
        }

        public bool IsCurrentPlayerTurn()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            return room != null
                && room.Status == RoomStatus.Playing
                && CurrentGame != null
                && CurrentGame.CurrentPlayerId == RoomDataManager.Instance.CurrentRoleId;
        }

        public bool CanSubmitMove()
        {
            var room = RoomDataManager.Instance.CurrentRoom;
            if (room == null || room.Status != RoomStatus.Playing || CurrentGame == null)
            {
                return false;
            }

            if (CurrentGame.RoomId != room.RoomId || CurrentGame.Round != room.Round)
            {
                return false;
            }

            if (CurrentGame.WinnerPlayerId != 0 || CurrentGame.IsDraw)
            {
                return false;
            }

            return CurrentGame.CurrentPlayerId == RoomDataManager.Instance.CurrentRoleId;
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
                return "对局中";
            }

            if (!HasRevealedResult(room, CurrentGame))
            {
                return "等待结算数据";
            }

            if (CurrentGame.IsDraw)
            {
                return "平局";
            }

            return CurrentGame.WinnerPlayerId == RoomDataManager.Instance.CurrentRoleId ? "你赢了" : "你输了";
        }

        private static bool IsGameDataStatus(RoomStatus status)
        {
            return status == RoomStatus.Playing || status == RoomStatus.Settling || status == RoomStatus.Settled;
        }

        private static bool HasRevealedResult(RoomInfo room, ConnectFourGameInfo game)
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

            return game.WinnerPlayerId != 0 || game.IsDraw;
        }

        private static async UniTask<TResponse> NetworkCall<TResponse>(MessageObject request)
            where TResponse : MessageObject, IResponseMessage
        {
            return await GameApp.Network.GetNetworkChannel("network").Call<TResponse>(request);
        }

        private static void FireChanged()
        {
            GameApp.Event.Fire(Instance, ConnectFourGameChangedEventArgs.Create());
        }
    }
}
