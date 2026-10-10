/**
 * 界面组常量。
 *
 * 对照 Unity `com.gameframex.unity.ui` UIGroupConstants/UIGroupNameConstants 全量 18 组:
 * 名称=深度,深度大者更底层(Hidden 最底,System 最顶)。
 */
export class UIGroupNameConstants {
    public static readonly Hidden: string = 'Hidden';
    public static readonly Background: string = 'Background';
    public static readonly Scene: string = 'Scene';
    public static readonly World: string = 'World';
    public static readonly Battle: string = 'Battle';
    public static readonly Hud: string = 'Hud';
    public static readonly Map: string = 'Map';
    public static readonly Floor: string = 'Floor';
    public static readonly Normal: string = 'Normal';
    public static readonly Fixed: string = 'Fixed';
    public static readonly Window: string = 'Window';
    public static readonly Tip: string = 'Tip';
    public static readonly Guide: string = 'Guide';
    public static readonly BlackBoard: string = 'BlackBoard';
    public static readonly Dialogue: string = 'Dialogue';
    public static readonly Loading: string = 'Loading';
    public static readonly Notify: string = 'Notify';
    public static readonly System: string = 'System';
}

export class UIGroupDefine {
    constructor(public readonly Depth: number, public readonly Name: string) { }
}

export class UIGroupConstants {
    public static readonly Hidden: UIGroupDefine = new UIGroupDefine(40, UIGroupNameConstants.Hidden);
    public static readonly Background: UIGroupDefine = new UIGroupDefine(35, UIGroupNameConstants.Background);
    public static readonly Scene: UIGroupDefine = new UIGroupDefine(30, UIGroupNameConstants.Scene);
    public static readonly World: UIGroupDefine = new UIGroupDefine(27, UIGroupNameConstants.World);
    public static readonly Battle: UIGroupDefine = new UIGroupDefine(25, UIGroupNameConstants.Battle);
    public static readonly Hud: UIGroupDefine = new UIGroupDefine(22, UIGroupNameConstants.Hud);
    public static readonly Map: UIGroupDefine = new UIGroupDefine(20, UIGroupNameConstants.Map);
    public static readonly Floor: UIGroupDefine = new UIGroupDefine(15, UIGroupNameConstants.Floor);
    public static readonly Normal: UIGroupDefine = new UIGroupDefine(10, UIGroupNameConstants.Normal);
    public static readonly Fixed: UIGroupDefine = new UIGroupDefine(0, UIGroupNameConstants.Fixed);
    public static readonly Window: UIGroupDefine = new UIGroupDefine(-10, UIGroupNameConstants.Window);
    public static readonly Tip: UIGroupDefine = new UIGroupDefine(-15, UIGroupNameConstants.Tip);
    public static readonly Guide: UIGroupDefine = new UIGroupDefine(-20, UIGroupNameConstants.Guide);
    public static readonly BlackBoard: UIGroupDefine = new UIGroupDefine(-22, UIGroupNameConstants.BlackBoard);
    public static readonly Dialogue: UIGroupDefine = new UIGroupDefine(-23, UIGroupNameConstants.Dialogue);
    public static readonly Loading: UIGroupDefine = new UIGroupDefine(-25, UIGroupNameConstants.Loading);
    public static readonly Notify: UIGroupDefine = new UIGroupDefine(-30, UIGroupNameConstants.Notify);
    public static readonly System: UIGroupDefine = new UIGroupDefine(-35, UIGroupNameConstants.System);
}
