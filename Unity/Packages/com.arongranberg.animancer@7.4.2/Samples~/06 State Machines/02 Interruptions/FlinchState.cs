// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>A <see cref="CharacterState"/> which activates itself when the character takes damage.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/interruptions">Interruptions</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/FlinchState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Interruptions - Flinch State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(FlinchState))]
    public sealed class FlinchState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipTransition _Animation;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Animation.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;

            Character.Health.OnHitReceived += () => Character.StateMachine.TryResetState(this);
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Animancer.Play(_Animation);
        }

        /************************************************************************************************************************/

        public override CharacterStatePriority Priority => CharacterStatePriority.High;

        public override bool CanInterruptSelf => true;

        /************************************************************************************************************************/
    }
}
