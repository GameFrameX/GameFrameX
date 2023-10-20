using System.Collections;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 协程组件
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Coroutine")]
    public class CoroutineComponent : GameFrameworkComponent
    {
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// 等待当前帧结束
        /// </summary>
        /// <returns></returns>
        private IEnumerator _WaitForEndOfFrameFinish(System.Action callback)
        {
            yield return _waitForEndOfFrame;
            callback?.Invoke();
        }

        /// <summary>
        /// 等待当前帧结束
        /// </summary>
        /// <param name="callback"></param>
        public void WaitForEndOfFrameFinish(System.Action callback)
        {
            StartCoroutine(_WaitForEndOfFrameFinish(callback));
        }
    }
}