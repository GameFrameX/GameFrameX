// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//  
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//  
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//  
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using GameFrameX.Apps.Common.Session;
using GameFrameX.Apps.Game.Room.Component;
using GameFrameX.Apps.Game.Room.Entity;
using GameFrameX.Apps.Player.Player.Entity;
using GameFrameX.Core.Abstractions.Attribute;
using GameFrameX.Core.Abstractions.Events;
using GameFrameX.Core.Timer.Handler;
using GameFrameX.Foundation.Utility;

namespace GameFrameX.Hotfix.Logic.Game.Room;

public class RoomComponentAgent : StateComponentAgent<RoomComponent, RoomListState>
{
    private const long DisconnectedPlayerAutoLeaveSeconds = 120;
    private const long ClosedRoomRetentionSeconds = 10 * 60;
    private const string RoomMaintenanceCronExpression = "0/10 * * * * ? *";

    private static readonly Dictionary<GameType, RoomRule> RoomRuleMap = new Dictionary<GameType, RoomRule>
    {
        [GameType.RockPaperScissors] = new RoomRule
        {
            GameType = GameType.RockPaperScissors,
            MinPlayerCount = 2,
            MaxPlayerCount = 2,
            DefaultNamePrefix = "石头剪刀布房间",
        },
    };

    public override async Task<bool> Active()
    {
        var isContinue = await base.Active();
        if (isContinue)
        {
            WithCronExpression<RoomMaintenanceTimer>(RoomMaintenanceCronExpression);
        }

        return isContinue;
    }

    public async Task OnReqRoomListAsync(ReqRoomList request, RespRoomList response)
    {
        var rooms = State.Rooms.Values
            .Where(room => request.GameType == GameType.None || room.GameType == request.GameType)
            .Where(room => request.IncludeClosed || room.Status != RoomStatus.Closed && room.Status != RoomStatus.Disbanded)
            .OrderBy(room => room.RoomId)
            .ToList();

        response.Rooms = new List<RoomInfo>();
        foreach (var room in rooms)
        {
            response.Rooms.Add(await ToMessageAsync(room));
        }
    }

    public async Task OnCreateRoomAsync(long roleId, ReqCreateRoom request, RespCreateRoom response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        if (!TryGetRule(request.GameType, response, out var rule))
        {
            return;
        }

        if (!TryResolvePlayerCount(request.MinPlayerCount, request.MaxPlayerCount, rule, response, out var minPlayerCount, out var maxPlayerCount))
        {
            return;
        }

        if (TryGetActiveRoom(roleId, out var activeRoom))
        {
            if (CanAutoCloseBeforeEnterNewRoom(activeRoom))
            {
                await CloseRoomAsync(activeRoom, RoomChangeType.Closed);
            }
            else
            {
                response.ErrorCode = (int)OperationStatusCode.HasExist;
                response.Room = await ToMessageAsync(activeRoom);
                return;
            }
        }

        var now = TimerHelper.UnixTimeSeconds();
        var roomId = State.NextRoomId++;
        var roomName = string.IsNullOrWhiteSpace(request.Name) ? $"{rule.DefaultNamePrefix}{roomId}" : request.Name.Trim();
        var room = new RoomState
        {
            RoomId = roomId,
            Name = roomName,
            GameType = request.GameType,
            OwnerRoleId = roleId,
            Status = RoomStatus.Waiting,
            MinPlayerCount = minPlayerCount,
            MaxPlayerCount = maxPlayerCount,
            Round = 1,
            CreatedTime = now,
            UpdatedTime = now,
        };
        room.PlayerIds.Add(roleId);
        State.Rooms[roomId] = room;
        State.PlayerRoomMap[roleId] = roomId;

        response.Room = await ToMessageAsync(room);
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Created);
    }

    public async Task OnJoinRoomAsync(long roleId, ReqJoinRoom request, RespJoinRoom response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        if (!TryGetRoom(request.RoomId, response, out var room))
        {
            return;
        }

        if (TryGetActiveRoom(roleId, out var activeRoom))
        {
            if (activeRoom.RoomId == request.RoomId)
            {
                response.Room = await ToMessageAsync(activeRoom);
                return;
            }

            if (CanAutoCloseBeforeEnterNewRoom(activeRoom))
            {
                await CloseRoomAsync(activeRoom, RoomChangeType.Closed);
            }
            else
            {
                response.ErrorCode = (int)OperationStatusCode.HasExist;
                response.Room = await ToMessageAsync(activeRoom);
                return;
            }
        }

        if (room.Status != RoomStatus.Waiting && room.Status != RoomStatus.Ready)
        {
            response.ErrorCode = (int)OperationStatusCode.Forbidden;
            response.Room = await ToMessageAsync(room);
            return;
        }

        if (room.PlayerIds.Count >= room.MaxPlayerCount)
        {
            response.ErrorCode = (int)OperationStatusCode.ServerFullyLoaded;
            response.Room = await ToMessageAsync(room);
            return;
        }

        room.PlayerIds.Add(roleId);
        room.Status = room.PlayerIds.Count >= room.MinPlayerCount ? RoomStatus.Ready : RoomStatus.Waiting;
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();
        State.PlayerRoomMap[roleId] = room.RoomId;

        response.Room = await ToMessageAsync(room);
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Joined);
    }

    public async Task OnLeaveRoomAsync(long roleId, ReqLeaveRoom request, RespLeaveRoom response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        if (!TryGetRoom(request.RoomId, response, out var room))
        {
            return;
        }

        if (!room.PlayerIds.Contains(roleId))
        {
            response.ErrorCode = (int)OperationStatusCode.NotFound;
            response.Room = await ToMessageAsync(room);
            return;
        }

        LeaveRoom(roleId, room);
        response.Room = await ToMessageAsync(room);
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Left);
    }

    [Service]
    public virtual Task MarkPlayerDisconnected(long roleId)
    {
        if (roleId <= 0 || !TryGetActiveRoom(roleId, out _))
        {
            return Task.CompletedTask;
        }

        State.DisconnectedPlayerTimeMap[roleId] = TimerHelper.UnixTimeSeconds();
        return OwnerComponent.WriteStateAsync();
    }

    [Service]
    public virtual Task MarkPlayerReconnected(long roleId)
    {
        if (roleId <= 0 || !State.DisconnectedPlayerTimeMap.Remove(roleId))
        {
            return Task.CompletedTask;
        }

        return OwnerComponent.WriteStateAsync();
    }

    public async Task OnStartRoomGameAsync(long roleId, ReqStartRoomGame request, RespStartRoomGame response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        if (!TryGetRoom(request.RoomId, response, out var room))
        {
            return;
        }

        if (room.OwnerRoleId != roleId || room.Status != RoomStatus.Ready)
        {
            response.ErrorCode = (int)OperationStatusCode.Forbidden;
            response.Room = await ToMessageAsync(room);
            return;
        }

        if (room.PlayerIds.Count < room.MinPlayerCount || room.PlayerIds.Count > room.MaxPlayerCount)
        {
            response.ErrorCode = (int)OperationStatusCode.Unprocessable;
            response.Room = await ToMessageAsync(room);
            return;
        }

        room.Status = RoomStatus.Playing;
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();

        response.Room = await ToMessageAsync(room);
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Started);
    }

    private void LeaveRoom(long roleId, RoomState room)
    {
        var previousStatus = room.Status;
        room.PlayerIds.Remove(roleId);
        State.PlayerRoomMap.Remove(roleId);
        State.DisconnectedPlayerTimeMap.Remove(roleId);
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();
        if (room.PlayerIds.Count == 0)
        {
            room.Status = RoomStatus.Closed;
            room.OwnerRoleId = 0;
        }
        else
        {
            if (room.OwnerRoleId == roleId)
            {
                room.OwnerRoleId = room.PlayerIds[0];
            }

            if (previousStatus != RoomStatus.Waiting && previousStatus != RoomStatus.Ready)
            {
                room.Round++;
            }

            room.Status = room.PlayerIds.Count >= room.MinPlayerCount ? RoomStatus.Ready : RoomStatus.Waiting;
        }
    }

    private async Task CloseRoomAsync(RoomState room, RoomChangeType changeType)
    {
        foreach (var roleId in room.PlayerIds)
        {
            State.PlayerRoomMap.Remove(roleId);
            State.DisconnectedPlayerTimeMap.Remove(roleId);
        }

        room.PlayerIds.Clear();
        room.OwnerRoleId = 0;
        room.Status = RoomStatus.Closed;
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, changeType);
    }

    [Service]
    public virtual Task<RoomState> GetRoom(long roomId)
    {
        State.Rooms.TryGetValue(roomId, out var room);
        return Task.FromResult(room);
    }

    [Service]
    public virtual async Task<RoomInfo> GetRoomInfo(long roomId)
    {
        State.Rooms.TryGetValue(roomId, out var room);
        return room == null ? null : await ToMessageAsync(room);
    }

    [Service]
    public virtual async Task<RoomInfo> EnterSettling(long roomId)
    {
        if (!State.Rooms.TryGetValue(roomId, out var room))
        {
            return null;
        }

        room.Status = RoomStatus.Settling;
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Settling);
        return await ToMessageAsync(room);
    }

    [Service]
    public virtual async Task<RoomInfo> MarkSettled(long roomId)
    {
        if (!State.Rooms.TryGetValue(roomId, out var room))
        {
            return null;
        }

        room.Status = RoomStatus.Settled;
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Settled);
        return await ToMessageAsync(room);
    }

    [Service]
    public virtual async Task<RoomInfo> RestartRoomGame(long roomId)
    {
        if (!State.Rooms.TryGetValue(roomId, out var room))
        {
            return null;
        }

        if (room.Status != RoomStatus.Settled || room.PlayerIds.Count < room.MinPlayerCount)
        {
            return await ToMessageAsync(room);
        }

        room.Round++;
        room.Status = RoomStatus.Playing;
        room.UpdatedTime = TimerHelper.UnixTimeSeconds();
        await OwnerComponent.WriteStateAsync();
        await NotifyRoomChangedAsync(room, RoomChangeType.Reset);
        return await ToMessageAsync(room);
    }

    private static bool CheckRoleId(long roleId, IResponseMessage response)
    {
        if (roleId > 0)
        {
            return true;
        }

        SetResponseErrorCode(response, (int)OperationStatusCode.Forbidden);
        return false;
    }

    private static bool TryGetRule(GameType gameType, IResponseMessage response, out RoomRule rule)
    {
        if (RoomRuleMap.TryGetValue(gameType, out rule))
        {
            return true;
        }

        SetResponseErrorCode(response, (int)OperationStatusCode.ParamErr);
        return false;
    }

    private static bool TryResolvePlayerCount(int requestMinPlayerCount, int requestMaxPlayerCount, RoomRule rule, IResponseMessage response, out int minPlayerCount, out int maxPlayerCount)
    {
        minPlayerCount = requestMinPlayerCount > 0 ? requestMinPlayerCount : rule.MinPlayerCount;
        maxPlayerCount = requestMaxPlayerCount > 0 ? requestMaxPlayerCount : rule.MaxPlayerCount;
        if (minPlayerCount <= 0 || maxPlayerCount <= 0 || minPlayerCount > maxPlayerCount)
        {
            SetResponseErrorCode(response, (int)OperationStatusCode.ParamErr);
            return false;
        }

        return true;
    }

    private bool TryGetRoom<TResponse>(long roomId, TResponse response, out RoomState room) where TResponse : IResponseMessage
    {
        if (roomId <= 0 || !State.Rooms.TryGetValue(roomId, out room))
        {
            room = null;
            SetResponseErrorCode(response, (int)OperationStatusCode.NotFound);
            return false;
        }

        return true;
    }

    private bool TryGetActiveRoom(long roleId, out RoomState room)
    {
        room = null;
        if (!State.PlayerRoomMap.TryGetValue(roleId, out var roomId))
        {
            return false;
        }

        if (!State.Rooms.TryGetValue(roomId, out room) || room.Status == RoomStatus.Closed || room.Status == RoomStatus.Disbanded)
        {
            State.PlayerRoomMap.Remove(roleId);
            room = null;
            return false;
        }

        return true;
    }

    private async Task<RoomInfo> ToMessageAsync(RoomState room)
    {
        var message = new RoomInfo
        {
            RoomId = room.RoomId,
            Name = room.Name,
            GameType = room.GameType,
            Status = room.Status,
            PlayerCount = room.PlayerIds.Count,
            MinPlayerCount = room.MinPlayerCount,
            MaxPlayerCount = room.MaxPlayerCount,
            OwnerRoleId = room.OwnerRoleId,
            Players = new List<RoomPlayerInfo>(),
            Round = room.Round,
            CreatedTime = room.CreatedTime,
            UpdatedTime = room.UpdatedTime,
        };

        for (var index = 0; index < room.PlayerIds.Count; index++)
        {
            var roleId = room.PlayerIds[index];
            message.Players.Add(await ToPlayerMessageAsync(room, roleId, index));
        }

        return message;
    }

    private async Task<RoomPlayerInfo> ToPlayerMessageAsync(RoomState room, long roleId, int seatIndex)
    {
        var playerState = await GameDb.FindAsync<PlayerState>(roleId);
        return new RoomPlayerInfo
        {
            RoleId = roleId,
            SeatIndex = seatIndex,
            IsOwner = roleId == room.OwnerRoleId,
            Name = string.IsNullOrWhiteSpace(playerState?.Name) ? $"玩家{roleId}" : playerState.Name,
            Avatar = playerState?.Avatar ?? 0,
            OnlineStatus = GetOnlineStatus(roleId),
            PlayerStatus = GetPlayerStatus(room.Status),
        };
    }

    private RoomPlayerOnlineStatus GetOnlineStatus(long roleId)
    {
        if (State.DisconnectedPlayerTimeMap.ContainsKey(roleId))
        {
            return RoomPlayerOnlineStatus.Reconnecting;
        }

        return SessionManager.GetByRoleId(roleId) == null ? RoomPlayerOnlineStatus.Offline : RoomPlayerOnlineStatus.Online;
    }

    private static RoomPlayerStatus GetPlayerStatus(RoomStatus roomStatus)
    {
        switch (roomStatus)
        {
            case RoomStatus.Waiting:
                return RoomPlayerStatus.Idle;
            case RoomStatus.Ready:
                return RoomPlayerStatus.ReadyInRoom;
            case RoomStatus.Playing:
                return RoomPlayerStatus.InGame;
            case RoomStatus.Settling:
                return RoomPlayerStatus.SettlingInRoom;
            case RoomStatus.Settled:
                return RoomPlayerStatus.SettledInRoom;
            default:
                return RoomPlayerStatus.PlayerStatusNone;
        }
    }

    private static void SetResponseErrorCode(IResponseMessage response, int errorCode)
    {
        var propertyInfo = response.GetType().GetProperty(nameof(RespRoomList.ErrorCode));
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(response, errorCode);
        }
    }

    private async Task NotifyRoomChangedAsync(RoomState room, RoomChangeType changeType)
    {
        var notify = new NotifyRoomChanged
        {
            ChangeType = changeType,
            Room = await ToMessageAsync(room),
        };

        var sessions = SessionManager.GetList(session => session.PlayerId > 0);
        foreach (var session in sessions)
        {
            await session.WriteAsync(notify);
        }
    }

    private async Task CleanupExpiredDisconnectedPlayersAsync()
    {
        var now = TimerHelper.UnixTimeSeconds();
        var roleIds = State.DisconnectedPlayerTimeMap
            .Where(pair => now - pair.Value >= DisconnectedPlayerAutoLeaveSeconds)
            .Select(pair => pair.Key)
            .ToList();
        if (roleIds.Count == 0)
        {
            return;
        }

        var changed = false;
        foreach (var roleId in roleIds)
        {
            if (SessionManager.GetByRoleId(roleId) != null)
            {
                State.DisconnectedPlayerTimeMap.Remove(roleId);
                changed = true;
                continue;
            }

            if (!TryGetActiveRoom(roleId, out var room))
            {
                State.DisconnectedPlayerTimeMap.Remove(roleId);
                changed = true;
                continue;
            }

            LeaveRoom(roleId, room);
            changed = true;
            await NotifyRoomChangedAsync(room, RoomChangeType.Left);
        }

        if (changed)
        {
            await OwnerComponent.WriteStateAsync();
        }
    }

    private async Task CleanupExpiredClosedRoomsAsync()
    {
        var now = TimerHelper.UnixTimeSeconds();
        var expiredRoomIds = State.Rooms.Values
            .Where(room => IsClosedRoom(room) && now - room.UpdatedTime >= ClosedRoomRetentionSeconds)
            .Select(room => room.RoomId)
            .ToList();
        if (expiredRoomIds.Count == 0)
        {
            return;
        }

        foreach (var roomId in expiredRoomIds)
        {
            if (!State.Rooms.TryGetValue(roomId, out var room))
            {
                continue;
            }

            foreach (var roleId in room.PlayerIds)
            {
                State.PlayerRoomMap.Remove(roleId);
                State.DisconnectedPlayerTimeMap.Remove(roleId);
            }

            State.Rooms.Remove(roomId);
        }

        await OwnerComponent.WriteStateAsync();
    }

    private static bool IsClosedRoom(RoomState room)
    {
        return room.Status == RoomStatus.Closed || room.Status == RoomStatus.Disbanded;
    }

    private static bool CanAutoCloseBeforeEnterNewRoom(RoomState room)
    {
        return room.Status == RoomStatus.Settled;
    }

    private async Task RunRoomMaintenanceAsync()
    {
        await CleanupExpiredDisconnectedPlayersAsync();
        await CleanupExpiredClosedRoomsAsync();
    }

    private sealed class RoomMaintenanceTimer : TimerHandler<RoomComponentAgent>
    {
        protected override Task HandleTimer(RoomComponentAgent agent, GameEventArgs gameEventArgs)
        {
            return agent.RunRoomMaintenanceAsync();
        }
    }
}
