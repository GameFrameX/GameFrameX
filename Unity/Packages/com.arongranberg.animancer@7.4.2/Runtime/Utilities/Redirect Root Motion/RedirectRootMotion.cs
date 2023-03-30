// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A component which takes the root motion from an <see cref="UnityEngine.Animator"/> and applies it to a
    /// different object.
    /// </summary>
    /// <remarks>
    /// This can be useful if the character's <see cref="Rigidbody"/> or <see cref="CharacterController"/> is on a
    /// parent of the <see cref="UnityEngine.Animator"/> to keep the model separate from the logical components.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/RedirectRootMotion_1
    /// 
    [HelpURL("https://kybernetik.com.au/animancer/api/Animancer/" + nameof(RedirectRootMotion<T>) + "_1")]
    [RequireComponent(typeof(Animator))]
    public abstract class RedirectRootMotion<T> : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The Animator which provides the root motion")]
        private Animator _Animator;

        /// <summary>The <see cref="UnityEngine.Animator"/> which provides the root motion.</summary>
        public ref Animator Animator => ref _Animator;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The object which the root motion will be applied to")]
        private T _Target;

        /// <summary>The object which the root motion will be applied to.</summary>
        public ref T Target => ref _Target;

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the <see cref="Target"/> and <see cref="Animator"/> are set and
        /// <see cref="Animator.applyRootMotion"/> is enabled.
        /// </summary>
        public bool ApplyRootMotion
            => Target != null
            && Animator != null
            && Animator.applyRootMotion;

        /************************************************************************************************************************/

        /// <summary>Automatically finds the <see cref="Animator"/> and <see cref="Target"/>.</summary>
        protected virtual void OnValidate()
        {
            TryGetComponent(out _Animator);

            if (_Target == null)
                _Target = transform.parent.GetComponentInParent<T>();
        }

        /************************************************************************************************************************/

        /// <summary>Applies the root motion from the <see cref="Animator"/> to the target object.</summary>
        protected abstract void OnAnimatorMove();

        /************************************************************************************************************************/
    }
}
