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
using GameFrameX.Apps.Game.RockPaperScissors.Component;
using GameFrameX.Apps.Game.RockPaperScissors.Entity;
using GameFrameX.Hotfix.Logic.Game.Room;

namespace GameFrameX.Hotfix.Logic.Game.RockPaperScissors;

public class RockPaperScissorsGameComponentAgent : StateComponentAgent<RockPaperScissorsGameComponent, RockPaperScissorsGameListState>
{
    public async Task OnReqGameInfoAsync(ReqRockPaperScissorsGameInfo request, RespRockPaperScissorsGameInfo response)
    {
        var roomAgent = await ActorManager.GetComponentAgent<RoomComponentAgent>();
        var room = await roomAgent.GetRoom(request.RoomId);
        if (!CheckRoom(room, response))
        {
            return;
        }

        var game = GetOrCreateGame(room.RoomId, room.Round);
        response.GameInfo = ToMessage(game, room.PlayerIds, CanRevealGestures(room.Status));
    }

    public async Task OnSubmitGestureAsync(long roleId, ReqSubmitRockPaperScissorsGesture request, RespSubmitRockPaperScissorsGesture response)
    {
        if (!CheckRoleId(roleId, response))
        {
            return;
        }

        if (!IsValidGesture(request.Gesture))
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
            response.GameInfo = BuildGameInfo(room, false);
            return;
        }

        var game = GetOrCreateGame(room.RoomId, room.Round);
        if (game.GestureMap.ContainsKey(roleId))
        {
            response.ErrorCode = (int)OperationStatusCode.HasExist;
            response.GameInfo = ToMessage(game, room.PlayerIds, false);
            return;
        }

        game.GestureMap[roleId] = request.Gesture;
        var revealGestures = false;
        if (game.GestureMap.Count == room.PlayerIds.Count)
        {
            await roomAgent.EnterSettling(room.RoomId);
            game.WinnerRoleId = CalculateWinner(room.PlayerIds, game);
            await OwnerComponent.WriteStateAsync();
            await roomAgent.MarkSettled(room.RoomId);
            revealGestures = true;
        }
        else
        {
            await OwnerComponent.WriteStateAsync();
        }

        response.GameInfo = ToMessage(game, room.PlayerIds, revealGestures);
        await NotifyGameChangedAsync(room.PlayerIds, response.GameInfo);
    }

    public async Task OnRestartGameAsync(long roleId, ReqRestartRockPaperScissorsGame request, RespRestartRockPaperScissorsGame response)
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
            response.GameInfo = BuildGameInfo(room, CanRevealGestures(room.Status));
            return;
        }

        var roomInfo = await roomAgent.RestartRoomGame(room.RoomId);
        if (roomInfo == null || roomInfo.Status != RoomStatus.Playing)
        {
            response.ErrorCode = (int)OperationStatusCode.Unprocessable;
            response.GameInfo = BuildGameInfo(room, CanRevealGestures(room.Status));
            return;
        }

        var game = GetOrCreateGame(room.RoomId, room.Round);
        game.GestureMap.Clear();
        game.WinnerRoleId = 0;
        await OwnerComponent.WriteStateAsync();

        response.GameInfo = ToMessage(game, room.PlayerIds, false);
        await NotifyGameChangedAsync(room.PlayerIds, response.GameInfo);
    }

    private RockPaperScissorsGameInfo BuildGameInfo(GameFrameX.Apps.Game.Room.Entity.RoomState room, bool revealGestures)
    {
        var game = GetOrCreateGame(room.RoomId, room.Round);
        return ToMessage(game, room.PlayerIds, revealGestures);
    }

    private RockPaperScissorsGameState GetOrCreateGame(long roomId, int round)
    {
        if (!State.Games.TryGetValue(roomId, out var game))
        {
            game = new RockPaperScissorsGameState
            {
                RoomId = roomId,
                Round = round,
            };
            State.Games[roomId] = game;
        }

        if (game.Round != round)
        {
            game.Round = round;
            game.GestureMap.Clear();
            game.WinnerRoleId = 0;
        }

        return game;
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

        if (room.GameType != GameType.RockPaperScissors)
        {
            SetResponseErrorCode(response, (int)OperationStatusCode.Forbidden);
            return false;
        }

        return true;
    }

    private static bool IsValidGesture(RockPaperScissorsGesture gesture)
    {
        return gesture == RockPaperScissorsGesture.Rock ||
               gesture == RockPaperScissorsGesture.Scissors ||
               gesture == RockPaperScissorsGesture.Paper;
    }

    private static long CalculateWinner(List<long> playerIds, RockPaperScissorsGameState game)
    {
        if (playerIds.Count != 2)
        {
            return 0;
        }

        var firstRoleId = playerIds[0];
        var secondRoleId = playerIds[1];
        var firstGesture = game.GestureMap[firstRoleId];
        var secondGesture = game.GestureMap[secondRoleId];
        if (firstGesture == secondGesture)
        {
            return 0;
        }

        return IsFirstWin(firstGesture, secondGesture) ? firstRoleId : secondRoleId;
    }

    private static bool IsFirstWin(RockPaperScissorsGesture firstGesture, RockPaperScissorsGesture secondGesture)
    {
        return firstGesture == RockPaperScissorsGesture.Rock && secondGesture == RockPaperScissorsGesture.Scissors ||
               firstGesture == RockPaperScissorsGesture.Scissors && secondGesture == RockPaperScissorsGesture.Paper ||
               firstGesture == RockPaperScissorsGesture.Paper && secondGesture == RockPaperScissorsGesture.Rock;
    }

    private static bool CanRevealGestures(RoomStatus roomStatus)
    {
        return roomStatus == RoomStatus.Settling || roomStatus == RoomStatus.Settled;
    }

    private static RockPaperScissorsGameInfo ToMessage(RockPaperScissorsGameState game, List<long> playerIds, bool revealGestures)
    {
        return new RockPaperScissorsGameInfo
        {
            RoomId = game.RoomId,
            Round = game.Round,
            WinnerRoleId = revealGestures ? game.WinnerRoleId : 0,
            Players = playerIds.Select(roleId => new RockPaperScissorsPlayerInfo
            {
                RoleId = roleId,
                HasGesture = game.GestureMap.ContainsKey(roleId),
                Gesture = revealGestures && game.GestureMap.TryGetValue(roleId, out var gesture) ? gesture : RockPaperScissorsGesture.None,
            }).ToList(),
        };
    }

    private static void SetResponseErrorCode(IResponseMessage response, int errorCode)
    {
        var propertyInfo = response.GetType().GetProperty(nameof(RespRockPaperScissorsGameInfo.ErrorCode));
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(response, errorCode);
        }
    }

    private static async Task NotifyGameChangedAsync(List<long> playerIds, RockPaperScissorsGameInfo gameInfo)
    {
        var notify = new NotifyRockPaperScissorsGameChanged
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
