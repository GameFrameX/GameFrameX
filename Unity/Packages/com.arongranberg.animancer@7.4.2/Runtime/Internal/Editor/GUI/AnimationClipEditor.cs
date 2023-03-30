// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] [Pro-Only] A custom Inspector for <see cref="AnimationClip"/>s</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimationClipEditor
    /// 
    [CustomEditor(typeof(AnimationClip))]
    public class AnimationClipEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        private const string DefaultEditorTypeName = nameof(UnityEditor) + "." + nameof(AnimationClipEditor);

        private static readonly Type
            DefaultEditorType = typeof(UnityEditor.Editor).Assembly.GetType(DefaultEditorTypeName);

        /************************************************************************************************************************/

        private UnityEditor.Editor _DefaultEditor;

        private bool TryGetDefaultEditor(out UnityEditor.Editor editor)
        {
            if (_DefaultEditor == null)
            {
                if (DefaultEditorType == null || AnimancerEditorUtilities.IsChangingPlayMode)
                {
                    editor = null;
                    return false;
                }

                _DefaultEditor = CreateEditor(targets, DefaultEditorType);
                _DefaultEditor.hideFlags = HideFlags.DontSave;
                DestroyOnPlayModeStateChanged(_DefaultEditor);
            }

            editor = _DefaultEditor;
            return true;
        }

        /************************************************************************************************************************/

        protected virtual void OnDestroy()
        {
            _DestroyOnPlayModeStateChanged?.Remove(_DefaultEditor);
            DestroyImmediate(_DefaultEditor);
        }

        /************************************************************************************************************************/

        private static HashSet<Object> _DestroyOnPlayModeStateChanged;

        private static void DestroyOnPlayModeStateChanged(Object obj)
        {
            if (_DestroyOnPlayModeStateChanged == null)
            {
                _DestroyOnPlayModeStateChanged = new HashSet<Object>();

                EditorApplication.playModeStateChanged += (change) =>
                {
                    foreach (var destroy in _DestroyOnPlayModeStateChanged)
                        DestroyImmediate(destroy);

                    _DestroyOnPlayModeStateChanged.Clear();
                };
            }

            _DestroyOnPlayModeStateChanged.Add(obj);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the regular Inspector then adds a better preview for <see cref="Sprite"/> animations.</summary>
        /// <remarks>Called by the Unity editor to draw the custom Inspector GUI elements.</remarks>
        public override void OnInspectorGUI()
        {
            if (DefaultEditorType == null)
            {
                EditorGUILayout.HelpBox(
                    $"Unable to find type '{DefaultEditorTypeName}' in '{typeof(UnityEditor.Editor).Assembly}'." +
                    $" The {nameof(AnimationClipEditor)} script will need to be fixed" +
                    $" or you can simply delete it to use Unity's regular {nameof(AnimationClip)} Inspector.", MessageType.Error);

                const string Label = "Delete " + nameof(AnimationClipEditor) + " Script";
                if (GUILayout.Button(Label))
                {
                    if (EditorUtility.DisplayDialog(Label,
                        $"Are you sure you want to delete the {nameof(AnimationClipEditor)} script?" +
                        $" This operation cannot be undone.",
                        "Delete", "Cancel"))
                    {
                        var script = MonoScript.FromScriptableObject(this);
                        var path = AssetDatabase.GetAssetPath(script);
                        AssetDatabase.DeleteAsset(path);
                    }
                }

                return;
            }

            if (TryGetDefaultEditor(out var editor))
                editor.OnInspectorGUI();

            if (GUILayout.Button("Open Animation Window"))
                EditorApplication.ExecuteMenuItem("Window/Animation/Animation");

            if (GUILayout.Button("Open Animancer Tools"))
                Tools.AnimancerToolsWindow.Open();

            var targets = this.targets;
            if (targets.Length == 1)
            {
                var clip = GetTargetClip(out var type);

                DrawEvents(clip);

                if (type == AnimationType.Sprite)
                {
                    InitializeSpritePreview(editor, clip);
                    DrawSpriteFrames(clip);
                }
            }
        }

        /************************************************************************************************************************/

        private AnimationClip GetTargetClip(out AnimationType type)
        {
            var clip = (AnimationClip)target;
            type = AnimationBindings.GetAnimationType(clip);
            return clip;
        }

        /************************************************************************************************************************/

        [SerializeField]
        private bool _ShowEvents = true;

        private void DrawEvents(AnimationClip clip)
        {
            var events = clip.events;
            if (events == null ||
                events.Length == 0)
                return;

            _ShowEvents = EditorGUILayout.Foldout(_ShowEvents, "Events", true);
            if (!_ShowEvents)
                return;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < events.Length; i++)
                {
                    var animationEvent = events[i];
                    EditorGUILayout.FloatField(animationEvent.functionName, animationEvent.time);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.IntField("Int", animationEvent.intParameter);
                    EditorGUILayout.FloatField("Float", animationEvent.floatParameter);
                    EditorGUILayout.TextField("String", animationEvent.stringParameter);
                    EditorGUILayout.ObjectField("Object", animationEvent.objectReferenceParameter, typeof(Object), false);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        /************************************************************************************************************************/

        [NonSerialized]
        private bool _HasInitializedSpritePreview;

        private void InitializeSpritePreview(UnityEditor.Editor editor, AnimationClip clip)
        {
            if (_HasInitializedSpritePreview
                || editor == null)
                return;

            _HasInitializedSpritePreview = true;

            // Get the avatar preview.

            var field = editor.GetType().GetField("m_AvatarPreview", AnimancerEditorUtilities.InstanceBindings);
            if (field == null)
                return;

            var preview = field.GetValue(editor);
            if (preview == null)
                return;

            var previewType = preview.GetType();

            // Make sure a proper preview object isn't already assigned.

            var previewObject = previewType.GetProperty("PreviewObject", AnimancerEditorUtilities.InstanceBindings);
            if (previewObject == null)
                return;

            var previewGameObject = previewObject.GetValue(preview) as GameObject;
            if (previewGameObject != null &&
                previewGameObject.GetComponentInChildren<Renderer>() != null)
                return;

            // Get the SetPreview method.

            var method = previewType.GetMethod("SetPreview",
                AnimancerEditorUtilities.InstanceBindings, null,
                new Type[] { typeof(GameObject) }, null);
            if (method == null)
                return;

            // Get the Sprite from the target animation's first keyframe.

            var keyframes = GetSpriteReferences(clip);
            if (keyframes == null ||
                keyframes.Length == 0)
                return;

            var sprite = keyframes[0].value as Sprite;
            if (sprite == null)
                return;

            // Create an object with an Animator and SpriteRenderer.
            // The Sprite must be assigned for it to be accepted as the preview object.

            var gameObject = EditorUtility.CreateGameObjectWithHideFlags("SpritePreview",
                HideFlags.HideInHierarchy | HideFlags.DontSave);
            gameObject.AddComponent<Animator>();
            gameObject.AddComponent<SpriteRenderer>().sprite = sprite;

            // Set it as the preview object (which creates a copy of it) and destroy it.

            method.Invoke(preview, new object[] { gameObject });

            DestroyImmediate(gameObject);
        }

        /************************************************************************************************************************/

        private static ConversionCache<int, string> _FrameCache;
        private static ConversionCache<float, string> _TimeCache;

        private static void DrawSpriteFrames(AnimationClip clip)
        {
            var keyframes = GetSpriteReferences(clip);
            if (keyframes == null)
                return;

            for (int i = 0; i < keyframes.Length; i++)
            {
                var keyframe = keyframes[i];
                var sprite = keyframe.value as Sprite;
                if (sprite != null)
                {
                    if (_FrameCache == null)
                    {
                        _FrameCache = new ConversionCache<int, string>(
                            (value) => $"Frame: {value}");

                        _TimeCache = new ConversionCache<float, string>(
                            (value) => $"Time: {value}s");
                    }

                    var texture = sprite.texture;

                    var area = GUILayoutUtility.GetRect(0, AnimancerGUI.LineHeight * 4);
                    var width = area.width;

                    var rect = sprite.rect;
                    area.width = area.height * rect.width / rect.height;

                    rect.x /= texture.width;
                    rect.y /= texture.height;
                    rect.width /= texture.width;
                    rect.height /= texture.height;

                    GUI.DrawTextureWithTexCoords(area, texture, rect);

                    var offset = area.width + AnimancerGUI.StandardSpacing;
                    area.x += offset;
                    area.width = width - offset;
                    area.height = AnimancerGUI.LineHeight;
                    area.y += Mathf.Round(area.height * 0.5f);

                    GUI.Label(area, _FrameCache.Convert(i));

                    AnimancerGUI.NextVerticalArea(ref area);
                    GUI.Label(area, _TimeCache.Convert(keyframe.time));

                    AnimancerGUI.NextVerticalArea(ref area);
                    GUI.Label(area, sprite.name);
                }
            }
        }

        /************************************************************************************************************************/

        private static ObjectReferenceKeyframe[] GetSpriteReferences(AnimationClip clip)
        {
            var bindings = AnimationBindings.GetBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                if (binding.path == "" &&
                    binding.type == typeof(SpriteRenderer) &&
                    binding.propertyName == "m_Sprite")
                    return AnimationUtility.GetObjectReferenceCurve(clip, binding);
            }

            return null;
        }

        /************************************************************************************************************************/
        #region Redirects
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DrawPreview(Rect previewArea)
        {
            if (TryGetDefaultEditor(out var editor))
                editor.DrawPreview(previewArea);
            else
                base.DrawPreview(previewArea);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetInfoString()
        {
            if (TryGetDefaultEditor(out var editor))
                return editor.GetInfoString();
            else
                return base.GetInfoString();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override GUIContent GetPreviewTitle()
        {
            if (TryGetDefaultEditor(out var editor))
                return editor.GetPreviewTitle();
            else
                return base.GetPreviewTitle();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool HasPreviewGUI()
        {
            if (TryGetDefaultEditor(out var editor))
                return editor.HasPreviewGUI();
            else
                return base.HasPreviewGUI();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInteractivePreviewGUI(Rect area, GUIStyle background)
        {
            if (TryGetDefaultEditor(out var editor))
                editor.OnInteractivePreviewGUI(area, background);
            else
                base.OnInteractivePreviewGUI(area, background);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnPreviewGUI(Rect area, GUIStyle background)
        {
            if (TryGetDefaultEditor(out var editor))
                editor.OnPreviewGUI(area, background);
            else
                base.OnPreviewGUI(area, background);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnPreviewSettings()
        {
            if (TryGetDefaultEditor(out var editor))
                editor.OnPreviewSettings();
            else
                base.OnPreviewSettings();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void ReloadPreviewInstances()
        {
            if (TryGetDefaultEditor(out var editor))
                editor.ReloadPreviewInstances();
            else
                base.ReloadPreviewInstances();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (TryGetDefaultEditor(out var editor))
                return editor.RenderStaticPreview(assetPath, subAssets, width, height);
            else
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool RequiresConstantRepaint()
        {
            if (TryGetDefaultEditor(out var editor))
                return editor.RequiresConstantRepaint();
            else
                return base.RequiresConstantRepaint();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool UseDefaultMargins()
        {
            if (TryGetDefaultEditor(out var editor))
                return editor.UseDefaultMargins();
            else
                return base.UseDefaultMargins();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

