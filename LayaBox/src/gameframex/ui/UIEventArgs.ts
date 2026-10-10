import GameEventArgs from "../event/GameEventArgs";
import type UIForm from "../ui/UIForm";

/**
 * 界面打开完成事件参数。
 */
export class UIFormOpenedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'UIFormOpenedEventArgs';

    public static Create(uiForm: UIForm): UIFormOpenedEventArgs {
        const args = new UIFormOpenedEventArgs();
        args.UIForm = uiForm;
        return args;
    }

    public get Id(): string {
        return UIFormOpenedEventArgs.EventId;
    }

    /** 打开的界面。 */
    public UIForm!: UIForm;
}

/**
 * 界面关闭完成事件参数。
 */
export class UIFormClosedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'UIFormClosedEventArgs';

    public static Create(uiForm: UIForm): UIFormClosedEventArgs {
        const args = new UIFormClosedEventArgs();
        args.UIForm = uiForm;
        return args;
    }

    public get Id(): string {
        return UIFormClosedEventArgs.EventId;
    }

    /** 关闭的界面。 */
    public UIForm!: UIForm;
}
