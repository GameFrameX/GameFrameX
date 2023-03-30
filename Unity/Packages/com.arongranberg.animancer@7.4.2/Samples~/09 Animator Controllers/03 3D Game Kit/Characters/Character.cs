// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>
    /// A centralised group of references to the common parts of a character and a state machine for their actions.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit">3D Game Kit</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/Character
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Character")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(Character))]
    public sealed class Character : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer => _Animancer;

        [SerializeField]
        private CharacterMovement _Movement;
        public CharacterMovement Movement => _Movement;

        [SerializeField]
        private CharacterParameters _Parameters;
        public CharacterParameters Parameters => _Parameters;

        /************************************************************************************************************************/

        [SerializeField]
        private CharacterState.StateMachine _StateMachine;
        public CharacterState.StateMachine StateMachine => _StateMachine;

        private void Awake()
        {
            StateMachine.InitializeAfterDeserialize();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Check if this <see cref="Character"/> should enter the Idle, Locomotion, or Airborne state depending on
        /// whether it is grounded and the movement input from the <see cref="Brain"/>.
        /// </summary>
        /// <remarks>
        /// We could add some null checks to this method to support characters that don't have all the standard states,
        /// such as a character that can't move or a flying character that never lands.
        /// </remarks>
        public bool CheckMotionState()
        {
            CharacterState state;
            if (Movement.IsGrounded)
            {
                state = Parameters.MovementDirection == default
                    ? StateMachine.DefaultState
                    : StateMachine.Locomotion;
            }
            else
            {
                state = StateMachine.Airborne;
            }

            return
                state != StateMachine.CurrentState &&
                StateMachine.TryResetState(state);
        }

        /************************************************************************************************************************/
    }
}
