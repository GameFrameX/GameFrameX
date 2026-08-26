export default class UtilityObject {
    /**
     * 执行输入对象的深拷贝，包括嵌套对象和数组。
     *
     * @param {any} obj - 要进行深拷贝的对象
     * @return {any} 深拷贝后的对象
     */
    public static deepCopy(obj: any): any {
        let newObj: any;
        if (obj instanceof Array) {
            newObj = [];
        } else if (obj instanceof Object) {
            newObj = {};
        } else {
            return obj;
        }

        let keys = Object.keys(obj);
        for (let i = 0, len = keys.length; i < len; i++) {
            let key = keys[i];
            newObj[key] = this.deepCopy(obj[key]);
        }
        return newObj;
    }
}