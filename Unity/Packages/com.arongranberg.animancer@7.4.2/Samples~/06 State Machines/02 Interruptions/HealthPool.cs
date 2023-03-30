// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Examples.FineControl;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>Manages a character's health and damage received.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/interruptions">Interruptions</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/HealthPool
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Interruptions - Health Pool")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(HealthPool))]
    public sealed class HealthPool : MonoBehaviour, IInteractable
    {
        /************************************************************************************************************************/

        // Normally, this class would have fields like maximum health and current health to keep track of how much
        // damage the character takes, but for this example we're just pretending the character was hit whenever
        // something interacts with it.

        public event Action OnHitReceived;

        /************************************************************************************************************************/

        public void Interact()
        {
            OnHitReceived?.Invoke();
        }

        /************************************************************************************************************************/
    }
}
