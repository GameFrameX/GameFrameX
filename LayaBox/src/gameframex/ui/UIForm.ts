import EventSystems from "../event/EventSystems";
import type GameEventArgs from "../event/GameEventArgs";
import type IUIGroup from "./IUIGroup";

/**
 * 界面基类(引擎无关)。
 *
 * 对照 Unity `com.gameframex.unity.ui/UIForm.cs` 同名生命周期与遮挡语义:
 * OnAwake / OnInit / OnOpen(userData) / BindEvent / LoadData / UpdateLocalization /
 * OnClose(isShutdown, userData) / OnPause / OnResume / OnCover / OnReveal / OnRefocus / OnUpdate。
 * 打开时序(UIManager 固定):Init → OnOpen → BindEvent → LoadData → UpdateLocalization → Show → 组 Refresh。
 * 遮挡时序(UIGroup.Refresh):被盖 OnPause+OnCover,恢复 OnResume+OnReveal。
 *
 * 与 Unity 的必要差异:
 * - MonoBehaviour 挂钩(Awake/OnEnable/OnDestroy)并入显式生命周期,由 UIManager/Helper 驱动;
 * - 回收池(ReferencePool/RecycleInterval)由 GC 承担,保留属性占位;
 * - 事件订阅记录内置:Subscribe 经本类登记,Dispose 统一退订(对应 Unity UIEventSubscriber)。
 */
export default abstract class UIForm {
    /** 序列号(打开时分配)。 */
    public SerialId: number = 0;

    /** 界面资源路径。 */
    public AssetPath: string = '';

    /** 界面资源名。 */
    public UIFormAssetName: string = '';

    /** 界面名(默认类名)。 */
    public Name: string = '';

    /** 所属界面组。 */
    public UIGroup: IUIGroup | null = null;

    /** 打开时透传的用户数据。 */
    public UserData: unknown = null;

    /** 是否可用(未暂停/未关闭)。 */
    public Available: boolean = false;

    /** 是否可见。 */
    public Visible: boolean = false;

    /** 组内深度。 */
    public DepthInUIGroup: number = 0;

    /** 打开本界面时是否暂停被覆盖界面。 */
    public PauseCoveredUIForm: boolean = true;

    /** 是否已执行 OnAwake。 */
    public IsAwake: boolean = false;

    /** 是否居中显示。 */
    public IsCenter: boolean = false;

    /** 是否启用打开动画。 */
    public EnableShowAnimation: boolean = false;

    /** @internal 遮挡/暂停状态(UIGroup 内部维护)。 */
    public __covered: boolean = false;
    public __paused: boolean = false;
    /** 打开动画名。 */
    public ShowAnimationName: string = '';

    /** 是否启用关闭动画。 */
    public EnableHideAnimation: boolean = false;

    /** 关闭动画名。 */
    public HideAnimationName: string = '';

    /** 引擎显示对象(由 FormHelper 注入;基类不感知具体类型)。 */
    public Handle: unknown = null;
    private readonly subscribedEvents: { id: string; handler: (sender: unknown, args: GameEventArgs) => void }[] = [];
    /** 完整名(组名.界面名)。 */
    public get FullName(): string {
        return `${this.UIGroup?.Name ?? ''}.${this.Name}`;
    }


    /**
     * 界面唤醒(实例创建后调用一次;业务在此初始化实例字段,等价 Unity OnAwake)。
     */
    public OnAwake(): void {
        this.IsAwake = true;
    }

    /**
     * 界面初始化(每次打开前,池化复用时也会再次触发)。
     */
    public OnInit(): void { }

    /**
     * 界面回收清理(关闭并进入回收时)。
     */
    public OnRecycle(): void {
        this.DepthInUIGroup = 0;
        this.PauseCoveredUIForm = true;
    }

    /**
     * 界面打开。
     * @param userData 用户数据
     */
    public OnOpen(userData: unknown): void {
        this.Available = true;
        this.Visible = true;
        this.UserData = userData;
    }

    /**
     * 绑定界面事件(打开时序中 OnOpen 之后)。
     */
    public BindEvent(): void { }

    /**
     * 加载界面数据(打开时序中 BindEvent 之后;异步动作不阻塞打开)。
     */
    public LoadData(): void { }

    /**
     * 刷新本地化文本(打开与语言切换时)。
     */
    public UpdateLocalization(): void { }

    /**
     * 界面关闭。
     * @param isShutdown 是否是关闭界面组/管理器时触发
     * @param userData 用户数据
     */
    public OnClose(isShutdown: boolean, userData: unknown): void {
        this.Available = false;
        this.Visible = false;
        void isShutdown;
        void userData;
    }

    /**
     * 界面暂停(被更高界面覆盖且自身 PauseCoveredUIForm)。
     */
    public OnPause(): void {
        this.Available = false;
        this.Visible = false;
    }

    /**
     * 界面恢复(覆盖界面关闭后)。
     */
    public OnResume(): void {
        this.Available = true;
        this.Visible = true;
    }

    /**
     * 界面被覆盖(无论是否暂停,只要被更高界面盖住)。
     */
    public OnCover(): void { }

    /**
     * 界面恢复可见(不再被覆盖)。
     */
    public OnReveal(): void { }

    /**
     * 界面重新聚焦(单实例模式重复打开置顶)。
     * @param userData 用户数据
     */
    public OnRefocus(userData: unknown): void {
        void userData;
    }

    /**
     * 界面轮询。
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public OnUpdate(_elapseSeconds: number, _realElapseSeconds: number): void { }

    /**
     * 组内深度变化通知。
     * @param uiGroupDepth 组深度
     * @param depthInUIGroup 组内深度
     */
    public OnDepthChanged(uiGroupDepth: number, depthInUIGroup: number): void {
        void uiGroupDepth;
        void depthInUIGroup;
    }

    /**
     * 视图绑定钩子(引擎 Helper 注入 Handle 后调用;子类在此 GetChild 绑定成员,
     * 对应 Unity FUI 绑定层 InitView)。
     */
    public InitView(): void { }

    /**
     * 订阅事件并登记,Dispose 时统一退订(业务统一走此入口避免泄漏)。
     * @param id 事件类型标识
     * @param handler 处理函数
     */
    public Subscribe<E extends GameEventArgs>(id: string, handler: (sender: unknown, args: E) => void): void {
        const bound = handler as (sender: unknown, args: GameEventArgs) => void;
        this.subscribedEvents.push({ id, handler: bound });
        EventSystems.Subscribe(id, bound);
    }

    /**
     * 销毁界面:统一退订全部事件。
     */
    public Dispose(): void {
        for (const item of this.subscribedEvents) {
            EventSystems.Unsubscribe(item.id, item.handler);
        }


        this.subscribedEvents.length = 0;
    }
}
