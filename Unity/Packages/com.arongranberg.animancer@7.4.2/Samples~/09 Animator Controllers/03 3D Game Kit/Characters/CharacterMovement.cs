// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using System;
using UnityEngine;
using static Animancer.Validate;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>The stats and logic for moving a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit">3D Game Kit</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/CharacterMovement
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Character Movement")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(CharacterMovement))]
    public sealed class CharacterMovement : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Character _Character;
        [SerializeField] private CharacterController _CharacterController;
        [SerializeField] private bool _FullMovementControl = true;

        /************************************************************************************************************************/

        [SerializeField, MetersPerSecond(Rule = Value.IsNotNegative)]
        private float _MaxSpeed = 8;
        public float MaxSpeed => _MaxSpeed;

        [SerializeField, MetersPerSecondPerSecond(Rule = Value.IsNotNegative)]
        private float _Acceleration = 20;
        public float Acceleration => _Acceleration;

        [SerializeField, MetersPerSecondPerSecond(Rule = Value.IsNotNegative)]
        private float _Deceleration = 25;
        public float Deceleration => _Deceleration;

        [SerializeField, DegreesPerSecond(Rule = Value.IsNotNegative)]
        private float _MinTurnSpeed = 400;
        public float MinTurnSpeed => _MinTurnSpeed;

        [SerializeField, DegreesPerSecond(Rule = Value.IsNotNegative)]
        private float _MaxTurnSpeed = 1200;
        public float MaxTurnSpeed => _MaxTurnSpeed;

        [SerializeField, MetersPerSecondPerSecond(Rule = Value.IsNotNegative)]
        private float _Gravity = 20;
        public float Gravity => _Gravity;

        [SerializeField, Multiplier(Rule = Value.IsNotNegative)]
        private float _StickingGravityProportion = 0.3f;
        public float StickingGravityProportion => _StickingGravityProportion;

        /************************************************************************************************************************/

        public bool IsGrounded { get; private set; }
        public Material GroundMaterial { get; private set; }

        /************************************************************************************************************************/

        public void UpdateSpeedControl()
        {
            var movement = _Character.Parameters.MovementDirection;

            _Character.Parameters.DesiredForwardSpeed = movement.magnitude * MaxSpeed;

            var deltaSpeed = movement != default ? Acceleration : Deceleration;
            _Character.Parameters.ForwardSpeed = Mathf.MoveTowards(
                _Character.Parameters.ForwardSpeed,
                _Character.Parameters.DesiredForwardSpeed,
                deltaSpeed * Time.deltaTime);
        }

        /************************************************************************************************************************/

        public float CurrentTurnSpeed
        {
            get
            {
                return Mathf.Lerp(
                    MaxTurnSpeed,
                    MinTurnSpeed,
                    _Character.Parameters.ForwardSpeed / _Character.Parameters.DesiredForwardSpeed);
            }
        }

        /************************************************************************************************************************/

        public bool GetTurnAngles(Vector3 direction, out float currentAngle, out float targetAngle)
        {
            if (direction == default)
            {
                currentAngle = float.NaN;
                targetAngle = float.NaN;
                return false;
            }

            var transform = this.transform;
            currentAngle = transform.eulerAngles.y;
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            return true;
        }

        /************************************************************************************************************************/

        public void TurnTowards(float currentAngle, float targetAngle, float speed)
        {
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, speed * Time.deltaTime);

            transform.eulerAngles = new Vector3(0, currentAngle, 0);
        }

        public void TurnTowards(Vector3 direction, float speed)
        {
            if (GetTurnAngles(direction, out var currentAngle, out var targetAngle))
                TurnTowards(currentAngle, targetAngle, speed);
        }

        /************************************************************************************************************************/

        private void OnAnimatorMove()
        {
            var movement = GetRootMotion();
            CheckGround(ref movement);
            UpdateGravity(ref movement);
            _CharacterController.Move(movement);

            IsGrounded = _CharacterController.isGrounded;

            transform.rotation *= _Character.Animancer.Animator.deltaRotation;
        }

        /************************************************************************************************************************/

        private Vector3 GetRootMotion()
        {
            var motion = _Character.StateMachine.CurrentState.RootMotion;

            if (!_FullMovementControl ||// If Full Movement Control is disabled in the Inspector.
                !_Character.StateMachine.CurrentState.FullMovementControl)// Or the current state does not want it.
                return motion;// Return the raw Root Motion.

            // If the Brain is not trying to move, we do not move.
            var direction = _Character.Parameters.MovementDirection;
            direction.y = 0;
            if (direction == default)
                return default;

            // Otherwise calculate the Root Motion only in the specified direction.
            direction.Normalize();
            var magnitude = Vector3.Dot(direction, motion);
            return direction * magnitude;
        }

        /************************************************************************************************************************/

        private void CheckGround(ref Vector3 movement)
        {
            if (!_CharacterController.isGrounded)
                return;

            const float GroundedRayDistance = 1f;

            var ray = new Ray(transform.position + GroundedRayDistance * 0.5f * Vector3.up, -Vector3.up);
            if (Physics.Raycast(ray, out var hit, GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                // Rotate the movement to lie along the ground vector.
                movement = Vector3.ProjectOnPlane(movement, hit.normal);

                // Store the current walking surface so the correct audio is played.
                var groundRenderer = hit.collider.GetComponentInChildren<Renderer>();
                GroundMaterial = groundRenderer ? groundRenderer.sharedMaterial : null;
            }
            else
            {
                GroundMaterial = null;
            }
        }

        /************************************************************************************************************************/

        private void UpdateGravity(ref Vector3 movement)
        {
            if (_CharacterController.isGrounded && _Character.StateMachine.CurrentState.StickToGround)
                _Character.Parameters.VerticalSpeed = -Gravity * StickingGravityProportion;
            else
                _Character.Parameters.VerticalSpeed -= Gravity * Time.deltaTime;

            movement.y += _Character.Parameters.VerticalSpeed * Time.deltaTime;
        }

        /************************************************************************************************************************/

        // Ignore these Animation Events because the attack animations will only start when we tell them to, so it
        // would be silly to use additional events for something we already directly caused. That sort of thing is only
        // necessary in Animator Controllers because they run their own logic to decide what they want to do.
        private void MeleeAttackStart(int throwing = 0) { }
        private void MeleeAttackEnd() { }

        /************************************************************************************************************************/
    }
}
