import GameApp from "../../src/gameframex/GameApp";
import LogHelper from "../../src/gameframex/log/LogHelper";
import type MessageObject from "../../src/gameframex/network/MessageObject";
import { MessageHandler } from "../../src/gameframex/network/ProtoMessageHandler";
import ProtoMessageHandler from "../../src/gameframex/network/ProtoMessageHandler";
import EventSystems from "../../src/gameframex/event/EventSystems";
import type GameEventArgs from "../../src/gameframex/event/GameEventArgs";
import { Room } from "../../src/gameframex/protobuf/_0400_Room";
import { RockPaperScissors } from "../../src/gameframex/protobuf/_0410_RockPaperScissors";
import { RoomChangedEventArgs, RockPaperScissorsGameChangedEventArgs } from "../Events/HotfixEventArgs";
import RoomDataManager from "./RoomDataManager";

/**
 * 猜拳玩法数据管理器。
 *
 * 对照 Unity `Hotfix.Manager.RockPaperScissorsDataManager`:出拳/再来一局/查询;
 * 订阅 RoomChangedEventArgs 自动同步玩法数据;NotifyGameChanged 驱动。
 */
export default class RockPaperScissorsDataManager {
    private static _instance: RockPaperScissorsDataManager | null = null;

    /** 全局单例(构造时注册消息 + 订阅房间变化)。 */
    public static get Instance(): RockPaperScissorsDataManager {
        this._instance ??= new RockPaperScissorsDataManager();
        return this._instance;
    }

    private currentGame: RockPaperScissors.RockPaperScissorsGameInfo | null = null;

    private constructor() {
        ProtoMessageHandler.add(this);
        EventSystems.CheckSubscribe(RoomChangedEventArgs.EventId, this.onRoomChanged);
    }

    /**
     * 当前对局信息。
     */
    public get CurrentGame(): RockPaperScissors.RockPaperScissorsGameInfo | null {
        return this.currentGame;
    }

    /**
     * 出拳。
     * @param gesture 手势
     */
    public async SubmitGesture(gesture: RockPaperScissors.RockPaperScissorsGesture): Promise<void> {
        const room = RoomDataManager.Instance.CurrentRoom;
        if (room == null) {
            return;
        }

        const request = new RockPaperScissors.ReqSubmitRockPaperScissorsGesture();
        request.RoomId = room.RoomId;
        request.Gesture = gesture;
        const response = await this.networkCall<RockPaperScissors.RespSubmitRockPaperScissorsGesture>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`出拳失败:${response.ErrorCode}`);
            return;
        }

        this.currentGame = response.GameInfo;
        this.fireChanged();
    }

    /**
     * 再来一局。
     */
    public async RestartGame(): Promise<void> {
        const room = RoomDataManager.Instance.CurrentRoom;
        if (room == null) {
            return;
        }

        const request = new RockPaperScissors.ReqRestartRockPaperScissorsGame();
        request.RoomId = room.RoomId;
        const response = await this.networkCall<RockPaperScissors.RespRestartRockPaperScissorsGame>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`再来一局失败:${response.ErrorCode}`);
            return;
        }

        this.currentGame = response.GameInfo;
        this.fireChanged();
    }

    /**
     * 请求对局信息。
     * @param roomId 房间 Id
     */
    public async RequestGameInfo(roomId: number): Promise<void> {
        const request = new RockPaperScissors.ReqRockPaperScissorsGameInfo();
        request.RoomId = roomId;
        const response = await this.networkCall<RockPaperScissors.RespRockPaperScissorsGameInfo>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`请求猜拳信息失败:${response.ErrorCode}`);
            return;
        }

        this.currentGame = response.GameInfo;
        this.fireChanged();
    }

    /**
     * 按当前房间状态自动同步玩法数据;房间空或非玩法阶段清空。
     */
    public async SynchronizeCurrentGameInfo(): Promise<void> {
        const room = RoomDataManager.Instance.CurrentRoom;
        if (room == null || !RockPaperScissorsDataManager.isGameDataStatus(room.Status)) {
            const changed = this.currentGame != null;
            this.currentGame = null;
            if (changed) {
                this.fireChanged();
            }

            return;
        }

        await this.RequestGameInfo(room.RoomId);
    }

    /**
     * 指定角色是否已出拳。
     */
    public HasGesture(roleId: number): boolean {
        if (this.currentGame == null) {
            return false;
        }

        const players = this.playerList();
        const playerInfo = players.find((m) => m.RoleId === roleId);
        return playerInfo != null && playerInfo.HasGesture;
    }

    /**
     * 当前玩家是否已出拳。
     */
    public HasCurrentPlayerGesture(): boolean {
        return this.HasGesture(RoomDataManager.Instance.CurrentRoleId);
    }

    /**
     * 取指定角色对局信息。
     */
    public GetPlayer(roleId: number): RockPaperScissors.RockPaperScissorsPlayerInfo | null {
        return this.currentGame == null ? null : this.playerList().find((m) => m.RoleId === roleId) ?? null;
    }

    /**
     * 当前玩家对局信息。
     */
    public GetCurrentPlayer(): RockPaperScissors.RockPaperScissorsPlayerInfo | null {
        return this.GetPlayer(RoomDataManager.Instance.CurrentRoleId);
    }

    /**
     * 对手对局信息。
     */
    public GetOpponentPlayer(): RockPaperScissors.RockPaperScissorsPlayerInfo | null {
        if (this.currentGame == null) {
            return null;
        }

        const roleId = RoomDataManager.Instance.CurrentRoleId;
        return this.playerList().find((m) => m.RoleId !== roleId) ?? null;
    }

    /**
     * 是否可出拳。
     */
    public CanSubmitGesture(): boolean {
        const room = RoomDataManager.Instance.CurrentRoom;
        return room != null && room.Status === Room.RoomStatus.Playing && !this.HasCurrentPlayerGesture();
    }

    /**
     * 是否可再来一局。
     */
    public CanRestartGame(): boolean {
        const room = RoomDataManager.Instance.CurrentRoom;
        return room != null
            && RoomDataManager.Instance.IsCurrentPlayerOwner()
            && room.Status === Room.RoomStatus.Settled
            && this.HasRevealedResult();
    }

    /**
     * 对局结果是否已揭示。
     */
    public HasRevealedResult(): boolean {
        return RockPaperScissorsDataManager.hasRevealedResult(RoomDataManager.Instance.CurrentRoom, this.currentGame);
    }

    /**
     * 结果文案。
     */
    public GetResultText(): string {
        const room = RoomDataManager.Instance.CurrentRoom;
        if (room == null) {
            return '未进入房间';
        }

        if (room.Status !== Room.RoomStatus.Settling && room.Status !== Room.RoomStatus.Settled) {
            return '等待双方出拳';
        }

        if (!RockPaperScissorsDataManager.hasRevealedResult(room, this.currentGame)) {
            return '等待结算数据';
        }

        if (this.currentGame!.WinnerRoleId === 0) {
            return '平局';
        }

        return this.currentGame!.WinnerRoleId === RoomDataManager.Instance.CurrentRoleId ? '你赢了' : '你输了';
    }

    /**
     * 监听对局变化通知。
     */
    @MessageHandler(RockPaperScissors.NotifyRockPaperScissorsGameChanged)
    private NotifyRockPaperScissorsGameChanged(msg: RockPaperScissors.NotifyRockPaperScissorsGameChanged): void {
        this.currentGame = msg.GameInfo;
        this.fireChanged();
    }

    private readonly onRoomChanged = (_sender: unknown, _args: GameEventArgs): void => {
        void this.SynchronizeCurrentGameInfo();
    };

    private playerList(): RockPaperScissors.RockPaperScissorsPlayerInfo[] {
        return ((this.currentGame?.Players ?? []) as unknown as RockPaperScissors.RockPaperScissorsPlayerInfo[]);
    }

    private static isGameDataStatus(status: Room.RoomStatus): boolean {
        return status === Room.RoomStatus.Playing || status === Room.RoomStatus.Settling || status === Room.RoomStatus.Settled;
    }

    private static hasRevealedResult(room: Room.RoomInfo | null, game: RockPaperScissors.RockPaperScissorsGameInfo | null): boolean {
        if (room == null || game == null) {
            return false;
        }

        if (room.Status !== Room.RoomStatus.Settling && room.Status !== Room.RoomStatus.Settled) {
            return false;
        }

        if (game.RoomId !== room.RoomId || game.Round !== room.Round) {
            return false;
        }

        const roomPlayers = (room.Players ?? []) as unknown as Room.RoomPlayerInfo[];
        if (roomPlayers.length === 0) {
            return false;
        }

        const gamePlayers = ((game.Players ?? []) as unknown as RockPaperScissors.RockPaperScissorsPlayerInfo[]);
        return roomPlayers.every((player) => {
            const gesturePlayer = gamePlayers.find((m) => m.RoleId === player.RoleId);
            return gesturePlayer != null
                && gesturePlayer.HasGesture
                && gesturePlayer.Gesture !== RockPaperScissors.RockPaperScissorsGesture.None;
        });
    }

    private async networkCall<TResponse extends MessageObject>(request: MessageObject): Promise<TResponse> {
        return GameApp.Network.GetNetworkChannel('network').Call<TResponse>(request);
    }

    private fireChanged(): void {
        EventSystems.Fire(RockPaperScissorsDataManager.Instance, RockPaperScissorsGameChangedEventArgs.Create());
    }
}
