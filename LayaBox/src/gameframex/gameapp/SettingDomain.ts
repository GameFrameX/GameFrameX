import SettingComponent from "../setting/SettingComponent";
import GameEntry from "../GameEntry";

/**
 * Setting 域注册。
 * @returns 设置组件实例(GameApp.Setting)
 */
export default function SettingDomain(): SettingComponent {
    const component = new SettingComponent();
    GameEntry.registerModule('Setting', component);
    return component;
}
