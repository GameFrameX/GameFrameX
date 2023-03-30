// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerNode"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/IAnimancerNodeDrawer
    /// 
    public interface IAnimancerNodeDrawer
    {
        /// <summary>Draws the details and controls for the target node in the Inspector.</summary>
        void DoGUI();
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerNode"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerNodeDrawer_1
    /// 
    public abstract class AnimancerNodeDrawer<T> : IAnimancerNodeDrawer where T : AnimancerNode
    {
        /************************************************************************************************************************/

        /// <summary>The node being managed.</summary>
        public T Target { get; protected set; }

        /// <summary>If true, the details of the <see cref="Target"/> will be expanded in the Inspector.</summary>
        public ref bool IsExpanded => ref Target._IsInspectorExpanded;

        /************************************************************************************************************************/

        /// <summary>The <see cref="GUIStyle"/> used for the area encompassing this drawer.</summary>
        protected abstract GUIStyle RegionStyle { get; }

        /************************************************************************************************************************/

        /// <summary>Draws the details and controls for the target <see cref="Target"/> in the Inspector.</summary>
        public virtual void DoGUI()
        {
            if (!Target.IsValid)
                return;

            BeginVerticalBox(RegionStyle);
            {
                DoHeaderGUI();
                DoDetailsGUI();
            }
            EndVerticalBox(RegionStyle);

            CheckContextMenu(GUILayoutUtility.GetLastRect());
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the name and other details of the <see cref="Target"/> in the GUI.
        /// </summary>
        protected virtual void DoHeaderGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            DoLabelGUI(area);
            DoFoldoutGUI(area);
        }

        /// <summary>
        /// Draws a field for the <see cref="AnimancerState.MainObject"/> if it has one, otherwise just a simple text
        /// label.
        /// </summary>
        protected abstract void DoLabelGUI(Rect area);

        /// <summary>Draws a foldout arrow to expand/collapse the node details.</summary>
        protected abstract void DoFoldoutGUI(Rect area);

        /// <summary>Draws the details of the <see cref="Target"/> in the GUI.</summary>
        protected abstract void DoDetailsGUI();

        /************************************************************************************************************************/

        /// <summary>
        /// Draws controls for <see cref="AnimancerState.IsPlaying"/>, <see cref="AnimancerNode.Speed"/>, and
        /// <see cref="AnimancerNode.Weight"/>.
        /// </summary>
        protected void DoNodeDetailsGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            area.xMin += EditorGUI.indentLevel * IndentSize;
            var xMin = area.xMin;

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Is Playing.
            var state = Target as AnimancerState;
            if (state != null)
            {
                var buttonArea = StealFromLeft(ref area, PlayButtonWidth, StandardSpacing);

                var content = state.IsPlaying
                    ? PauseButtonContent
                    : PlayButtonContent;

                if (CompactMiniButton(buttonArea, content))
                    state.IsPlaying = !state.IsPlaying;
            }

            SplitHorizontally(area, "Speed", "Weight",
                out var speedWidth, out var weightWidth, out var speedRect, out var weightRect);

            // Speed.
            EditorGUIUtility.labelWidth = speedWidth;
            EditorGUI.BeginChangeCheck();
            var speed = EditorGUI.FloatField(speedRect, "Speed", Target.Speed);
            if (EditorGUI.EndChangeCheck())
                Target.Speed = speed;
            if (TryUseClickEvent(speedRect, 2))
                Target.Speed = Target.Speed != 1 ? 1 : 0;

            // Weight.
            EditorGUIUtility.labelWidth = weightWidth;
            EditorGUI.BeginChangeCheck();
            var weight = EditorGUI.FloatField(weightRect, "Weight", Target.Weight);
            if (EditorGUI.EndChangeCheck())
                SetWeight(Mathf.Max(weight, 0));
            if (TryUseClickEvent(weightRect, 2))
                SetWeight(Target.Weight != 1 ? 1 : 0);

            // Not really sure why this is necessary.
            // It allows the dummy ID added when the Real Speed is hidden to work properly.
            GUIUtility.GetControlID(FocusType.Passive);

            // Real Speed (Mixer Synchronization changes the internal Playable Speed without setting the State Speed).
            speed = (float)Target._Playable.GetSpeed();
            if (Target.Speed != speed)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    area = LayoutSingleLineRect(SpacingMode.Before);
                    area.xMin = xMin;

                    var label = BeginTightLabel("Real Speed");
                    EditorGUIUtility.labelWidth = CalculateLabelWidth(label);
                    EditorGUI.FloatField(area, label, speed);
                    EndTightLabel();
                }
            }
            else// Add a dummy ID so that subsequent IDs don't change when the Real Speed appears or disappears.
            {
                GUIUtility.GetControlID(FocusType.Passive);
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;

            DoFadeDetailsGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether changing the <see cref="AnimancerNode.Weight"/> should normalize its siblings.</summary>
        protected virtual bool AutoNormalizeSiblingWeights => false;

        private void SetWeight(float weight)
        {
            if (weight < 0 ||
                weight > 1 ||
                Mathf.Approximately(Target.Weight, 1) ||
                !AutoNormalizeSiblingWeights)
                goto JustSetWeight;

            var parent = Target.Parent;
            if (parent == null)
                goto JustSetWeight;

            var totalWeight = 0f;
            var siblingCount = parent.ChildCount;
            for (int i = 0; i < siblingCount; i++)
            {
                var sibling = parent.GetChild(i);
                if (sibling.IsValid())
                    totalWeight += sibling.Weight;
            }

            // If the weights weren't previously normalized, don't normalize them now.
            if (!Mathf.Approximately(totalWeight, 1))
                goto JustSetWeight;

            var siblingWeightMultiplier = (totalWeight - weight) / (totalWeight - Target.Weight);

            for (int i = 0; i < siblingCount; i++)
            {
                var sibling = parent.GetChild(i);
                if (sibling != Target && sibling.IsValid())
                    sibling.Weight *= siblingWeightMultiplier;
            }

            JustSetWeight:
            Target.Weight = weight;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="AnimancerNode.FadeSpeed"/> and <see cref="AnimancerNode.TargetWeight"/>.</summary>
        private void DoFadeDetailsGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);

            var speedLabel = GetNarrowText("Fade Speed");
            var targetLabel = GetNarrowText("Target Weight");

            SplitHorizontally(area, speedLabel, targetLabel,
                out var speedWidth, out var weightWidth, out var speedRect, out var weightRect);

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            // Fade Speed.
            EditorGUIUtility.labelWidth = speedWidth;
            Target.FadeSpeed = EditorGUI.DelayedFloatField(speedRect, speedLabel, Target.FadeSpeed);
            if (TryUseClickEvent(speedRect, 2))
            {
                Target.FadeSpeed = Target.FadeSpeed != 0 || AnimancerPlayable.DefaultFadeDuration == 0 ?
                    0 :
                    Math.Abs(Target.Weight - Target.TargetWeight) / AnimancerPlayable.DefaultFadeDuration;
            }

            // Target Weight.
            EditorGUIUtility.labelWidth = weightWidth;
            Target.TargetWeight = Mathf.Max(0, EditorGUI.FloatField(weightRect, targetLabel, Target.TargetWeight));
            if (TryUseClickEvent(weightRect, 2))
            {
                if (Target.TargetWeight != Target.Weight)
                    Target.TargetWeight = Target.Weight;
                else if (Target.TargetWeight != 1)
                    Target.TargetWeight = 1;
                else
                    Target.TargetWeight = 0;
            }

            if (EditorGUI.EndChangeCheck() && Target.FadeSpeed != 0)
                Target.StartFade(Target.TargetWeight, 1 / Target.FadeSpeed);

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <summary>
        /// The menu label prefix used for details about the <see cref="Target"/>.
        /// </summary>
        protected const string DetailsPrefix = "Details/";

        /// <summary>
        /// Checks if the current event is a context menu click within the `clickArea` and opens a context menu with various
        /// functions for the <see cref="Target"/>.
        /// </summary>
        protected void CheckContextMenu(Rect clickArea)
        {
            if (!TryUseClickEvent(clickArea, 1))
                return;

            var menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent(Target.ToString()));

            PopulateContextMenu(menu);

            menu.AddItem(new GUIContent(DetailsPrefix + "Log Details"), false,
                () => Debug.Log(Target.GetDescription(), Target.Root?.Component as Object));

            menu.AddItem(new GUIContent(DetailsPrefix + "Log Details Of Everything"), false,
                () => Debug.Log(Target.Root.GetDescription(), Target.Root?.Component as Object));
            AnimancerPlayableDrawer.AddPlayableGraphVisualizerFunction(menu, DetailsPrefix, Target.Root._Graph);

            menu.ShowAsContext();
        }

        /// <summary>Adds functions relevant to the <see cref="Target"/>.</summary>
        protected abstract void PopulateContextMenu(GenericMenu menu);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

