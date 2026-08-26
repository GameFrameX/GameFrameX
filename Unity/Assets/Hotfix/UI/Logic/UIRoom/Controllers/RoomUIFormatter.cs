using Hotfix.Manager;
using Hotfix.Proto;

namespace Hotfix.UI
{
    /// <summary>
    /// Room UI 共享的纯函数工具集合。
    /// Pure helper functions shared by all Room UI controllers.
    /// </summary>
    internal static class RoomUIFormatter
    {
        public static string CreateRoomName()
        {
            var player = PlayerManager.Instance.PlayerInfo;
            if (player == null || string.IsNullOrEmpty(player.Name))
            {
                return "猜拳房间";
            }

            return $"{player.Name}的猜拳房间";
        }

        public static string GetRoomName(RoomInfo room)
        {
            if (room == null || string.IsNullOrEmpty(room.Name))
            {
                return "猜拳房间";
            }

            return room.Name;
        }

        public static string GetRoomStatusText(RoomStatus status)
        {
            switch (status)
            {
                case RoomStatus.Waiting:
                    return "等待中";
                case RoomStatus.Ready:
                    return "可开始";
                case RoomStatus.Playing:
                    return "游戏中";
                case RoomStatus.Settling:
                    return "结算中";
                case RoomStatus.Settled:
                    return "已结算";
                case RoomStatus.Closed:
                    return "已关闭";
                case RoomStatus.Disbanded:
                    return "已解散";
                default:
                    return "未知";
            }
        }

        public static string GetGestureText(RockPaperScissorsGesture gesture)
        {
            switch (gesture)
            {
                case RockPaperScissorsGesture.Rock:
                    return "石头";
                case RockPaperScissorsGesture.Scissors:
                    return "剪刀";
                case RockPaperScissorsGesture.Paper:
                    return "布";
                default:
                    return "无";
            }
        }

        public static string GetGestureStateText(RockPaperScissorsPlayerInfo player, RoomStatus roomStatus)
        {
            if (player == null)
            {
                return "未开始";
            }

            if (roomStatus == RoomStatus.Settling || roomStatus == RoomStatus.Settled)
            {
                return GetGestureText(player.Gesture);
            }

            return player.HasGesture ? "已出拳" : "未出拳";
        }

        public static string BuildPlayerName(RoomPlayerInfo player, bool isSelf)
        {
            var name = string.IsNullOrEmpty(player.Name) ? $"玩家{player.RoleId}" : player.Name;
            if (player.IsOwner)
            {
                name += " 房主";
            }

            return isSelf ? "我: " + name : name;
        }

        public static string GetPlayerName(RoomPlayerInfo player, bool isSelf)
        {
            var name = string.IsNullOrEmpty(player.Name) ? $"玩家{player.RoleId}" : player.Name;
            return isSelf ? "我: " + name : name;
        }

        public static bool IsGameDataStatus(RoomStatus status)
        {
            return status == RoomStatus.Playing || status == RoomStatus.Settling || status == RoomStatus.Settled;
        }

        public static bool CanJoinRoom(RoomInfo room, bool hasCurrentRoom)
        {
            if (room == null || hasCurrentRoom)
            {
                return false;
            }

            return (room.Status == RoomStatus.Waiting || room.Status == RoomStatus.Ready)
                && room.PlayerCount < room.MaxPlayerCount;
        }
    }
}
