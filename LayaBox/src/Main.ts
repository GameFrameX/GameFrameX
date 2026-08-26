import GameApp from "./gameframex/GameApp";
import nettest from "./gameframex/nettest";

const { regClass, property } = Laya;

@regClass()
export class Main extends Laya.Script {

    onStart() {
        console.log("Game start");
        Laya.stage.on(Laya.Event.KEY_DOWN, this, (e: Laya.Event) => {
            // 判断按下的键值
            if (e.keyCode === Laya.Keyboard.A) {
                // 执行相应的操作
                console.log("按下了A键");
                nettest.A();
            } else if (e.keyCode === Laya.Keyboard.S) {
                // 执行相应的操作
                console.log("按下了S键");
                nettest.S();
            } else if (e.keyCode === Laya.Keyboard.D) {
                // 执行相应的操作
                console.log("按下了D键");
                nettest.D();
            } else if (e.keyCode === Laya.Keyboard.F) {
                // 执行相应的操作
                console.log("按下了F键");
                nettest.F();
            }
        });
    }
}