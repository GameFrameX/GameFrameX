import type UIForm from "./UIForm";

/** UI 界面构造函数。 */
export type UIFormCtor<T extends UIForm = UIForm> = new () => T;

/**
 * 界面实例化助手接口(引擎实现:FairyGUI / 预留 UGUI 式后端)。
 *
 * 对照 Unity `UIFormHelperBase` 的三个职责:实例化、创建逻辑对象、释放。
 */
export default interface IUIFormHelper {
    /** 实例化界面显示对象(如 UIPackage.CreateObject)。 */
    InstantiateUIForm(packageName: string, resName: string): Promise<unknown>;

    /** 把显示对象与逻辑类接合并挂入界面组(触发 OnAwake/InitView/组根 AddChild)。 */
    CreateUIForm<T extends UIForm>(uiFormInstance: unknown, formCtor: UIFormCtor<T>, uiForm: T): Promise<void>;

    /** 释放界面显示对象。 */
    ReleaseUIForm(uiForm: UIForm): void;

    /** 显示界面(含打开动画;动画完成后回调)。 */
    Show(uiForm: UIForm, complete: () => void): void;

    /** 隐藏界面(含关闭动画;动画完成后回调)。 */
    Hide(uiForm: UIForm, complete: () => void): void;
}

/**
 * 界面组助手接口:创建组根显示容器。
 */
export interface IUIGroupHelper {
    /** 创建组根容器并挂到舞台。 */
    CreateGroupContainer(groupName: string, depth: number): unknown;

    /** 组内深度排序变化时同步显示层级。 */
    RefreshGroupContainer(groupName: string): void;

    /** 移除组根容器。 */
    RemoveGroupContainer(groupName: string): void;
}
