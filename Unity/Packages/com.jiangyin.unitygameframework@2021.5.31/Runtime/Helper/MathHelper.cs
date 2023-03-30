using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public static class MathHelper
    {
        /// <summary>
        /// 检查两个矩形是否相交
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckIntersect(RectInt src, RectInt target)
        {
            int minx = Math.Max(src.x, target.x);
            int miny = Math.Max(src.y, target.y);
            int maxx = Math.Min(src.x + src.width, target.x + target.width);
            int maxy = Math.Min(src.y + src.height, target.y + target.height);
            if (minx >= maxx || miny >= maxy)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查两个矩形是否相交
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="w1"></param>
        /// <param name="h1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="w2"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public static bool CheckIntersect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            int minx = Math.Max(x1, x2);
            int miny = Math.Max(y1, y2);
            int maxx = Math.Min(x1 + w1, x2 + w2);
            int maxy = Math.Min(y1 + h1, y2 + h2);
            if (minx >= maxx || miny >= maxy)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查两个矩形是否相交，并返回相交的区域
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="w1"></param>
        /// <param name="h1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="w2"></param>
        /// <param name="h2"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        private static bool CheckIntersect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2, out RectInt rect)
        {
            rect = default;
            int minx = Math.Max(x1, x2);
            int miny = Math.Max(y1, y2);
            int maxx = Math.Min(x1 + w1, x2 + w2);
            int maxy = Math.Min(y1 + h1, y2 + h2);
            if (minx >= maxx || miny >= maxy)
            {
                return false;
            }

            rect.x = minx;
            rect.y = miny;
            rect.width = Math.Abs(maxx - minx);
            rect.height = Math.Abs(maxy - miny);
            return true;
        }

        /// <summary>
        /// 检查两个矩形相交的点
        /// </summary>
        /// <param name="x1">A 坐标X</param>
        /// <param name="y1">A 坐标Y</param>
        /// <param name="w1">A 宽度</param>
        /// <param name="h1">A 高度</param>
        /// <param name="x2">B 坐标X</param>
        /// <param name="y2">B 坐标Y</param>
        /// <param name="w2">B 宽度</param>
        /// <param name="h2">B 高度</param>
        /// <param name="intersectPoints">交叉点列表</param>
        /// <returns>返回是否相交</returns>
        public static bool CheckIntersectPoints(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2, int[] intersectPoints)
        {
            Vector2Int dPt = new Vector2Int();

            if (false == CheckIntersect(x1, y1, w1, h1, x2, y2, w2, h2, out RectInt dRecr))
            {
                return false;
            }

            for (var i = 0; i < w1; i++)
            {
                for (var n = 0; n < h1; n++)
                {
                    if (intersectPoints[i * h1 + n] == 1)
                    {
                        dPt.x = x1 + i;
                        dPt.y = y1 + n;
                        if (dRecr.Contains(dPt))
                        {
                            intersectPoints[i * h1 + n] = 0;
                        }
                    }
                }
            }

            return true;
        }
    }
}