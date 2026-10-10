import GameEntry from "./gameframex/GameEntry";
import ApplicationStartupEntry from "./scripts/framework/startup/ApplicationStartupEntry";

const { regClass } = Laya;

/**
 * 引擎入口:仅做引擎接线——启动框架与启动链、按帧驱动模块轮询。
 */
@regClass()
export class Main extends Laya.Script {

    onStart() {
        ApplicationStartupEntry.run();
    }

    onUpdate(): void {
        const deltaSeconds = Laya.timer.delta / 1000;
        GameEntry.update(deltaSeconds, deltaSeconds);
    }

    onDestroy(): void {
        GameEntry.shutdown();
    }
}

if (typeof window !== 'undefined') {
    (window as unknown as { Main: typeof Main }).Main = Main;
}
