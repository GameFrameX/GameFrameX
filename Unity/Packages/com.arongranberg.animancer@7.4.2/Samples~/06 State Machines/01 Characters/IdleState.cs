// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>A <see cref="CharacterState"/> which plays an animation.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/characters">Characters</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/IdleState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Characters - Idle State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(IdleState))]
    public sealed class IdleState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipTransition _Animation;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Animancer.Play(_Animation);
        }

        /************************************************************************************************************************/
    }
}
