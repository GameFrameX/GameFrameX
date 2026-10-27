// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and related international regulations.
//   使用本项目须严格遵守相应法律法规与开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 Apache License 2.0 单协议分发，
//   This project is licensed solely under the Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:   https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================


using GameFrameX.Apps.Common.Session;
using GameFrameX.Apps.Game.ConnectFour.Component;
using GameFrameX.Apps.Game.ConnectFour.Entity;
using GameFrameX.Hotfix.Logic.Game.Room;

namespace GameFrameX.Hotfix.Logic.Game.ConnectFour;

public class ConnectFourGameComponentAgent : StateComponentAgent<ConnectFourGameComponent, ConnectFourGameListState>
{
    public async Task OnReqGameInfoAsync(ReqConnectFourGameInfo request, RespConnectFourGameInfo response)
    {
        var roomAgent = await ActorManager.GetComponentAgent<RoomComponentAgent>();
        var room = await roomAgent.GetRoom(request.RoomId);
        if (!CheckRoom(room, response))
        {
            return;
        }

        var game = GetOrCreateGame(room.RoomId, room.Round, room.PlayerIds);
        response.GameInfo = ToMessage(game, room.PlayerIds);
    }

    /// <summary>
    /// 处理落子请求。回合制权威校验顺序：房间玩法匹配 → 对局进行中且为房间成员 → 对局未结束 → 轮到该玩家 →
    /// 该列未满；全部通过后按重力落子并做四连/平局判定。任一校验失败都不改变棋盘状态。
    /// </summary>
    public async Task OnSubmitMoveAsync(long roleId, ReqSubmitConnectFourMove request, RespSubmitConnectFourMove response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        if (!ConnectFourBoardLogic.IsValidColumn(request.Column))
        {
            response.ErrorCode = (int)OperationStatusCode.ParamErr;
            return;
        }

        var roomAgent = await ActorManager.GetComponentAgent<RoomComponentAgent>();
        var room = await roomAgent.GetRoom(request.RoomId);
        if (!CheckRoom(room, response))
        {
            return;
        }

        if (room.Status != RoomStatus.Playing || !room.PlayerIds.Contains(roleId))
        {
            response.ErrorCode = (int)OperationStatusCode.Forbidden;
            response.GameInfo = BuildGameInfo(room);
            return;
        }

        var game = GetOrCreateGame(room.RoomId, room.Round, room.PlayerIds);
        if (game.WinnerRoleId != 0 || game.IsDraw)
        {
            // 对局已分出胜负或平局，拒绝继续落子。
            response.ErrorCode = (int)OperationStatusCode.Unprocessable;
            response.GameInfo = ToMessage(game, room.PlayerIds);
            return;
        }

        if (game.CurrentPlayerId != roleId)
        {
            // 回合制校验：只有当前回合玩家可以落子。
            response.ErrorCode = (int)OperationStatusCode.Forbidden;
            response.GameInfo = ToMessage(game, room.PlayerIds);
            return;
        }

        var dropRow = ConnectFourBoardLogic.FindDropRow(game.Board, request.Column);
        if (dropRow < 0)
        {
            // 该列已落满，属于不可处理的落子请求。
            response.ErrorCode = (int)OperationStatusCode.Unprocessable;
            response.GameInfo = ToMessage(game, room.PlayerIds);
            return;
        }

        var seat = room.PlayerIds.IndexOf(roleId) + 1;
        game.Board[dropRow * ConnectFourBoardLogic.Width + request.Column] = seat;
        game.MoveCount++;
        game.LastMoveColumn = request.Column;
        game.LastMoveRow = dropRow;

        if (ConnectFourBoardLogic.CheckWin(game.Board, dropRow, request.Column))
        {
            game.WinnerRoleId = roleId;
            game.CurrentPlayerId = 0;
            await roomAgent.EnterSettling(room.RoomId);
            await OwnerComponent.WriteStateAsync();
            await roomAgent.MarkSettled(room.RoomId);
        }
        else if (ConnectFourBoardLogic.IsBoardFull(game.Board))
        {
            // 棋盘落满且无四连，判定平局。
            game.IsDraw = true;
            game.CurrentPlayerId = 0;
            await roomAgent.EnterSettling(room.RoomId);
            await OwnerComponent.WriteStateAsync();
            await roomAgent.MarkSettled(room.RoomId);
        }
        else
        {
            game.CurrentPlayerId = room.PlayerIds.First(otherRoleId => otherRoleId != roleId);
            await OwnerComponent.WriteStateAsync();
        }

        response.GameInfo = ToMessage(game, room.PlayerIds);
        await NotifyGameChangedAsync(room.PlayerIds, response.GameInfo);
    }

    /// <summary>
    /// 处理再来一局请求。仅房主在房间已结算时可发起；成功后房间回到 Playing、局数+1，棋盘按新房间局数重置。
    /// </summary>
    public async Task OnRestartGameAsync(long roleId, ReqRestartConnectFourGame request, RespRestartConnectFourGame response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        var roomAgent = await ActorManager.GetComponentAgent<RoomComponentAgent>();
        var room = await roomAgent.GetRoom(request.RoomId);
        if (!CheckRoom(room, response))
        {
            return;
        }

        if (room.OwnerRoleId != roleId || room.Status != RoomStatus.Settled)
        {
            response.ErrorCode = (int)OperationStatusCode.Forbidden;
            response.GameInfo = BuildGameInfo(room);
            return;
        }

        var roomInfo = await roomAgent.RestartRoomGame(room.RoomId);
        if (roomInfo == null || roomInfo.Status != RoomStatus.Playing)
        {
            response.ErrorCode = (int)OperationStatusCode.Unprocessable;
            response.GameInfo = BuildGameInfo(room);
            return;
        }

        var game = GetOrCreateGame(room.RoomId, room.Round, room.PlayerIds);
        await OwnerComponent.WriteStateAsync();

        response.GameInfo = ToMessage(game, room.PlayerIds);
        await NotifyGameChangedAsync(room.PlayerIds, response.GameInfo);
    }

    private ConnectFourGameInfo BuildGameInfo(GameFrameX.Apps.Game.Room.Entity.RoomState room)
    {
        var game = GetOrCreateGame(room.RoomId, room.Round, room.PlayerIds);
        return ToMessage(game, room.PlayerIds);
    }

    /// <summary>
    /// 取房间对应的对局数据。新建与房间局数前进（再来一局）两种情况都会重置棋盘与回合
    /// （先手 = 房间成员列表第一个玩家）——新建时若不重置，CurrentPlayerId 恒为 0，首局任何落子都会被回合校验拒绝。
    /// </summary>
    private ConnectFourGameState GetOrCreateGame(long roomId, int round, List<long> playerIds)
    {
        if (!State.Games.TryGetValue(roomId, out var game))
        {
            game = new ConnectFourGameState
            {
                RoomId = roomId,
                Round = round,
            };
            State.Games[roomId] = game;
            ResetGame(game, playerIds);
            return game;
        }

        if (game.Round != round)
        {
            game.Round = round;
            ResetGame(game, playerIds);
        }

        return game;
    }

    /// <summary>
    /// 重置对局：清空棋盘、胜负与平局标记、落子计数，先手交给房间成员列表的第一个玩家。
    /// </summary>
    private static void ResetGame(ConnectFourGameState game, List<long> playerIds)
    {
        Array.Clear(game.Board, 0, game.Board.Length);
        game.WinnerRoleId = 0;
        game.IsDraw = false;
        game.MoveCount = 0;
        game.LastMoveColumn = -1;
        game.LastMoveRow = -1;
        game.CurrentPlayerId = playerIds.Count > 0 ? playerIds[0] : 0;
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

    private static bool CheckRoom(GameFrameX.Apps.Game.Room.Entity.RoomState room, IResponseMessage response)
    {
        if (room == null)
        {
            SetResponseErrorCode(response, (int)OperationStatusCode.NotFound);
            return false;
        }

        if (room.GameType != GameType.ConnectFour)
        {
            SetResponseErrorCode(response, (int)OperationStatusCode.Forbidden);
            return false;
        }

        return true;
    }

    private static ConnectFourGameInfo ToMessage(ConnectFourGameState game, List<long> playerIds)
    {
        return new ConnectFourGameInfo
        {
            RoomId = game.RoomId,
            Round = game.Round,
            CurrentPlayerId = game.CurrentPlayerId,
            WinnerPlayerId = game.WinnerRoleId,
            IsDraw = game.IsDraw,
            MoveCount = game.MoveCount,
            LastMoveColumn = game.LastMoveColumn,
            LastMoveRow = game.LastMoveRow,
            Board = new List<int>(game.Board),
            Players = playerIds.Select((roleId, index) => new ConnectFourPlayerInfo
            {
                PlayerId = roleId,
                SeatIndex = index + 1,
            }).ToList(),
        };
    }

    private static void SetResponseErrorCode(IResponseMessage response, int errorCode)
    {
        var propertyInfo = response.GetType().GetProperty(nameof(RespConnectFourGameInfo.ErrorCode));
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(response, errorCode);
        }
    }

    private static async Task NotifyGameChangedAsync(List<long> playerIds, ConnectFourGameInfo gameInfo)
    {
        var notify = new NotifyConnectFourGameChanged
        {
            GameInfo = gameInfo,
        };

        foreach (var roleId in playerIds)
        {
            var session = SessionManager.GetByRoleId(roleId);
            if (session != null)
            {
                await session.WriteAsync(notify);
            }
        }
    }
}
