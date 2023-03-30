// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// An example of how you can wrap a <see cref="RuntimeAnimatorController"/> containing a single blend tree in a
    /// <see cref="Float1ControllerState"/> to easily control its parameter.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/locomotion/linear-blending">Linear Blending</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Locomotion/LinearBlendTreeLocomotion
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Locomotion - Linear Blend Tree Locomotion")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Locomotion) + "/" + nameof(LinearBlendTreeLocomotion))]
    public sealed class LinearBlendTreeLocomotion : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private Float1ControllerTransitionAsset.UnShared _Controller;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Controller);
        }

        /************************************************************************************************************************/

        /// <summary>Controlled by a <see cref="UnityEngine.UI.Slider"/>.</summary>
        public float Speed
        {
            get => _Controller.State.Parameter;
            set => _Controller.State.Parameter = value;
        }

        /************************************************************************************************************************/
    }
}
