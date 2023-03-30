// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using Animancer.Editor.Tools;
using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A custom Inspector for <see cref="Sprite"/>s which allows you to directly edit them instead of just showing
    /// their details like the default one does.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SpriteEditor
    /// 
    [CustomEditor(typeof(Sprite), true), CanEditMultipleObjects]
    public class SpriteEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        private const string
            NameTooltip = "The asset name of the sprite",
            RectTooltip = "The texture area occupied by the sprite",
            PivotTooltip = "The origin point of the sprite relative to its Rect",
            BorderTooltip = "The edge sizes used when 9-Slicing the sprite for the UI system (ignored by SpriteRenderers)";

        [NonSerialized]
        private SerializedProperty
            _Name,
            _Rect,
            _Pivot,
            _Border;

        [NonSerialized]
        private NormalizedPixelField[]
            _RectFields,
            _PivotFields,
            _BorderFields;

        [NonSerialized]
        private bool _HasBeenModified;

        [NonSerialized]
        private Target[] _Targets;

        private readonly struct Target
        {
            public readonly Sprite Sprite;
            public readonly string AssetPath;
            public readonly TextureImporter Importer;

            public Target(Object target)
            {
                Sprite = target as Sprite;
                AssetPath = AssetDatabase.GetAssetPath(target);
                Importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes this editor.</summary>
        protected virtual void OnEnable()
        {
            var targets = this.targets;
            _Targets = new Target[targets.Length];
            for (int i = 0; i < targets.Length; i++)
                _Targets[i] = new Target(targets[i]);

            InitializePreview();

            _Name = serializedObject.FindProperty($"m{nameof(_Name)}");

            _Rect = serializedObject.FindProperty($"m{nameof(_Rect)}");
            if (_Rect != null)
            {
                _RectFields = new NormalizedPixelField[]
                {
                    new NormalizedPixelField(_Rect.FindPropertyRelative(nameof(Rect.x)), new GUIContent("X (Left)",
                        "The distance from the left edge of the texture to the left edge of the sprite"), false),
                    new NormalizedPixelField(_Rect.FindPropertyRelative(nameof(Rect.y)), new GUIContent("Y (Bottom)",
                        "The distance from the bottom edge of the texture to the bottom edge of the sprite"), false),
                    new NormalizedPixelField(_Rect.FindPropertyRelative(nameof(Rect.width)), new GUIContent("Width",
                        "The horizontal size of the sprite"), false),
                    new NormalizedPixelField(_Rect.FindPropertyRelative(nameof(Rect.height)), new GUIContent("Height",
                        "The vertical size of the sprite"), false),
                };
            }

            _Pivot = serializedObject.FindProperty($"m{nameof(_Pivot)}");
            if (_Pivot != null)
            {
                _PivotFields = new NormalizedPixelField[]
                {
                    new NormalizedPixelField(_Pivot.FindPropertyRelative(nameof(Vector2.x)), new GUIContent("X",
                        "The horizontal distance from the left edge of the sprite to the pivot point"), true),
                    new NormalizedPixelField(_Pivot.FindPropertyRelative(nameof(Vector2.y)), new GUIContent("Y",
                        "The vertical distance from the bottom edge of the sprite to the pivot point"), true),
                };
            }

            _Border = serializedObject.FindProperty($"m{nameof(_Border)}");
            if (_Border != null)
            {
                _BorderFields = new NormalizedPixelField[]
                {
                    new NormalizedPixelField(_Border.FindPropertyRelative(nameof(Vector4.x)), new GUIContent("Left",
                        BorderTooltip), false),
                    new NormalizedPixelField(_Border.FindPropertyRelative(nameof(Vector4.y)), new GUIContent("Bottom",
                        BorderTooltip), false),
                    new NormalizedPixelField(_Border.FindPropertyRelative(nameof(Vector4.z)), new GUIContent("Right",
                        BorderTooltip), false),
                    new NormalizedPixelField(_Border.FindPropertyRelative(nameof(Vector4.w)), new GUIContent("Top",
                        BorderTooltip), false),
                };
            }
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up this editor.</summary>
        protected virtual void OnDisable()
        {
            CleanUpPreview();

            if (_HasBeenModified)
            {
                var sprite = target as Sprite;
                if (sprite == null)
                    return;

                if (EditorUtility.DisplayDialog("Unapplied Import Settings",
                    $"Unapplied import settings for '{sprite.name}' in '{AssetDatabase.GetAssetPath(sprite)}'",
                    nameof(Apply), nameof(Revert)))
                    Apply();
            }
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <summary>Are all targets set to <see cref="SpriteImportMode.Multiple"/>?</summary>
        private bool AllSpriteModeMultiple
        {
            get
            {
                for (int i = 0; i < _Targets.Length; i++)
                {
                    var importer = _Targets[i].Importer;
                    if (importer == null ||
                        importer.spriteImportMode != SpriteImportMode.Multiple)
                        return false;
                }

                return true;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Called by the Unity editor to draw the custom Inspector GUI elements.</summary>
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            DoNameGUI();

            // If any target isn't set to Multiple, disable the GUI because only renaming will work.
            var enabled = GUI.enabled;
            if (!AllSpriteModeMultiple)
                GUI.enabled = false;

            DoRectGUI();
            DoPivotGUI();
            DoBorderGUI();

            GUI.enabled = enabled;

            if (EditorGUI.EndChangeCheck())
                _HasBeenModified = true;

            GUILayout.Space(AnimancerGUI.StandardSpacing);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUI.enabled = _HasBeenModified;
                if (GUILayout.Button(nameof(Revert)))
                    Revert();
                if (GUILayout.Button(nameof(Apply)))
                    Apply();
            }
            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        private void DoNameGUI()
        {
            GUILayout.BeginHorizontal();
            var enabled = GUI.enabled;

            if (_Name.hasMultipleDifferentValues)
                GUI.enabled = false;

            using (ObjectPool.Disposable.AcquireContent(out var label, "Name", NameTooltip))
                EditorGUILayout.PropertyField(_Name, label, true);

            GUI.enabled = true;

            var changed = EditorGUI.EndChangeCheck();// Exclude the Rename button from the main change check.

            if (GUILayout.Button("Rename Tool", EditorStyles.miniButton, AnimancerGUI.DontExpandWidth))
                AnimancerToolsWindow.Open(typeof(RenameSpritesTool));

            EditorGUI.BeginChangeCheck();
            if (changed)
                GUI.changed = true;

            GUI.enabled = enabled;
            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        private void DoRectGUI()
        {
            var texture = ((Sprite)target).texture;
            _RectFields[0].normalizeMultiplier = _RectFields[2].normalizeMultiplier = 1f / texture.width;
            _RectFields[1].normalizeMultiplier = _RectFields[3].normalizeMultiplier = 1f / texture.height;

            using (ObjectPool.Disposable.AcquireContent(out var label, "Rect", RectTooltip))
                NormalizedPixelField.DoGroupGUI(_Rect, label, _RectFields);
        }

        /************************************************************************************************************************/

        private void DoPivotGUI()
        {
            var showMixedValue = EditorGUI.showMixedValue;

            var targets = this.targets;
            var size = targets[0] is Sprite sprite ? sprite.rect.size : Vector2.one;
            for (int i = 1; i < targets.Length; i++)
            {
                sprite = targets[i] as Sprite;
                if (sprite == null || !sprite.rect.size.Equals(size))
                    EditorGUI.showMixedValue = true;
            }

            _PivotFields[0].normalizeMultiplier = 1f / size.x;
            _PivotFields[1].normalizeMultiplier = 1f / size.y;

            using (ObjectPool.Disposable.AcquireContent(out var label, "Pivot", PivotTooltip))
                NormalizedPixelField.DoGroupGUI(_Pivot, label, _PivotFields);

            EditorGUI.showMixedValue = showMixedValue;
        }

        /************************************************************************************************************************/

        private void DoBorderGUI()
        {
            var size = _Rect.rectValue.size;
            _BorderFields[0].normalizeMultiplier = _BorderFields[2].normalizeMultiplier = 1f / size.x;
            _BorderFields[1].normalizeMultiplier = _BorderFields[3].normalizeMultiplier = 1f / size.y;

            using (ObjectPool.Disposable.AcquireContent(out var label, "Border", BorderTooltip))
                NormalizedPixelField.DoGroupGUI(_Border, label, _BorderFields);
        }

        /************************************************************************************************************************/

        private void Revert()
        {
            AnimancerGUI.Deselect();
            _HasBeenModified = false;
            serializedObject.Update();
        }

        /************************************************************************************************************************/

        private void Apply()
        {
            AnimancerGUI.Deselect();
            _HasBeenModified = false;
            var targets = this.targets;

            var hasError = false;

            for (int i = 0; i < _Targets.Length; i++)
            {
                var target = _Targets[i];
                if (target.Sprite == null ||
                    target.Importer == null)
                    continue;

                var data = new SpriteDataEditor(target.Importer);
                Apply(data, target.Sprite, ref hasError);

                if (!hasError)
                    data.Apply();
            }

            for (int i = 0; i < targets.Length; i++)
                if (targets[i] == null)
                    return;

            serializedObject.Update();
        }

        /************************************************************************************************************************/

        private void Apply(SpriteDataEditor data, Sprite sprite, ref bool hasError)
        {
            if (data.SpriteCount == 0)
            {
                if (!_Name.hasMultipleDifferentValues)
                {
                    var path = AssetDatabase.GetAssetPath(sprite);
                    if (path != null)
                    {
                        AssetDatabase.RenameAsset(path, _Name.stringValue);
                        hasError = true;// Don't apply the importer.
                    }
                }

                return;
            }

            var index = data.IndexOf(sprite);
            if (index < 0)
            {
                hasError = true;
                return;
            }

            if (!_Name.hasMultipleDifferentValues)
                data.SetName(index, _Name.stringValue);

            if (!_Rect.hasMultipleDifferentValues)
                data.SetRect(index, _Rect.rectValue);

            if (!_Pivot.hasMultipleDifferentValues)
                data.SetPivot(index, _Pivot.vector2Value);

            if (!_Border.hasMultipleDifferentValues)
                data.SetBorder(index, _Border.vector4Value);

            if (!data.ValidateBounds(index, sprite))
                hasError = true;
        }

        /************************************************************************************************************************/
        #region Normalized Pixel Field
        /************************************************************************************************************************/

        /// <summary>
        /// A wrapper around a <see cref="SerializedProperty"/> to display it using two float fields where one is
        /// normalized and the other is not.
        /// </summary>
        private class NormalizedPixelField
        {
            /************************************************************************************************************************/

            /// <summary>The target property.</summary>
            public readonly SerializedProperty Property;

            /// <summary>The label to display next to the property.</summary>
            public readonly GUIContent Label;

            /// <summary>Is the serialized property value normalized?</summary>
            public readonly bool IsNormalized;

            /// <summary>The multiplier to turn a non-normalized value into a normalized one.</summary>
            public float normalizeMultiplier;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="NormalizedPixelField"/>.</summary>
            public NormalizedPixelField(SerializedProperty property, GUIContent label, bool isNormalized)
            {
                Property = property;
                Label = label;
                IsNormalized = isNormalized;
            }

            /************************************************************************************************************************/

            /// <summary>Draws a group of <see cref="NormalizedPixelField"/>s.</summary>
            public static void DoGroupGUI(SerializedProperty baseProperty, GUIContent label, NormalizedPixelField[] fields)
            {
                var height = (AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing) * (fields.Length + 1);
                var area = GUILayoutUtility.GetRect(0, height);

                area.height = AnimancerGUI.LineHeight;
                label = EditorGUI.BeginProperty(area, label, baseProperty);
                GUI.Label(area, label);
                EditorGUI.EndProperty();

                EditorGUI.indentLevel++;

                for (int i = 0; i < fields.Length; i++)
                {
                    AnimancerGUI.NextVerticalArea(ref area);
                    fields[i].DoTwinFloatFieldGUI(area);
                }

                EditorGUI.indentLevel--;
            }

            /************************************************************************************************************************/

            /// <summary>Draws this <see cref="NormalizedPixelField"/>.</summary>
            public void DoTwinFloatFieldGUI(Rect area)
            {
                var drawer = IsNormalized ?
                    NormalizedPixelFieldAttribute.Normalized :
                    NormalizedPixelFieldAttribute.Pixel;

                drawer.CalculateMultipliers(normalizeMultiplier);
                drawer.OnGUI(area, Property, Label);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Normalized Pixel Field Attribute
        /************************************************************************************************************************/

        private class NormalizedPixelFieldAttribute : UnitsAttribute
        {
            /************************************************************************************************************************/

            private static new readonly float[] Multipliers = new float[2];

            public void CalculateMultipliers(float normalizeMultiplier)
            {
                if (UnitIndex == 0)// Pixels.
                {
                    Multipliers[0] = 1;
                    Multipliers[1] = normalizeMultiplier;
                }
                else// Normalized.
                {
                    Multipliers[0] = 1f / normalizeMultiplier;
                    Multipliers[1] = 1;
                }
            }

            /************************************************************************************************************************/

            private static new readonly CompactUnitConversionCache[] DisplayConverters =
            {
                new CompactUnitConversionCache("px"),
                AnimationTimeAttribute.XSuffix,
            };

            /************************************************************************************************************************/

            public static readonly NormalizedPixelFieldAttribute Pixel = new NormalizedPixelFieldAttribute(false);
            public static readonly NormalizedPixelFieldAttribute Normalized = new NormalizedPixelFieldAttribute(true);

            /************************************************************************************************************************/

            public NormalizedPixelFieldAttribute(bool isNormalized)
            {
#if UNITY_EDITOR
                SetUnits(Multipliers, DisplayConverters, isNormalized ? 1 : 0);
                Rule = Validate.Value.IsFinite;
#endif
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override int GetLineCount(SerializedProperty property, GUIContent label) => 1;

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Preview
        /************************************************************************************************************************/

        private static readonly Type
            DefaultEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SpriteInspector");

        private readonly Dictionary<Object, UnityEditor.Editor>
            TargetToDefaultEditor = new Dictionary<Object, UnityEditor.Editor>();

        /************************************************************************************************************************/

        private void InitializePreview()
        {
            foreach (var target in targets)
            {
                if (!TargetToDefaultEditor.ContainsKey(target))
                {
                    var editor = CreateEditor(target, DefaultEditorType);
                    TargetToDefaultEditor.Add(target, editor);
                }
            }
        }

        /************************************************************************************************************************/

        private void CleanUpPreview()
        {
            foreach (var editor in TargetToDefaultEditor.Values)
                DestroyImmediate(editor);

            TargetToDefaultEditor.Clear();
        }

        /************************************************************************************************************************/

        private bool TryGetDefaultEditor(out UnityEditor.Editor editor)
            => TargetToDefaultEditor.TryGetValue(target, out editor);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetInfoString()
        {
            if (!TryGetDefaultEditor(out var editor))
                return null;

            return editor.GetInfoString();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (!TryGetDefaultEditor(out var editor))
                return null;

            return editor.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool HasPreviewGUI()
        {
            return TryGetDefaultEditor(out var editor) && editor.HasPreviewGUI();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnPreviewGUI(Rect area, GUIStyle background)
        {
            if (TryGetDefaultEditor(out var editor))
                editor.OnPreviewGUI(area, background);

            var sprite = target as Sprite;
            if (sprite == null)
                return;

            EditorGUI.BeginChangeCheck();
            FitAspectRatio(ref area, sprite);
            DoPivotDotGUI(area, sprite);
            if (EditorGUI.EndChangeCheck())
                _HasBeenModified = true;
        }

        /************************************************************************************************************************/

        private static void FitAspectRatio(ref Rect area, Sprite sprite)
        {
            var areaAspect = area.width / area.height;
            var spriteAspect = sprite.rect.width / sprite.rect.height;
            if (areaAspect != spriteAspect)
            {
                if (areaAspect > spriteAspect)
                {
                    var width = area.height * spriteAspect;
                    area.x += (area.width - width) * 0.5f;
                    area.width = width;
                }
                else
                {
                    var height = area.width / spriteAspect;
                    area.y += (area.height - height) * 0.5f;
                    area.height = height;
                }
            }
        }

        /************************************************************************************************************************/

        private static readonly int PivotDotControlIDHint = "PivotDot".GetHashCode();

        private static GUIStyle _PivotDot;
        private static GUIStyle _PivotDotActive;

        [NonSerialized] private Vector2 _MouseDownPivot;

        private void DoPivotDotGUI(Rect area, Sprite sprite)
        {
            if (_PivotDot == null)
                _PivotDot = "U2D.pivotDot";
            if (_PivotDotActive == null)
                _PivotDotActive = "U2D.pivotDotActive";

            Vector2 pivot;
            if (_Pivot.hasMultipleDifferentValues)
            {
                pivot = sprite.pivot;
                pivot.x /= sprite.rect.width;
                pivot.y /= sprite.rect.height;
            }
            else
            {
                pivot = _Pivot.vector2Value;
            }
            pivot.x *= area.width;
            pivot.y *= area.height;

            var pivotArea = new Rect(
                area.x + pivot.x - _PivotDot.fixedWidth * 0.5f,
                area.yMax - pivot.y - _PivotDot.fixedHeight * 0.5f,
                _PivotDot.fixedWidth,
                _PivotDot.fixedHeight);

            var controlID = GUIUtility.GetControlID(PivotDotControlIDHint, FocusType.Keyboard);

            var currentEvent = Event.current;
            switch (currentEvent.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0 && pivotArea.Contains(Event.current.mousePosition) && !currentEvent.alt)
                    {
                        GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;
                        _MouseDownPivot = pivot;
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        pivot = Event.current.mousePosition;
                        pivot.x = InverseLerpUnclamped(area.x, area.xMax, pivot.x);
                        pivot.y = InverseLerpUnclamped(area.yMax, area.y, pivot.y);

                        if (currentEvent.control)
                        {
                            pivot.x = Mathf.Round(pivot.x * sprite.rect.width) / sprite.rect.width;
                            pivot.y = Mathf.Round(pivot.y * sprite.rect.height) / sprite.rect.height;
                        }

                        _Pivot.vector2Value = pivot;

                        GUI.changed = true;
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (currentEvent.button == 0 || currentEvent.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    break;

                case EventType.KeyDown:
                    if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
                    {
                        _Pivot.vector2Value = _MouseDownPivot;
                        GUIUtility.hotControl = 0;
                        GUI.changed = true;
                        currentEvent.Use();
                    }
                    break;

                case EventType.Repaint:
                    EditorGUIUtility.AddCursorRect(pivotArea, MouseCursor.Arrow, controlID);
                    var style = GUIUtility.hotControl == controlID ? _PivotDotActive : _PivotDot;
                    style.Draw(pivotArea, GUIContent.none, controlID);
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The opposite of <see cref="Mathf.LerpUnclamped(float, float, float)"/>.</summary>
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (a == b)
                return 0;
            else
                return (value - a) / (b - a);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

