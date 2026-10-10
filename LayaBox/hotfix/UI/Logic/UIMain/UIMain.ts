import UIForm from "../../../../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../../../../src/gameframex/ui/UIGroupConstants";
import GameApp from "../../../../src/gameframex/GameApp";
import PlayerManager from "../../../Manager/PlayerManager";
import UIBag from "../UIBag/UIBag";
import UIRoomListPanel from "../UIRoom/UIRoomListPanel";

/**
 * 主界面。
 *
 * 对照 Unity `Hotfix.UI.UIMain`:头像/昵称/等级展示;背包与房间入口。
 */
export default class UIMain extends UIForm {
    public static readonly UIPackageName: string = 'UIMain';
    public static readonly UIResName: string = 'UIMain';

    public m_player_name!: fgui.GTextField;
    public m_player_level!: fgui.GTextField;
    public m_bag_button!: fgui.GButton;
    public m_room_button!: fgui.GButton;

    /**
     * 视图绑定(对照 Unity FUI 生成绑定层 InitView)。
     */
    public InitView(): void {
        const self = this.Handle as fgui.GComponent;
        this.m_player_name = self.getChild('player_name') as fgui.GTextField;
        this.m_player_level = self.getChild('player_level') as fgui.GTextField;
        this.m_bag_button = self.getChild('bag_button') as fgui.GButton;
        const roomButton = self.getChild('room_button');
        if (roomButton != null) {
            this.m_room_button = roomButton as fgui.GButton;
        }
    }

    /**
     * 挂 Normal 组。
     */
    public OnAwake(): void {
        this.UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Normal.Name, UIGroupConstants.Normal.Depth);
        super.OnAwake();
    }

    /**
     * 打开:展示玩家信息并绑定入口按钮。
     * @param userData 用户数据
     */
    public OnOpen(userData: unknown): void {
        super.OnOpen(userData);
        const playerInfo = PlayerManager.Instance.PlayerInfo;
        this.m_player_name.text = playerInfo.Name;
        this.m_player_level.text = '当前等级:' + playerInfo.Level;
        this.m_bag_button.onClick(this, this.OnBagBtnClick);
        this.m_room_button?.onClick(this, this.OnRoomBtnClick);
    }

    private OnBagBtnClick(): void {
        void GameApp.UI.OpenAsync(UIBag, { packageName: UIBag.UIPackageName, resName: UIBag.UIResName }, null, { groupName: UIGroupConstants.Window.Name });
    }

    private OnRoomBtnClick(): void {
        void GameApp.UI.OpenAsync(UIRoomListPanel, { packageName: UIRoomListPanel.UIPackageName, resName: UIRoomListPanel.UIResName }, null, { groupName: UIGroupConstants.Window.Name });
    }
}
