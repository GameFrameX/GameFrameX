// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>A system for synchronizing the <see cref="AnimancerState.NormalizedTime"/> of certain animations.</summary>
    /// <example>
    /// <list type="number">
    /// <item>Store a <see cref="TimeSynchronizer{T}"/> in a field.</item>
    /// <item>Call any of the <see cref="StoreTime(AnimancerState)"/> methods before playing a new animation.</item>
    /// <item>Then call any of the <see cref="SyncTime(AnimancerState, T, float)"/> methods after playing the animation.</item>
    /// </list>
    /// Example: <see href="https://kybernetik.com.au/animancer/docs/examples/directional-sprites/character#synchronization">Character Controller -> Synchronization</see>
    /// <code>
    /// public enum AnimationType
    /// {
    ///     None,
    ///     Movement,
    /// }
    /// 
    /// [SerializeField] private AnimancerComponent _Animancer;
    /// 
    /// private readonly TimeSynchronizer&lt;AnimationType&gt;
    ///     TimeSynchronizer = new TimeSynchronizer&lt;AnimationType&gt;();
    /// 
    /// public AnimancerState Play(AnimationClip clip, AnimationType type)
    /// {
    ///     TimeSynchronizer.StoreTime(_Animancer);
    ///     var state = _Animancer.Play(clip);
    ///     TimeSynchronizer.SyncTime(state, type);
    ///     return state;
    /// }
    /// </code>
    /// </example>
    /// https://kybernetik.com.au/animancer/api/Animancer/TimeSynchronizer_1
    /// 
    public class TimeSynchronizer<T>
    {
        /************************************************************************************************************************/

        /// <summary>The group that the current animation is in.</summary>
        public T CurrentGroup { get; set; }

        /// <summary>Should synchronization be applied when the <see cref="CurrentGroup"/> is at its default value?</summary>
        /// <remarks>This is false by default so that the default group represents "ungrouped".</remarks>
        public bool SynchronizeDefaultGroup { get; set; }

        /// <summary>The stored <see cref="AnimancerState.NormalizedTimeD"/>.</summary>
        public double NormalizedTime { get; set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TimeSynchronizer{T}"/>.</summary>
        public TimeSynchronizer()
        { }

        /// <summary>Creates a new <see cref="TimeSynchronizer{T}"/>.</summary>
        public TimeSynchronizer(T group, bool synchronizeDefaultGroup = false)
        {
            CurrentGroup = group;
            SynchronizeDefaultGroup = synchronizeDefaultGroup;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the <see cref="AnimancerState.NormalizedTimeD"/> of the <see cref="AnimancerLayer.CurrentState"/>.
        /// </summary>
        public void StoreTime(AnimancerLayer layer)
            => StoreTime(layer.CurrentState);

        /// <summary>Stores the <see cref="AnimancerState.NormalizedTimeD"/> of the `state`.</summary>
        public void StoreTime(AnimancerState state)
            => NormalizedTime = state != null ? state.NormalizedTimeD : 0;

        /************************************************************************************************************************/

        /// <summary>
        /// Applies the <see cref="NormalizedTime"/> to the <see cref="AnimancerLayer.CurrentState"/> if the `group`
        /// matches the <see cref="CurrentGroup"/>.
        /// </summary>
        public bool SyncTime(AnimancerLayer layer, T group)
            => SyncTime(layer.CurrentState, group, Time.deltaTime);

        /// <summary>
        /// Applies the <see cref="NormalizedTime"/> to the <see cref="AnimancerLayer.CurrentState"/> if the `group`
        /// matches the <see cref="CurrentGroup"/>.
        /// </summary>
        public bool SyncTime(AnimancerLayer layer, T group, float deltaTime)
            => SyncTime(layer.CurrentState, group, deltaTime);

        /// <summary>
        /// Applies the <see cref="NormalizedTime"/> to the `state` if the `group` matches the
        /// <see cref="CurrentGroup"/>.
        /// </summary>
        public bool SyncTime(AnimancerState state, T group)
            => SyncTime(state, group, Time.deltaTime);

        /// <summary>
        /// Applies the <see cref="NormalizedTime"/> to the `state` if the `group` matches the
        /// <see cref="CurrentGroup"/>.
        /// </summary>
        public bool SyncTime(AnimancerState state, T group, float deltaTime)
        {
            if (state == null ||
                !EqualityComparer<T>.Default.Equals(CurrentGroup, group) ||
                (!SynchronizeDefaultGroup && EqualityComparer<T>.Default.Equals(default, group)))
            {
                CurrentGroup = group;
                return false;
            }

            // Setting the Time forces it to stay at that value after the next animation update.
            // But we actually want it to keep playing, so we need to add deltaTime manually.
            state.TimeD = NormalizedTime * state.Length + deltaTime * state.EffectiveSpeed;
            return true;
        }

        /************************************************************************************************************************/
    }
}
