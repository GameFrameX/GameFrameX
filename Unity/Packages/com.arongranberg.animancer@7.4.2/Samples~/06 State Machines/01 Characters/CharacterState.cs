// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>A state for a <see cref="Character"/>.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/characters">Characters</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/CharacterState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Characters - Character State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(CharacterState))]
    public abstract class CharacterState : StateBehaviour
    {
        /************************************************************************************************************************/

        [System.Serializable]
        public class StateMachine : StateMachine<CharacterState>.WithDefault { }

        /************************************************************************************************************************/

        [SerializeField]
        private Character _Character;
        public Character Character => _Character;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            gameObject.GetComponentInParentOrChildren(ref _Character);
        }
#endif

        /************************************************************************************************************************/
        // Explained in the Interruptions example.
        /************************************************************************************************************************/

        public virtual CharacterStatePriority Priority => CharacterStatePriority.Low;

        public virtual bool CanInterruptSelf => false;

        public override bool CanExitState
        {
            get
            {
                // There are several different ways of accessing the state change details:
                // var nextState = StateChange<CharacterState>.NextState;
                // var nextState = this.GetNextState();
                var nextState = _Character.StateMachine.NextState;
                if (nextState == this)
                    return CanInterruptSelf;
                else if (Priority == CharacterStatePriority.Low)
                    return true;
                else
                    return nextState.Priority > Priority;
            }
        }

        /************************************************************************************************************************/
    }
}
