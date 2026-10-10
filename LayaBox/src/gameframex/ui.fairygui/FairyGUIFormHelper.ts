import type IUIFormHelper from "../ui/IUIFormHelper";
import type { UIFormCtor } from "../ui/IUIFormHelper";
import type UIForm from "../ui/UIForm";
import type { GObjectLike, GComponentLike } from "./FairyGUIPackageComponent";
import FairyGUIPackageComponent from "./FairyGUIPackageComponent";
declare const fgui: {
    UIPackage: {
        getByName(name: string): unknown;
        createObject(pkgName: string, resName: string): GObjectLike;
    };
    GComponent: new () => GComponentLike;
    GRoot: { inst: GComponentLike };
};

/** 组根容器注册表:组名 → GComponent。 */
const groupContainers = new Map<string, GComponentLike>();

/**
 * FairyGUI 界面助手。
 *
 * 对照 Unity `FairyGUIFormHelper` + `FairyGUIUIGroupHelper`:
 * InstantiateUIForm=UIPackage.CreateObject(包未加载先经包组件加载);
 * CreateUIForm=注入 Handle → InitView 绑定成员 → 挂组容器(容器挂 GRoot.inst,
 * sortingOrder = -depth,深度大者更底层,对齐 Unity SetDepth z=depth*100 语义);
 * Show/Hide 当前为立即显隐(EnableShow/HideAnimation 为动画插槽)。
 */
export default class FairyGUIFormHelper implements IUIFormHelper {
    /**
     * 实例化界面显示对象。
     * @param packageName 包名
     * @param resName 组件资源名
     */
    public async InstantiateUIForm(packageName: string, resName: string): Promise<unknown> {
        if (fgui.UIPackage.getByName(packageName) == null) {
            await FairyGUIPackageComponent.instance.AddPackageAsync(`fairygui/${packageName}`);
        }

        return fgui.UIPackage.createObject(packageName, resName);
    }

    /**
     * 接合显示对象与逻辑界面:注入 Handle → InitView 绑定成员 → 挂组容器。
     * @param uiFormInstance 显示对象(GObject)
     * @param _formCtor 界面类(实例由 UIManager 创建,形参对齐 Unity 形状)
     * @param uiForm 界面实例
     */
    public async CreateUIForm<T extends UIForm>(uiFormInstance: unknown, _formCtor: UIFormCtor<T>, uiForm: T): Promise<void> {
        const gObject = uiFormInstance as GObjectLike;
        uiForm.Handle = gObject;
        uiForm.InitView();

        const group = uiForm.UIGroup;
        if (group != null) {
            const container = ensureGroupContainer(group.Name, group.Depth);
            container.addChild(gObject);
            if (uiForm.IsCenter) {
                gObject.center();
            }
        }
    }

    /**
     * 释放界面显示对象。
     * @param uiForm 界面
     */
    public ReleaseUIForm(uiForm: UIForm): void {
        const gObject = uiForm.Handle as GObjectLike | null;
        gObject?.dispose();
        uiForm.Handle = null;
    }

    /**
     * 显示界面(当前立即显隐;EnableShowAnimation 为动画注入点)。
     * @param uiForm 界面
     * @param complete 完成回调
     */
    public Show(uiForm: UIForm, complete: () => void): void {
        setVisible(uiForm, true, complete);
    }

    /**
     * 隐藏界面(当前立即显隐;EnableHideAnimation 为动画注入点)。
     * @param uiForm 界面
     * @param complete 完成回调
     */
    public Hide(uiForm: UIForm, complete: () => void): void {
        setVisible(uiForm, false, complete);
    }
}

function setVisible(uiForm: UIForm, visible: boolean, complete: () => void): void {
    const gObject = uiForm.Handle as GObjectLike | null;
    if (gObject != null) {
        gObject.visible = visible;
    }

    complete();
}

function ensureGroupContainer(groupName: string, depth: number): GComponentLike {
    let container = groupContainers.get(groupName);
    if (container == null) {
        container = new fgui.GComponent();
        container.name = groupName;
        container.sortingOrder = -depth;
        fgui.GRoot.inst.addChild(container);
        groupContainers.set(groupName, container);
    }

    return container;
}

/** 供测试与外部清理组容器。 */
export function clearGroupContainers(): void {
    for (const container of groupContainers.values()) {
        container.dispose();
    }

    groupContainers.clear();
}
