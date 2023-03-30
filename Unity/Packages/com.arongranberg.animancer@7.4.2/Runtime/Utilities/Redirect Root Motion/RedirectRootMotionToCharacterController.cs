// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A component which takes the root motion from an <see cref="Animator"/> and applies it to a
    /// <see cref="CharacterController"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/RedirectRootMotionToCharacterController
    /// 
    [AddComponentMenu("Animancer/Redirect Root Motion To Character Controller")]
    [HelpURL("https://kybernetik.com.au/animancer/api/Animancer/" + nameof(RedirectRootMotionToCharacterController))]
    public class RedirectRootMotionToCharacterController : RedirectRootMotion<CharacterController>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnAnimatorMove()
        {
            if (!ApplyRootMotion)
                return;

            Target.Move(Animator.deltaPosition);
            Target.transform.rotation *= Animator.deltaRotation;
        }

        /************************************************************************************************************************/
    }
}
