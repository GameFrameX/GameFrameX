import Singleton from "../basic/Singleton";

/**
 * 随机工具类
 */
export default class UtilityRandom extends Singleton<UtilityRandom>() {
    /**
     * 随机参数
     */
    private a: number = 9991;
    private c: number = 6700417;
    private m: number = 239641;

    /**
     * 随机种子
     */
    private r: number = 1;
    /**
     * 初始化随机数生成器的种子值。
     *
     * @param {number} value - 新的种子值
     * @return {void} 
     */
    initSeed(value: number): void {
        this.r = Math.max(1, value);
    }
    /**
     * 生成下一个种子的方法，基于线性同余生成器算法。
     *
     * @return {number} 下一个种子
     */
    nextSeed(): number {
        this.r = (this.r * this.a + this.c) % this.m;
        return this.r;
    }
    /**
     * 生成一个随机数。
     *
     * @return {number} 生成的随机数
     */
    getRandom(): number {
        return this.nextSeed() / this.m;
    }

    /**
     * 返回一个随机整数 [0-N)
     */
    getRandomIndex(value: number): number {
        return this.nextSeed() % value;
    }
    /**
     * 获取一个区间的随机数 (from, end)
     * @param {number} from 最小值
     * @param {number} end 最大值
     * @returns {number}
     */
    limit(from: number, end: number): number {
        let min = Math.min(from, end);
        let max = Math.max(from, end);
        let range = max - min;
        return min + Math.random() * range;
    }

    /**
     * 获取一个区间的随机数 (from, end)
     * @param {number} from 最小值
     * @param {number} end 最大值
     * @returns {number}
     */
    limitInt(from: number, end: number): number {
        return Math.floor(this.limit(from, end));
    }

    /**
     * 获取一个球体区间的随机数 (from, end)
     * @param {number} radius 最小值
     * @returns {number}
     */
    limitSphere(radius: number, out?: Laya.Vector3): Laya.Vector3 {
        out = out || new Laya.Vector3();
        let x = this.limit(-1.0, 1.0);
        let y = this.limit(-1.0, 1.0);
        let z = this.limit(-1.0, 1.0);
        out.set(x, y, z);
        out.normalize();
        out.scale(radius, out);
        return out;
    }

    /**
     * 获取一个区间的随机数 (min, max)
     * @param {number} min 最小值
     * @param {number} max 最大值
     * @returns {number}
     */
    limitBox(min: Laya.Vector3, max: Laya.Vector3, out?: Laya.Vector3): Laya.Vector3 {
        out = out || new Laya.Vector3();
        let x = this.limit(min.x, max.x);
        let y = this.limit(min.y, max.y);
        let z = this.limit(min.z, max.z);
        out.set(x, y, z);
        return out;
    }

    /**
     * 在一个数组中随机获取一个元素
     * @param {Array} arr 数组
     * @returns 随机出来的结果
     */
    randomArray(arr: any[]): any {
        let index = (Math.random() * arr.length) | 0;
        return arr[index];
    }
}