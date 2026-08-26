export default class name {
    /**
     * 格式化字符串
     * @param format - 格式字符串
     * @param args - 可变数量的参数
     * @returns 格式化后的字符串
     */
    formatString(format: string, ...args: any[]): string {
        return format.replace(/{(\d+)}/g, (match, index) => {
            const argIndex = parseInt(index, 10);
            return typeof args[argIndex] !== 'undefined' ? args[argIndex] : match;
        });
    }

    /**
     * 判断是否是空字符串
     * @param str
     */
    public static isEmptyString(str: string): boolean {
        return str != null && str.trim() === '';
    }
    /**
     * 将长度大于n的字符串变成"..."
     * @param str - 输入的字符串
     * @param len - 截断的字符长度.默认为4
     * @returns 处理后的字符串
     */
    shortenString(str: string, len: number = 4): string {
        if (str.length > len) {
            return str.slice(0, len) + '...';
        }
        return str;
    }
}