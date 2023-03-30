// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    /// This script is basically the same as <see cref="PlayAnimationOnClick"/>, except that it uses
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/basics/transitions">Transitions</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Basics/PlayTransitionOnClick
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Basics - Play Transition On Click")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Basics) + "/" + nameof(PlayTransitionOnClick))]
    public sealed class PlayTransitionOnClick : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipTransition _Idle;
        [SerializeField] private ClipTransition _Action;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            // Transitions store their events so we only initialize them once on startup
            // instead of setting the event every time the animation is played.
            _Action.Events.OnEnd = OnActionEnd;

            // The Fade Duration of this transition will be ignored because nothing else is playing yet so there is
            // nothing to fade from.
            _Animancer.Play(_Idle);
        }

        /************************************************************************************************************************/

        private void OnActionEnd()
        {
            _Animancer.Play(_Idle);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            if (ExampleInput.LeftMouseUp)
            {
                _Animancer.Play(_Action);

                // If you want to cross fade without using Transitions or override the fade duration of a Transition
                // then you can simply use the second parameter in the Play method.
                // _Animancer.Play(_Action, 0.25f);

                // When cross fading, setting the state.Time like the PlayAnimationOnClick script would prevent it from
                // smoothly blending so if you want to restart the animation you can use FadeMode.FromStart.
                // _Animancer.Play(_Action, 0.25f, FadeMode.FromStart);

                // But if you use transitions then you don't need to specify each of those because the Fade Duration is
                // set in the Inspector and it automatically picks the FadeMode based on whether the Start Time check
                // box is enabled or not.
            }
        }

        /************************************************************************************************************************/
    }
}
