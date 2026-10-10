import TimerHelper from "../timer/TimerHelper";

/**
 * Timer 域注册:TimerHelper 为纯静态工具,无模块生命周期,直接挂载类型。
 * @returns TimerHelper 类型(GameApp.Timer)
 */
export default function TimerDomain(): typeof TimerHelper {
    return TimerHelper;
}
