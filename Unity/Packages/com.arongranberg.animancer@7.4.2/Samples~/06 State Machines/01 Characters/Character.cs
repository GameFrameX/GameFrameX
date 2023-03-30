// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>
    /// A centralised group of references to the common parts of a character and a state machine for their actions.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/characters">Characters</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/Character
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Characters - Character")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(Character))]
    [DefaultExecutionOrder(-10000)]// Initialize the StateMachine before anything uses it.
    public sealed class Character : MonoBehaviour
    {
        /************************************************************************************************************************/
        // Used in the Characters example.
        /************************************************************************************************************************/

        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer => _Animancer;

        [SerializeField]
        private CharacterState.StateMachine _StateMachine;
        public CharacterState.StateMachine StateMachine => _StateMachine;

        private void Awake()
        {
            StateMachine.InitializeAfterDeserialize();
        }

        /************************************************************************************************************************/
        // Used in the Interruptions example.
        /************************************************************************************************************************/

        [SerializeField]
        private HealthPool _Health;
        public HealthPool Health => _Health;

        /************************************************************************************************************************/
        // Used in the Brains example.
        /************************************************************************************************************************/

        [SerializeField]
        private CharacterParameters _Parameters;
        public CharacterParameters Parameters => _Parameters;

        /************************************************************************************************************************/
        // Used in the Weapons example.
        /************************************************************************************************************************/

        [SerializeField]
        private Equipment _Equipment;
        public Equipment Equipment => _Equipment;

        /************************************************************************************************************************/
    }
}
