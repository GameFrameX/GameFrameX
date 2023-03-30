// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>Plays an animation to demonstrate the basic usage of Animancer.</summary>
    /// <remarks>
    /// If you actually want to only play one animation on an object and don't need any of the other features of
    /// Animancer, you can use the <see cref="SoloAnimation"/> component to do so without needing an extra script.
    /// </remarks>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/basics/quick-play">Quick Play</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Basics/PlayAnimationOnEnable
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Basics - Play Animation On Enable")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Basics) + "/" + nameof(PlayAnimationOnEnable))]
    public sealed class PlayAnimationOnEnable : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Animation;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Animation);
        }

        /************************************************************************************************************************/
    }
}
