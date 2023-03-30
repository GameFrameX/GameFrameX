// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

namespace Animancer
{
    /// <summary>An object which has an <see cref="AnimancerEvent.Sequence.Serializable"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IHasEvents
    /// 
    public interface IHasEvents
    {
        /************************************************************************************************************************/

        /// <summary>Events which will be triggered as the animation plays.</summary>
        AnimancerEvent.Sequence Events { get; }

        /// <summary>Events which will be triggered as the animation plays.</summary>
        ref AnimancerEvent.Sequence.Serializable SerializedEvents { get; }

        /************************************************************************************************************************/
    }

    /// <summary>A combination of <see cref="ITransition"/> and <see cref="IHasEvents"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ITransitionWithEvents
    /// 
    public interface ITransitionWithEvents : ITransition, IHasEvents { }
}

