// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// An example of how you can use a <see cref="LinearMixerState"/> to mix a set of animations based on a
    /// <see cref="Speed"/> parameter.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/locomotion/linear-blending">Linear Blending</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Locomotion/LinearMixerLocomotion
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Locomotion - Linear Mixer Locomotion")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Locomotion) + "/" + nameof(LinearMixerLocomotion))]
    public sealed class LinearMixerLocomotion : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private LinearMixerTransitionAsset.UnShared _Mixer;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Mixer);
        }

        /************************************************************************************************************************/

        /// <summary>Controlled by a <see cref="UnityEngine.UI.Slider"/>.</summary>
        public float Speed
        {
            get => _Mixer.State.Parameter;
            set => _Mixer.State.Parameter = value;
        }

        /************************************************************************************************************************/
    }
}
