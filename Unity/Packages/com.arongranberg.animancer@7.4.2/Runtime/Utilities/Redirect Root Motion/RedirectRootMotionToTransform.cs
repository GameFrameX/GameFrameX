// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A component which takes the root motion from an <see cref="Animator"/> and applies it to a
    /// <see cref="Transform"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/RedirectRootMotionToTransform
    /// 
    [AddComponentMenu("Animancer/Redirect Root Motion To Transform")]
    [HelpURL("https://kybernetik.com.au/animancer/api/Animancer/" + nameof(RedirectRootMotionToTransform))]
    public class RedirectRootMotionToTransform : RedirectRootMotion<Transform>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnAnimatorMove()
        {
            if (!ApplyRootMotion)
                return;

            Target.position += Animator.deltaPosition;
            Target.rotation *= Animator.deltaRotation;
        }

        /************************************************************************************************************************/
    }
}
