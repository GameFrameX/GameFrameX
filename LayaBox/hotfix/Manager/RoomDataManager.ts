import GameApp from "../../src/gameframex/GameApp";
import EventSystems from "../../src/gameframex/event/EventSystems";
import LogHelper from "../../src/gameframex/log/LogHelper";
import type MessageObject from "../../src/gameframex/network/MessageObject";
import { MessageHandler } from "../../src/gameframex/network/ProtoMessageHandler";
import ProtoMessageHandler from "../../src/gameframex/network/ProtoMessageHandler";
import { Room } from "../../src/gameframex/protobuf/_0400_Room";
import { RoomChangedEventArgs } from "../Events/HotfixEventArgs";
import PlayerManager from "./PlayerManager";

/**
 * 房间数据管理器。
 *
 * 对照 Unity `Hotfix.Manager.RoomDataManager`:房间列表/进出房间/房间状态,
 * NotifyRoomChanged 驱动,变化发 RoomChangedEventArgs。
 */
export default class RoomDataManager {
    private static _instance: RoomDataManager | null = null;

    /** 全局单例(构造时注册消息)。 */
    public static get Instance(): RoomDataManager {
        this._instance ??= new RoomDataManager();
        return this._instance;
    }

    private readonly rooms: Room.RoomInfo[] = [];
    private currentRoom: Room.RoomInfo | null = null;

    private constructor() {
        ProtoMessageHandler.add(this);
    }

    /**
     * 当前所在房间。
     */
    public get CurrentRoom(): Room.RoomInfo | null {
        return this.currentRoom;
    }

    /**
     * 房间列表。
     */
    public get Rooms(): Room.RoomInfo[] {
        return this.rooms;
    }

    /**
     * 邀请文案。
     */
    public get InviteText(): string {
        return this.currentRoom == null ? '' : `邀请你加入房间 ${this.currentRoom.RoomId}:${this.currentRoom.Name}`;
    }

    /**
     * 当前角色 Id。
     */
    public get CurrentRoleId(): number {
        return PlayerManager.Instance.PlayerInfo == null ? 0 : PlayerManager.Instance.PlayerInfo.Id;
    }

    /**
     * 请求房间列表。
     * @param gameType 游戏类型(默认猜拳)
     */
    public async RequestRoomList(gameType: Room.GameType = Room.GameType.RockPaperScissors): Promise<void> {
        const request = new Room.ReqRoomList();
        request.GameType = gameType;
        request.IncludeClosed = false;
        const response = await this.networkCall<Room.RespRoomList>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`请求房间列表失败:${response.ErrorCode}`);
            return;
        }

        this.rooms.length = 0;
        this.rooms.push(...((response.Rooms ?? []) as unknown as Room.RoomInfo[]));
        if (RoomDataManager.isVisibleRoom(this.currentRoom) && !this.rooms.some((m) => m.RoomId === this.currentRoom!.RoomId)) {
            this.rooms.push(this.currentRoom!);
        }

        this.fireChanged();
    }

    /**
     * 创建房间。
     * @param roomName 房间名
     */
    public async CreateRoom(roomName: string = ''): Promise<void> {
        const request = new Room.ReqCreateRoom();
        request.GameType = Room.GameType.RockPaperScissors;
        request.Name = roomName;
        request.MinPlayerCount = 2;
        request.MaxPlayerCount = 2;
        const response = await this.networkCall<Room.RespCreateRoom>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`创建房间失败:${response.ErrorCode}`);
            return;
        }

        this.currentRoom = response.Room;
        this.upsertRoom(response.Room);
        this.fireChanged();
    }

    /**
     * 加入房间。
     * @param roomId 房间 Id
     */
    public async JoinRoom(roomId: number): Promise<void> {
        const request = new Room.ReqJoinRoom();
        request.RoomId = roomId;
        const response = await this.networkCall<Room.RespJoinRoom>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`加入房间失败:${response.ErrorCode}`);
            return;
        }

        this.currentRoom = response.Room;
        this.upsertRoom(response.Room);
        this.fireChanged();
    }

    /**
     * 退出房间。
     */
    public async LeaveRoom(): Promise<void> {
        if (this.currentRoom == null) {
            return;
        }

        const request = new Room.ReqLeaveRoom();
        request.RoomId = this.currentRoom.RoomId;
        const response = await this.networkCall<Room.RespLeaveRoom>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`退出房间失败:${response.ErrorCode}`);
            return;
        }

        if (RoomDataManager.isVisibleRoom(response.Room)) {
            this.upsertRoom(response.Room);
        } else if (response.Room != null) {
            this.removeRoom(response.Room.RoomId);
        }

        this.currentRoom = null;
        this.fireChanged();
    }

    /**
     * 开始游戏。
     */
    public async StartGame(): Promise<void> {
        if (this.currentRoom == null) {
            return;
        }

        const request = new Room.ReqStartRoomGame();
        request.RoomId = this.currentRoom.RoomId;
        const response = await this.networkCall<Room.RespStartRoomGame>(request);
        if (response.ErrorCode !== 0) {
            LogHelper.log(`开始游戏失败:${response.ErrorCode}`);
            return;
        }

        this.currentRoom = response.Room;
        this.upsertRoom(response.Room);
        this.fireChanged();
    }

    /**
     * 当前玩家是否房主。
     */
    public IsCurrentPlayerOwner(): boolean {
        return this.currentRoom != null && this.currentRoom.OwnerRoleId === this.CurrentRoleId;
    }

    /**
     * 是否可开始游戏。
     */
    public CanStartGame(): boolean {
        return this.currentRoom != null && this.IsCurrentPlayerOwner() && this.currentRoom.Status === Room.RoomStatus.Ready;
    }

    /**
     * 对手房间玩家信息。
     */
    public GetOpponentRoomPlayer(): Room.RoomPlayerInfo | null {
        if (this.currentRoom == null) {
            return null;
        }

        const roleId = this.CurrentRoleId;
        return ((this.currentRoom.Players ?? []) as unknown as Room.RoomPlayerInfo[]).find((m) => m.RoleId !== roleId) ?? null;
    }

    /**
     * 监听房间变化通知。
     */
    @MessageHandler(Room.NotifyRoomChanged)
    private NotifyRoomChanged(msg: Room.NotifyRoomChanged): void {
        if (msg.Room == null) {
            return;
        }

        if (this.currentRoom != null && this.currentRoom.RoomId === msg.Room.RoomId) {
            if (RoomDataManager.isVisibleRoom(msg.Room)) {
                this.currentRoom = msg.Room;
                this.upsertRoom(msg.Room);
            } else {
                this.currentRoom = null;
                this.removeRoom(msg.Room.RoomId);
            }
        } else if (RoomDataManager.isVisibleRoom(msg.Room)) {
            this.upsertRoom(msg.Room);
        } else {
            this.removeRoom(msg.Room.RoomId);
        }

        this.fireChanged();
    }

    private upsertRoom(room: Room.RoomInfo): void {
        if (!RoomDataManager.isVisibleRoom(room)) {
            return;
        }

        const index = this.rooms.findIndex((m) => m.RoomId === room.RoomId);
        if (index >= 0) {
            this.rooms[index] = room;
            return;
        }

        this.rooms.push(room);
    }

    private removeRoom(roomId: number): void {
        const index = this.rooms.findIndex((m) => m.RoomId === roomId);
        if (index >= 0) {
            this.rooms.splice(index, 1);
        }
    }

    private static isVisibleRoom(room: Room.RoomInfo | null): boolean {
        return room != null && room.Status !== Room.RoomStatus.Closed && room.Status !== Room.RoomStatus.Disbanded;
    }

    private async networkCall<TResponse extends MessageObject>(request: MessageObject): Promise<TResponse> {
        return GameApp.Network.GetNetworkChannel('network').Call<TResponse>(request);
    }

    private fireChanged(): void {
        EventSystems.Fire(RoomDataManager.Instance, RoomChangedEventArgs.Create());
    }
}
