// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>A <see cref="CharacterState"/> which plays an animation then returns to idle.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/characters">Characters</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/ActionState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Characters - Action State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(ActionState))]
    public sealed class ActionState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipTransition _Animation;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Animation.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Animancer.Play(_Animation);
        }

        /************************************************************************************************************************/
        // Explained in the Interruptions example.
        /************************************************************************************************************************/

        public override CharacterStatePriority Priority => CharacterStatePriority.Medium;

        public override bool CanInterruptSelf => true;

        /************************************************************************************************************************/
    }
}
