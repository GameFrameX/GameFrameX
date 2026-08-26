export default class UtilityTime {
    /**
     * 获取时间戳转换成完整时间 年-月-日 时:分:秒
     * @param timestamp 时间戳
     */
    public static getTimestampToString(timestamp: number) {
        let date = new Date(timestamp * 1000); // 将时间戳转换为毫秒，并创建一个新的Date对象

        let year = date.getFullYear(); // 获取年份
        let month = ("0" + (date.getMonth() + 1)).slice(-2); // 获取月份，需要+1并补零
        let day = ("0" + date.getDate()).slice(-2); // 获取日期，需要补零
        let hours = ("0" + date.getHours()).slice(-2); // 获取小时，需要补零
        let minutes = ("0" + date.getMinutes()).slice(-2); // 获取分钟，需要补零
        let seconds = ("0" + date.getSeconds()).slice(-2); // 获取秒钟，需要补零

        let formattedTime = `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`; // 格式化时间
        return formattedTime;
    }
    /**
     * 获取今天剩余的时间（毫秒）。
     *
     * @return {number} 今天剩余的时间（毫秒）。
     */
    public static getTheRestOfToday(): number {
        const date: Date = new Date();
        date.setHours(23);
        date.setMinutes(59);
        date.setSeconds(59);
        date.setMilliseconds(999);

        return date.getTime() + 1 - Date.now();
    }
    /**
     * 获取当前的时间戳,单位秒
     */
    public static get getCurrentTimestampWithSecond(): number {
        const currentDate = new Date();
        return Math.floor(currentDate.getTime() / 1000);
    }

    /**
     * 获取当前的时间戳,单位毫秒
     */
    public static get getCurrentTimestamp(): number {
        const currentDate = new Date();
        return currentDate.getTime();
    }
}