// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A component which takes the root motion from an <see cref="Animator"/> and applies it to a
    /// <see cref="Rigidbody"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/RedirectRootMotionToRigidbody
    /// 
    [AddComponentMenu("Animancer/Redirect Root Motion To Rigidbody")]
    [HelpURL("https://kybernetik.com.au/animancer/api/Animancer/" + nameof(RedirectRootMotionToRigidbody))]
    public class RedirectRootMotionToRigidbody : RedirectRootMotion<Rigidbody>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnAnimatorMove()
        {
            if (!ApplyRootMotion)
                return;

            Target.MovePosition(Target.position + Animator.deltaPosition);
            Target.MoveRotation(Target.rotation * Animator.deltaRotation);
        }

        /************************************************************************************************************************/
    }
}
