/**
 * 时间工具。
 *
 * 对照 Unity `GameFrameX.Runtime.TimerHelper` 常用子集:
 * UnixTimeSeconds / UnixTimeMilliseconds / SyncServerTime / Difference*。
 * 语义:Unix* 返回"已叠加服务器校时偏移"的当前时间戳;偏移量 = 服务器时间 - 本地 UTC 时间。
 */
export default class TimerHelper {
    private static TimeOffsetSeconds: number = 0;
    private static TimeOffsetMilliseconds: number = 0;

    /**
     * 当前 Unix 时间戳(秒,含服务器校时偏移)。
     */
    public static UnixTimeSeconds(): number {
        return Math.floor(Date.now() / 1000) + this.TimeOffsetSeconds;
    }

    /**
     * 当前 Unix 时间戳(毫秒,含服务器校时偏移)。
     */
    public static UnixTimeMilliseconds(): number {
        return Date.now() + this.TimeOffsetMilliseconds;
    }

    /**
     * 与服务器时间同步:记录本地与服务器的时间差,此后 Unix* 均按服务器时间返回。
     * @param serverUnixTimeSeconds 服务器 Unix 时间戳(秒)
     * @param serverUnixTimeMilliseconds 服务器 Unix 时间戳(毫秒)
     */
    public static SyncServerTime(serverUnixTimeSeconds: number, serverUnixTimeMilliseconds: number): void {
        this.TimeOffsetSeconds = serverUnixTimeSeconds - Math.floor(Date.now() / 1000);
        this.TimeOffsetMilliseconds = serverUnixTimeMilliseconds - Date.now();
    }

    /**
     * 当前与指定时间戳(秒)的差值:current - timestamp。
     */
    public static DifferenceSeconds(timestamp: number): number {
        return this.UnixTimeSeconds() - timestamp;
    }

    /**
     * 当前与指定时间戳(毫秒)的差值:current - timestamp。
     */
    public static DifferenceMilliseconds(timestamp: number): number {
        return this.UnixTimeMilliseconds() - timestamp;
    }
}
