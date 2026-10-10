import UIForm from "../../../../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../../../../src/gameframex/ui/UIGroupConstants";
import GameApp from "../../../../src/gameframex/GameApp";
import type GameEventArgs from "../../../../src/gameframex/event/GameEventArgs";
import RoomDataManager from "../../../Manager/RoomDataManager";
import type { Room } from "../../../../src/gameframex/protobuf/_0400_Room";
import { RoomChangedEventArgs } from "../../../Events/HotfixEventArgs";
import UIRoomBattlePanel from "./UIRoomBattlePanel";
import { RoomUIFormatter } from "./RoomUIFormatter";

const ROOM_LIST_REFRESH_INTERVAL_MS = 5000;

/**
 * 房间列表面板。
 *
 * 对照 Unity `UIRoomListPanel` + UIRoomListPanelController:
 * 5s 定时刷新 + RoomChanged 事件刷新;创建/加入房间后进入对战面板。
 */
export default class UIRoomListPanel extends UIForm {
    public static readonly UIPackageName: string = 'UIRoom';
    public static readonly UIResName: string = 'UIRoomListPanel';

    public m_summary_text!: fgui.GTextField;
    public m_tips_text!: fgui.GTextField;
    public m_room_list!: fgui.GList;
    public m_create_button!: fgui.GButton;
    public m_refresh_button!: fgui.GButton;
    public m_close_button!: fgui.GButton;

    private refreshTimer: number | null = null;
    private readonly onRoomChanged = (_sender: unknown, _args: GameEventArgs): void => {
        this.refresh();
        void this.enterBattleIfAlreadyInRoom();
    };

    /**
     * 视图绑定。
     */
    public InitView(): void {
        const self = this.Handle as fgui.GComponent;
        this.m_summary_text = self.getChild('summary_text') as fgui.GTextField;
        this.m_tips_text = self.getChild('tips_text') as fgui.GTextField;
        this.m_room_list = self.getChild('room_list') as fgui.GList;
        this.m_create_button = self.getChild('create_button') as fgui.GButton;
        this.m_refresh_button = self.getChild('refresh_button') as fgui.GButton;
        this.m_close_button = self.getChild('close_button') as fgui.GButton;
    }

    /**
     * 挂 Window 组。
     */
    public OnAwake(): void {
        this.UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Window.Name, UIGroupConstants.Window.Depth);
        super.OnAwake();
    }

    /**
     * 打开:绑定按钮/列表渲染,启动刷新循环。
     * @param userData 用户数据
     */
    public OnOpen(userData: unknown): void {
        super.OnOpen(userData);
        this.Subscribe(RoomChangedEventArgs.EventId, this.onRoomChanged);

        this.m_refresh_button.onClick(this, (): void => void RoomDataManager.Instance.RequestRoomList());
        this.m_create_button.onClick(this, (): void => void this.createRoomAndEnter());
        this.m_close_button.onClick(this, (): void => GameApp.UI.CloseUIForm(this));
        this.m_room_list.itemRenderer = (index: number, item: fgui.GObject): void => this.renderRoomItem(index, item);
        this.m_room_list.on(fgui.Events.CLICK_ITEM, this, this.onRoomItemClick);

        this.refresh();
        void RoomDataManager.Instance.RequestRoomList();
        void this.enterBattleIfAlreadyInRoom();
        this.refreshTimer = setInterval((): void => {
            void RoomDataManager.Instance.RequestRoomList();
        }, ROOM_LIST_REFRESH_INTERVAL_MS);
    }

    /**
     * 关闭:停刷新循环(事件退订由 Dispose 统一处理)。
     * @param isShutdown 是否关闭整组
     * @param userData 用户数据
     */
    public OnClose(isShutdown: boolean, userData: unknown): void {
        if (this.refreshTimer != null) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
        }

        super.OnClose(isShutdown, userData);
    }

    private renderRoomItem(index: number, item: fgui.GObject): void {
        const rooms = RoomDataManager.Instance.Rooms;
        const room = rooms[index];
        if (room == null) {
            return;
        }

        const component = item as fgui.GComponent;
        UIRoomListPanel.itemRooms.set(component, room);
        const nameText = component.getChild('name_text') as fgui.GTextField | null;
        if (nameText != null) {
            nameText.text = `${RoomUIFormatter.GetRoomName(room)} (${room.PlayerCount}/${room.MaxPlayerCount})`;
        }

        const stateText = component.getChild('state_text') as fgui.GTextField | null;
        if (stateText != null) {
            stateText.text = RoomUIFormatter.GetRoomStatusText(room.Status);
        }
    }

    private static readonly itemRooms: WeakMap<fgui.GObject, Room.RoomInfo> = new WeakMap();

    private onRoomItemClick(context: { data?: unknown }): void {
        const component = context.data as fgui.GComponent | undefined;
        const room = component != null ? UIRoomListPanel.itemRooms.get(component) : undefined;
        if (room != null) {
            void this.joinRoomAndEnter(room.RoomId);
        }
    }

    private async createRoomAndEnter(): Promise<void> {
        await RoomDataManager.Instance.CreateRoom(RoomUIFormatter.CreateRoomName());
        await this.enterBattleIfAlreadyInRoom();
    }

    private async joinRoomAndEnter(roomId: number): Promise<void> {
        await RoomDataManager.Instance.JoinRoom(roomId);
        await this.enterBattleIfAlreadyInRoom();
    }

    private async enterBattleIfAlreadyInRoom(): Promise<void> {
        if (RoomDataManager.Instance.CurrentRoom == null) {
            return;
        }

        GameApp.UI.CloseUIForm(this);
        await GameApp.UI.OpenAsync(UIRoomBattlePanel, { packageName: UIRoomBattlePanel.UIPackageName, resName: UIRoomBattlePanel.UIResName }, null, { groupName: UIGroupConstants.Window.Name });
    }

    private refresh(): void {
        const manager = RoomDataManager.Instance;
        this.m_summary_text.text = `当前玩家:${manager.CurrentRoleId}  房间数:${manager.Rooms.length}`;
        this.m_tips_text.text = manager.CurrentRoom == null
            ? '加入或创建房间后会进入对战界面。'
            : '你已在房间中,正在进入对战界面。';
        this.m_room_list.numItems = manager.Rooms.length;
    }
}
