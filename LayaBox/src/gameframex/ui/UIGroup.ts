import UIForm from "./UIForm";
import IUIGroup from "./IUIGroup";

/**
 * 界面组。
 *
 * 对照 Unity `GameFrameX.UI.Runtime.UIGroup` 的遮挡/暂停转移语义:
 * - 栈顶界面:活跃可见;
 * - 除栈顶外全部界面视为被覆盖(covered):进入时 OnCover,离开 covered 态时 OnReveal;
 * - 自顶向下第一个 PauseCoveredUIForm 界面之下的全部界面视为被暂停(paused):
 *   进入时 OnPause,离开时 OnResume。
 * 移除界面时:被移除界面先 OnCover + OnPause(保持视觉栈一致),再交由 UIManager 走 Hide/OnClose。
 */
export default class UIGroup implements IUIGroup {
    private readonly m_UIForms: UIForm[] = [];

    /**
     * 组名。
     */
    public readonly Name: string;

    /**
     * 组深度(数值大者更底层)。
     */
    public readonly Depth: number;

    /**
     * 组内界面数量。
     */
    public get UIFormCount(): number {
        return this.m_UIForms.length;
    }

    /**
     * 当前组内最顶界面。
     */
    public get CurrentUIForm(): UIForm | null {
        return this.m_UIForms.length > 0 ? this.m_UIForms[this.m_UIForms.length - 1] : null;
    }

    constructor(name: string, depth: number) {
        this.Name = name;
        this.Depth = depth;
    }

    /**
     * 是否存在指定界面。
     * @param uiForm 界面
     */
    public InternalHasInstanceUIForm(name: string, uiForm: UIForm): boolean {
        return this.m_UIForms.includes(uiForm) || this.m_UIForms.some((f) => f.Name === name);
    }

    /**
     * 加入界面到栈顶并刷新遮挡状态。
     * @param uiForm 界面
     */
    public AddUIForm(uiForm: UIForm): void {
        this.m_UIForms.push(uiForm);
        uiForm.UIGroup = this;
        uiForm.DepthInUIGroup = this.m_UIForms.length;
        uiForm.OnDepthChanged(this.Depth, uiForm.DepthInUIGroup);
        this.Refresh();
    }

    /**
     * 移除界面:先对其触发 OnCover/OnPause(维持关闭时序),再刷新剩余界面。
     * @param uiForm 界面
     */
    public RemoveUIForm(uiForm: UIForm): void {
        const index = this.m_UIForms.indexOf(uiForm);
        if (index < 0) {
            return;
        }

        if (!uiForm.__covered) {
            uiForm.OnCover();
        }

        if (uiForm.Available) {
            uiForm.OnPause();
        }

        this.m_UIForms.splice(index, 1);
        uiForm.__covered = false;
        uiForm.__paused = false;
        uiForm.OnDepthChanged(this.Depth, 0);
        this.Refresh();
    }

    /**
     * 重新聚焦:移到栈顶。
     * @param uiForm 界面
     */
    public RefocusUIForm(uiForm: UIForm): void {
        const index = this.m_UIForms.indexOf(uiForm);
        if (index < 0 || index === this.m_UIForms.length - 1) {
            return;
        }

        this.m_UIForms.splice(index, 1);
        this.m_UIForms.push(uiForm);
        uiForm.OnRefocus(uiForm.UserData);
        this.Refresh();
    }

    /**
     * 组内界面列表。
     */
    public GetUIForms(): UIForm[] {
        return [...this.m_UIForms];
    }

    /**
     * 组内界面轮询。
     * @param elapseSeconds 逻辑流逝秒
     * @param realElapseSeconds 真实流逝秒
     */
    public Update(elapseSeconds: number, realElapseSeconds: number): void {
        for (const form of this.m_UIForms) {
            form.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }

    /**
     * 刷新遮挡/暂停状态并触发转移回调。
     */
    private Refresh(): void {
        const covered = new Set<UIForm>();
        const paused = new Set<UIForm>();
        // 自顶向下:除栈顶外全部 covered;从第一个 PauseCoveredUIForm 的被盖界面起(含),
        // 之下全部 paused(对照 Unity:旧栈顶若 PauseCoveredUIForm → OnPause)
        let pauseBarrierReached = false;
        for (let i = this.m_UIForms.length - 1; i >= 0; i--) {
            const form = this.m_UIForms[i];
            if (i < this.m_UIForms.length - 1) {
                covered.add(form);
                if (!pauseBarrierReached && form.PauseCoveredUIForm) {
                    pauseBarrierReached = true;
                }

                if (pauseBarrierReached) {
                    paused.add(form);
                }
            }
        }
        for (const form of this.m_UIForms) {
            const isCovered = covered.has(form);
            const isPaused = paused.has(form);

            if (isCovered && !form.__covered) {
                form.OnCover();
            } else if (!isCovered && form.__covered) {
                form.OnReveal();
            }

            if (isPaused && !form.__paused) {
                form.OnPause();
            } else if (!isPaused && form.__paused) {
                form.OnResume();
            }

            form.__covered = isCovered;
            form.__paused = isPaused;
        }
    }
}
