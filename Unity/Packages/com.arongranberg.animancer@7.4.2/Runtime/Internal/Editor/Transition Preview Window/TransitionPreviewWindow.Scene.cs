// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionPreviewWindow
    partial class TransitionPreviewWindow
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="Scene"/> of the current <see cref="TransitionPreviewWindow"/> instance.</summary>
        public static Scene InstanceScene => _Instance != null ? _Instance._Scene : null;

        /************************************************************************************************************************/

        /// <summary>Temporary scene management for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
        /// </remarks>
        [Serializable]
        public class Scene
        {
            /************************************************************************************************************************/
            #region Fields and Properties
            /************************************************************************************************************************/

            /// <summary><see cref="HideFlags.HideAndDontSave"/> without <see cref="HideFlags.NotEditable"/>.</summary>
            private const HideFlags HideAndDontSave = HideFlags.HideInHierarchy | HideFlags.DontSave;

            /// <summary>The scene displayed by the <see cref="TransitionPreviewWindow"/>.</summary>
            [SerializeField]
            private UnityEngine.SceneManagement.Scene _Scene;

            /// <summary>The root object in the preview scene.</summary>
            public Transform PreviewSceneRoot { get; private set; }

            /// <summary>The root of the model in the preview scene. A child of the <see cref="PreviewSceneRoot"/>.</summary>
            public Transform InstanceRoot { get; private set; }

            /// <summary>
            /// An instance of the <see cref="Settings.SceneEnvironment"/>.
            /// A child of the <see cref="PreviewSceneRoot"/>.
            /// </summary>
            public GameObject EnvironmentInstance { get; private set; }

            /************************************************************************************************************************/

            [SerializeField]
            private Transform _OriginalRoot;

            /// <summary>The original model which was instantiated to create the <see cref="InstanceRoot"/>.</summary>
            public Transform OriginalRoot
            {
                get => _OriginalRoot;
                set
                {
                    _OriginalRoot = value;
                    InstantiateModel();

                    if (value != null)
                        Settings.AddModel(value.gameObject);
                }
            }

            /************************************************************************************************************************/

            /// <summary>The <see cref="Animator"/> components attached to the <see cref="InstanceRoot"/> and its children.</summary>
            public Animator[] InstanceAnimators { get; private set; }

            [SerializeField] private int _SelectedInstanceAnimator;
            [NonSerialized] private AnimationType _SelectedInstanceType;

            /// <summary>The <see cref="Animator"/> component currently being used for the preview.</summary>
            public Animator SelectedInstanceAnimator
            {
                get
                {
                    if (InstanceAnimators == null ||
                        InstanceAnimators.Length == 0)
                        return null;

                    if (_SelectedInstanceAnimator > InstanceAnimators.Length)
                        _SelectedInstanceAnimator = InstanceAnimators.Length;

                    return InstanceAnimators[_SelectedInstanceAnimator];
                }
            }

            /************************************************************************************************************************/

            [NonSerialized]
            private AnimancerPlayable _Animancer;

            /// <summary>The <see cref="AnimancerPlayable"/> being used for the preview.</summary>
            public AnimancerPlayable Animancer
            {
                get
                {
                    if ((_Animancer == null || !_Animancer.IsValid) &&
                        InstanceRoot != null)
                    {
                        var animator = SelectedInstanceAnimator;
                        if (animator != null)
                        {
                            AnimancerPlayable.SetNextGraphName($"{animator.name} (Animancer Preview)");
                            _Animancer = AnimancerPlayable.Create();
                            _Animancer.CreateOutput(
                                new AnimancerEditorUtilities.DummyAnimancerComponent(animator, _Animancer));
                            _Animancer.RequirePostUpdate(Animations.WindowMatchStateTime.Instance);
                            _Instance._Animations.NormalizedTime = _Instance._Animations.NormalizedTime;
                        }
                    }

                    return _Animancer;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Initialization
            /************************************************************************************************************************/

            /// <summary>Initializes this <see cref="Scene"/>.</summary>
            public void OnEnable()
            {
                EditorSceneManager.sceneOpening += OnSceneOpening;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;

                duringSceneGui += DoCustomGUI;

                CreateScene();
                if (OriginalRoot == null)
                    OriginalRoot = Settings.TrySelectBestModel();
            }

            /************************************************************************************************************************/

            private void CreateScene()
            {
                _Scene = EditorSceneManager.NewPreviewScene();
                _Scene.name = "Transition Preview";
                _Instance.customScene = _Scene;

                PreviewSceneRoot = EditorUtility.CreateGameObjectWithHideFlags(
                    $"{nameof(Animancer)}.{nameof(TransitionPreviewWindow)}", HideAndDontSave).transform;
                SceneManager.MoveGameObjectToScene(PreviewSceneRoot.gameObject, _Scene);
                _Instance.customParentForDraggedObjects = PreviewSceneRoot;

                OnEnvironmentPrefabChanged();
            }

            /************************************************************************************************************************/

            internal void OnEnvironmentPrefabChanged()
            {
                DestroyImmediate(EnvironmentInstance);

                var prefab = Settings.SceneEnvironment;
                if (prefab != null)
                    EnvironmentInstance = Instantiate(prefab, PreviewSceneRoot);
            }

            /************************************************************************************************************************/

            private void InstantiateModel()
            {
                DestroyModelInstance();

                if (_OriginalRoot == null)
                    return;

                PreviewSceneRoot.gameObject.SetActive(false);
                InstanceRoot = Instantiate(_OriginalRoot, PreviewSceneRoot);
                InstanceRoot.localPosition = default;
                InstanceRoot.name = _OriginalRoot.name;

                DisableUnnecessaryComponents(InstanceRoot.gameObject);

                InstanceAnimators = InstanceRoot.GetComponentsInChildren<Animator>();
                for (int i = 0; i < InstanceAnimators.Length; i++)
                {
                    var animator = InstanceAnimators[i];
                    animator.enabled = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.fireEvents = false;
                    animator.updateMode = AnimatorUpdateMode.Normal;
                }

                PreviewSceneRoot.gameObject.SetActive(true);

                SetSelectedAnimator(_SelectedInstanceAnimator);
                FocusCamera();
                _Instance._Animations.GatherAnimations();
            }

            /************************************************************************************************************************/

            /// <summary>Disables all unnecessary components on the `root` or its children.</summary>
            private static void DisableUnnecessaryComponents(GameObject root)
            {
                var behaviours = root.GetComponentsInChildren<Behaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];

                    // Other undesirable components aren't Behaviours anyway: Transform, MeshFilter, Renderer
                    if (behaviour is Animator)
                        continue;

                    var type = behaviour.GetType();
                    if (type.IsDefined(typeof(ExecuteAlways), true) ||
                        type.IsDefined(typeof(ExecuteInEditMode), true))
                        continue;

                    behaviour.enabled = false;
                    behaviour.hideFlags |= HideFlags.NotEditable;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Sets the <see cref="SelectedInstanceAnimator"/>.</summary>
            public void SetSelectedAnimator(int index)
            {
                DestroyAnimancerInstance();

                var animator = SelectedInstanceAnimator;
                if (animator != null && animator.enabled)
                {
                    animator.Rebind();
                    animator.enabled = false;
                    return;
                }

                _SelectedInstanceAnimator = index;

                animator = SelectedInstanceAnimator;
                if (animator != null)
                {
                    animator.enabled = true;
                    _SelectedInstanceType = AnimationBindings.GetAnimationType(animator);
                    _Instance.in2DMode = _SelectedInstanceType == AnimationType.Sprite;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Called when the target transition property is changed.</summary>
            public void OnTargetPropertyChanged()
            {
                _SelectedInstanceAnimator = 0;
                if (_ExpandedHierarchy != null)
                    _ExpandedHierarchy.Clear();

                OriginalRoot = AnimancerEditorUtilities.FindRoot(_Instance._TransitionProperty.TargetObject);
                if (OriginalRoot == null)
                    OriginalRoot = Settings.TrySelectBestModel();

                _Instance._Animations.NormalizedTime = 0;

                _Instance.in2DMode = _SelectedInstanceType == AnimationType.Sprite;
            }

            /************************************************************************************************************************/

            private void FocusCamera()
            {
                var bounds = CalculateBounds(InstanceRoot);

                var rotation = _Instance.in2DMode ?
                    Quaternion.identity :
                    Quaternion.Euler(35, 135, 0);

                var size = bounds.extents.magnitude * 1.5f;
                if (size == float.PositiveInfinity)
                    return;
                else if (size == 0)
                    size = 10;

                _Instance.LookAt(bounds.center, rotation, size, _Instance.in2DMode, true);
            }

            /************************************************************************************************************************/

            private static Bounds CalculateBounds(Transform transform)
            {
                var renderers = transform.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    return default;

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Execution
            /************************************************************************************************************************/

            /// <summary>Called when the window GUI is drawn.</summary>
            public void OnGUI()
            {
                if (!AnimancerEditorUtilities.IsChangingPlayMode && InstanceRoot == null)
                    InstantiateModel();

                if (_Animancer != null && _Animancer.IsGraphPlaying)
                    AnimancerGUI.RepaintEverything();

                if (Selection.activeObject == _Instance &&
                    Event.current.type == EventType.KeyUp &&
                    Event.current.keyCode == KeyCode.F)
                    FocusCamera();
            }

            /************************************************************************************************************************/

            private void OnPlayModeChanged(PlayModeStateChange change)
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        DestroyModelInstance();
                        break;
                }
            }

            /************************************************************************************************************************/

            private void OnSceneOpening(string path, OpenSceneMode mode)
            {
                if (mode == OpenSceneMode.Single)
                    DestroyModelInstance();
            }

            /************************************************************************************************************************/

            private void DoCustomGUI(SceneView sceneView)
            {
                var animancer = Animancer;
                if (animancer != null &&
                    sceneView is TransitionPreviewWindow instance &&
                    AnimancerUtilities.TryGetWrappedObject(Transition, out ITransitionGUI gui) &&
                    instance._TransitionProperty != null)
                {
                    EditorGUI.BeginChangeCheck();

                    using (TransitionDrawer.DrawerContext.Get(instance._TransitionProperty))
                    {
                        try
                        {
                            gui.OnPreviewSceneGUI(new TransitionPreviewDetails(animancer));
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                        AnimancerGUI.RepaintEverything();
                }
            }

            /************************************************************************************************************************/

            /// <summary>Is the `obj` a <see cref="GameObject"/> in the preview scene?</summary>
            public bool IsSceneObject(Object obj)
            {
                return
                    obj is GameObject gameObject &&
                    gameObject.transform.IsChildOf(PreviewSceneRoot);
            }

            /************************************************************************************************************************/

            [SerializeField]
            private List<Transform> _ExpandedHierarchy;

            /// <summary>A list of all objects with their child hierarchy expanded.</summary>
            public List<Transform> ExpandedHierarchy
            {
                get
                {
                    if (_ExpandedHierarchy == null)
                        _ExpandedHierarchy = new List<Transform>();
                    return _ExpandedHierarchy;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Cleanup
            /************************************************************************************************************************/

            /// <summary>Called by <see cref="TransitionPreviewWindow.OnDisable"/>.</summary>
            public void OnDisable()
            {
                EditorSceneManager.sceneOpening -= OnSceneOpening;
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;

                duringSceneGui -= DoCustomGUI;

                DestroyAnimancerInstance();

                EditorSceneManager.ClosePreviewScene(_Scene);
            }

            /************************************************************************************************************************/

            /// <summary>Called by <see cref="TransitionPreviewWindow.OnDestroy"/>.</summary>
            public void OnDestroy()
            {
                if (PreviewSceneRoot != null)
                {
                    DestroyImmediate(PreviewSceneRoot.gameObject);
                    PreviewSceneRoot = null;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Destroys the <see cref="InstanceRoot"/>.</summary>
            public void DestroyModelInstance()
            {
                DestroyAnimancerInstance();

                if (InstanceRoot == null)
                    return;

                DestroyImmediate(InstanceRoot.gameObject);
                InstanceRoot = null;
                InstanceAnimators = null;
            }

            /************************************************************************************************************************/

            private void DestroyAnimancerInstance()
            {
                if (_Animancer == null)
                    return;

                _Animancer.CancelPostUpdate(Animations.WindowMatchStateTime.Instance);
                _Animancer.DestroyGraph();
                _Animancer = null;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

#endif

