// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/LinearMixerTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Mixer Transition/Linear", order = Strings.AssetMenuOrder + 3)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(LinearMixerTransitionAsset))]
    public class LinearMixerTransitionAsset : AnimancerTransitionAsset<LinearMixerTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<LinearMixerTransitionAsset, LinearMixerTransition, LinearMixerState>,
            LinearMixerState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/LinearMixerTransition
    [Serializable]
    public class LinearMixerTransition : MixerTransition<LinearMixerState, float>,
        LinearMixerState.ITransition, ICopyable<LinearMixerTransition>
    {
        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Should setting the Parameter above the highest threshold increase the Speed of the mixer proportionally?")]
        private bool _ExtrapolateSpeed = true;

        /// <summary>[<see cref="SerializeField"/>]
        /// Should setting the <see cref="MixerState{TParameter}.Parameter"/> above the highest threshold increase the
        /// <see cref="AnimancerNode.Speed"/> of the mixer proportionally?
        /// </summary>
        public ref bool ExtrapolateSpeed => ref _ExtrapolateSpeed;

        /************************************************************************************************************************/

        /// <summary>
        /// Are all <see cref="ManualMixerTransition{TMixer}.Animations"/> assigned and
        /// <see cref="MixerTransition{TMixer, TParameter}.Thresholds"/> unique and sorted in ascending order?
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (!base.IsValid)
                    return false;

                var previous = float.NegativeInfinity;

                var thresholds = Thresholds;
                for (int i = 0; i < thresholds.Length; i++)
                {
                    var threshold = thresholds[i];
                    if (threshold < previous)
                        return false;
                    else
                        previous = threshold;
                }

                return true;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override LinearMixerState CreateState()
        {
            State = new LinearMixerState();
            InitializeState();
            return State;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Apply(AnimancerState state)
        {
            base.Apply(state);
            State.ExtrapolateSpeed = _ExtrapolateSpeed;
        }

        /************************************************************************************************************************/

        /// <summary>Sorts all states so that their thresholds go from lowest to highest.</summary>
        /// <remarks>This method uses Bubble Sort which is inefficient for large numbers of states.</remarks>
        public void SortByThresholds()
        {
            var thresholdCount = Thresholds.Length;
            if (thresholdCount <= 1)
                return;

            var speedCount = Speeds.Length;
            var syncCount = SynchronizeChildren.Length;

            var previousThreshold = Thresholds[0];

            for (int i = 1; i < thresholdCount; i++)
            {
                var threshold = Thresholds[i];
                if (threshold >= previousThreshold)
                {
                    previousThreshold = threshold;
                    continue;
                }

                Thresholds.Swap(i, i - 1);
                Animations.Swap(i, i - 1);

                if (i < speedCount)
                    Speeds.Swap(i, i - 1);

                if (i == syncCount && !SynchronizeChildren[i - 1])
                {
                    var sync = SynchronizeChildren;
                    Array.Resize(ref sync, ++syncCount);
                    sync[i - 1] = true;
                    sync[i] = false;
                    SynchronizeChildren = sync;
                }
                else if (i < syncCount)
                {
                    SynchronizeChildren.Swap(i, i - 1);
                }

                if (i == 1)
                {
                    i = 0;
                    previousThreshold = float.NegativeInfinity;
                }
                else
                {
                    i -= 2;
                    previousThreshold = Thresholds[i];
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(LinearMixerTransition copyFrom)
        {
            CopyFrom((MixerTransition<LinearMixerState, float>)copyFrom);

            if (copyFrom == null)
            {
                _ExtrapolateSpeed = true;
                return;
            }

            _ExtrapolateSpeed = copyFrom._ExtrapolateSpeed;
        }

        /************************************************************************************************************************/
        #region Drawer
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [UnityEditor.CustomPropertyDrawer(typeof(LinearMixerTransition), true)]
        public class Drawer : MixerTransitionDrawer
        {
            /************************************************************************************************************************/

            private static GUIContent _SortingErrorContent;
            private static GUIStyle _SortingErrorStyle;

            /// <inheritdoc/>
            protected override void DoThresholdGUI(Rect area, int index)
            {
                var color = GUI.color;

                if (index > 0)
                {
                    var previousThreshold = CurrentThresholds.GetArrayElementAtIndex(index - 1);
                    var currentThreshold = CurrentThresholds.GetArrayElementAtIndex(index);
                    if (previousThreshold.floatValue >= currentThreshold.floatValue)
                    {
                        if (_SortingErrorContent == null)
                            _SortingErrorContent = new GUIContent(Editor.AnimancerGUI.LoadIcon("console.erroricon.sml"))
                            {
                                tooltip = "Linear Mixer Thresholds must always be unique and sorted in ascending order (click to sort)"
                            };

                        if (_SortingErrorStyle == null)
                            _SortingErrorStyle = new GUIStyle(GUI.skin.label)
                            {
                                padding = new RectOffset(),
                            };

                        var iconArea = Editor.AnimancerGUI.StealFromRight(ref area, area.height, Editor.AnimancerGUI.StandardSpacing);
                        if (GUI.Button(iconArea, _SortingErrorContent, _SortingErrorStyle))
                        {
                            Editor.Serialization.RecordUndo(Context.Property);
                            ((LinearMixerTransition)Context.Transition).SortByThresholds();
                        }

                        GUI.color = Editor.AnimancerGUI.ErrorFieldColor;
                    }
                }

                base.DoThresholdGUI(area, index);

                GUI.color = color;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddThresholdFunctionsToMenu(UnityEditor.GenericMenu menu)
            {
                const string EvenlySpaced = "Evenly Spaced";

                var count = CurrentThresholds.arraySize;
                if (count <= 1)
                {
                    menu.AddDisabledItem(new GUIContent(EvenlySpaced));
                }
                else
                {
                    var first = CurrentThresholds.GetArrayElementAtIndex(0).floatValue;
                    var last = CurrentThresholds.GetArrayElementAtIndex(count - 1).floatValue;

                    if (last == first)
                        last++;

                    AddPropertyModifierFunction(menu, $"{EvenlySpaced} ({first} to {last})", (_) =>
                    {
                        for (int i = 0; i < count; i++)
                        {
                            CurrentThresholds.GetArrayElementAtIndex(i).floatValue = Mathf.Lerp(first, last, i / (float)(count - 1));
                        }
                    });
                }

                AddCalculateThresholdsFunction(menu, "From Speed",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.magnitude : float.NaN);
                AddCalculateThresholdsFunction(menu, "From Velocity X",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.x : float.NaN);
                AddCalculateThresholdsFunction(menu, "From Velocity Y",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.y : float.NaN);
                AddCalculateThresholdsFunction(menu, "From Velocity Z",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.z : float.NaN);
                AddCalculateThresholdsFunction(menu, "From Angular Speed (Rad)",
                    (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed : float.NaN);
                AddCalculateThresholdsFunction(menu, "From Angular Speed (Deg)",
                    (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed * Mathf.Rad2Deg : float.NaN);
            }

            /************************************************************************************************************************/

            private void AddCalculateThresholdsFunction(UnityEditor.GenericMenu menu, string label,
                Func<Object, float, float> calculateThreshold)
            {
                AddPropertyModifierFunction(menu, label, (property) =>
                {
                    var count = CurrentAnimations.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        var state = CurrentAnimations.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (state == null)
                            continue;

                        var threshold = CurrentThresholds.GetArrayElementAtIndex(i);
                        var value = calculateThreshold(state, threshold.floatValue);
                        if (!float.IsNaN(value))
                            threshold.floatValue = value;
                    }
                });
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}
