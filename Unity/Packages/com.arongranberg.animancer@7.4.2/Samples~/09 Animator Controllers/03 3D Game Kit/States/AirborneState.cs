// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>A <see cref="CharacterState"/> which plays an "airborne" animation.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit/airborne">3D Game Kit/Airborne</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/AirborneState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Airborne State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(AirborneState))]
    public sealed class AirborneState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private LinearMixerTransition _Animations;
        [SerializeField, MetersPerSecond] private float _JumpSpeed = 10;
        [SerializeField, MetersPerSecond] private float _JumpAbortSpeed = 10;
        [SerializeField, Multiplier] private float _TurnSpeedProportion = 5.4f;
        [SerializeField] private LandingState _LandingState;
        [SerializeField] private UnityEvent _PlayAudio;// See the Read Me.

        private bool _IsJumping;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _IsJumping = false;
            Character.Animancer.Play(_Animations);
        }

        /************************************************************************************************************************/

        public override bool StickToGround => false;

        /************************************************************************************************************************/

        /// <summary>
        /// The airborne animations do not have root motion, so we just let the brain determine which way to go.
        /// </summary>
        public override Vector3 RootMotion
            => Character.Parameters.MovementDirection * (Character.Parameters.ForwardSpeed * Time.deltaTime);

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            // When you jump, do not start checking if you have landed until you stop going up.
            if (_IsJumping)
            {
                if (Character.Parameters.VerticalSpeed <= 0)
                    _IsJumping = false;
            }
            else
            {
                // If we have a landing state, try to enter it.
                if (_LandingState != null)
                {
                    if (Character.StateMachine.TrySetState(_LandingState))
                        return;
                }
                else// Otherwise check the default transitions to Idle or Locomotion.
                {
                    if (Character.CheckMotionState())
                        return;
                }

                // If the jump was cancelled but we are still going up, apply some extra downwards acceleration in
                // addition to the regular graivty applied in Character.OnAnimatorMove.
                if (Character.Parameters.VerticalSpeed > 0)
                    Character.Parameters.VerticalSpeed -= _JumpAbortSpeed * Time.deltaTime;
            }

            _Animations.State.Parameter = Character.Parameters.VerticalSpeed;

            Character.Movement.UpdateSpeedControl();

            var movement = Character.Parameters.MovementDirection;

            // Since we do not have quick turn animations like the LocomotionState, we just increase the turn speed
            // when the direction we want to go is further away from the direction we are currently facing.
            var turnSpeed = Vector3.Angle(Character.transform.forward, movement) * (1f / 180) *
                _TurnSpeedProportion *
                Character.Movement.CurrentTurnSpeed;

            Character.Movement.TurnTowards(movement, turnSpeed);
        }

        /************************************************************************************************************************/

        public bool TryJump()
        {
            // We did not override CanEnterState to check if the Character is grounded because this state is also used
            // if you walk off a ledge, so instead we check that condition here when specifically attempting to jump.
            if (Character.Movement.IsGrounded &&
                Character.StateMachine.TryResetState(this))
            {
                // Entering this state would have called OnEnable.

                _IsJumping = true;
                Character.Parameters.VerticalSpeed = _JumpSpeed;

                // In the 3D Game Kit the jump sound is actually triggered whenever you have a positive VerticalSpeed
                // when you become airborne, which could happen if you go up a ramp for example.
                _PlayAudio.Invoke();

                return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        public void CancelJump() => _IsJumping = false;

        /************************************************************************************************************************/
    }
}
