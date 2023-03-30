// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Examples.Events
{
    /// <summary>Various utility delegates which can be assigned to Animancer Events.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/events/utilities">Event Utilities</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Events/EventUtilities
    /// 
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Events) + "/" + nameof(EventUtilities))]
    public static class EventUtilities
    {
        /************************************************************************************************************************/
        // Since the methods in this class are intended for use with Animancer Events, they are declared as delegate fields
        // rather than regular methods so that assigning them doesn't create any garbage.
        /************************************************************************************************************************/
        // If you intend to use any of these methods in your own scripts, it is recommended that you copy the ones you need into
        // your own utility class so that you can delete the Animancer Examples once you are done with them.
        /************************************************************************************************************************/

        /// <summary>
        /// Logs a message with the details of the <see cref="AnimancerEvent.CurrentEvent"/> and
        /// <see cref="AnimancerEvent.CurrentState"/>.
        /// </summary>
        /// <example>
        /// Go through every event in a transition and make it Log the event in addition to its normal callback:
        /// <para></para><code>
        /// [SerializeField] private ClipTransition _Transition;
        /// 
        /// private void Awake()
        /// {
        ///     for (int i = 0; i &lt; _Transition.Events.Count; i++)
        ///     {
        ///         _Transition.Events.AddCallback(i, EventUtilities.LogCurrentEvent);
        ///     }
        /// }
        /// </code></example>
        public static readonly Action LogCurrentEvent = () =>
        {
            Debug.Log(
                $"An {nameof(AnimancerEvent)} was triggered:" +
                $"\n- Event: {AnimancerEvent.CurrentEvent}" +
                $"\n- State: {AnimancerEvent.CurrentState.GetDescription()}",
                AnimancerEvent.CurrentState.Root?.Component as Object);
        };

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="AnimancerState.Time"/> of the <see cref="AnimancerEvent.CurrentState"/> to 0.</summary>
        /// <example>
        /// Play a non-looping animation but force it to loop:
        /// <para></para><code>
        /// [SerializeField] private AnimancerComponent _Animancer;
        /// [SerializeField] private AnimationClip _NonLoopingClip;
        /// 
        /// private void Awake()
        /// {
        ///     var state = _Animancer.Play(_NonLoopingClip);
        ///     state.Events.OnEnd = EventUtilities.RestartCurrentState;
        /// }
        /// </code></example>
        public static readonly Action RestartCurrentState = () =>
        {
            AnimancerEvent.CurrentState.Time = 0;
        };

        /************************************************************************************************************************/

        /// <summary>
        /// Pauses the <see cref="AnimancerEvent.CurrentState"/> at the current
        /// <see cref="AnimancerEvent.normalizedTime"/>.
        /// </summary>
        /// <remarks>
        /// This can be useful for having an animation which stops at certain times to wait for something else to
        /// happen without needing to separate the animation into separate <see cref="AnimationClip"/>s.
        /// </remarks>
        public static readonly Action PauseAtCurrentEvent = () =>
        {
            AnimancerEvent.CurrentState.IsPlaying = false;
            AnimancerEvent.CurrentState.NormalizedTime = AnimancerEvent.CurrentEvent.normalizedTime;
        };

        /************************************************************************************************************************/
    }
}
