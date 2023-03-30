// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine.Playables;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerState
    partial class AnimancerState
    {
        /// <summary>
        /// Uses <see cref="AnimancerPlayable.RequirePostUpdate"/> to pause a <see cref="AnimancerNode._Playable"/>.
        /// </summary>
        public class DelayedPause : Key, IUpdatable
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerNode.Root"/>.</summary>
            public AnimancerPlayable Root { get; set; }

            /// <summary>The state that will be paused.</summary>
            public AnimancerState State { get; set; }

            /************************************************************************************************************************/

            /// <summary>
            /// Gets a <see cref="DelayedPause"/> from the <see cref="ObjectPool"/> and initializes it for the `state`.
            /// </summary>
            public static void Register(AnimancerState state)
            {
                var root = state.Root;
                if (root == null)
                    return;

                var pause = ObjectPool.Acquire<DelayedPause>();
                pause.Root = root;
                pause.State = state;

                root.RequirePostUpdate(pause);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Pauses the <see cref="State"/> if <see cref="IsPlaying"/> hasn't been set to true and returns this
            /// object to the <see cref="ObjectPool"/>.
            /// </summary>
            public void Update()
            {
                if (!State.IsPlaying)
                    State._Playable.Pause();

                Root.CancelPostUpdate(this);

                Root = null;
                State = null;

                ObjectPool.Release(this);
            }

            /************************************************************************************************************************/
        }
    }
}

