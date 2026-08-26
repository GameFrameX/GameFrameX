/**
 * 日志
 */
export default class LogHelper {
    public static isDebug: boolean = true;

    public static log(message?: any, ...optionalParams: any[]): void
    /**
     * Log messages to the console if debugging is enabled.
     *
     * @param {...any} args - the messages to be logged
     * @return {void} 
     */
    public static log(...args: any[]): void {
        if (this.isDebug) {
            console.log.apply(console, [this.getMagicLine()].concat(args));
        }
    }

    public static error(message?: any, ...optionalParams: any[]): void;
    /**
     * A method to log error messages if debug mode is enabled.
     *
     * @param {...any} args - the error message or object to be logged
     * @return {void} 
     */
    public static error(...args: any[]): void {
        if (this.isDebug) {
            console.log.apply(console, [this.getMagicLine()].concat(args));
        }
    }

    /**
     * Print the given value if debugging is enabled.
     *
     * @param {string} value - the value to be printed
     * @return {void} 
     */
    public static print(value: string): void {
        if (this.isDebug) {
            Laya.Log.print(value);
        }
    }

    /**
     * Get the line where the error occurred.
     *
     * @param {boolean} printStack - whether to print the entire stack trace
     * @return {String} the line where the error occurred
     */
    private static getMagicLine(printStack: boolean = false): String {
        const e: Error = new Error('where is the line.');
        const array: string[] = e.stack.split('\n');

        let line: string;
        if (printStack === false) {
            line = array[3];

            const s0: string = '    at ';
            const reg0: number = line.indexOf(s0);
            if (reg0 === 0) {
                line = line.substring(s0.length);
            }
        }
        else {
            let i: number;
            for (i = array.length - 1; i > 0; i--) {
                if (array[i].indexOf('js/bundle.js') === -1) {
                    array.splice(i, 1);
                }
            }
            array[0] = '';
            array.splice(1, 2);
            line = array.join('\n');
        }

        return "";// line.replace(/file:\/\/\//g, '');
    }
}