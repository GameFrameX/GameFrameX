export default class UtilityString {
    /**
     * 格式化字符串,以 {0} {1} ... 占位
     * @param format - 格式字符串
     * @param args - 可变数量的参数
     * @returns 格式化后的字符串
     */
    public static formatString(format: string, ...args: unknown[]): string {
        return format.replace(/{(\d+)}/g, (match, index) => {
            const argIndex = parseInt(index, 10);
            return typeof args[argIndex] !== 'undefined' ? String(args[argIndex]) : match;
        });
    }

    /**
     * 判断是否是空字符串:null/undefined 或去除首尾空白后为空
     * @param str - 待判断字符串
     */
    public static isEmptyString(str: string): boolean {
        return str == null || str.trim() === '';
    }

    /**
     * 将长度大于 n 的字符串截断并以 "..." 结尾
     * @param str - 输入的字符串
     * @param len - 截断的字符长度,默认为 4
     * @returns 处理后的字符串
     */
    public static shortenString(str: string, len: number = 4): string {
        if (str.length > len) {
            return str.slice(0, len) + '...';
        }
        return str;
    }
}