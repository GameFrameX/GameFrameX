import GameEntry from "./GameEntry";
import HTTP from "./http/HTTP";
import FsmManager from "./fsm/FsmManager";
import ProcedureManager from "./procedure/ProcedureManager";
import EventSystems from "./event/EventSystems";
import TimerHelper from "./timer/TimerHelper";
import NetworkManager from "./network/NetworkManager";
import UIManager from "./ui/UIManager";
import ProtoMessageHelper from "./network/ProtoMessageHelper";
import LogHelper from "./log/LogHelper";
import FsmDomain from "./gameapp/FsmDomain";
import ProcedureDomain from "./gameapp/ProcedureDomain";
import EventDomain from "./gameapp/EventDomain";
import TimerDomain from "./gameapp/TimerDomain";
import NetworkDomain from "./gameapp/NetworkDomain";
import UIDomain from "./gameapp/UIDomain";
import ConfigLocalizationSoundDomains from "./gameapp/ConfigLocalizationSoundDomains";
import ConfigComponent from "./config/ConfigComponent";
import LocalizationComponent from "./localization/LocalizationComponent";
import SoundComponent from "./sound/SoundComponent";
import SettingComponent from "./setting/SettingComponent";
import SystemInfo from "./setting/SystemInfo";
import SettingDomain from "./gameapp/SettingDomain";
import { loadProtoRoot } from "./network/ProtoRootLoader";

/**
 * 游戏框架静态门面。
 *
 * 对照 Unity `GameApp`(partial 分文件):TS 侧以 gameapp/ 下分域注册函数组合,
 * 域命名与 Unity GameApp.* 逐一对应。Config/Localization/Sound/Setting 域随 Phase 4 接入。
 */
export default class GameApp {
    /** 事件系统。 */
    public static Event: EventSystems;

    /** 配置组件。 */
    public static Config: ConfigComponent;

    /** 本地化组件。 */
    public static Localization: LocalizationComponent;

    /** 音效组件。 */
    public static Sound: SoundComponent;

    /** 本地设置。 */
    public static Setting: SettingComponent;

    /** 系统信息(静态类直接挂载)。 */
    public static SystemInfo: typeof SystemInfo;

    /** 有限状态机管理器。 */
    public static Fsm: FsmManager;

    /** 流程管理器。 */
    public static Procedure: ProcedureManager;

    /** 网络管理器。 */
    public static Network: NetworkManager;

    /** 界面管理器。 */
    public static UI: UIManager;

    /** 时间工具(静态类直接挂载)。 */
    public static Timer: typeof TimerHelper;

    /** HTTP 工具(存量)。 */
    public static get http(): HTTP {
        return HTTP.instance;
    }

    private static _initialized: boolean = false;

    /**
     * 框架启动入口:分域注册模块、加载 protobuf 消息表、初始化 GameEntry。
     * 由 Main.ts 调用;重复调用直接返回。宿主未注入 globalThis.protobuf 时跳过消息表加载并记日志。
     */
    public static async initialize(): Promise<void> {
        if (this._initialized) {
            return;
        }

        this.Event = EventDomain();
        this.Fsm = FsmDomain();
        this.Network = NetworkDomain();
        this.Procedure = ProcedureDomain();
        this.Timer = TimerDomain();
        this.UI = UIDomain();
        this.Setting = SettingDomain();
        this.SystemInfo = SystemInfo;
        const configDomains = ConfigLocalizationSoundDomains();
        this.Config = configDomains.Config;
        this.Localization = configDomains.Localization;
        this.Sound = configDomains.Sound;

        if ((globalThis as Record<string, unknown>).protobuf != null) {
            await loadProtoRoot();
            LogHelper.log(`protobuf registered: ${ProtoMessageHelper.registeredCount} messages`);
        } else {
            LogHelper.log('globalThis.protobuf not present, skip proto load (host should call loadProtoRoot)');
        }

        await GameEntry.initialize();
        this._initialized = true;
    }

    /**
     * 关闭框架(逆序 shutdown 全部模块)。
     */
    public static shutdown(): void {
        GameEntry.shutdown();
        this._initialized = false;
    }
}
