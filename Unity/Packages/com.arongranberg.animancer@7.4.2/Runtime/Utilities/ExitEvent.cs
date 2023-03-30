// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>[Pro-Only] A callback for when an <see cref="AnimancerNode.EffectiveWeight"/> becomes 0.</summary>
    /// 
    /// <remarks>
    /// Most <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machine</see> systems
    /// already have their own mechanism for notifying your code when a state is exited so this system is generally
    /// only useful when something like that is not already available.
    /// </remarks>
    /// 
    /// <example><code>
    /// [SerializeField] private AnimancerComponent _Animancer;
    /// [SerializeField] private AnimationClip _Clip;
    /// 
    /// private void Awake()
    /// {
    ///     // Play the animation.
    ///     var state = _Animancer.Play(_Clip);
    ///     
    ///     // Then give its state an exit event.
    ///     ExitEvent.Register(state, () => Debug.Log("State Exited"));
    ///     
    ///     // That event will never actually get triggered because we are never playing anything else.
    /// }
    /// </code>
    /// Unlike Animancer Events, an <see cref="ExitEvent"/> will not be cleared automatically when you play something
    /// (because that's the whole point) so if you are playing the same animation repeatedly you will need to check its
    /// <see cref="AnimancerNode.EffectiveWeight"/> before registering the event (otherwise all the callbacks you register will
    /// stay active).
    /// <para></para><code>
    /// private void Update()
    /// {
    ///     // Only register the exit event if the state was at 0 weight before.
    ///     var state = _Animancer.GetOrCreate(_Clip);
    ///     if (state.EffectiveWeight == 0)
    ///         ExitEvent.Register(state, () => Debug.Log("State Exited"));
    ///         
    ///     // Then play the state normally.
    ///     _Animancer.Play(state);
    ///     // _Animancer.Play(_Clip); would work too, but we already have the state so using it directly is faster.
    /// }
    /// </code></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/ExitEvent
    /// 
    public class ExitEvent : Key, IUpdatable
    {
        /************************************************************************************************************************/

        private Action _Callback;
        private AnimancerNode _Node;

        /************************************************************************************************************************/

        /// <summary>
        /// Registers the `callback` to be triggered when the <see cref="AnimancerNode.EffectiveWeight"/> becomes 0.
        /// </summary>
        /// <remarks>
        /// The <see cref="AnimancerNode.EffectiveWeight"/> is only checked at the end of the animation update so if it
        /// is set to 0 then back to a higher number before the next update, the callback will not be triggered.
        /// </remarks>
        public static void Register(AnimancerNode node, Action callback)
        {
#if UNITY_ASSERTIONS
            AnimancerUtilities.Assert(node != null, "Node is null.");
            AnimancerUtilities.Assert(node.IsValid, "Node is not valid.");
#endif

            var exit = ObjectPool.Acquire<ExitEvent>();
            exit._Callback = callback;
            exit._Node = node;
            node.Root.RequirePostUpdate(exit);
        }

        /************************************************************************************************************************/

        /// <summary>Removes a registered <see cref="ExitEvent"/> and returns true if there was one.</summary>
        public static bool Unregister(AnimancerPlayable animancer)
        {
            for (int i = animancer.PostUpdatableCount - 1; i >= 0; i--)
            {
                if (animancer.GetPostUpdatable(i) is ExitEvent exit)
                {
                    animancer.CancelPostUpdate(exit);
                    exit.Release();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a registered <see cref="ExitEvent"/> targeting the specified `node` and returns true if there was
        /// one.
        /// </summary>
        public static bool Unregister(AnimancerNode node)
        {
            var animancer = node.Root;
            for (int i = animancer.PostUpdatableCount - 1; i >= 0; i--)
            {
                if (animancer.GetPostUpdatable(i) is ExitEvent exit &&
                    exit._Node == node)
                {
                    animancer.CancelPostUpdate(exit);
                    exit.Release();
                    return true;
                }
            }

            return false;
        }

        /************************************************************************************************************************/

        void IUpdatable.Update()
        {
            if (_Node.IsValid() && _Node.EffectiveWeight > 0)
                return;

            _Callback();
            _Node.Root.CancelPostUpdate(this);
            Release();
        }

        /************************************************************************************************************************/

        private void Release()
        {
            _Callback = null;
            _Node = null;
            ObjectPool.Release(this);
        }

        /************************************************************************************************************************/
    }
}
