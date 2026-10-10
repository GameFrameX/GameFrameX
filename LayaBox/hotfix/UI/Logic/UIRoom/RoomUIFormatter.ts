import { Room } from "../../../../src/gameframex/protobuf/_0400_Room";
import { RockPaperScissors } from "../../../../src/gameframex/protobuf/_0410_RockPaperScissors";

type RoomInfo = Room.RoomInfo;
type RoomPlayerInfo = Room.RoomPlayerInfo;
type GesturePlayer = RockPaperScissors.RockPaperScissorsPlayerInfo;
type RoomStatus = Room.RoomStatus;

/**
 * 房间界面文案工具。
 *
 * 对照 Unity `RoomUIFormatter`。
 */
export class RoomUIFormatter {
    private static roomNameCounter: number = 0;

    /**
     * 生成默认建房名。
     */
    public static CreateRoomName(): string {
        this.roomNameCounter++;
        return `laya-room-${this.roomNameCounter}`;
    }

    /**
     * 房间名(空名回退 Id)。
     */
    public static GetRoomName(room: RoomInfo): string {
        return room.Name == null || room.Name === '' ? `房间${room.RoomId}` : room.Name;
    }

    /**
     * 房间状态文案。
     */
    public static GetRoomStatusText(status: RoomStatus): string {
        switch (status) {
            case Room.RoomStatus.Waiting: return '等待中';
            case Room.RoomStatus.Ready: return '可开始';
            case Room.RoomStatus.Playing: return '游戏中';
            case Room.RoomStatus.Settling: return '结算中';
            case Room.RoomStatus.Settled: return '已结算';
            case Room.RoomStatus.Closed: return '已关闭';
            case Room.RoomStatus.Disbanded: return '已解散';
            default: return '未知';
        }
    }

    /**
     * 是否玩法阶段。
     */
    public static IsGameDataStatus(status: RoomStatus): boolean {
        return status === Room.RoomStatus.Playing || status === Room.RoomStatus.Settling || status === Room.RoomStatus.Settled;
    }

    /**
     * 玩家显示名(房主加标记)。
     */
    public static BuildPlayerName(player: RoomPlayerInfo, isSelf: boolean): string {
        const suffix = isSelf ? '(我)' : '';
        return `${player.Name}${suffix}`;
    }

    /**
     * 手势文案。
     */
    public static GetGestureText(gesture: RockPaperScissors.RockPaperScissorsGesture): string {
        switch (gesture) {
            case RockPaperScissors.RockPaperScissorsGesture.Rock: return '石头';
            case RockPaperScissors.RockPaperScissorsGesture.Scissors: return '剪刀';
            case RockPaperScissors.RockPaperScissorsGesture.Paper: return '布';
            default: return '-';
        }
    }

    /**
     * 玩家手势状态文案。
     */
    public static GetGestureStateText(gesturePlayer: GesturePlayer | null | undefined, roomStatus: RoomStatus): string {
        if (gesturePlayer == null || !gesturePlayer.HasGesture) {
            return '-';
        }

        if (roomStatus !== Room.RoomStatus.Settling && roomStatus !== Room.RoomStatus.Settled) {
            return '已出拳';
        }

        return this.GetGestureText(gesturePlayer.Gesture);
    }
}
