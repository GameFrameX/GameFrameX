// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

namespace Animancer.Examples.StateMachines
{
    /// <summary>Levels of importance for <see cref="CharacterState"/>s.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/interruptions">Interruptions</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/CharacterStatePriority
    /// 
    public enum CharacterStatePriority
    {
        // Enums are ints starting at 0 by default.
        // This means you can compare them with numerical operators like < and >.

        Low,// Could specify "Low = 0," if we want to be explicit or change the order.
        Medium,// Medium = 1,
        High,// High = 2,
    }
}
