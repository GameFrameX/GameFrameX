// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.Layers
{
    /// <summary>Demonstrates how to use layers to play multiple animations at once on different body parts.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/layers">Layers</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Layers/LayeredAnimationManager
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Layers - Layered Animation Manager")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Layers) + "/" + nameof(LayeredAnimationManager))]
    public sealed class LayeredAnimationManager : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AvatarMask _ActionMask;
        [SerializeField, Seconds] private float _ActionFadeDuration = AnimancerPlayable.DefaultFadeDuration;

        private AnimancerLayer _BaseLayer;
        private AnimancerLayer _ActionLayer;
        private bool _CanPlayActionFullBody;

        /************************************************************************************************************************/

        private void Awake()
        {
            _BaseLayer = _Animancer.Layers[0];
            _ActionLayer = _Animancer.Layers[1];

            _ActionLayer.SetMask(_ActionMask);
        }

        /************************************************************************************************************************/

        public void PlayBase(ITransition transition, bool canPlayActionFullBody)
        {
            _CanPlayActionFullBody = canPlayActionFullBody;

            if (_CanPlayActionFullBody && _ActionLayer.TargetWeight > 0)
            {
                PlayActionFullBody(_ActionFadeDuration);
            }
            else
            {
                _BaseLayer.Play(transition);
            }
        }

        /************************************************************************************************************************/

        public void PlayAction(ITransition transition)
        {
            _ActionLayer.Play(transition);

            if (_CanPlayActionFullBody)
                PlayActionFullBody(transition.FadeDuration);
        }

        /************************************************************************************************************************/

        private void PlayActionFullBody(float fadeDuration)
        {
            var actionState = _ActionLayer.CurrentState;
            var baseState = _BaseLayer.Play(actionState.Clip, fadeDuration);
            baseState.NormalizedTime = actionState.NormalizedTime;
        }

        /************************************************************************************************************************/

        public void FadeOutUpperBody()
        {
            _ActionLayer.StartFade(0, _ActionFadeDuration);
        }

        /************************************************************************************************************************/
    }
}
