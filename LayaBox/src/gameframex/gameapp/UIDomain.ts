import UIManager from "../ui/UIManager";
import GameEntry from "../GameEntry";
import FairyGUIFormHelper from "../ui.fairygui/FairyGUIFormHelper";

/**
 * UI 域注册:创建 UIManager 并注入 FairyGUI 界面助手。
 * @returns 界面管理器实例(GameApp.UI)
 */
export default function UIDomain(): UIManager {
    const manager = new UIManager();
    manager.SetUIFormHelper(new FairyGUIFormHelper());
    GameEntry.registerModule('UI', manager);
    return manager;
}
