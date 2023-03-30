// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Examples.FineControl;
using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// Controls a <see cref="SpiderBot"/> with a <see cref="MixerTransition2D"/> and <see cref="Rigidbody"/> to allow
    /// the bot to move around the scene in any direction.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/locomotion/directional-blending">Directional Blending</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Locomotion/SpiderBotController
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Locomotion - Spider Bot Controller")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Locomotion) + "/" + nameof(SpiderBotController))]
    public sealed class SpiderBotController : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private SpiderBot _SpiderBot;
        [SerializeField] private Rigidbody _Body;
        [SerializeField, DegreesPerSecond] private float _TurnSpeed = 90;
        [SerializeField, MetersPerSecond] private float _MovementSpeed = 1.5f;
        [SerializeField, Multiplier] private float _SprintMultiplier = 2;

        /************************************************************************************************************************/

        private MixerState<Vector2> _MoveState;
        private Vector3 _MovementDirection;

        /************************************************************************************************************************/

        private void Awake()
        {
            // _SpiderBot.Move is an ITransition which doesn't have a Parameter property for us to control in Update.
            // So we need to create its state, type cast it to MixerState<Vector2>, and store it in a field.
            // Then we will be able to control that field's Parameter in Update.
            var state = _SpiderBot.Animancer.States.GetOrCreate(_SpiderBot.Move);
            _MoveState = (MixerState<Vector2>)state;
        }

        /************************************************************************************************************************/

        private void Update()
        {
            // Calculate the movement direction.
            _MovementDirection = GetMovementDirection();

            // The bot should be moving whenever the direction isn't zero.
            _SpiderBot.IsMoving = _MovementDirection != default;

            // If the movement state is playing and not fading out:
            if (_MoveState.IsActive)
            {
                // Rotate towards the same angle around the Y axis as the camera.
                var eulerAngles = transform.eulerAngles;
                var targetEulerY = Camera.main.transform.eulerAngles.y;
                eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetEulerY, _TurnSpeed * Time.deltaTime);
                transform.eulerAngles = eulerAngles;

                // The movement direction is in world space, so we need to convert it to the bot's local space to be
                // appropriate for its current rotation. We do this by using dot-products to determine how much of that
                // direction lies along each axis. This would be unnecessary if we did not rotate at all.
                _MoveState.Parameter = new Vector2(
                    Vector3.Dot(transform.right, _MovementDirection),
                    Vector3.Dot(transform.forward, _MovementDirection));

                // Set its speed depending on whether you are sprinting or not.
                _MoveState.Speed = ExampleInput.LeftMouseHold ? _SprintMultiplier : 1;
            }
            else// Otherwise stop it entirely.
            {
                _MoveState.Parameter = default;
                _MoveState.Speed = 0;
            }
        }

        /************************************************************************************************************************/

        private Vector3 GetMovementDirection()
        {
            // Get a ray from the main camera in the direction of the mouse cursor.
            var ray = Camera.main.ScreenPointToRay(ExampleInput.MousePosition);

            // Do a raycast with it and stop trying to move it it does not hit anything.
            // Note that this object is set to the Ignore Raycast layer so that the raycast will not hit it.
            if (!Physics.Raycast(ray, out var raycastHit))// Note the exclamation mark !
                return default;

            // If the ray hit something, calculate the horizontal direction from this object to that point.
            var direction = raycastHit.point - transform.position;
            direction.y = 0;

            // Calculate how far we could move this frame at max speed.
            var movementThisFrame = _MovementSpeed * _SprintMultiplier * Time.fixedDeltaTime;

            // If we are close to the destination, stop moving.
            var distance = direction.magnitude;
            if (distance <= movementThisFrame)
            {
                return default;
            }
            else
            {
                // Otherwise normalize the direction so that we do not change speed based on distance.
                // Calling direction.Normalize() would do the same thing, but would calculate the magnitude again.
                return direction / distance;
            }
        }

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            // Set the velocity so that Unity will move the Rigidbody in the desired direction.
            _Body.velocity = _MoveState.Speed * _MovementSpeed * _MovementDirection;
        }

        /************************************************************************************************************************/
    }
}
