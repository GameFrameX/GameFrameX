import UIForm from "../../../../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../../../../src/gameframex/ui/UIGroupConstants";
import GameApp from "../../../../src/gameframex/GameApp";
import type GameEventArgs from "../../../../src/gameframex/event/GameEventArgs";
import LogHelper from "../../../../src/gameframex/log/LogHelper";
import { Room } from "../../../../src/gameframex/protobuf/_0400_Room";
import { RockPaperScissors } from "../../../../src/gameframex/protobuf/_0410_RockPaperScissors";
import RoomDataManager from "../../../Manager/RoomDataManager";
import RockPaperScissorsDataManager from "../../../Manager/RockPaperScissorsDataManager";
import { RoomChangedEventArgs, RockPaperScissorsGameChangedEventArgs } from "../../../Events/HotfixEventArgs";
import UIRoomListPanel from "./UIRoomListPanel";
import { RoomUIFormatter } from "./RoomUIFormatter";

/**
 * 房间对战面板(猜拳)。
 *
 * 对照 Unity `UIRoomBattlePanel` + UIRoomBattlePanelController(合并为单类,对齐 C1 简化约定):
 * RoomChanged/GameChanged 事件全量刷新;出拳/开始/再来一局/退出;结算文案内联
 * (Unity 的 UIRoomResultPopup 弹窗简化为面板内"再来一局"按钮)。
 */
export default class UIRoomBattlePanel extends UIForm {
    public static readonly UIPackageName: string = 'UIRoom';
    public static readonly UIResName: string = 'UIRoomBattlePanel';

    public m_room_title_text!: fgui.GTextField;
    public m_room_state_text!: fgui.GTextField;
    public m_invite_text!: fgui.GTextField;
    public m_result_text!: fgui.GTextField;
    public m_arena_tips_text!: fgui.GTextField;
    public m_tips_text!: fgui.GTextField;
    public m_self_name_text!: fgui.GTextField;
    public m_self_state_text!: fgui.GTextField;
    public m_self_gesture_text!: fgui.GTextField;
    public m_opponent_name_text!: fgui.GTextField;
    public m_opponent_state_text!: fgui.GTextField;
    public m_opponent_gesture_text!: fgui.GTextField;
    public m_rock_button!: fgui.GButton;
    public m_scissors_button!: fgui.GButton;
    public m_paper_button!: fgui.GButton;
    public m_start_button!: fgui.GButton;
    public m_sync_button!: fgui.GButton;
    public m_invite_button!: fgui.GButton;
    public m_leave_button!: fgui.GButton;
    public m_restart_button: fgui.GButton | null = null;

    private readonly onRoomChanged = (_sender: unknown, _args: GameEventArgs): void => this.refresh();
    private readonly onGameChanged = (_sender: unknown, _args: GameEventArgs): void => this.refresh();

    /**
     * 视图绑定。
     */
    public InitView(): void {
        const self = this.Handle as fgui.GComponent;
        const child = (name: string): fgui.GObject | null => self.getChild(name) ?? null;
        this.m_room_title_text = child('room_title_text') as fgui.GTextField;
        this.m_room_state_text = child('room_state_text') as fgui.GTextField;
        this.m_invite_text = child('invite_text') as fgui.GTextField;
        this.m_result_text = child('result_text') as fgui.GTextField;
        this.m_arena_tips_text = child('arena_tips_text') as fgui.GTextField;
        this.m_tips_text = child('tips_text') as fgui.GTextField;
        this.m_self_name_text = child('self_name_text') as fgui.GTextField;
        this.m_self_state_text = child('self_state_text') as fgui.GTextField;
        this.m_self_gesture_text = child('self_gesture_text') as fgui.GTextField;
        this.m_opponent_name_text = child('opponent_name_text') as fgui.GTextField;
        this.m_opponent_state_text = child('opponent_state_text') as fgui.GTextField;
        this.m_opponent_gesture_text = child('opponent_gesture_text') as fgui.GTextField;
        this.m_rock_button = child('rock_button') as fgui.GButton;
        this.m_scissors_button = child('scissors_button') as fgui.GButton;
        this.m_paper_button = child('paper_button') as fgui.GButton;
        this.m_start_button = child('start_button') as fgui.GButton;
        this.m_sync_button = child('sync_button') as fgui.GButton;
        this.m_invite_button = child('invite_button') as fgui.GButton;
        this.m_leave_button = child('leave_button') as fgui.GButton;
        // 结果弹窗简化:编辑器若提供 restart_button 则作为"再来一局"入口
        const restart = child('restart_button');
        this.m_restart_button = restart != null ? restart as fgui.GButton : null;
    }

    /**
     * 挂 Window 组。
     */
    public OnAwake(): void {
        this.UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Window.Name, UIGroupConstants.Window.Depth);
        super.OnAwake();
    }

    /**
     * 打开:订阅事件 + 绑定按钮 + 首刷。
     * @param userData 用户数据
     */
    public OnOpen(userData: unknown): void {
        super.OnOpen(userData);
        this.Subscribe(RoomChangedEventArgs.EventId, this.onRoomChanged);
        this.Subscribe(RockPaperScissorsGameChangedEventArgs.EventId, this.onGameChanged);

        this.m_invite_button.onClick(this, this.copyInviteText);
        this.m_leave_button.onClick(this, (): void => void this.leaveRoomAndReturnList());
        this.m_start_button.onClick(this, (): void => void RoomDataManager.Instance.StartGame());
        this.m_sync_button.onClick(this, this.onSyncGameInfoClick);
        this.m_rock_button.onClick(this, (): void => void RockPaperScissorsDataManager.Instance.SubmitGesture(RockPaperScissors.RockPaperScissorsGesture.Rock));
        this.m_scissors_button.onClick(this, (): void => void RockPaperScissorsDataManager.Instance.SubmitGesture(RockPaperScissors.RockPaperScissorsGesture.Scissors));
        this.m_paper_button.onClick(this, (): void => void RockPaperScissorsDataManager.Instance.SubmitGesture(RockPaperScissors.RockPaperScissorsGesture.Paper));
        this.m_restart_button?.onClick(this, (): void => void RockPaperScissorsDataManager.Instance.RestartGame());

        if (RoomDataManager.Instance.CurrentRoom == null) {
            void this.returnList();
            return;
        }

        this.refresh();
        void RockPaperScissorsDataManager.Instance.SynchronizeCurrentGameInfo();
    }

    private refresh(): void {
        const room = RoomDataManager.Instance.CurrentRoom;
        if (room == null) {
            return;
        }

        const game = RockPaperScissorsDataManager.Instance.CurrentGame;
        this.m_room_title_text.text = `房间 #${room.RoomId}  ${RoomUIFormatter.GetRoomName(room)}`;
        this.m_room_state_text.text = `${RoomUIFormatter.GetRoomStatusText(room.Status)}  ${room.PlayerCount}/${room.MaxPlayerCount}  局数:${room.Round}  房主:${room.OwnerRoleId}`;
        this.m_invite_text.text = RoomDataManager.Instance.InviteText;
        this.m_result_text.text = this.getArenaResultText(room);
        this.m_arena_tips_text.text = this.getArenaTipsText(room);
        this.m_tips_text.text = this.getTipsText(room);

        const currentRoleId = RoomDataManager.Instance.CurrentRoleId;
        const players = (room.Players ?? []) as unknown as Room.RoomPlayerInfo[];
        this.refreshPlayer(players.find((m) => m.RoleId === currentRoleId) ?? null, true, room.Status, game);
        this.refreshPlayer(players.find((m) => m.RoleId !== currentRoleId) ?? null, false, room.Status, game);

        this.m_start_button.enabled = RoomDataManager.Instance.CanStartGame();
        this.m_sync_button.enabled = RoomUIFormatter.IsGameDataStatus(room.Status);
        const canGesture = RockPaperScissorsDataManager.Instance.CanSubmitGesture();
        this.m_rock_button.enabled = canGesture;
        this.m_scissors_button.enabled = canGesture;
        this.m_paper_button.enabled = canGesture;
        this.m_restart_button && (this.m_restart_button.enabled = RockPaperScissorsDataManager.Instance.CanRestartGame());
    }

    private refreshPlayer(player: Room.RoomPlayerInfo | null, isSelf: boolean, roomStatus: Room.RoomStatus, game: RockPaperScissors.RockPaperScissorsGameInfo | null): void {
        if (player == null) {
            if (isSelf) {
                this.m_self_name_text.text = '我';
                this.m_self_state_text.text = '空位';
                this.m_self_gesture_text.text = '-';
            } else {
                this.m_opponent_name_text.text = '等待玩家';
                this.m_opponent_state_text.text = '空位';
                this.m_opponent_gesture_text.text = '-';
            }

            return;
        }

        const gamePlayers = ((game?.Players ?? []) as unknown as RockPaperScissors.RockPaperScissorsPlayerInfo[]);
        const gesturePlayer = gamePlayers.find((m) => m.RoleId === player.RoleId) ?? null;
        const gestureText = RoomUIFormatter.GetGestureStateText(gesturePlayer, roomStatus);
        if (isSelf) {
            this.m_self_name_text.text = RoomUIFormatter.BuildPlayerName(player, true);
            this.m_self_state_text.text = gesturePlayer != null && gesturePlayer.HasGesture ? '已出拳' : '思考中';
            this.m_self_gesture_text.text = gestureText;
        } else {
            this.m_opponent_name_text.text = RoomUIFormatter.BuildPlayerName(player, false);
            this.m_opponent_state_text.text = gesturePlayer != null && gesturePlayer.HasGesture ? '已出拳' : '思考中';
            this.m_opponent_gesture_text.text = gestureText;
        }
    }

    private onSyncGameInfoClick(): void {
        const room = RoomDataManager.Instance.CurrentRoom;
        if (room == null) {
            return;
        }

        void RockPaperScissorsDataManager.Instance.RequestGameInfo(room.RoomId);
    }

    private async leaveRoomAndReturnList(): Promise<void> {
        await RoomDataManager.Instance.LeaveRoom();
        await this.returnList();
    }

    private async returnList(): Promise<void> {
        GameApp.UI.CloseUIForm(this);
        await GameApp.UI.OpenAsync(UIRoomListPanel, { packageName: UIRoomListPanel.UIPackageName, resName: UIRoomListPanel.UIResName }, null, { groupName: UIGroupConstants.Window.Name });
    }

    private copyInviteText(): void {
        // Web 剪贴板尽力而为;不可用时仅记录
        const text = RoomDataManager.Instance.InviteText;
        void navigator.clipboard?.writeText(text).catch((): void => void 0);
        LogHelper.log(text);
    }

    private getArenaResultText(room: Room.RoomInfo): string {
        if (room.Status === Room.RoomStatus.Settled || room.Status === Room.RoomStatus.Settling) {
            return RockPaperScissorsDataManager.Instance.GetResultText();
        }

        if (room.Status === Room.RoomStatus.Playing) {
            return '请选择出拳';
        }

        if (room.Status === Room.RoomStatus.Ready) {
            return RoomDataManager.Instance.IsCurrentPlayerOwner() ? '可以开始游戏' : '等待房主开始';
        }

        return '等待玩家加入';
    }

    private getArenaTipsText(room: Room.RoomInfo): string {
        if (room.Status === Room.RoomStatus.Playing) {
            return RockPaperScissorsDataManager.Instance.HasCurrentPlayerGesture() ? '你已出拳,等待对手。' : '选择石头、剪刀或布。';
        }

        if (room.Status === Room.RoomStatus.Settled) {
            return RoomDataManager.Instance.IsCurrentPlayerOwner() ? '结算已完成,可再来一局。' : '结算已完成,等待房主再开。';
        }

        return '房主开始后,双方选择石头、剪刀或布。';
    }

    private getTipsText(room: Room.RoomInfo): string {
        if (room.Status === Room.RoomStatus.Waiting) {
            return '可以复制邀请信息,邀请第二名玩家加入。';
        }

        if (room.Status === Room.RoomStatus.Ready) {
            return RoomDataManager.Instance.IsCurrentPlayerOwner() ? '玩家已就绪,可以开始游戏。' : '玩家已就绪,等待房主开始。';
        }

        return '房间内状态由服务器推送同步。';
    }
}
