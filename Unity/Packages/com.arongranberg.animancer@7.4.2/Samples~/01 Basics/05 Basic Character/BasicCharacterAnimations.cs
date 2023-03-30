// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    /// Combines <see cref="BasicMovementAnimations"/> and <see cref="PlayTransitionOnClick"/> into one script.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/basics/character">Basic Character</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Basics/BasicCharacterAnimations
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Basics - Basic Character Animations")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Basics) + "/" + nameof(BasicCharacterAnimations))]
    public sealed class BasicCharacterAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipTransition _Idle;
        [SerializeField] private ClipTransition _Move;
        [SerializeField] private ClipTransition _Action;

        private enum State
        {
            /// <summary><see cref="_Idle"/> or <see cref="_Move"/>.</summary>
            NotActing,

            /// <summary><see cref="_Action"/>.</summary>
            Acting,
        }

        private State _CurrentState;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Action.Events.OnEnd = OnActionEnd;
        }

        /************************************************************************************************************************/

        private void OnActionEnd()
        {
            _CurrentState = State.NotActing;
            UpdateMovement();
        }

        /************************************************************************************************************************/

        private void Update()
        {
            switch (_CurrentState)
            {
                case State.NotActing:
                    UpdateMovement();
                    UpdateAction();
                    break;

                case State.Acting:
                    UpdateAction();
                    break;
            }
        }

        /************************************************************************************************************************/

        private void UpdateMovement()
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

        private void UpdateAction()
        {
            if (ExampleInput.LeftMouseUp)
            {
                _CurrentState = State.Acting;
                _Animancer.Play(_Action);
            }
        }

        /************************************************************************************************************************/
    }
}
