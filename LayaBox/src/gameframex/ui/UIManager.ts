import UIForm from "./UIForm";
import type IUIFormHelper from "./IUIFormHelper";
import type { UIFormCtor } from "./IUIFormHelper";
import UIGroup from "./UIGroup";
import type IUIGroup from "./IUIGroup";
import IModule from "../IModule";
import EventSystems from "../event/EventSystems";
import { UIFormOpenedEventArgs, UIFormClosedEventArgs } from "./UIEventArgs";

/** 打开界面的资源描述。 */
export interface UIFormPath {
    /** FairyGUI 包名。 */
    packageName: string;
    /** 包内组件资源名。 */
    resName: string;
}

/** 打开选项。 */
export interface OpenUIFormOptions {
    /** 目标组(默认 Normal)。 */
    groupName?: string;
    /** 打开时是否暂停被覆盖界面(默认 true,挂在界面实例上)。 */
    pauseCoveredUIForm?: boolean;
    /** 是否全屏。 */
    isFullScreen?: boolean;
}

/**
 * 界面管理器(GameApp.UI)。
 *
 * 对照 Unity `UIComponent/FairyGUI UIManager`:
 * 打开时序固定 Init → OnOpen → BindEvent → LoadData → UpdateLocalization → Show(动画) → 组 Refresh;
 * 关闭时序 OnCover/OnPause → Hide(动画) → OnClose → 组 Refresh → 释放。
 * 打开/关闭完成经 EventSystems 发事件(UIFormOpened/UIFormClosed)。
 */
export default class UIManager implements IModule {
    private readonly groups: Map<string, UIGroup> = new Map();
    private readonly openForms: Map<number, UIForm> = new Map();
    private helper: IUIFormHelper | null = null;
    private serial: number = 0;

    /**
     * 注入引擎界面助手(FairyGUIFormHelper)。须在 OpenAsync 前调用。
     * @param helper 界面助手
     */
    public SetUIFormHelper(helper: IUIFormHelper): void {
        this.helper = helper;
    }

    /**
     * 是否存在界面组。
     * @param name 组名
     */
    public HasUIGroup(name: string): boolean {
        return this.groups.has(name);
    }

    /**
     * 获取界面组(不存在则按深度创建)。
     * @param name 组名
     * @param depth 组深度(仅创建时生效)
     */
    public GetUIGroup(name: string, depth: number = 10): IUIGroup {
        let group = this.groups.get(name);
        if (group == null) {
            group = new UIGroup(name, depth);
            this.groups.set(name, group);
        }

        return group;
    }

    /**
     * 全部界面组。
     */
    public GetAllUIGroups(): IUIGroup[] {
        return [...this.groups.values()];
    }

    /**
     * 打开界面。
     * @param formCtor 界面类
     * @param path 资源描述(包名/组件名)
     * @param userData 用户数据
     * @param options 打开选项
     * @returns 界面实例(时序已走完 OnOpen→BindEvent→LoadData)
     */
    public async OpenAsync<T extends UIForm>(formCtor: UIFormCtor<T>, path: UIFormPath, userData: unknown = null, options: OpenUIFormOptions = {}): Promise<T> {
        if (this.helper == null) {
            throw new Error('UIFormHelper not set (call UIManager.SetUIFormHelper first)');
        }

        const instance = await this.helper.InstantiateUIForm(path.packageName, path.resName);
        const uiForm = new formCtor();
        await this.helper.CreateUIForm(instance, formCtor, uiForm);

        if (!uiForm.IsAwake) {
            uiForm.OnAwake();
        }

        uiForm.PauseCoveredUIForm = options.pauseCoveredUIForm ?? true;
        uiForm.SerialId = ++this.serial;
        uiForm.Name = formCtor.name;
        uiForm.UIFormAssetName = path.resName;
        uiForm.AssetPath = `${path.packageName}/${path.resName}`;
        this.openForms.set(uiForm.SerialId, uiForm);
        uiForm.OnInit();

        const group = (uiForm.UIGroup ?? this.GetUIGroup(options.groupName ?? 'Normal')) as UIGroup;
        if (!group.InternalHasInstanceUIForm(uiForm.Name, uiForm)) {
            group.AddUIForm(uiForm);
        }

        uiForm.OnOpen(userData);
        uiForm.BindEvent();
        uiForm.LoadData();
        uiForm.UpdateLocalization();
        if (uiForm.EnableShowAnimation) {
            this.helper.Show(uiForm, (): void => void 0);
        }

        EventSystems.Fire(this, UIFormOpenedEventArgs.Create(uiForm));
        return uiForm;
    }

    /**
     * 关闭界面。
     * @param uiForm 界面实例
     * @param userData 用户数据
     */
    public CloseUIForm(uiForm: UIForm, userData: unknown = null): void {
        if (!this.openForms.has(uiForm.SerialId)) {
            return;
        }

        this.openForms.delete(uiForm.SerialId);
        uiForm.UIGroup?.RemoveUIForm(uiForm);
        const helper = this.helper;
        const finish = (): void => {
            uiForm.OnClose(false, userData);
            uiForm.Dispose();
            helper?.ReleaseUIForm(uiForm);
            EventSystems.Fire(this, UIFormClosedEventArgs.Create(uiForm));
        };

        if (uiForm.EnableHideAnimation && helper != null) {
            helper.Hide(uiForm, finish);
        } else {
            helper?.Hide(uiForm, finish);
        }
    }

    /**
     * 按序列号关闭界面。
     * @param serialId 序列号
     * @param userData 用户数据
     */
    public CloseUIFormBySerialId(serialId: number, userData: unknown = null): void {
        const form = this.openForms.get(serialId);
        if (form != null) {
            this.CloseUIForm(form, userData);
        }
    }

    /**
     * 关闭整组界面。
     * @param groupName 组名
     * @param userData 用户数据
     */
    public CloseUIGroup(groupName: string, userData: unknown = null): void {
        const group = this.groups.get(groupName);
        if (group == null) {
            return;
        }

        for (const form of group.GetUIForms()) {
            this.CloseUIForm(form, userData);
        }
    }

    /**
     * 模块初始化(由 GameEntry 调用)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块轮询:驱动全部组内界面 OnUpdate(由 GameEntry 每帧调用)。
     */
    public update(elapseSeconds: number, realElapseSeconds: number): void {
        for (const group of this.groups.values()) {
            group.Update(elapseSeconds, realElapseSeconds);
        }
    }

    /**
     * 模块关闭:关闭全部界面与组。
     */
    public shutdown(): void {
        for (const group of this.groups.values()) {
            for (const form of group.GetUIForms()) {
                form.OnClose(true, null);
                form.Dispose();
                this.helper?.ReleaseUIForm(form);
            }
        }

        this.groups.clear();
        this.openForms.clear();
    }
}
