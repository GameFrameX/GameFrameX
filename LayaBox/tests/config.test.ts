import ConfigComponent from "../src/gameframex/config/ConfigComponent";
import LocalizationComponent, { type LocalizationRow } from "../src/gameframex/localization/LocalizationComponent";
import SoundComponent, { type SoundsConfigRow } from "../src/gameframex/sound/SoundComponent";
import { TablesComponent } from "../hotfix/Config/Generate/schema";
import achievementJson from "../assets/resources/config/tables_tbachievementconfig.json";
import soundsJson from "../assets/resources/config/tables_tbsoundsconfig.json";
import itemJson from "../assets/resources/config/tables_tbitemconfig.json";
import localizationJson from "../assets/resources/config/local_tblocalization.json";
import { test, assert, assertEqual } from "./harness";

/**
 * 行数期望值 = Unity 侧输出(../Unity/Assets/Bundles/Config)实测:
 * 两端同源同工具(luban)生成,数值一致即验收;Excel 变更后同步更新。
 */
const EXPECTED_COUNTS = { achievement: 2, sounds: 8, item: 12, localization: 114 };

const DATA: Record<string, unknown[]> = {
    tables_tbachievementconfig: achievementJson,
    tables_tbsoundsconfig: soundsJson,
    tables_tbitemconfig: itemJson,
    local_tblocalization: localizationJson,
};

async function loadTables(): Promise<TablesComponent> {
    const config = new ConfigComponent();
    await config.loadAsync(Object.keys(DATA), async (file) => DATA[file]);
    const tables = new TablesComponent((file: string) => config.getLoaded(file));
    config.bindTables(tables);
    return tables;
}

test('Config: 生成 TablesComponent 从 JSON 构造,4 表行数与 Unity 侧一致', async () => {
    const tables = await loadTables();

    assertEqual(tables.TbAchievementConfig.getDataList().length, EXPECTED_COUNTS.achievement, 'TbAchievementConfig 行数一致');
    assertEqual(tables.TbSoundsConfig.getDataList().length, EXPECTED_COUNTS.sounds, 'TbSoundsConfig 行数一致');
    assertEqual(tables.TbItemConfig.getDataList().length, EXPECTED_COUNTS.item, 'TbItemConfig 行数一致');
    assertEqual(tables.TbLocalization.getDataList().length, EXPECTED_COUNTS.localization, 'TbLocalization 行数一致');
    assert(tables.TbItemConfig.getDataList().length > 0, '表非空');
});

test('Config: GetConfig 选择子取表与按键查询;未绑定抛错', async () => {
    const config = new ConfigComponent();
    let threw = false;
    try {
        config.GetConfig((t) => t as never);
    } catch {
        threw = true;
    }
    assert(threw, '表未绑定时抛错');

    const tables = await loadTables();
    config.bindTables(tables);
    const items = config.GetConfig((t) => (t as TablesComponent).TbItemConfig);
    const first = items.getDataList()[0];
    assert(items.get(first.id) === first, '按 id 查询命中');
});

test('Localization: TbLocalization 灌注 + GetString + 语言切换', async () => {
    const tables = await loadTables();
    const localization = new LocalizationComponent();
    localization.Load(tables.TbLocalization.getDataList() as unknown as LocalizationRow[]);

    const rows = tables.TbLocalization.getDataList();
    assert(rows.length > 0, '本地化行非空');
    const sample = rows[0];
    assert(localization.HasKey(sample.key), '灌注后 key 命中');

    localization.Language = 'ChineseSimplified';
    const zhValue = sample['ChineseSimplified'];
    if (typeof zhValue === 'string' && zhValue.length > 0) {
        assertEqual(localization.GetString(sample.key), zhValue, 'ChineseSimplified 取值一致');
    }

    localization.Language = 'English';
    const enValue = sample['English'];
    if (typeof enValue === 'string' && enValue.length > 0) {
        assertEqual(localization.GetString(sample.key), enValue, '切语言后 English 生效');
    }

    assertEqual(localization.GetString('__no_such_key__'), '__no_such_key__', '缺失 key 回退 key 本身');
});

test('Sound: SoundsConfig 驱动播放(注入播放器验证查表链)', async () => {
    const tables = await loadTables();
    const sound = new SoundComponent();
    const played: { path: string; volume: number }[] = [];
    sound.SetPlayer((path_, volume) => played.push({ path: path_, volume }));

    sound.Load(tables.TbSoundsConfig.getDataList() as unknown as SoundsConfigRow[]);
    const first = tables.TbSoundsConfig.getDataList()[0];
    sound.PlaySound(first.id);
    assertEqual(played.length, 1, '播放一次');
    assertEqual(played[0].path, first.path, '路径来自配置');

    sound.PlaySound(987654321);
    assertEqual(played.length, 1, '未知 Id 不播放');
});
