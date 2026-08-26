import Singleton from "../basic/Singleton";

export default class UtilityMath extends Singleton<UtilityMath>() {
    public static readonly Radian2Angle: number = 180.0 / Math.PI;
    public static readonly Angle2Radian: number = Math.PI / 180.0;

    public static readonly aVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly bVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly cVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly dVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly eVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly fVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly gVec3: Laya.Vector3 = new Laya.Vector3();
    public static readonly hVec3: Laya.Vector3 = new Laya.Vector3();
    /**
     * 弧度制转换为角度值
     * @param {number} radian
     * @returns {number}
     */
    public static getAngle(radian: number): number {
        return radian * this.Radian2Angle;
    }

    /**
     * 角度值转换为弧度制
     * @param {number} angle
     */
    public static getRadian(angle: number): number {
        return angle * this.Angle2Radian;
    }

    /**
     * 获取两点间弧度
     * @param {Point} p1
     * @param {Point} p2
     * @returns {number}
     */
    public static getRadianTwoPoint(p1: Laya.Point, p2: Laya.Point): number {
        let xdis = p2.x - p1.x;
        let ydis = p2.y - p1.y;
        return Math.atan2(ydis, xdis);
    }

    /**
     * 获取两点间旋转角度（顺时针）
     * @param {Point} p1
     * @param {Point} p2
     * @returns {number}
     */
    public static getAngleTwoPoint(p1: Laya.Point, p2: Laya.Point): number {
        let vy = p2.y - p1.y;
        let vx = p2.x - p1.x;
        let ang;

        if (vy == 0) {
            if (vx < 0) {
                return 180;
            }
            return 0;
        }

        if (vx == 0) { //正切是vy/vx所以vx==0排除
            if (vy > 0) {
                ang = 90;
            }
            else if (vy < 0) {
                ang = 270;
            }
            return ang;
        }

        ang = this.getAngle(Math.atan(Math.abs(vy) / Math.abs(vx)));
        if (vx > 0) {
            if (vy < 0) {
                ang = 360 - ang;
            }
        }
        else {
            if (vy > 0) {
                ang = 180 - ang;
            }
            else {
                ang = 180 + ang;
            }
        }
        return ang;
    }

    /**
     * 获取两点间距离
     * @param {Point} p1
     * @param {Point} p2
     * @returns {number}
     */
    public static getDistance(p1: Laya.Point, p2: Laya.Point): number {
        let disX = p2.x - p1.x;
        let disY = p2.y - p1.y;
        let disQ = Math.pow(disX, 2) + Math.pow(disY, 2);
        return Math.sqrt(disQ);
    }

    /**
     * 精确到小数点后多少位（舍尾）
     * @param {number} 精确值
     * @param {number} 精确位数
     * @return {number}
     * */
    public static exactCount(exactValue: number, count: number = 0): number {
        let num = Math.pow(10, count);
        let value = (exactValue * num) | 0;
        return value / num;
    }

    /**
     * [0-1]区间获取二次贝塞尔曲线点切线角度
     * @param {Point} p0起点
     * @param {Point} p1控制点
     * @param {Point} p2终点
     * @param {number} t [0-1]区间
     * @return {number}
     * */
    public static getBezierCutAngle(p0: Laya.Point, p1: Laya.Point, p2: Laya.Point, t: number): number {
        let _x = 2 * (p0.x * (t - 1) + p1.x * (1 - 2 * t) + p2.x * t);
        let _y = 2 * (p0.y * (t - 1) + p1.y * (1 - 2 * t) + p2.y * t);
        let angle = this.getAngle(Math.atan2(_y, _x));
        return angle;
    }

    /**
     * [0-1]区间获取二次贝塞尔曲线上某点坐标
     * @param {Point} p0 起点
     * @param {Point} p1 控制点
     * @param {Point} p2 终点
     * @param {number} t [0-1]区间
     * @param {Point} 缓存的点对象，如不存在则生成新的点对象
     * @return {Laya.Point}
     * */
    public static getBezierPoint(p0: Laya.Point, p1: Laya.Point, p2: Laya.Point, t: number, point?: Laya.Point): Laya.Point {
        if (!point) {
            point = new Laya.Point();
        }
        point.x = (1 - t) * (1 - t) * p0.x + 2 * t * (1 - t) * p1.x + t * t * p2.x;
        point.y = (1 - t) * (1 - t) * p0.y + 2 * t * (1 - t) * p1.y + t * t * p2.y;
        return point;
    }

    /**
     * [0-1]区间获取三次贝塞尔曲线上某点坐标
     * @param {Point} p0 起点
     * @param {Point} p1 控制点
     * @param {Point} p2 控制点
     * @param {Point} p3 终点
     * @param {number} t [0-1]区间
     * @param {Point} 缓存的点对象，如不存在则生成新的点对象
     * @return {Laya.Point}
     * */
    public static getBezier3Point(p0: Laya.Point, p1: Laya.Point, p2: Laya.Point, p3: Laya.Point, t: number, point?: Laya.Point): Laya.Point {
        if (!point) {
            point = new Laya.Point();
        }
        let cx = 3 * (p1.x - p0.x);
        let bx = 3 * (p2.x - p1.x) - cx;
        let ax = p3.x - p0.x - cx - bx;
        let cy = 3 * (p1.y - p0.y);
        let by = 3 * (p2.y - p1.y) - cy;
        let ay = p3.y - p0.y - cy - by;
        point.x = ax * t * t * t + bx * t * t + cx * t + p0.x;
        point.y = ay * t * t * t + by * t * t + cy * t + p0.y;
        return point;
    }

    /**
     * [0-1]区间获取三次贝塞尔曲线点切线角度
     * @param {Point} p0起点
     * @param {Point} p1控制点
     * @param {Point} p2控制点
     * @param {Point} p3终点
     * @param {number} t [0-1]区间
     * @return {number}
     * */
    public static getBezier3CutAngle(p0: Laya.Point, p1: Laya.Point, p2: Laya.Point, p3: Laya.Point, t: number): number {
        let _x = p0.x * 3 * (1 - t) * (1 - t) * (-1) +
            3 * p1.x * ((1 - t) * (1 - t) + t * 2 * (1 - t) * (-1)) +
            3 * p2.x * (2 * t * (1 - t) + t * t * (-1)) +
            p3.x * 3 * t * t;
        let _y = p0.y * 3 * (1 - t) * (1 - t) * (-1) +
            3 * p1.y * ((1 - t) * (1 - t) + t * 2 * (1 - t) * (-1)) +
            3 * p2.y * (2 * t * (1 - t) + t * t * (-1)) +
            p3.y * 3 * t * t;
        let angle = this.getAngle(Math.atan2(_y, _x));
        return angle;
    }

    /**
     * 二阶贝塞尔曲线：
     * @param p1 
     * @param p2 
     * @param p3 
     * @param t 
     * @param out
     */
    public static getBezier2Vector3(p1: Laya.Vector3, p2: Laya.Vector3, p3: Laya.Vector3, t: number, out: Laya.Vector3): Laya.Vector3 {
        let t1 = Math.pow((1 - t), 2);
        let t2 = 2 * t * (1 - t);
        let t3 = t * t;
        out.set(0, 0, 0);
        p1.scale(t1, UtilityMath.aVec3);
        out.vadd(UtilityMath.aVec3, out);
        p2.scale(t2, UtilityMath.aVec3);
        out.vadd(UtilityMath.aVec3, out);
        p3.scale(t3, UtilityMath.aVec3);
        out.vadd(UtilityMath.aVec3, out);
        return out;
    }

    /**
     * 颜色转html
     * @param color 
     * @returns 
     */
    public static color2String(color: Laya.Color): string {
        var r, g, b, a;
        r = Math.ceil(color.r * 255).toString(16);
        r = r.length == 1 ? '0' + r : r;
        g = Math.ceil(color.g * 255).toString(16);
        g = g.length == 1 ? '0' + g : g;
        b = Math.ceil(color.b * 255).toString(16);
        b = b.length == 1 ? '0' + b : b;
        a = Math.ceil(color.a * 255).toString(16);
        a = b.length == 1 ? '0' + a : a;
        return '#' + r + g + b;
    }

    /**
     * 限制值在一定范围内
     * @param value 
     * @param min 
     * @param max 
     * @returns 
     */
    public static clamp(value: number, min: number, max: number): number {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}