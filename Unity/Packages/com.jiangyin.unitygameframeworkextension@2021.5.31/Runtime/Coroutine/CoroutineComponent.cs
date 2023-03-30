using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 协程组件
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Coroutine")]
    public class CoroutineComponent : GameFrameworkComponent
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}