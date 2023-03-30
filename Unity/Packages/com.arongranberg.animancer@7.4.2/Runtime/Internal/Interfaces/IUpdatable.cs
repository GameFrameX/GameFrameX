// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only] An object that can be updated during Animancer's animation updates.</summary>
    ///
    /// <example>
    /// Register to receive updates using <see cref="AnimancerPlayable.RequirePreUpdate"/> or
    /// <see cref="AnimancerPlayable.RequirePostUpdate"/> and stop
    /// receiving updates using <see cref="AnimancerPlayable.CancelPreUpdate"/> or
    /// <see cref="AnimancerPlayable.CancelPostUpdate"/>.
    /// <para></para><code>
    /// public sealed class MyUpdatable : Key, IUpdatable
    /// {
    ///     private AnimancerComponent _Animancer;
    ///
    ///     public void StartUpdating(AnimancerComponent animancer)
    ///     {
    ///         _Animancer = animancer;
    ///         
    ///         // If you want Update to be called before the playables get updated.
    ///         _Animancer.Playable.RequirePreUpdate(this);
    ///         
    ///         // If you want Update to be called after the playables get updated.
    ///         _Animancer.Playable.RequirePostUpdate(this);
    ///     }
    ///
    ///     public void StopUpdating()
    ///     {
    ///         // If you used RequirePreUpdate.
    ///         _Animancer.Playable.CancelPreUpdate(this);
    ///         
    ///         // If you used RequirePostUpdate.
    ///         _Animancer.Playable.CancelPostUpdate(this);
    ///     }
    ///
    ///     void IUpdatable.Update()
    ///     {
    ///         // Called during every animation update.
    ///         
    ///         // AnimancerPlayable.Current can be used to access the system it is being updated by.
    ///     }
    /// }
    /// </code></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/IUpdatable
    /// 
    public interface IUpdatable : Key.IListItem
    {
        /************************************************************************************************************************/

        /// <summary>Called during every <see cref="Animator"/> update.</summary>
        /// <remarks>The <see cref="Animator.updateMode"/> determines the update rate.</remarks>
        void Update();

        /************************************************************************************************************************/
    }
}

