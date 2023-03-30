// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] An object that can draw custom GUI elements relating to transitions.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ITransitionGUI
    /// 
    public interface ITransitionGUI
    {
        /************************************************************************************************************************/

        /// <summary>Called while drawing the GUI for the <see cref="TransitionPreviewWindow"/> scene.</summary>
        void OnPreviewSceneGUI(TransitionPreviewDetails details);

        /// <summary>
        /// Called while drawing the background GUI for the <see cref="TimelineGUI"/> for the
        /// <see cref="IHasEvents.Events"/>.
        /// </summary>
        void OnTimelineBackgroundGUI();

        /// <summary>
        /// Called while drawing the foreground GUI for the <see cref="TimelineGUI"/> for the
        /// <see cref="IHasEvents.Events"/>.
        /// </summary>
        void OnTimelineForegroundGUI();

        /************************************************************************************************************************/
    }
}

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Details about the current preview used by <see cref="ITransitionGUI.OnPreviewSceneGUI"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionPreviewDetails
    /// 
    public readonly struct TransitionPreviewDetails
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerPlayable"/> used to play the preview.</summary>
        public readonly AnimancerPlayable Animancer;

        /// <summary>The <see cref="UnityEngine.Transform"/> of the <see cref="Animator"/> used to play the preview.</summary>
        public Transform Transform => Animancer.Component.Animator.transform;

        /************************************************************************************************************************/

        /// <summary>The <see cref="SerializedProperty"/> representing the target transition.</summary>
        public static SerializedProperty Property => TransitionDrawer.Context.Property;

        /// <summary>The current <see cref="ITransitionDetailed"/>.</summary>
        public static ITransitionDetailed Transition => TransitionDrawer.Context.Transition;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionPreviewDetails"/>.</summary>
        public TransitionPreviewDetails(AnimancerPlayable animancer)
        {
            Animancer = animancer;
        }

        /************************************************************************************************************************/
    }
}

#endif

