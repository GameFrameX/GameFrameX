// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.Layers
{
    /// <summary>Demonstrates how to use layers to play multiple animations at once on different body parts.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/layers">Layers</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Layers/LayeredCharacterAnimations
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Layers - Layered Character Animations")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Layers) + "/" + nameof(LayeredCharacterAnimations))]
    public sealed class LayeredCharacterAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipTransition _Idle;
        [SerializeField] private ClipTransition _Move;
        [SerializeField] private ClipTransition _Action;
        [SerializeField] private AvatarMask _ActionMask;
        [SerializeField, Seconds] private float _ActionFadeOutDuration = AnimancerPlayable.DefaultFadeDuration;

        /************************************************************************************************************************/

        private AnimancerLayer _BaseLayer;
        private AnimancerLayer _ActionLayer;

        /************************************************************************************************************************/

        private void Awake()
        {
            _BaseLayer = _Animancer.Layers[0];
            _ActionLayer = _Animancer.Layers[1];// First access to a layer creates it.

            _ActionLayer.SetMask(_ActionMask);
            _ActionLayer.SetDebugName("Action Layer");

            _Action.Events.OnEnd = OnActionEnd;
        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateMovement();
            UpdateAction();
        }

        /************************************************************************************************************************/

        private void UpdateMovement()
        {
            float forward = ExampleInput.WASD.y;
            if (forward > 0)
            {
                _BaseLayer.Play(_Move);
            }
            else
            {
                _BaseLayer.Play(_Idle);
            }
        }

        /************************************************************************************************************************/

        private void UpdateAction()
        {
            if (ExampleInput.LeftMouseUp)
            {
                _ActionLayer.Play(_Action);
            }
        }

        /************************************************************************************************************************/

        private void OnActionEnd()
        {
            _ActionLayer.StartFade(0, _ActionFadeOutDuration);
        }

        /************************************************************************************************************************/
    }
}
