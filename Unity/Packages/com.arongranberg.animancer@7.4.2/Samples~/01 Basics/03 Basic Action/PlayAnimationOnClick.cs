// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    /// Starts with an idle animation and performs an action when the user clicks the mouse, then returns to idle.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/basics/action">Basic Action</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.Basics/PlayAnimationOnClick
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Basics - Play Animation On Click")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(Basics) + "/" + nameof(PlayAnimationOnClick))]
    public sealed class PlayAnimationOnClick : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Idle;
        [SerializeField] private AnimationClip _Action;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Idle);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            if (ExampleInput.LeftMouseUp)
            {
                // Play the action animation and grab the internal state which controls it.
                AnimancerState state = _Animancer.Play(_Action);

                // Go back to the beginning of the animation.
                // Otherwise if the animation was already playing, it would continue from there.
                state.Time = 0;

                // When the animation reaches its end, call the OnEnable method to go back to idle.
                // The Events examples explain this feature in more detail.
                state.Events.OnEnd = OnEnable;
            }
        }

        /************************************************************************************************************************/
    }
}
