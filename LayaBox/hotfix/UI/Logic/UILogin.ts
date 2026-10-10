import UIForm from "../../../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../../../src/gameframex/ui/UIGroupConstants";
import GameApp from "../../../src/gameframex/GameApp";
import { User } from "../../../src/gameframex/protobuf/_0300_User";
import LogHelper from "../../../src/gameframex/log/LogHelper";
import WebSocketTransport from "../../../src/gameframex/network/WebSocketTransport";
import NetworkChannel from "../../../src/gameframex/network/NetworkChannel";
/**
 * 登录界面。
 *
 * 对照 Unity `Assets/Hotfix/UI/Logic/UILogin/UILogin.cs` + FUI 绑定层(UILogin/Components/UILogin.cs)
 * 合并移植:绑定成员 m_ErrorText/m_enter/m_UserName/m_Password 与 InitView 内容逐名对齐。
 * 差异:Unity 走 HTTP(WebProtoBuff.Post);Laya 契约(spec §3.2)走 WS 通道 RPC。
 * 登录链后续环节(UIPlayerList/UIPlayerCreate/UIMain)在 Phase 6 移植,当前成功后仅记录并关闭自身。
 */
export default class UILogin extends UIForm {
    /** FairyGUI 包名(对照 FUI.UIPackageName)。 */
    public static readonly UIPackageName: string = 'UILogin';

    /** 包内组件资源名(对照 FUI.UIResName)。 */
    public static readonly UIResName: string = 'UILogin';

    /** 绑定成员(对照 Unity FUI 生成绑定层)。 */
    public m_ErrorText!: { text: string };
    public m_enter!: { onClick(caller: unknown, listener: () => void): void };
    public m_UserName!: { text: string };
    public m_Password!: { text: string };
    /** 服务器地址(Phase 5 启动流程接管为全局配置;当前默认本地联调)。 */
    public static ServerUrl: string = 'ws://127.0.0.1:39110';


    /**
     * 视图绑定:GetChild 逐名对齐 Unity 绑定层 InitView。
     */
    public InitView(): void {
        const self = this.Handle as {
            getChild(name: string): unknown;
        };
        this.m_ErrorText = self.getChild('ErrorText') as { text: string };
        this.m_enter = self.getChild('enter') as { onClick(caller: unknown, listener: () => void): void };
        this.m_UserName = self.getChild('UserName') as { text: string };
        this.m_Password = self.getChild('Password') as { text: string };
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

        const req = new User.ReqLogin();
        req.SdkType = 0;
        req.SdkToken = '';
        req.UserName = userName;
        req.Password = password;
        req.Platform = 'laya';

        const channel = this.ensureChannel();
        let respLogin: User.RespLogin;
        try {
            respLogin = await channel.Call<User.RespLogin>(req);
        } catch (error) {
            this.m_ErrorText.text = `登录失败:${error instanceof Error ? error.message : String(error)}`;
            return;
        }

        LogHelper.log(`登录成功: Id=${String(respLogin.Id)} RoleName=${respLogin.RoleName ?? ''}`);

        // Phase 6 接管:ReqPlayerList → UIPlayerList/UIPlayerCreate → UIMain
        GameApp.UI.CloseUIForm(this);
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
