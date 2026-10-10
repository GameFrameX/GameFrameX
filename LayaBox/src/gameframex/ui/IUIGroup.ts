import type UIForm from "./UIForm";

/**
 * 界面组接口。
 *
 * 对照 Unity `GameFrameX.UI.Runtime.IUIGroup`:组内界面栈、遮挡暂停语义由实现承载。
 */
export default interface IUIGroup {
    /** 组名。 */
    readonly Name: string;

    /** 组深度(数值大者更底层)。 */
    readonly Depth: number;

    /** 组内界面数量。 */
    readonly UIFormCount: number;

    /** 当前组内最顶界面。 */
    readonly CurrentUIForm: UIForm | null;

    /** 加入界面(栈顶)。 */
    AddUIForm(uiForm: UIForm): void;

    /** 移除界面。 */
    RemoveUIForm(uiForm: UIForm): void;

    /** 重新聚焦界面(移到栈顶)。 */
    RefocusUIForm(uiForm: UIForm): void;

    /** 组内界面列表。 */
    GetUIForms(): UIForm[];

    /** 组内界面轮询。 */
    Update(elapseSeconds: number, realElapseSeconds: number): void;
}
