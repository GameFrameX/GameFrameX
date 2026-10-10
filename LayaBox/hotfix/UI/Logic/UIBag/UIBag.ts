import UIForm from "../../../../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../../../../src/gameframex/ui/UIGroupConstants";
import GameApp from "../../../../src/gameframex/GameApp";
import type GameEventArgs from "../../../../src/gameframex/event/GameEventArgs";
import BagManager, { type BagItem } from "../../../Manager/BagManager";
import { BagChangedEventArgs } from "../../../Events/HotfixEventArgs";

/**
 * 背包界面。
 *
 * 对照 Unity `Hotfix.UI.UIBag`(FGUI 轨):类型标签 + 道具列表 + 选中使用 + 关闭。
 */
export default class UIBag extends UIForm {
    public static readonly UIPackageName: string = 'UIBag';
    public static readonly UIResName: string = 'UIBag';

    public m_type_list!: fgui.GList;
    public m_list!: fgui.GList;

    private readonly bagItems: BagItem[] = [];
    private selectedBagItem: BagItem | null = null;
    private currentTypeFilter: number = -1;

    private readonly onBagChanged = (_sender: unknown, _args: GameEventArgs): void => {
        this.refreshList();
    };

    /**
     * 视图绑定(m_content 子组件成员)。
     */
    public InitView(): void {
        const self = this.Handle as fgui.GComponent;
        const content = self.getChild('content');
        const owner = (content ?? self) as fgui.GComponent;
        this.m_type_list = owner.getChild('type_list') as fgui.GList;
        this.m_list = owner.getChild('list') as fgui.GList;
    }

    /**
     * 挂 Window 组。
     */
    public OnAwake(): void {
        this.UIGroup = GameApp.UI.GetUIGroup(UIGroupConstants.Window.Name, UIGroupConstants.Window.Depth);
        super.OnAwake();
    }

    /**
     * 打开:绑定列表/事件,拉取背包。
     * @param userData 用户数据
     */
    public OnOpen(userData: unknown): void {
        super.OnOpen(userData);
        this.Subscribe(BagChangedEventArgs.EventId, this.onBagChanged);

        this.m_type_list.itemRenderer = (index: number, item: fgui.GObject): void => this.renderTypeItem(index, item);
        this.m_type_list.on(fgui.Events.CLICK_ITEM, this, this.onTypeItemClick);
        this.m_list.itemRenderer = (index: number, item: fgui.GObject): void => this.renderBagItem(index, item);
        this.m_list.on(fgui.Events.CLICK_ITEM, this, this.onBagItemClick);

        void BagManager.Instance.RequestGetBagInfo();
        this.refreshList();
    }

    private renderTypeItem(index: number, item: fgui.GObject): void {
        const title = (item as fgui.GComponent).getChild('title') as fgui.GTextField | null;
        if (title != null) {
            title.text = this.typeNames[index] ?? '';
        }
    }

    private onTypeItemClick(context: { data?: unknown }): void {
        const component = context.data as fgui.GComponent;
        this.currentTypeFilter = this.m_type_list.getChildIndex(component);
        this.refreshList();
    }

    private renderBagItem(index: number, item: fgui.GObject): void {
        const bagItem = this.bagItems[index];
        if (bagItem == null) {
            return;
        }

        const component = item as fgui.GComponent;
        const nameText = component.getChild('name_text') as fgui.GTextField | null;
        if (nameText != null) {
            nameText.text = `道具 ${bagItem.ItemId}`;
        }

        const countText = component.getChild('count_text') as fgui.GTextField | null;
        if (countText != null) {
            countText.text = `x${bagItem.Count}`;
        }
    }

    private onBagItemClick(context: { data?: unknown }): void {
        const component = context.data as fgui.GComponent;
        const index = this.m_list.getChildIndex(component);
        this.selectedBagItem = this.bagItems[index] ?? null;
        if (this.selectedBagItem != null) {
            void BagManager.Instance.RequestUseItem(this.selectedBagItem.ItemId, 1);
        }
    }

    /** 类型过滤索引:-1 = 全部(标签简化:全部/装备/消耗品/材料/货币,对齐 Unity ItemType)。 */
    private readonly typeNames: string[] = ['全部', '装备', '消耗品', '材料', '货币'];

    private refreshList(): void {
        const items = BagManager.Instance.GetItems();
        this.bagItems.length = 0;
        this.bagItems.push(...items);
        this.m_type_list.numItems = this.typeNames.length;
        this.m_list.numItems = this.bagItems.length;
    }
}
