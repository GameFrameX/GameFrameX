import UIForm from "../../../../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../../../../src/gameframex/ui/UIGroupConstants";
import GameApp from "../../../../src/gameframex/GameApp";
import LogHelper from "../../../../src/gameframex/log/LogHelper";
import NetworkChannel from "../../../../src/gameframex/network/NetworkChannel";
import WebSocketTransport from "../../../../src/gameframex/network/WebSocketTransport";
import { User } from "../../../../src/gameframex/protobuf/_0300_User";
import AccountManager from "../../../Manager/AccountManager";
import PlayerManager from "../../../Manager/PlayerManager";
import BagManager from "../../../Manager/BagManager";
import UIMain from "../UIMain/UIMain";

/**
 * 登录界面。
 *
 * 对照 Unity `Hotfix.UI.UILogin` + FUI 绑定层合并移植:m_ErrorText/m_enter/m_UserName/m_Password 逐名对齐。
 * 差异:Unity 走 HTTP 登录 + UIPlayerList/UIPlayerCreate 选角(UGUI-only 资产,FGUI 单轨不存在);
 * Laya 契约走 WS 通道:ReqLogin → ReqPlayerList(空则 ReqPlayerCreate)→ ReqPlayerLogin(自动选第一个角色)
 * → UIMain → 拉背包/属性(对齐 Unity UIPlayerList.Login 链)。
 */
export default class UILogin extends UIForm {
    /** FairyGUI 包名(对照 FUI.UIPackageName)。 */
    public static readonly UIPackageName: string = 'UILogin';

    /** 包内组件资源名(对照 FUI.UIResName)。 */
    public static readonly UIResName: string = 'UILogin';

    /** 服务器地址(Phase 5 启动流程接管为全局配置;当前默认本地联调)。 */
    public static ServerUrl: string = 'ws://127.0.0.1:39110';

    /** 绑定成员(对照 Unity FUI 生成绑定层)。 */
    public m_ErrorText!: fgui.GTextField;
    public m_enter!: fgui.GButton;
    public m_UserName!: fgui.GTextInput;
    public m_Password!: fgui.GTextInput;

    /**
     * 视图绑定:GetChild 逐名对齐 Unity 绑定层 InitView。
     */
    public InitView(): void {
        const self = this.Handle as fgui.GComponent;
        this.m_ErrorText = self.getChild('ErrorText') as fgui.GTextField;
        this.m_enter = self.getChild('enter') as fgui.GButton;
        this.m_UserName = self.getChild('UserName') as fgui.GTextInput;
        this.m_Password = self.getChild('Password') as fgui.GTextInput;
    }

    /**
     * 界面唤醒:挂 Normal 组(对照 Unity OnAwake)。
     */
    public OnAwake(): void {
        this.UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Normal.Name, UIGroupConstants.Normal.Depth);
        super.OnAwake();
    }

    /**
     * 界面打开:绑定登录按钮(对照 Unity OnOpen 的 m_enter.onClick.Set)。
     * @param userData 用户数据
     */
    public OnOpen(userData: unknown): void {
        this.m_enter.onClick(this, this.OnLoginClick);
        super.OnOpen(userData);
    }

    private OnLoginClick(): void {
        void this.Login();
    }

    private async Login(): Promise<void> {
        const userName = this.m_UserName.text;
        const password = this.m_Password.text;
        if (userName == null || userName.trim() === '' || password == null || password.trim() === '') {
            this.m_ErrorText.text = '用户名或密码不能为空';
            return;
        }

        const channel = this.ensureChannel();
        const request = new User.ReqLogin();
        request.SdkType = 0;
        request.SdkToken = '';
        request.UserName = userName;
        request.Password = password;
        request.Platform = 'laya';

        try {
            const respLogin = await channel.Call<User.RespLogin>(request);
            LogHelper.log(`登录成功: Id=${String(respLogin.Id)}`);

            // 角色列表(空则自动建角,对齐服务端最小流程)
            const reqPlayerList = new User.ReqPlayerList();
            reqPlayerList.Id = respLogin.Id;
            const respPlayerList = await channel.Call<User.RespPlayerList>(reqPlayerList);
            if ((respPlayerList.ErrorCode ?? 0) !== 0) {
                this.m_ErrorText.text = `角色列表失败:${respPlayerList.ErrorCode}`;
                return;
            }

            let playerList = (respPlayerList.PlayerList ?? []) as unknown as User.PlayerInfo[];
            if (playerList.length === 0) {
                const reqCreate = new User.ReqPlayerCreate();
                reqCreate.Id = respLogin.Id;
                reqCreate.Name = `${userName}-p1`;
                const respCreate = await channel.Call<User.RespPlayerCreate>(reqCreate);
                if ((respCreate.ErrorCode ?? 0) !== 0) {
                    this.m_ErrorText.text = `创建角色失败:${respCreate.ErrorCode}`;
                    return;
                }

                playerList = [respCreate.PlayerInfo];
            }

            AccountManager.Instance.PlayerList = playerList;

            // 自动选角登录(UGUI-only UIPlayerList 的 FGUI 单轨替代)
            const selected = playerList[0];
            const reqPlayerLogin = new User.ReqPlayerLogin();
            reqPlayerLogin.Id = selected.Id;
            const respPlayerLogin = await channel.Call<User.RespPlayerLogin>(reqPlayerLogin);
            if ((respPlayerLogin.ErrorCode ?? 0) !== 0) {
                this.m_ErrorText.text = `角色登录失败:${respPlayerLogin.ErrorCode}`;
                return;
            }

            PlayerManager.Instance.PlayerInfo = respPlayerLogin.PlayerInfo;
        } catch (error) {
            this.m_ErrorText.text = `登录失败:${error instanceof Error ? error.message : String(error)}`;
            return;
        }

        await GameApp.UI.OpenAsync(UIMain, { packageName: UIMain.UIPackageName, resName: UIMain.UIResName });
        GameApp.UI.CloseUIForm(this);
        await BagManager.Instance.RequestGetBagInfo();
    }

    private ensureChannel(): NetworkChannel {
        if (!GameApp.Network.HasNetworkChannel('network')) {
            const channel = new NetworkChannel('network', new WebSocketTransport(), { autoReconnect: true });
            GameApp.Network.CreateNetworkChannel(channel);
            channel.Connect(UILogin.ServerUrl);
        }

        return GameApp.Network.GetNetworkChannel('network');
    }
}
