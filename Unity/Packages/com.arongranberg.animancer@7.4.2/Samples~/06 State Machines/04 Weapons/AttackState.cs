// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>A <see cref="CharacterState"/> which can perform <see cref="Weapon.AttackAnimations"/> in sequence.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/weapons">Weapons</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/AttackState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Weapons - Attack State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(AttackState))]
    public sealed class AttackState : CharacterState
    {
        /************************************************************************************************************************/

        private int _AttackIndex = int.MaxValue;

        public Weapon Weapon => Character.Equipment.Weapon;

        /************************************************************************************************************************/

        public override bool CanEnterState =>
            Weapon != null &&
            Weapon.AttackAnimations.Length > 0;

        /************************************************************************************************************************/

        /// <summary>
        /// Start at the beginning of the sequence by default, but if the previous attack has not faded out yet then
        /// perform the next attack instead.
        /// </summary>
        private void OnEnable()
        {
            if (ShouldRestartCombo())
            {
                _AttackIndex = 0;
            }
            else
            {
                _AttackIndex++;
            }

            var animation = Weapon.AttackAnimations[_AttackIndex];
            animation.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;

            Character.Animancer.Play(animation);
        }

        /************************************************************************************************************************/

        private bool ShouldRestartCombo()
        {
            var attackAnimations = Weapon.AttackAnimations;

            if (_AttackIndex >= attackAnimations.Length - 1)
                return true;

            var state = attackAnimations[_AttackIndex].State;
            if (state == null ||
                state.Weight == 0)
                return true;

            return false;
        }

        /************************************************************************************************************************/

        public override CharacterStatePriority Priority => CharacterStatePriority.Medium;

        /************************************************************************************************************************/
    }
}
