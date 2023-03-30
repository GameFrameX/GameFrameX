// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.DirectionalSprites
{
    /// <summary>
    /// Animates a character to either stand idle or walk using animations defined in
    /// <see cref="DirectionalAnimationSet"/>s.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/directional-sprites/basics">Directional Basics</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.DirectionalSprites/DirectionalBasics
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Directional Sprites - Directional Basics")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(DirectionalSprites) + "/" + nameof(DirectionalBasics))]
    public sealed class DirectionalBasics : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private DirectionalAnimationSet _Idles;
        [SerializeField] private DirectionalAnimationSet _Walks;
        [SerializeField] private Vector2 _Facing = Vector2.down;

        /************************************************************************************************************************/

        private void Update()
        {
            var input = ExampleInput.WASD;
            if (input != default)
            {
                _Facing = input;

                Play(_Walks);

                // Play could return the AnimancerState it gets from _Animancer.Play,
                // But we can also just access it using _Animancer.States.Current.

                var isRunning = ExampleInput.LeftShiftHold;
                _Animancer.States.Current.Speed = isRunning ? 2 : 1;
            }
            else
            {
                // When we're not moving, we still remember the direction we're facing
                // so we can continue using the correct idle animation for that direction.
                Play(_Idles);
            }
        }

        /************************************************************************************************************************/

        private void Play(DirectionalAnimationSet animations)
        {
            // Instead of only a single animation, we have a different one for each direction we can face.
            // So we get whichever is appropriate for that direction and play it.

            var clip = animations.GetClip(_Facing);
            _Animancer.Play(clip);

            // Or we could do that in one line:
            // _Animancer.Play(animations.GetClip(_Facing));
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Sets the character's starting sprite in Edit Mode so you can see it while working in the scene.
        /// </summary>
        private void OnValidate()
        {
            if (_Idles != null)
                _Idles.GetClip(_Facing).EditModeSampleAnimation(_Animancer);
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
