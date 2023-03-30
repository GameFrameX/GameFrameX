// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    /// Plays a movement animation while the user holds W or Up Arrow.
    /// Otherwise plays an idle animation.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/basics/movement">Basic Movement</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Basics/BasicMovementAnimations
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Basics - Basic Movement Animations")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Basics) + "/" + nameof(BasicMovementAnimations))]
    public sealed class BasicMovementAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Idle;
        [SerializeField] private AnimationClip _Move;

        /************************************************************************************************************************/

        private void Update()
        {
            float forward = ExampleInput.WASD.y;
            if (forward > 0)
            {
                _Animancer.Play(_Move);
            }
            else
            {
                _Animancer.Play(_Idle);
            }
        }

        /************************************************************************************************************************/
    }
}
