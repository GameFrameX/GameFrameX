using System;
using DG.Tweening;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Do Tween 帮助类
    /// </summary>
    public static class DoTweenHelper
    {
        /// <summary>
        /// 关闭对象上的全部动画
        /// </summary>
        /// <param name="gameObject">物体对象</param>
        /// <param name="complete">是否直接完成动画</param>
        public static void Kill(GameObject gameObject, bool complete = false)
        {
            DOTween.Kill(gameObject, complete);
        }

        /// <summary>
        /// 从浮点值到另一个值
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <returns></returns>
        public static Tweener To(float startValue, float endValue, float time, Action<float> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <param name="complete">完成回调</param>
        /// <returns></returns>
        public static Tweener To(float startValue, float endValue, float time, Action<float> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <returns></returns>
        public static Tweener To(Vector3 startValue, Vector3 endValue, float time, Action<Vector3> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <param name="complete">完成回调</param>
        /// <returns></returns>
        public static Tweener To(Vector3 startValue, Vector3 endValue, float time, Action<Vector3> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <returns></returns>
        public static Tweener To(Vector3Int startValue, Vector3Int endValue, float time, Action<Vector3Int> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(new Vector3Int((int)m.x, (int)m.y, (int)m.z)); }, endValue, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <returns></returns>
        public static Tweener To(Vector2 startValue, Vector2 endValue, float time, Action<Vector2> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <param name="complete">完成回调</param>
        /// <returns></returns>
        public static Tweener To(Vector2 startValue, Vector2 endValue, float time, Action<Vector2> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <returns></returns>
        public static Tweener To(Vector2Int startValue, Vector2Int endValue, float time, Action<Vector2Int> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(new Vector2Int((int)m.x, (int)m.y)); }, endValue, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startValue">开始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="time">持续时长</param>
        /// <param name="update">更新回调</param>
        /// <param name="complete">完成回调</param>
        /// <returns></returns>
        public static Tweener To(Vector2Int startValue, Vector2Int endValue, float time, Action<Vector2Int> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(new Vector2Int((int)m.x, (int)m.y)); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }
    }
}