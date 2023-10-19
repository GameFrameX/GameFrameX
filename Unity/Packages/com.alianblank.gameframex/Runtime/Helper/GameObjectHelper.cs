using UnityEngine;
using UnityEngine.Rendering;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 游戏对象帮助类
    /// </summary>
    public static class GameObjectHelper
    {
        /// <summary>
        /// 销毁子物体
        /// </summary>
        /// <param name="go"></param>
        public static void RemoveChildren(GameObject go)
        {
            for (var i = go.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(go.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 销毁游戏物体
        /// </summary>
        /// <param name="gameObject"></param>
        public static void DestroyObject(this GameObject gameObject)
        {
            if (!ReferenceEquals(gameObject, null))
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(gameObject);
                    return;
                }

                Object.Destroy(gameObject, 0.1f);
            }
        }

        /// <summary>
        /// 销毁游戏物体
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(GameObject gameObject)
        {
            gameObject.DestroyObject();
        }

        /// <summary>
        /// 销毁游戏组件
        /// </summary>
        /// <param name="component"></param>
        public static void DestroyComponent(Component component)
        {
            if (!ReferenceEquals(component, null))
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(component);
                    return;
                }

                Object.Destroy(component, 0.1f);
            }
        }

        /// <summary>
        /// 根据游戏对象名称查询子对象
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject FindChildGamObjectByName(GameObject gameObject, string name)
        {
            var tfs = gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in tfs)
            {
                if (t.name.EqualsFast(name))
                {
                    return t.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// 创建游戏对象
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject Create(Transform parent, string name)
        {
            Debug.Assert(!ReferenceEquals(parent, null), nameof(parent) + " == null");
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            return gameObject;
        }

        /// <summary>
        /// 创建游戏对象
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject Create(GameObject parent, string name)
        {
            Debug.Assert(!ReferenceEquals(parent, null), nameof(parent) + " == null");
            return Create(parent.transform, name);
        }

        /// <summary>
        /// 重置游戏对象的变换数据
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static void ResetTransform(GameObject gameObject)
        {
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 设置对象的显示排序层
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        /// <param name="sortingLayer">显示层</param>
        public static void SetSortingGroupLayer(GameObject gameObject, string sortingLayer)
        {
            SortingGroup[] sortingGroups = gameObject.GetComponentsInChildren<SortingGroup>();
            foreach (SortingGroup sg in sortingGroups)
            {
                sg.sortingLayerName = sortingLayer;
            }
        }

        /// <summary>
        /// 设置对象的层
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        /// <param name="layer">层</param>
        /// <param name="children">是否设置子物体</param>
        public static void SetLayer(GameObject gameObject, int layer, bool children = true)
        {
            if (gameObject.layer != layer)
            {
                gameObject.layer = layer;
            }

            if (children)
            {
                Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
                foreach (var sg in transforms)
                {
                    sg.gameObject.layer = layer;
                }
            }
        }
    }
}