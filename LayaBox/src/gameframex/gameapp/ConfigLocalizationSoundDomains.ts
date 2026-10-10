import ConfigComponent from "../config/ConfigComponent";
import LocalizationComponent from "../localization/LocalizationComponent";
import SoundComponent from "../sound/SoundComponent";
import GameEntry from "../GameEntry";

/**
 * Config/Localization/Sound 域注册(三者同链:配置加载 → 本地化灌注 → 音效配置灌注)。
 * @returns 三域实例(元组)
 */
export default function ConfigLocalizationSoundDomains(): { Config: ConfigComponent; Localization: LocalizationComponent; Sound: SoundComponent } {
    const config = new ConfigComponent();
    GameEntry.registerModule('Config', config);
    const localization = new LocalizationComponent();
    GameEntry.registerModule('Localization', localization);
    const sound = new SoundComponent();
    GameEntry.registerModule('Sound', sound);
    return { Config: config, Localization: localization, Sound: sound };
}
