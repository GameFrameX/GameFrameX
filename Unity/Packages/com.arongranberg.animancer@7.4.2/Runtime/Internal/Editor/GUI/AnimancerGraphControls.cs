// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using Animancer.Units;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws manual controls for the <see cref="AnimancerPlayable.Graph"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGraphControls
    /// 
    public static class AnimancerGraphControls
    {
        /************************************************************************************************************************/

        /// <summary>Draws manual controls for the <see cref="AnimancerPlayable.Graph"/>.</summary>
        public static void DoGraphGUI(AnimancerPlayable playable, out Rect area)
        {
            BeginVerticalBox(GUI.skin.box);

            DoRootGUI(playable);

            DoAddAnimationGUI(playable);

            EndVerticalBox(GUI.skin.box);

            area = GUILayoutUtility.GetLastRect();
        }

        /************************************************************************************************************************/

        private static void DoRootGUI(AnimancerPlayable playable)
        {
            var labelWidth = EditorGUIUtility.labelWidth;

            using (ObjectPool.Disposable.AcquireContent(out var speedLabel, "Speed"))
            {
                var isPlayingContent = playable.IsGraphPlaying
                    ? PauseButtonContent
                    : PlayButtonContent;

                var speedWidth = CalculateLabelWidth(speedLabel.text);

                var area = LayoutSingleLineRect();

                // Playing.

                var isPlayingArea = area;
                isPlayingArea.x += StandardSpacing;
                isPlayingArea.width = PlayButtonWidth;

                if (GUI.Button(isPlayingArea, isPlayingContent, EditorStyles.miniButton))
                    playable.IsGraphPlaying = !playable.IsGraphPlaying;

                // Frame Step.

                if (playable.IsGraphPlaying)
                {
                    GUIUtility.GetControlID(FocusType.Passive);
                }
                else
                {
                    isPlayingArea.x += isPlayingArea.width + StandardSpacing;

                    if (GUI.Button(isPlayingArea, StepForwardButtonContent, EditorStyles.miniButton))
                        playable.Evaluate(AnimancerSettings.FrameStep);
                }

                // Speed.

                var speedArea = area;
                speedArea.xMin = isPlayingArea.xMax + StandardSpacing;

                EditorGUIUtility.labelWidth = speedWidth;
                EditorGUI.BeginChangeCheck();
                var unitConverter = AnimationSpeedAttribute.DisplayConverters[0];
                var speed = UnitsAttribute.DoSpecialFloatField(speedArea, speedLabel, playable.Speed, unitConverter);
                if (EditorGUI.EndChangeCheck())
                    playable.Speed = speed;
                if (TryUseClickEvent(speedArea, 2))
                    playable.Speed = playable.Speed != 1 ? 1 : 0;
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/
        #region Add Animation
        /************************************************************************************************************************/

        /// <summary>Are the Add Animation controls active?</summary>
        private static bool _ShowAddAnimation;

        /************************************************************************************************************************/

        /// <summary>Adds a function to show or hide the "Add Animation" field.</summary>
        public static void AddAddAnimationFunction(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Animation"),
                _ShowAddAnimation,
                () => _ShowAddAnimation = !_ShowAddAnimation);
        }

        /************************************************************************************************************************/

        private static void DoAddAnimationGUI(AnimancerPlayable playable)
        {
            if (!_ShowAddAnimation)
                return;

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var source = EditorGUILayout.ObjectField("Add Animation", null, typeof(Object), false);

            EditorGUI.indentLevel = indentLevel;

            if (source == null)
                return;

            using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var set))
            {
                set.GatherFromSource(source);
                foreach (var clip in set)
                    playable.Layers[0].GetOrCreateState(clip);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

