import IModule from "../IModule";
import LogHelper from "../log/LogHelper";

/** 音效配置行最小结构(TbSoundsConfig 生成行,luban camelCase 字段)。 */
export interface SoundsConfigRow {
    id: number;
    path: string;
    volume?: number;
}

/**
 * 音效组件(GameApp.Sound)。
 *
 * 对照 Unity `SoundComponent` 最小面:按配置 id 查表播放;
 * 播放走宿主注入的 playSound(Laya.SoundManager 适配),无引擎依赖可在 node 单测。
 */
export default class SoundComponent implements IModule {
    private readonly configs: Map<number, SoundsConfigRow> = new Map();
    private player: ((path: string, volume: number) => void) | null = null;

    /**
     * 注入播放实现(引擎层:Laya.SoundManager.playSound)。
     * @param player 播放函数
     */
    public SetPlayer(player: (path: string, volume: number) => void): void {
        this.player = player;
    }

    /**
     * 灌入 TbSoundsConfig 全部行。
     * @param rows 音效配置行集合
     */
    public Load(rows: SoundsConfigRow[]): void {
        this.configs.clear();
        for (const row of rows) {
            this.configs.set(row.id, row);
        }
    }

    /**
     * 按配置 Id 播放音效。
     * @param id 音效配置 Id
     */
    public PlaySound(id: number): void {
        const row = this.configs.get(id);
        if (row == null) {
            LogHelper.log(`sound config ${id} not found`);
            return;
        }

        this.player?.(row.path, row.volume ?? 1);
    }

    /**
     * 模块初始化(由 GameEntry 调用)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块轮询(无逐帧动作)。
     */
    public update(_elapseSeconds: number, _realElapseSeconds: number): void { }

    /**
     * 模块关闭。
     */
    public shutdown(): void {
        this.configs.clear();
    }
}
