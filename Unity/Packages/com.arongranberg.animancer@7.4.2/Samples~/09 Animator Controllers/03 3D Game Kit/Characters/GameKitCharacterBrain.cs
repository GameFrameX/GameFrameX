// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using Animancer.Units;
using UnityEngine;
using static Animancer.Validate;

namespace Animancer.Examples.AnimatorControllers.GameKit
{
    /// <summary>A brain which controls the character using keyboard input.</summary>
    /// <remarks>This class serves the same purpose as <c>PlayerInput</c> from the 3D Game Kit.</remarks>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit">3D Game Kit</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.AnimatorControllers.GameKit/GameKitCharacterBrain
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Game Kit - Game Kit Character Brain")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(AnimatorControllers) + "." + nameof(GameKit) + "/" + nameof(GameKitCharacterBrain))]
    public sealed class GameKitCharacterBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private Character _Character;
        [SerializeField] private AirborneState _Jump;
        [SerializeField] private CharacterState _Attack;

        [SerializeField]
        [Seconds(Rule = Value.IsNotNegative)]
        private float _AttackInputTimeOut = 0.5f;

        private StateMachine<CharacterState>.InputBuffer _InputBuffer;

        /************************************************************************************************************************/

        private void Awake()
        {
            _InputBuffer = new StateMachine<CharacterState>.InputBuffer(_Character.StateMachine);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateMovement();
            UpdateActions();
        }

        /************************************************************************************************************************/

        private void UpdateMovement()
        {
            var input = ExampleInput.WASD;
            if (input == default)
            {
                _Character.Parameters.MovementDirection = default;
                return;
            }

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
        }

        /************************************************************************************************************************/

        private void UpdateActions()
        {
            // Jump gets priority for better platforming.
            if (ExampleInput.SpaceDown)
            {
                _Jump.TryJump();
            }
            else if (ExampleInput.SpaceUp)
            {
                _Jump.CancelJump();
            }

            if (ExampleInput.LeftMouseDown)
            {
                _InputBuffer.Buffer(_Attack, _AttackInputTimeOut);
            }

            _InputBuffer.Update();
        }

        /************************************************************************************************************************/
    }
}
