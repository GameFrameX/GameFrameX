using System.Collections.Generic;
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
    /// 房间数据管理器，负责房间列表、进出房间与房间状态。
    /// Room data manager: room list, join/leave, and current room state.
    /// </summary>
    public sealed class RoomDataManager : GameFrameworkSingleton<RoomDataManager>, IMessageHandler
    {
        private readonly List<RoomInfo> _rooms = new List<RoomInfo>();

        public RoomInfo CurrentRoom { get; private set; }

        public IReadOnlyList<RoomInfo> Rooms
        {
            get { return _rooms; }
        }

        public string InviteText
        {
            get
            {
                return CurrentRoom == null
                    ? string.Empty
                    : $"邀请你加入房间 {CurrentRoom.RoomId}：{CurrentRoom.Name}";
            }
        }

        public long CurrentRoleId
        {
            get
            {
                return PlayerManager.Instance.PlayerInfo == null
                    ? 0
                    : PlayerManager.Instance.PlayerInfo.Id;
            }
        }

        public RoomDataManager()
        {
            Register();
        }

        public void Register()
        {
            ProtoMessageHandler.Add(this);
        }

        [MessageHandler(typeof(NotifyRoomChanged), nameof(NotifyRoomChanged))]
        private void NotifyRoomChanged(NotifyRoomChanged msg)
        {
            if (msg.Room == null)
            {
                return;
            }

            if (CurrentRoom != null && CurrentRoom.RoomId == msg.Room.RoomId)
            {
                if (IsVisibleRoom(msg.Room))
                {
                    CurrentRoom = msg.Room;
                    UpsertRoom(msg.Room);
                }
                else
                {
                    CurrentRoom = null;
                    RemoveRoom(msg.Room.RoomId);
                }
            }
            else if (IsVisibleRoom(msg.Room))
            {
                UpsertRoom(msg.Room);
            }
            else
            {
                RemoveRoom(msg.Room.RoomId);
            }

            FireChanged();
        }

        public async UniTask RequestRoomList(GameType gameType = GameType.RockPaperScissors)
        {
            var response = await NetworkCall<RespRoomList>(new ReqRoomList
            {
                GameType = gameType,
                IncludeClosed = false
            });
            if (response.ErrorCode != default)
            {
                Log.Warning($"请求房间列表失败:{response.ErrorCode}");
                return;
            }

            _rooms.Clear();
            _rooms.AddRange(response.Rooms);
            if (IsVisibleRoom(CurrentRoom) && _rooms.All(m => m.RoomId != CurrentRoom.RoomId))
            {
                _rooms.Add(CurrentRoom);
            }

            FireChanged();
        }

        public async UniTask CreateRoom(string roomName = "")
        {
            var response = await NetworkCall<RespCreateRoom>(new ReqCreateRoom
            {
                GameType = GameType.RockPaperScissors,
                Name = roomName,
                MinPlayerCount = 2,
                MaxPlayerCount = 2
            });
            if (response.ErrorCode != default)
            {
                Log.Warning($"创建房间失败:{response.ErrorCode}");
                return;
            }

            CurrentRoom = response.Room;
            UpsertRoom(response.Room);
            FireChanged();
        }

        public async UniTask JoinRoom(long roomId)
        {
            var response = await NetworkCall<RespJoinRoom>(new ReqJoinRoom { RoomId = roomId });
            if (response.ErrorCode != default)
            {
                Log.Warning($"加入房间失败:{response.ErrorCode}");
                return;
            }

            CurrentRoom = response.Room;
            UpsertRoom(response.Room);
            FireChanged();
        }

        public async UniTask LeaveRoom()
        {
            if (CurrentRoom == null)
            {
                return;
            }

            var response = await NetworkCall<RespLeaveRoom>(new ReqLeaveRoom { RoomId = CurrentRoom.RoomId });
            if (response.ErrorCode != default)
            {
                Log.Warning($"退出房间失败:{response.ErrorCode}");
                return;
            }

            if (IsVisibleRoom(response.Room))
            {
                UpsertRoom(response.Room);
            }
            else if (response.Room != null)
            {
                RemoveRoom(response.Room.RoomId);
            }

            CurrentRoom = null;
            FireChanged();
        }

        public async UniTask StartGame()
        {
            if (CurrentRoom == null)
            {
                return;
            }

            var response = await NetworkCall<RespStartRoomGame>(new ReqStartRoomGame { RoomId = CurrentRoom.RoomId });
            if (response.ErrorCode != default)
            {
                Log.Warning($"开始游戏失败:{response.ErrorCode}");
                return;
            }

            CurrentRoom = response.Room;
            UpsertRoom(response.Room);
            FireChanged();
        }

        public bool IsCurrentPlayerOwner()
        {
            return CurrentRoom != null && CurrentRoom.OwnerRoleId == CurrentRoleId;
        }

        public bool CanStartGame()
        {
            return CurrentRoom != null && IsCurrentPlayerOwner() && CurrentRoom.Status == RoomStatus.Ready;
        }

        public RoomPlayerInfo GetOpponentRoomPlayer()
        {
            if (CurrentRoom == null)
            {
                return null;
            }

            var roleId = CurrentRoleId;
            return CurrentRoom.Players.FirstOrDefault(m => m.RoleId != roleId);
        }

        private void UpsertRoom(RoomInfo room)
        {
            if (!IsVisibleRoom(room))
            {
                return;
            }

            var index = _rooms.FindIndex(m => m.RoomId == room.RoomId);
            if (index >= 0)
            {
                _rooms[index] = room;
                return;
            }

            _rooms.Add(room);
        }

        private void RemoveRoom(long roomId)
        {
            _rooms.RemoveAll(m => m.RoomId == roomId);
        }

        private static bool IsVisibleRoom(RoomInfo room)
        {
            return room != null && room.Status != RoomStatus.Closed && room.Status != RoomStatus.Disbanded;
        }

        private static async UniTask<TResponse> NetworkCall<TResponse>(MessageObject request)
            where TResponse : MessageObject, IResponseMessage
        {
            return await GameApp.Network.GetNetworkChannel("network").Call<TResponse>(request);
        }

        private static void FireChanged()
        {
            GameApp.Event.Fire(Instance, RoomChangedEventArgs.Create());
        }
    }
}
