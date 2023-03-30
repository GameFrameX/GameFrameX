// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionPreviewWindow
    partial class TransitionPreviewWindow
    {
        /// <summary>Persistent settings for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
        /// </remarks>
        [Serializable]
        internal class Settings : AnimancerSettings.Group
        {
            /************************************************************************************************************************/

            private static Settings Instance => AnimancerSettings.TransitionPreviewWindow;

            /************************************************************************************************************************/

            public static void DoInspectorGUI()
            {
                AnimancerSettings.SerializedObject.Update();

                EditorGUI.indentLevel++;

                DoMiscGUI();
                DoEnvironmentGUI();
                DoModelsGUI();
                DoHierarchyGUI();

                EditorGUI.indentLevel--;

                AnimancerSettings.SerializedObject.ApplyModifiedProperties();
            }

            /************************************************************************************************************************/
            #region Misc
            /************************************************************************************************************************/

            private static void DoMiscGUI()
            {
                Instance.DoPropertyField(nameof(_AutoClose));
            }

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Should this window automatically close if the target object is destroyed?")]
            private bool _AutoClose = true;

            public static bool AutoClose => Instance._AutoClose;

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Should the scene lighting be enabled?")]
            private bool _SceneLighting = false;

            public static bool SceneLighting
            {
                get => Instance._SceneLighting;
                set
                {
                    if (SceneLighting == value)
                        return;

                    var property = Instance.GetSerializedProperty(nameof(_SceneLighting));
                    property.boolValue = value;
                    AnimancerSettings.SerializedObject.ApplyModifiedProperties();
                }
            }

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Should the skybox be visible?")]
            private bool _ShowSkybox = false;

            public static bool ShowSkybox
            {
                get => Instance._ShowSkybox;
                set
                {
                    if (ShowSkybox == value)
                        return;

                    var property = Instance.GetSerializedProperty(nameof(_ShowSkybox));
                    property.boolValue = value;
                    AnimancerSettings.SerializedObject.ApplyModifiedProperties();
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Environment
            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("If set, the default preview scene lighting will be replaced with this prefab.")]
            private GameObject _SceneEnvironment;

            public static GameObject SceneEnvironment => Instance._SceneEnvironment;

            /************************************************************************************************************************/

            private static void DoEnvironmentGUI()
            {
                EditorGUI.BeginChangeCheck();

                Instance.DoPropertyField(nameof(_SceneEnvironment));

                if (EditorGUI.EndChangeCheck())
                {
                    AnimancerSettings.SerializedObject.ApplyModifiedProperties();
                    InstanceScene.OnEnvironmentPrefabChanged();
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Models
            /************************************************************************************************************************/

            private static void DoModelsGUI()
            {
                var property = ModelsProperty;
                var count = property.arraySize = EditorGUILayout.DelayedIntField(nameof(Models), property.arraySize);

                // Drag and Drop to add model.
                var area = GUILayoutUtility.GetLastRect();
                AnimancerGUI.HandleDragAndDrop<GameObject>(area,
                    (gameObject) =>
                    {
                        return
                            EditorUtility.IsPersistent(gameObject) &&
                            !Models.Contains(gameObject) &&
                            gameObject.GetComponentInChildren<Animator>() != null;
                    },
                    (gameObject) =>
                    {
                        var modelsProperty = ModelsProperty;// Avoid Closure.
                        modelsProperty.serializedObject.Update();

                        var i = modelsProperty.arraySize;
                        modelsProperty.arraySize = i + 1;
                        modelsProperty.GetArrayElementAtIndex(i).objectReferenceValue = gameObject;
                        modelsProperty.serializedObject.ApplyModifiedProperties();
                    });

                if (count == 0)
                    return;

                property.isExpanded = EditorGUI.Foldout(area, property.isExpanded, GUIContent.none, true);
                if (!property.isExpanded)
                    return;

                EditorGUI.indentLevel++;

                var model = property.GetArrayElementAtIndex(0);
                for (int i = 0; i < count; i++)
                {
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(model);

                    if (GUILayout.Button("x", AnimancerGUI.MiniButton))
                    {
                        Serialization.RemoveArrayElement(property, i);
                        property.serializedObject.ApplyModifiedProperties();

                        AnimancerGUI.Deselect();
                        GUIUtility.ExitGUI();
                        return;
                    }

                    GUILayout.Space(EditorStyles.objectField.margin.right);
                    GUILayout.EndHorizontal();
                    model.Next(false);
                }

                EditorGUI.indentLevel--;
            }

            /************************************************************************************************************************/

            [SerializeField]
            private List<GameObject> _Models;

            /// <summary>The models previously used in the <see cref="TransitionPreviewWindow"/>.</summary>
            /// <remarks>Accessing this property removes missing and duplicate models from the list.</remarks>
            public static List<GameObject> Models
            {
                get
                {
                    var instance = Instance;
                    AnimancerEditorUtilities.RemoveMissingAndDuplicates(ref instance._Models);
                    return instance._Models;
                }
            }

            private static SerializedProperty ModelsProperty => Instance.GetSerializedProperty(nameof(_Models));

            /************************************************************************************************************************/

            public static void AddModel(GameObject model)
            {
                if (model == DefaultHumanoid ||
                    model == DefaultSprite)
                    return;

                if (EditorUtility.IsPersistent(model))
                {
                    AddModel(Models, model);
                    AnimancerSettings.SetDirty();
                }
                else
                {
                    AddModel(TemporarySettings.PreviewModels, model);
                }
            }

            private static void AddModel(List<GameObject> models, GameObject model)
            {
                // Remove if it was already there so that when we add it, it will be moved to the end.
                var index = models.LastIndexOf(model);// Search backwards because it's more likely to be near the end.
                if (index >= 0 && index < models.Count)
                    models.RemoveAt(index);

                models.Add(model);
            }

            /************************************************************************************************************************/

            private static GameObject _DefaultHumanoid;

            public static GameObject DefaultHumanoid
            {
                get
                {
                    if (_DefaultHumanoid == null)
                    {
                        // Try to load Animancer's DefaultHumanoid.
                        var path = AssetDatabase.GUIDToAssetPath("c9f3e1113795a054c939de9883b31fed");
                        if (!string.IsNullOrEmpty(path))
                        {
                            _DefaultHumanoid = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (_DefaultHumanoid != null)
                                return _DefaultHumanoid;
                        }

                        // Otherwise try to load Unity's DefaultAvatar.
                        _DefaultHumanoid = EditorGUIUtility.Load("Avatar/DefaultAvatar.fbx") as GameObject;

                        if (_DefaultHumanoid == null)
                        {
                            // Otherwise just create an empty object.
                            _DefaultHumanoid = EditorUtility.CreateGameObjectWithHideFlags(
                                "DefaultAvatar", HideFlags.HideAndDontSave, typeof(Animator));
                            _DefaultHumanoid.transform.parent = _Instance._Scene.PreviewSceneRoot;
                        }
                    }

                    return _DefaultHumanoid;
                }
            }

            public static bool IsDefaultHumanoid(GameObject gameObject) => gameObject == DefaultHumanoid;

            /************************************************************************************************************************/

            private static GameObject _DefaultSprite;

            public static GameObject DefaultSprite
            {
                get
                {
                    if (_DefaultSprite == null)
                    {
                        _DefaultSprite = EditorUtility.CreateGameObjectWithHideFlags(
                            "DefaultSprite", HideFlags.HideAndDontSave, typeof(Animator), typeof(SpriteRenderer));
                        _DefaultSprite.transform.parent = _Instance._Scene.PreviewSceneRoot;
                    }

                    return _DefaultSprite;
                }
            }

            public static bool IsDefaultSprite(GameObject gameObject) => gameObject == DefaultSprite;

            /************************************************************************************************************************/

            /// <summary>
            /// Tries to choose the most appropriate model to use based on the properties animated by the target
            /// <see cref="Transition"/>.
            /// </summary>
            public static Transform TrySelectBestModel()
            {
                var transition = Transition;
                if (transition == null)
                    return null;

                using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
                {
                    clips.GatherFromSource(transition);
                    if (clips.Count == 0)
                        return null;

                    var model = TrySelectBestModel(clips, TemporarySettings.PreviewModels);
                    if (model != null)
                        return model;

                    model = TrySelectBestModel(clips, Models);
                    if (model != null)
                        return model;

                    foreach (var clip in clips)
                    {
                        var type = AnimationBindings.GetAnimationType(clip);
                        switch (type)
                        {
                            case AnimationType.Humanoid:
                                return DefaultHumanoid.transform;

                            case AnimationType.Sprite:
                                return DefaultSprite.transform;
                        }
                    }

                    return null;
                }
            }

            /************************************************************************************************************************/

            private static Transform TrySelectBestModel(HashSet<AnimationClip> clips, List<GameObject> models)
            {
                var animatableBindings = new HashSet<EditorCurveBinding>[models.Count];

                for (int i = 0; i < models.Count; i++)
                {
                    animatableBindings[i] = AnimationBindings.GetBindings(models[i]).ObjectBindings;
                }

                var bestMatchIndex = -1;
                var bestMatchCount = 0;
                foreach (var clip in clips)
                {
                    var clipBindings = AnimationBindings.GetBindings(clip);

                    for (int iModel = animatableBindings.Length - 1; iModel >= 0; iModel--)
                    {
                        var modelBindings = animatableBindings[iModel];
                        var matches = 0;

                        for (int iBinding = 0; iBinding < clipBindings.Length; iBinding++)
                        {
                            if (modelBindings.Contains(clipBindings[iBinding]))
                                matches++;
                        }

                        if (bestMatchCount < matches && matches > clipBindings.Length / 2)
                        {
                            bestMatchCount = matches;
                            bestMatchIndex = iModel;

                            // If it matches all bindings, use it.
                            if (bestMatchCount == clipBindings.Length)
                                goto FoundBestMatch;
                        }
                    }
                }

                FoundBestMatch:
                if (bestMatchIndex >= 0)
                    return models[bestMatchIndex].transform;
                else
                    return null;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Scene Hierarchy
            /************************************************************************************************************************/

            private static void DoHierarchyGUI()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Preview Scene Hierarchy");
                DoHierarchyGUI(_Instance._Scene.PreviewSceneRoot);
                GUILayout.EndVertical();
            }

            /************************************************************************************************************************/

            private static GUIStyle _HierarchyButtonStyle;

            private static void DoHierarchyGUI(Transform root)
            {
                var area = AnimancerGUI.LayoutSingleLineRect();

                if (_HierarchyButtonStyle == null)
                    _HierarchyButtonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        alignment = TextAnchor.MiddleLeft,
                    };

                if (GUI.Button(EditorGUI.IndentedRect(area), root.name, _HierarchyButtonStyle))
                {
                    Selection.activeTransform = root;
                    GUIUtility.ExitGUI();
                }

                var childCount = root.childCount;
                if (childCount == 0)
                    return;

                var expandedHierarchy = _Instance._Scene.ExpandedHierarchy;
                var index = expandedHierarchy != null ? expandedHierarchy.IndexOf(root) : -1;
                var isExpanded = EditorGUI.Foldout(area, index >= 0, GUIContent.none);
                if (isExpanded)
                {
                    if (index < 0)
                        expandedHierarchy.Add(root);

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < childCount; i++)
                        DoHierarchyGUI(root.GetChild(i));
                    EditorGUI.indentLevel--;
                }
                else if (index >= 0)
                {
                    expandedHierarchy.RemoveAt(index);
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

#endif

