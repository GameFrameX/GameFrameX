import type IUIFormHelper from "../src/gameframex/ui/IUIFormHelper";
import type { UIFormCtor } from "../src/gameframex/ui/IUIFormHelper";
import type UIForm from "../src/gameframex/ui/UIForm";
/**
 * 测试用界面助手:不依赖任何 UI 引擎,记录生命周期调用。
 */
export default class FakeUIFormHelper implements IUIFormHelper {
    public readonly calls: string[] = [];
    public readonly released: UIForm[] = [];

    public async InstantiateUIForm(_packageName: string, _resName: string): Promise<unknown> {
        this.calls.push('instantiate');
        return { fake: 'gobject' };
    }

    public async CreateUIForm<T extends UIForm>(instance: unknown, _formCtor: UIFormCtor<T>, uiForm: T): Promise<void> {
        this.calls.push('create');
        uiForm.Handle = instance;
        uiForm.InitView();
    }

    public ReleaseUIForm(uiForm: UIForm): void {
        this.calls.push('release');
        this.released.push(uiForm);
        uiForm.Handle = null;
    }

    public Show(_uiForm: UIForm, complete: () => void): void {
        this.calls.push('show');
        complete();
    }

    public Hide(_uiForm: UIForm, complete: () => void): void {
        this.calls.push('hide');
        complete();
    }
}
