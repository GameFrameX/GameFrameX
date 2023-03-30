// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Persistent settings used by Animancer.</summary>
    /// <remarks>
    /// This asset automatically creates itself when first accessed such as when opening the
    /// <see cref="Animancer.Editor.TransitionPreviewWindow"/> or viewing an <see cref="AnimationTimeAttribute"/>.
    /// <para></para>
    /// The default location is <em>Assets/Plugins/Animancer/Editor</em>, but you can freely move it (and the whole
    /// Animancer folder) anywhere in your project.
    /// <para></para>
    /// These settings can also be accessed via the Settings in the <see cref="Tools.AnimancerToolsWindow"/>
    /// (<c>Window/Animation/Animancer Tools</c>).
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerSettings
    /// 
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Editor) + "/" + nameof(AnimancerSettings))]
    public class AnimancerSettings : ScriptableObject
    {
        /************************************************************************************************************************/

        private static AnimancerSettings _Instance;

        /// <summary>
        /// Loads an existing <see cref="AnimancerSettings"/> if there is one anywhere in your project, but otherwise
        /// creates a new one and saves it in the same folder as this script.
        /// </summary>
        public static AnimancerSettings Instance
        {
            get
            {
                if (_Instance != null)
                    return _Instance;

                _Instance = AnimancerEditorUtilities.FindAssetOfType<AnimancerSettings>();

                if (_Instance != null)
                    return _Instance;

                _Instance = CreateInstance<AnimancerSettings>();
                _Instance.name = "Animancer Settings";
                _Instance.hideFlags = HideFlags.DontSaveInBuild;

                var script = MonoScript.FromScriptableObject(_Instance);
                var path = AssetDatabase.GetAssetPath(script);
                path = Path.Combine(Path.GetDirectoryName(path), $"{_Instance.name}.asset");
                AssetDatabase.CreateAsset(_Instance, path);

                return _Instance;
            }
        }

        /************************************************************************************************************************/

        private SerializedObject _SerializedObject;

        /// <summary>The <see cref="SerializedProperty"/> representing the <see cref="Instance"/>.</summary>
        public static SerializedObject SerializedObject
            => Instance._SerializedObject ?? (Instance._SerializedObject = new SerializedObject(Instance));

        /************************************************************************************************************************/

        private readonly Dictionary<string, SerializedProperty>
            SerializedProperties = new Dictionary<string, SerializedProperty>();

        private static SerializedProperty GetSerializedProperty(string propertyPath)
        {
            var properties = Instance.SerializedProperties;
            if (!properties.TryGetValue(propertyPath, out var property))
            {
                property = SerializedObject.FindProperty(propertyPath);
                properties.Add(propertyPath, property);
            }

            return property;
        }

        /************************************************************************************************************************/

        /// <summary>Base class for groups of fields that can be serialized inside <see cref="AnimancerSettings"/>.</summary>
        public abstract class Group
        {
            /************************************************************************************************************************/

            private string _BasePropertyPath;

            /// <summary>[Internal] Sets the prefix for <see cref="GetSerializedProperty"/>.</summary>
            internal void SetBasePropertyPath(string propertyPath)
            {
                _BasePropertyPath = propertyPath + ".";
            }

            /************************************************************************************************************************/

            /// <summary>Returns a <see cref="SerializedProperty"/> relative to the base of this group.</summary>
            protected SerializedProperty GetSerializedProperty(string propertyPath)
                => AnimancerSettings.GetSerializedProperty(_BasePropertyPath + propertyPath);

            /************************************************************************************************************************/

            /// <summary>
            /// Draws a <see cref="EditorGUILayout.PropertyField(SerializedProperty, GUILayoutOption[])"/> for a
            /// property in this group.
            /// </summary>
            protected SerializedProperty DoPropertyField(string propertyPath)
            {
                var property = GetSerializedProperty(propertyPath);
                EditorGUILayout.PropertyField(property, true);
                return property;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the serialized fields.</summary>
        protected virtual void OnEnable()
        {
            if (_TransitionPreviewWindow == null)
                _TransitionPreviewWindow = new TransitionPreviewWindow.Settings();
            _TransitionPreviewWindow.SetBasePropertyPath(nameof(_TransitionPreviewWindow));
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorUtility.SetDirty"/> on the <see cref="Instance"/>.</summary>
        public static new void SetDirty() => EditorUtility.SetDirty(_Instance);

        /************************************************************************************************************************/

        [SerializeField]
        private TransitionPreviewWindow.Settings _TransitionPreviewWindow;

        /// <summary>Settings for the <see cref="TransitionPreviewWindow"/>.</summary>
        internal static TransitionPreviewWindow.Settings TransitionPreviewWindow => Instance._TransitionPreviewWindow;

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationTimeAttribute.Settings _AnimationTimeFields;

        /// <summary>Settings for the <see cref="AnimationTimeAttribute"/>.</summary>
        public static AnimationTimeAttribute.Settings AnimationTimeFields => Instance._AnimationTimeFields;

        /************************************************************************************************************************/

        [SerializeField, Range(0.01f, 1)]
        [Tooltip("The amount of time between repaint commands when 'Display Options/Repaint Constantly' is disabled")]
        private float _InspectorRepaintInterval = 0.25f;

        /// <summary>
        /// The amount of time between repaint commands when
        /// <see cref="AnimancerPlayableDrawer.RepaintConstantly"/> is disabled.
        /// </summary>
        public static float InspectorRepaintInterval => Instance._InspectorRepaintInterval;

        /************************************************************************************************************************/

        [SerializeField]
        [Seconds(Rule = Validate.Value.IsNotNegative)]
        [DefaultValue(0.02f)]
        [Tooltip("The amount of time that will be added by a single frame step")]
        private float _FrameStep = 0.02f;

        /// <summary>The amount of time that will be added by a single frame step (in seconds).</summary>
        public static float FrameStep => Instance._FrameStep;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The frame rate to use for new animations")]
        private float _NewAnimationFrameRate = 12;

        /// <summary>The frame rate to use for new animations.</summary>
        public static SerializedProperty NewAnimationFrameRate => GetSerializedProperty(nameof(_NewAnimationFrameRate));

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Should Animancer Event Callbacks be hidden in the Inspector?")]
        private bool _HideEventCallbacks;

        /// <summary>Should Animancer Event Callbacks be hidden in the Inspector?</summary>
        public static bool HideEventCallbacks => Instance._HideEventCallbacks;

        /************************************************************************************************************************/

        /// <summary>A custom Inspector for <see cref="AnimancerSettings"/>.</summary>
        [CustomEditor(typeof(AnimancerSettings), true), CanEditMultipleObjects]
        public class Editor : UnityEditor.Editor
        {
            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.BeginHorizontal();

                using (ObjectPool.Disposable.AcquireContent(out var label, "Disabled Warnings"))
                {
                    EditorGUI.BeginChangeCheck();
                    var value = EditorGUILayout.EnumFlagsField(label, Validate.PermanentlyDisabledWarnings);
                    if (EditorGUI.EndChangeCheck())
                        Validate.PermanentlyDisabledWarnings = (OptionalWarning)value;
                }

                if (GUILayout.Button("Help", EditorStyles.miniButton, AnimancerGUI.DontExpandWidth))
                    Application.OpenURL(Strings.DocsURLs.OptionalWarning);

                EditorGUILayout.EndHorizontal();
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
    }

#endif
