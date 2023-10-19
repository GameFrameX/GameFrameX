using System;
using DG.Tweening;
using UnityEngine;

namespace GameFrameX.Extension.DoTween
{
    public static class DoTweenHelper
    {
        public static void Kill(GameObject gameObject, bool complete = false)
        {
            DOTween.Kill(gameObject, complete);
        }

        public static Tweener To(float startValue, float endValue, float time, Action<float> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time);
        }

        public static Tweener To(float startValue, float endValue, float time, Action<float> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }

        public static Tweener To(Vector3 startValue, Vector3 endValue, float time, Action<Vector3> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time);
        }

        public static Tweener To(Vector3 startValue, Vector3 endValue, float time, Action<Vector3> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }

        public static Tweener To(Vector3Int startValue, Vector3Int endValue, float time, Action<Vector3Int> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(new Vector3Int((int) m.x, (int) m.y, (int) m.z)); }, endValue, time);
        }

        public static Tweener To(Vector2 startValue, Vector2 endValue, float time, Action<Vector2> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time);
        }

        public static Tweener To(Vector2 startValue, Vector2 endValue, float time, Action<Vector2> update, Action complete)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(m); }, endValue, time).OnComplete(() => { complete?.Invoke(); });
        }

        public static Tweener To(Vector2Int startValue, Vector2Int endValue, float time, Action<Vector2Int> update)
        {
            return DOTween.To(() => startValue, (m) => { update?.Invoke(new Vector2Int((int) m.x, (int) m.y)); }, endValue, time);
        }
    }
}