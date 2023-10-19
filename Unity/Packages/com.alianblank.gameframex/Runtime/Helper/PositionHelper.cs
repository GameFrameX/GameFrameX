using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 坐标帮助类
    /// </summary>
    public static class PositionHelper
    {
        public static Vector3 RayCastV2ToV3(Vector2 pos)
        {
            return new Vector3(pos.x, 0, pos.y);
        }

        public static Vector3 RayCastXYToV3(float x, float y)
        {
            return new Vector3(x, 0, y);
        }

        public static Vector3 RayCastV3ToV3(Vector3 pos)
        {
            return new Vector3(pos.x, 0, pos.z);
        }

        public static Quaternion AngleToQuaternion(int angle)
        {
            return Quaternion.AngleAxis(-angle, Vector3.up) * Quaternion.AngleAxis(90, Vector3.up);
        }

        public static Quaternion GetVector3ToQuaternion(Vector3 source, Vector3 dire)
        {
            Vector3 nowPos = source;
            if (nowPos == dire)
            {
                return new Quaternion();
            }

            Vector3 direction = (dire - nowPos).normalized;
            return Quaternion.LookRotation(direction, Vector3.up);
        }


        public static float Distance2D(Vector3 v1, Vector3 v2)
        {
            Vector2 d1 = new Vector2(v1.x, v1.z);
            Vector2 d2 = new Vector2(v2.x, v2.z);
            return Vector2.Distance(d1, d2);
        }

        public static Quaternion GetAngleToQuaternion(float angle)
        {
            return Quaternion.AngleAxis(-angle, Vector3.up) * Quaternion.AngleAxis(90, Vector3.up);
        }

        public static float Vector3ToAngle360(Vector3 from, Vector3 to)
        {
            float angle = Vector3.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            return cross.y > 0 ? angle : 360 - angle;
        }

        /// <summary>
        ///  求点到直线的距离，采用数学公式Ax+By+C = 0; d = A*p.x + B * p.y + C / sqrt(A^2 + B ^ 2)
        /// </summary>
        /// <param name="startPoint">线的起点</param>
        /// <param name="endPoint">线的终点</param>
        /// <param name="point">点</param>
        /// <returns></returns>
        public static float DistanceOfPointToVector(Vector3 startPoint, Vector3 endPoint, Vector3 point)
        {
            Vector2 startVe2 = startPoint.IgnoreYAxis();
            Vector2 endVe2 = endPoint.IgnoreYAxis();
            float A = endVe2.y - startVe2.y;
            float B = startVe2.x - endVe2.x;
            float C = endVe2.x * startVe2.y - startVe2.x * endVe2.y;
            float denominator = Mathf.Sqrt(A * A + B * B);
            Vector2 pointVe2 = point.IgnoreYAxis();
            return Mathf.Abs((A * pointVe2.x + B * pointVe2.y + C) / denominator);
            ;
        }

        /// <summary>
        /// 判断射线是否碰撞到球体，如果碰撞到，返回射线起点到碰撞点之间的距离
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="center">中心点</param>
        /// <param name="redis">半径</param>
        /// <param name="dist">距离</param>
        /// <returns></returns>
        public static bool RayCastSphere(Ray ray, Vector3 center, float redis, out float dist)
        {
            dist = 0;
            Vector3 ma = center - ray.origin;
            float distance = Vector3.Cross(ma, ray.direction).magnitude / ray.direction.magnitude;
            if (distance < redis)
            {
                float op = PythagoreanTheorem(Vector3.Distance(center, ray.origin), distance);
                float rp = PythagoreanTheorem(redis, distance);
                dist = op - rp;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 勾股定理
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float PythagoreanTheorem(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// 去掉三维向量的Y轴，把向量投射到xz平面。
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector2 IgnoreYAxis(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        /// <summary>
        /// 判断目标点是否位于向量的左边
        /// </summary>
        /// <returns>True is on left, false is on right</returns>
        public static bool PointOnLeftSideOfVector(this Vector3 vector3, Vector3 originPoint, Vector3 point)
        {
            Vector2 originVec2 = originPoint.IgnoreYAxis();

            Vector2 pointVec2 = (point.IgnoreYAxis() - originVec2).normalized;

            Vector2 vector2 = vector3.IgnoreYAxis();

            float verticalX = originVec2.x;

            float verticalY = (-verticalX * vector2.x) / vector2.y;

            Vector2 norVertical = (new Vector2(verticalX, verticalY)).normalized;

            float dotValue = Vector2.Dot(norVertical, pointVec2);

            return dotValue < 0f;
        }
    }
}