// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Layers
{
    /// <summary>Demonstrates how to use layers to play multiple animations at once on different body parts.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/layers">Layers</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Layers/DynamicLayeredCharacterAnimations
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Layers - Dynamic Layered Character Animations")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Layers) + "/" + nameof(DynamicLayeredCharacterAnimations))]
    public sealed class DynamicLayeredCharacterAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private LayeredAnimationManager _AnimationManager;
        [SerializeField] private ClipTransition _Idle;
        [SerializeField] private ClipTransition _Move;
        [SerializeField] private ClipTransition _Action;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Action.Events.OnEnd = _AnimationManager.FadeOutUpperBody;
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
                _AnimationManager.PlayBase(_Move, false);
            }
            else
            {
                _AnimationManager.PlayBase(_Idle, true);
            }
        }

        /************************************************************************************************************************/

        private void UpdateAction()
        {
            if (ExampleInput.LeftMouseUp)
            {
                _AnimationManager.PlayAction(_Action);
            }
        }

        /************************************************************************************************************************/
    }
}
