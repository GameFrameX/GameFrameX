using System;
using System.Collections.Generic;
using FairyGUI;
using GameFrameX.Runtime;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// FGUI 路径帮助类
    /// </summary>
    public static class FUIPathFinderHelper
    {
        /// <summary>
        /// 根据UI对象获取路径
        /// </summary>
        /// <param name="o">UI对象</param>
        /// <returns>UI所在路径</returns>
        public static string GetUIPath(GObject o)
        {
            var ls = new List<string>();
            SearchParent(o, ls);
            ls.Reverse();
            return string.Join("/", ls);
        }

        private static void SearchParent(GObject o, List<string> st)
        {
            if (o.parent != null)
            {
                st.Add(o.name);
                SearchParent(o.parent, st);
            }
            else
            {
                st.Add(o.name);
            }
        }

        /// <summary>
        /// 根据路径获取FUI对象
        /// </summary>
        /// <param name="path">UI路径</param>
        /// <returns>UI对象</returns>
        public static GObject GetUIFromPath(string path)
        {
            //GRoot / UISynthesisScene / ContentBox / ListSelect / 1990197248 / icon

            string[] arr = path.Split('/');

            var q = new Queue<string>();
            foreach (string v in arr)
            {
                if (v == "GRoot")
                {
                    continue;
                }

                q.Enqueue(v);
            }

            try
            {
                GObject child = SearchChild(GRoot.inst, q);
                return child;
            }
            catch (Exception exception)
            {
                Log.Error("error uiPath : can not found ui by this path :" + path + ", error : " + exception);
            }

            return null;
        }

        private static GObject SearchChild(GComponent o, Queue<string> q)
        {
            //防错
            if (q.Count <= 0)
            {
                return o;
            }

            string path = q.Dequeue();
            GObject child = null;
            if (path[0] == '$')
            {
                child = o.GetChild(path);
                if (child == null)
                {
                    string at = path.Substring(1);
                    int index = int.Parse(at);

                    if (index < 0 || index >= o.numChildren)
                    {
                        throw new Exception("eror path");
                    }

                    child = o.GetChildAt(index);
                }
            }
            else
            {
                child = o.GetChild(path);
            }

            if (child == null)
            {
                throw new Exception("error path");
            }

            if (q.Count <= 0)
            {
                // 说明没有下级了
                return child;
            }

            if (child is GComponent)
            {
                return SearchChild(child as GComponent, q);
            }

            throw new Exception("error path");
        }

        /// <summary>
        /// 路径是否包含该对象
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="o">对象</param>
        /// <returns></returns>
        public static bool SearchPathInclude(string path, GObject o)
        {
            if ("all".ToLower() == path)
            {
                return false;
            }

            var q = new List<string>();

            foreach (string v in path.Split('/'))
            {
                if (v == "GRoot")
                {
                    continue;
                }

                q.Add(v);
            }

            GObject current = o;
            var list = new List<GObject>();
            list.Add(current);
            while (current.parent != null && current.parent.name != "GRoot")
            {
                current = current.parent;
                list.Add(current);
            }

            // 反转链表
            list.Reverse();

            if (list.Count < q.Count)
            {
                // 路径长度小于,肯定是不对的
                return false;
            }

            for (int i = 0; i < q.Count; i++)
            {
                if (list[i].name == q[i])
                {
                    continue;
                }

                if (q[i][0] == '$')
                {
                    string at = q[i].Substring(1);
                    int index = int.Parse(at);
                    if (list[i].parent.GetChildIndex(list[i]) == index)
                    {
                        continue;
                    }

                    {
                        return false;
                    }
                }

                return false;
            }

            return true;
        }
    }
}