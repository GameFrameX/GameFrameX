/**
 * 泛型对象池
 */
export default class ObjectPool<T> {
    private pool: T[] = [];
    private maxSize: number;
    private objectFactory: () => T;
    /**
     * 对象池构造函数
     * @param maxSize 对象池最大容量
     * @param objectFactory 当对象池为空时的创建工厂函数
     */
    constructor(maxSize: number, objectFactory: () => T) {
        this.maxSize = maxSize;
        this.objectFactory = objectFactory;
    }

    /**
     * 存放对象到对象池
     * @param obj 
     */
    addObject(obj: T) {
        if (this.pool.length < this.maxSize) {
            this.pool.push(obj);
        } else {
            console.log("Object pool is full, cannot add more objects.");
        }
    }

    /**
     * 从对象池获取对象
     * @returns 
     */
    getObject(): T {
        if (this.pool.length > 0) {
            return this.pool.pop();
        } else {
            return this.objectFactory();
        }
    }
}
