// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>Uses player input to control a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/characters">Characters</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/BasicCharacterBrain
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Characters - Basic Character Brain")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(BasicCharacterBrain))]
    public sealed class BasicCharacterBrain : MonoBehaviour
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
            float forward = ExampleInput.WASD.y;
            if (forward > 0)
            {
                _Character.StateMachine.TrySetState(_Move);
            }
            else
            {
                _Character.StateMachine.TrySetDefaultState();
            }
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
