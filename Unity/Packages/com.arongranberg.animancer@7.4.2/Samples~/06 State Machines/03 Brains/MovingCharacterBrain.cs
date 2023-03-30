// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>Uses player input to control a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/brains">Brains</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/MovingCharacterBrain
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Brains - Moving Character Brain")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(MovingCharacterBrain))]
    public sealed class MovingCharacterBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Character _Character;
        [SerializeField] private CharacterState _Move;
        [SerializeField] private CharacterState _Action;

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateMovement();
            UpdateAction();
        }

        /************************************************************************************************************************/

        private void UpdateMovement()
        {
            var input = ExampleInput.WASD;
            if (input != default)
            {
                // Get the camera's forward and right vectors and flatten them onto the XZ plane.
                var camera = Camera.main.transform;

                var forward = camera.forward;
                forward.y = 0;
                forward.Normalize();

                var right = camera.right;
                right.y = 0;
                right.Normalize();

                // Build the movement vector by multiplying the input by those axes.
                _Character.Parameters.MovementDirection =
                   right * input.x +
                   forward * input.y;

                // Enter the movement state if we aren't already in it.
                _Character.StateMachine.TrySetState(_Move);
            }
            else// If we aren't trying to move, clear the movement vector and return to idle.
            {
                _Character.Parameters.MovementDirection = default;
                _Character.StateMachine.TrySetDefaultState();
            }

            // Indicate whether the character wants to run or not.
            _Character.Parameters.WantsToRun = ExampleInput.LeftShiftHold;
        }

        /************************************************************************************************************************/

        private void UpdateAction()
        {
            if (ExampleInput.LeftMouseUp)
                _Character.StateMachine.TryResetState(_Action);
        }

        /************************************************************************************************************************/
    }
}
