// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Various GUI utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGUI
    /// 
    public static class AnimancerGUI
    {
        /************************************************************************************************************************/
        #region Standard Values
        /************************************************************************************************************************/

        /// <summary>The highlight color used for fields showing a warning.</summary>
        public static readonly Color
            WarningFieldColor = new Color(1, 0.9f, 0.6f);

        /// <summary>The highlight color used for fields showing an error.</summary>
        public static readonly Color
            ErrorFieldColor = new Color(1, 0.6f, 0.6f);

        /************************************************************************************************************************/

        /// <summary><see cref="GUILayout.ExpandWidth"/> set to false.</summary>
        public static readonly GUILayoutOption[]
            DontExpandWidth = { GUILayout.ExpandWidth(false) };

        /************************************************************************************************************************/

        /// <summary>Returns <see cref="EditorGUIUtility.singleLineHeight"/>.</summary>
        public static float LineHeight => EditorGUIUtility.singleLineHeight;

        /************************************************************************************************************************/

        /// <summary>Returns <see cref="EditorGUIUtility.standardVerticalSpacing"/>.</summary>
        public static float StandardSpacing => EditorGUIUtility.standardVerticalSpacing;

        /************************************************************************************************************************/

        private static float _IndentSize = -1;

        /// <summary>
        /// The number of pixels of indentation for each <see cref="EditorGUI.indentLevel"/> increment.
        /// </summary>
        public static float IndentSize
        {
            get
            {
                if (_IndentSize < 0)
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    _IndentSize = EditorGUI.IndentedRect(new Rect()).x;
                    EditorGUI.indentLevel = indentLevel;
                }

                return _IndentSize;
            }
        }

        /************************************************************************************************************************/

        private static float _ToggleWidth = -1;

        /// <summary>The width of a standard <see cref="GUISkin.toggle"/> with no label.</summary>
        public static float ToggleWidth
        {
            get
            {
                if (_ToggleWidth == -1)
                    _ToggleWidth = GUI.skin.toggle.CalculateWidth(GUIContent.none);
                return _ToggleWidth;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The color of the standard label text.</summary>
        public static Color TextColor => GUI.skin.label.normal.textColor;

        /************************************************************************************************************************/

        private static GUIStyle _MiniButton;

        /// <summary>A more compact <see cref="EditorStyles.miniButton"/> with a fixed size as a tiny box.</summary>
        public static GUIStyle MiniButton
        {
            get
            {
                if (_MiniButton == null)
                {
                    _MiniButton = new GUIStyle(EditorStyles.miniButton)
                    {
                        margin = new RectOffset(0, 0, 2, 0),
                        padding = new RectOffset(2, 3, 2, 2),
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = LineHeight,
                        fixedWidth = LineHeight - 1
                    };
                }

                return _MiniButton;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Layout
        /************************************************************************************************************************/

        /// <summary>Calls <see cref="UnityEditorInternal.InternalEditorUtility.RepaintAllViews"/>.</summary>
        public static void RepaintEverything()
            => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        /************************************************************************************************************************/

        /// <summary>Indicates where <see cref="LayoutSingleLineRect"/> should add the <see cref="StandardSpacing"/>.</summary>
        public enum SpacingMode
        {
            /// <summary>No extra space.</summary>
            None,

            /// <summary>Add extra space before the new area.</summary>
            Before,

            /// <summary>Add extra space after the new area.</summary>
            After,

            /// <summary>Add extra space before and after the new area.</summary>
            BeforeAndAfter
        }

        /// <summary>
        /// Uses <see cref="GUILayoutUtility.GetRect(float, float)"/> to get a <see cref="Rect"/> occupying a single
        /// standard line with the <see cref="StandardSpacing"/> added according to the specified `spacing`.
        /// </summary>
        public static Rect LayoutSingleLineRect(SpacingMode spacing = SpacingMode.None)
        {
            Rect rect;
            switch (spacing)
            {
                case SpacingMode.None:
                    return GUILayoutUtility.GetRect(0, LineHeight);

                case SpacingMode.Before:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing);
                    rect.yMin += StandardSpacing;
                    return rect;

                case SpacingMode.After:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing);
                    rect.height -= StandardSpacing;
                    return rect;

                case SpacingMode.BeforeAndAfter:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing * 2);
                    rect.yMin += StandardSpacing;
                    rect.height -= StandardSpacing;
                    return rect;

                default:
                    throw new ArgumentException($"Unsupported {nameof(StandardSpacing)}: " + spacing, nameof(spacing));
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Rect.height"/> is positive, this method moves the <see cref="Rect.y"/> by that amount and
        /// adds the <see cref="EditorGUIUtility.standardVerticalSpacing"/>.
        /// </summary>
        public static void NextVerticalArea(ref Rect area)
        {
            if (area.height > 0)
                area.y += area.height + StandardSpacing;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Subtracts the `width` from the left side of the `area` and returns a new <see cref="Rect"/> occupying the
        /// removed section.
        /// </summary>
        public static Rect StealFromLeft(ref Rect area, float width, float padding = 0)
        {
            var newRect = new Rect(area.x, area.y, width, area.height);
            area.xMin += width + padding;
            return newRect;
        }

        /// <summary>
        /// Subtracts the `width` from the right side of the `area` and returns a new <see cref="Rect"/> occupying the
        /// removed section.
        /// </summary>
        public static Rect StealFromRight(ref Rect area, float width, float padding = 0)
        {
            area.width -= width + padding;
            return new Rect(area.xMax + padding, area.y, width, area.height);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Divides the given `area` such that the fields associated with both labels will have equal space
        /// remaining after the labels themselves.
        /// </summary>
        public static void SplitHorizontally(Rect area, string label0, string label1,
             out float width0, out float width1, out Rect rect0, out Rect rect1)
        {
            width0 = CalculateLabelWidth(label0);
            width1 = CalculateLabelWidth(label1);

            const float Padding = 1;

            rect0 = rect1 = area;

            var remainingWidth = area.width - width0 - width1 - Padding;
            rect0.width = width0 + remainingWidth * 0.5f;
            rect1.xMin = rect0.xMax + Padding;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Calls <see cref="GUIStyle.CalcMinMaxWidth"/> and returns the max width.</summary>
        public static float CalculateWidth(this GUIStyle style, GUIContent content)
        {
            style.CalcMinMaxWidth(content, out _, out var width);
            return Mathf.Ceil(width);
        }

        /// <summary>[Animancer Extension] Calls <see cref="GUIStyle.CalcMinMaxWidth"/> and returns the max width.</summary>
        public static float CalculateWidth(this GUIStyle style, string text)
        {
            using (ObjectPool.Disposable.AcquireContent(out var content, text, null, false))
                return style.CalculateWidth(content);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="ConversionCache{TKey, TValue}"/> for calculating the GUI width occupied by text using the
        /// specified `style`.
        /// </summary>
        public static ConversionCache<string, float> CreateWidthCache(GUIStyle style)
            => new ConversionCache<string, float>((text) => style.CalculateWidth(text));

        /************************************************************************************************************************/

        private static ConversionCache<string, float> _LabelWidthCache;

        /// <summary>
        /// Calls <see cref="GUIStyle.CalcMinMaxWidth"/> using <see cref="GUISkin.label"/> and returns the max
        /// width. The result is cached for efficient reuse.
        /// </summary>
        public static float CalculateLabelWidth(string text)
        {
            if (_LabelWidthCache == null)
                _LabelWidthCache = CreateWidthCache(GUI.skin.label);

            return _LabelWidthCache.Convert(text);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Begins a vertical layout group using the given style and decreases the
        /// <see cref="EditorGUIUtility.labelWidth"/> to compensate for the indentation.
        /// </summary>
        public static void BeginVerticalBox(GUIStyle style)
        {
            if (style == null)
            {
                GUILayout.BeginVertical();
                return;
            }

            GUILayout.BeginVertical(style);
            EditorGUIUtility.labelWidth -= style.padding.left;
        }

        /// <summary>
        /// Ends a layout group started by <see cref="BeginVerticalBox"/> and restores the
        /// <see cref="EditorGUIUtility.labelWidth"/>.
        /// </summary>
        public static void EndVerticalBox(GUIStyle style)
        {
            if (style != null)
                EditorGUIUtility.labelWidth += style.padding.left;

            GUILayout.EndVertical();
        }

        /************************************************************************************************************************/

        /// <summary>Clears the <see cref="Selection.objects"/> then returns it to its current state.</summary>
        /// <remarks>
        /// This forces the <see cref="UnityEditorInternal.ReorderableList"/> drawer to adjust to height changes which
        /// it unfortunately doesn't do on its own..
        /// </remarks>
        public static void ReSelectCurrentObjects()
        {
            var selection = Selection.objects;
            Selection.objects = Array.Empty<Object>();
            EditorApplication.delayCall += () =>
                EditorApplication.delayCall += () =>
                    Selection.objects = selection;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Labels
        /************************************************************************************************************************/

        private static GUIStyle _WeightLabelStyle;
        private static float _WeightLabelWidth = -1;

        /// <summary>
        /// Draws a label showing the `weight` aligned to the right side of the `area` and reduces its
        /// <see cref="Rect.width"/> to remove that label from its area.
        /// </summary>
        public static void DoWeightLabel(ref Rect area, float weight)
        {
            var label = WeightToShortString(weight, out var isExact);

            if (_WeightLabelStyle == null)
                _WeightLabelStyle = new GUIStyle(GUI.skin.label);

            if (_WeightLabelWidth < 0)
            {
                _WeightLabelStyle.fontStyle = FontStyle.Italic;
                _WeightLabelWidth = _WeightLabelStyle.CalculateWidth("0.0");
            }

            _WeightLabelStyle.normal.textColor = Color.Lerp(Color.grey, TextColor, weight);
            _WeightLabelStyle.fontStyle = isExact ? FontStyle.Normal : FontStyle.Italic;

            var weightArea = StealFromRight(ref area, _WeightLabelWidth);

            GUI.Label(weightArea, label, _WeightLabelStyle);
        }

        /************************************************************************************************************************/

        private static ConversionCache<float, string> _ShortWeightCache;

        /// <summary>Returns a string which approximates the `weight` into no more than 3 digits.</summary>
        private static string WeightToShortString(float weight, out bool isExact)
        {
            isExact = true;

            if (weight == 0)
                return "0.0";
            if (weight == 1)
                return "1.0";

            isExact = false;

            if (weight >= -0.5f && weight < 0.05f)
                return "~0.";
            if (weight >= 0.95f && weight < 1.05f)
                return "~1.";

            if (weight <= -99.5f)
                return "-??";
            if (weight >= 999.5f)
                return "???";

            if (_ShortWeightCache == null)
                _ShortWeightCache = new ConversionCache<float, string>((value) =>
                {
                    if (value < -9.5f) return $"{value:F0}";
                    if (value < -0.5f) return $"{value:F0}.";
                    if (value < 9.5f) return $"{value:F1}";
                    if (value < 99.5f) return $"{value:F0}.";
                    return $"{value:F0}";
                });

            var rounded = weight > 0 ? Mathf.Floor(weight * 10) : Mathf.Ceil(weight * 10);
            isExact = Mathf.Approximately(weight * 10, rounded);

            return _ShortWeightCache.Convert(weight);
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="EditorGUIUtility.labelWidth"/> from before <see cref="BeginTightLabel"/>.</summary>
        private static float _TightLabelWidth;

        /// <summary>Stores the <see cref="EditorGUIUtility.labelWidth"/> and changes it to the exact width of the `label`.</summary>
        public static string BeginTightLabel(string label)
        {
            _TightLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = CalculateLabelWidth(label) + EditorGUI.indentLevel * IndentSize;
            return GetNarrowText(label);
        }

        /// <summary>Reverts <see cref="EditorGUIUtility.labelWidth"/> to its previous value.</summary>
        public static void EndTightLabel()
        {
            EditorGUIUtility.labelWidth = _TightLabelWidth;
        }

        /************************************************************************************************************************/

        private static ConversionCache<string, string> _NarrowTextCache;

        /// <summary>
        /// Returns the `text` without any spaces if <see cref="EditorGUIUtility.wideMode"/> is false.
        /// Otherwise simply returns the `text` without any changes.
        /// </summary>
        public static string GetNarrowText(string text)
        {
            if (EditorGUIUtility.wideMode ||
                string.IsNullOrEmpty(text))
                return text;

            if (_NarrowTextCache == null)
                _NarrowTextCache = new ConversionCache<string, string>((str) => str.Replace(" ", ""));

            return _NarrowTextCache.Convert(text);
        }

        /************************************************************************************************************************/

        /// <summary>Loads an icon texture and sets it to use <see cref="FilterMode.Bilinear"/>.</summary>
        public static Texture LoadIcon(string name)
        {
            var icon = (Texture)EditorGUIUtility.Load(name);
            if (icon != null)
                icon.filterMode = FilterMode.Bilinear;
            return icon;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorGUIUtility.IconContent(string)"/> if the `content` was null.</summary>
        public static GUIContent IconContent(ref GUIContent content, string name)
        {
            if (content == null)
                content = EditorGUIUtility.IconContent(name);
            return content;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a button using <see cref="EditorStyles.miniButton"/> and <see cref="DontExpandWidth"/>.</summary>
        public static bool CompactMiniButton(GUIContent content)
            => GUILayout.Button(content, EditorStyles.miniButton, DontExpandWidth);

        /// <summary>Draws a button using <see cref="EditorStyles.miniButton"/>.</summary>
        public static bool CompactMiniButton(Rect area, GUIContent content)
            => GUI.Button(area, content, EditorStyles.miniButton);

        /************************************************************************************************************************/

        private static GUIContent
            _PlayButtonContent,
            _PauseButtonContent,
            _StepBackwardButtonContent,
            _StepForwardButtonContent;

        /// <summary><see cref="IconContent(ref GUIContent, string)"/> for a play button.</summary>
        public static GUIContent PlayButtonContent
            => IconContent(ref _PlayButtonContent, "PlayButton");

        /// <summary><see cref="IconContent(ref GUIContent, string)"/> for a pause button.</summary>
        public static GUIContent PauseButtonContent
            => IconContent(ref _PauseButtonContent, "PauseButton");

        /// <summary><see cref="IconContent(ref GUIContent, string)"/> for a step backward button.</summary>
        public static GUIContent StepBackwardButtonContent
            => IconContent(ref _StepBackwardButtonContent, "Animation.PrevKey");

        /// <summary><see cref="IconContent(ref GUIContent, string)"/> for a step forward button.</summary>
        public static GUIContent StepForwardButtonContent
            => IconContent(ref _StepForwardButtonContent, "Animation.NextKey");

        /************************************************************************************************************************/

        private static float _PlayButtonWidth;

        /// <summary>The default width of <see cref="PlayButtonContent"/> using <see cref="EditorStyles.miniButton"/>.</summary>
        public static float PlayButtonWidth
        {
            get
            {
                if (_PlayButtonWidth <= 0)
                    EditorStyles.miniButton.CalcMinMaxWidth(PlayButtonContent, out _PlayButtonWidth, out _);
                return _PlayButtonWidth;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Events
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true and uses the current event if it is <see cref="EventType.MouseUp"/> inside the specified
        /// `area`.
        /// </summary>
        public static bool TryUseClickEvent(Rect area, int button = -1)
        {
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseUp ||
                (button >= 0 && currentEvent.button != button) ||
                !area.Contains(currentEvent.mousePosition))
                return false;

            GUI.changed = true;
            currentEvent.Use();

            if (currentEvent.button == 2)
                Deselect();

            return true;
        }

        /// <summary>
        /// Returns true and uses the current event if it is <see cref="EventType.MouseUp"/> inside the last GUI Layout
        /// <see cref="Rect"/> that was drawn.
        /// </summary>
        public static bool TryUseClickEventInLastRect(int button = -1)
            => TryUseClickEvent(GUILayoutUtility.GetLastRect(), button);

        /************************************************************************************************************************/

        /// <summary>
        /// Invokes `onDrop` if the <see cref="Event.current"/> is a drag and drop event inside the `dropArea`.
        /// </summary>
        public static void HandleDragAndDrop<T>(Rect dropArea, Func<T, bool> validate, Action<T> onDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link) where T : class
        {
            if (!dropArea.Contains(Event.current.mousePosition))
                return;

            bool isDrop;
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    isDrop = false;
                    break;

                case EventType.DragPerform:
                    isDrop = true;
                    break;

                default:
                    return;
            }

            TryDrop(DragAndDrop.objectReferences, validate, onDrop, isDrop, mode);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Updates the <see cref="DragAndDrop.visualMode"/> or calls `onDrop` for each of the `objects`.
        /// </summary>
        private static void TryDrop<T>(IEnumerable objects, Func<T, bool> validate, Action<T> onDrop, bool isDrop,
            DragAndDropVisualMode mode) where T : class
        {
            if (objects == null)
                return;

            var droppedAny = false;

            foreach (var obj in objects)
            {
                var t = obj as T;

                if (t != null && (validate == null || validate(t)))
                {
                    Deselect();

                    if (!isDrop)
                    {
                        DragAndDrop.visualMode = mode;
                        break;
                    }
                    else
                    {
                        onDrop(t);
                        droppedAny = true;
                    }
                }
            }

            if (droppedAny)
                GUIUtility.ExitGUI();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Uses <see cref="HandleDragAndDrop"/> to deal with drag and drop operations involving
        /// <see cref="AnimationClip"/>s of <see cref="IAnimationClipSource"/>s.
        /// </summary>
        public static void HandleDragAndDropAnimations(Rect dropArea, Action<AnimationClip> onDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link)
        {
            HandleDragAndDrop(dropArea, (clip) => !clip.legacy, onDrop, mode);

            HandleDragAndDrop<IAnimationClipSource>(dropArea, null, (source) =>
            {
                using (ObjectPool.Disposable.AcquireList<AnimationClip>(out var clips))
                {
                    source.GetAnimationClips(clips);
                    TryDrop(clips, (clip) => !clip.legacy, onDrop, true, mode);
                }
            }, mode);

            HandleDragAndDrop<IAnimationClipCollection>(dropArea, null, (collection) =>
            {
                using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
                {
                    collection.GatherAnimationClips(clips);
                    TryDrop(clips, (clip) => !clip.legacy, onDrop, true, mode);
                }
            }, mode);
        }

        /************************************************************************************************************************/

        /// <summary>Deselects any selected IMGUI control.</summary>
        public static void Deselect() => GUIUtility.keyboardControl = 0;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

