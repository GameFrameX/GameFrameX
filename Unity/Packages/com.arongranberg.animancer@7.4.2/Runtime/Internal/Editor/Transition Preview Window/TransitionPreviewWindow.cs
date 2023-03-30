// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// An <see cref="EditorWindow"/> which allows the user to preview animation transitions separately from the rest
    /// of the scene in Edit Mode or Play Mode.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionPreviewWindow
    /// 
    [HelpURL(Strings.DocsURLs.TransitionPreviews)]
#if UNITY_2020_1_OR_NEWER
    [EditorWindowTitle]// Prevent the base SceneView from trying to use this type name to find the icon.
#endif
    public partial class TransitionPreviewWindow : SceneView
    {
        /************************************************************************************************************************/
        #region Public API
        /************************************************************************************************************************/

        private static Texture _Icon;

        /// <summary>The icon image used by this window.</summary>
        public static Texture Icon
        {
            get
            {
                if (_Icon == null)
                {
                    // Possible icons: "UnityEditor.LookDevView", "SoftlockInline", "ViewToolOrbit", "ClothInspector.ViewValue".
                    var name = EditorGUIUtility.isProSkin ? "ViewToolOrbit On" : "ViewToolOrbit";

                    _Icon = AnimancerGUI.LoadIcon(name);
                    if (_Icon == null)
                        _Icon = EditorGUIUtility.whiteTexture;
                }

                return _Icon;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Focusses the <see cref="TransitionPreviewWindow"/> or creates one if none exists.
        /// Or closes the existing window if it was already previewing the `transitionProperty`.
        /// </summary>
        public static void OpenOrClose(SerializedProperty transitionProperty)
        {
            transitionProperty = transitionProperty.Copy();

            EditorApplication.delayCall += () =>
            {
                if (!IsPreviewing(transitionProperty))
                {
                    // To avoid Unity giving a warning about camera rotation in 2D Mode:
                    // Set all scene views to not 2D mode and store their previous state.
                    var sceneViews = SceneView.sceneViews.ToArray();
                    var was2D = new bool[sceneViews.Length];
                    for (int i = 0; i < sceneViews.Length; i++)
                    {
                        var sceneView = (SceneView)sceneViews[i];
                        was2D[i] = sceneView.in2DMode;
                        sceneView.in2DMode = false;
                    }

                    GetWindow<TransitionPreviewWindow>(typeof(SceneView))
                        .SetTargetProperty(transitionProperty);

                    // Then after opening the window immediately return each scene view back to its previous state.
                    for (int i = 0; i < sceneViews.Length; i++)
                    {
                        var sceneView = (SceneView)sceneViews[i];
                        sceneView.in2DMode = was2D[i];
                    }
                }
                else
                {
                    _Instance.Close();
                }
            };
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimancerState.NormalizedTime"/> of the current transition. Can only be set if the property
        /// being previewed matches the current <see cref="TransitionDrawer.Context"/>.
        /// </summary>
        public static float PreviewNormalizedTime
        {
            get => _Instance._Animations.NormalizedTime;
            set
            {
                if (value.IsFinite() &&
                    IsPreviewingCurrentProperty())
                    _Instance._Animations.NormalizedTime = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="AnimancerState"/> of the current transition if the property being previewed matches
        /// the <see cref="TransitionDrawer.Context"/>. Otherwise returns null.
        /// </summary>
        public static AnimancerState GetCurrentState()
        {
            if (!IsPreviewingCurrentProperty() ||
                _Instance._Scene.Animancer == null)
                return null;

            _Instance._Scene.Animancer.States.TryGet(Transition, out var state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Is the current <see cref="TransitionDrawer.DrawerContext.Property"/> being previewed at the moment?
        /// </summary>
        public static bool IsPreviewingCurrentProperty()
        {
            return
                TransitionDrawer.Context != null &&
                IsPreviewing(TransitionDrawer.Context.Property);
        }

        /// <summary>Is the `property` being previewed at the moment?</summary>
        public static bool IsPreviewing(SerializedProperty property)
        {
            return
                _Instance != null &&
                _Instance._TransitionProperty.IsValid() &&
                Serialization.AreSameProperty(property, _Instance._TransitionProperty);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Messages
        /************************************************************************************************************************/

        private static TransitionPreviewWindow _Instance;

        [SerializeField] private Object[] _PreviousSelection;
        [SerializeField] private Animations _Animations;
        [SerializeField] private Scene _Scene;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnEnable()
        {
            _Instance = this;

#if ! UNITY_2020_1_OR_NEWER
            // Unity 2019 logs an error message when opening this window.
            // This is because the base SceneView has a [EditorWindowTitle] attribute which looks for the icon by
            // name, but since it's internal before Unity 2020 we can't replace it to prevent it from doing so.
            // Error: Unable to load the icon: 'Animancer.Editor.TransitionPreviewWindow'.
            using (BlockAllLogs.Activate())
#endif
            {
                base.OnEnable();
            }

            name = "Transition Preview Window";
            titleContent = new GUIContent("Transition Preview", Icon);
            autoRepaintOnSceneChange = true;
            sceneViewState.showSkybox = Settings.ShowSkybox;
            sceneLighting = Settings.SceneLighting;

            if (_Scene == null)
                _Scene = new Scene();
            if (_Animations == null)
                _Animations = new Animations();

            if (_TransitionProperty.IsValid() &&
                !CanBePreviewed(_TransitionProperty))
            {
                DestroyTransitionProperty();
            }

            _Scene.OnEnable();

            Selection.selectionChanged += OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload += DeselectPreviewSceneObjects;

            // Re-select next frame.
            // This fixes an issue where the Inspector header displays differently after a domain reload.
            if (Selection.activeObject == this)
            {
                Selection.activeObject = null;
                EditorApplication.delayCall += () => Selection.activeObject = this;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnDisable()
        {
            base.OnDisable();
            _Scene.OnDisable();
            _Instance = null;
            Selection.selectionChanged -= OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= DeselectPreviewSceneObjects;
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up this window.</summary>
        protected virtual new void OnDestroy()
        {
            base.OnDestroy();
            _Scene.OnDestroy();
            DestroyTransitionProperty();

            using (ObjectPool.Disposable.AcquireList<Object>(out var objects))
            {
                for (int i = 0; i < _PreviousSelection.Length; i++)
                {
                    var obj = _PreviousSelection[i];
                    if (obj != null)
                        objects.Add(obj);
                }
                Selection.objects = objects.ToArray();
            }

            _TransitionProperty = null;

            AnimancerGUI.RepaintEverything();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
#if UNITY_2021_2_OR_NEWER
        protected override void OnSceneGUI()
#else
        protected override void OnGUI()
#endif
        {
            _Instance = this;

#if UNITY_2021_2_OR_NEWER
            base.OnSceneGUI();
#else
            base.OnGUI();
#endif

            _Scene.OnGUI();

            Settings.ShowSkybox = sceneViewState.showSkybox;
            Settings.SceneLighting = sceneLighting;
        }

        /************************************************************************************************************************/

        /// <summary>Called multiple times per second while this window is visible.</summary>
        private void Update()
        {
            if (Selection.activeObject == null)
                Selection.activeObject = this;

            if (Settings.AutoClose && !_TransitionProperty.IsValid())
            {
                Close();
                return;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns false.</summary>
        /// <remarks>Returning true makes it draw the main scene instead of the custom scene in Unity 2020.</remarks>
        protected override bool SupportsStageHandling() => false;

        /************************************************************************************************************************/

        private void OnSelectionChanged()
        {
            if (Selection.activeObject == null)
                EditorApplication.delayCall += () => Selection.activeObject = this;
        }

        /************************************************************************************************************************/

        private void DeselectPreviewSceneObjects()
        {
            using (ObjectPool.Disposable.AcquireList<Object>(out var objects))
            {
                var selection = Selection.objects;
                for (int i = 0; i < selection.Length; i++)
                {
                    var obj = selection[i];
                    if (!_Scene.IsSceneObject(obj))
                        objects.Add(obj);
                }
                Selection.objects = objects.ToArray();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition Property
        /************************************************************************************************************************/

        [SerializeField]
        private Serialization.PropertyReference _TransitionProperty;

        /// <summary>The <see cref="SerializedProperty"/> currently being previewed.</summary>
        public static SerializedProperty TransitionProperty => _Instance._TransitionProperty;

        /************************************************************************************************************************/

        /// <summary>The <see cref="ITransitionDetailed"/> currently being previewed.</summary>
        public static ITransitionDetailed Transition
        {
            get
            {
                var property = _Instance._TransitionProperty;
                if (!property.IsValid())
                    return null;

                return property.Property.GetValue<ITransitionDetailed>();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether the `property` is able to be previewed by this system.</summary>
        public static bool CanBePreviewed(SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            if (accessor == null)
                return false;

            var type = accessor.GetFieldElementType(property);
            if (typeof(ITransitionDetailed).IsAssignableFrom(type))
                return true;

            var value = accessor.GetValue(property);
            return
                value != null &&
                typeof(ITransitionDetailed).IsAssignableFrom(value.GetType());
        }

        /************************************************************************************************************************/

        private void SetTargetProperty(SerializedProperty property)
        {
            if (property.serializedObject.targetObjects.Length != 1)
            {
                Close();
                throw new ArgumentException($"{nameof(TransitionPreviewWindow)} does not support multi-object selection.");
            }

            if (!CanBePreviewed(property))
            {
                Close();
                throw new ArgumentException($"The specified property does not implement {nameof(ITransitionDetailed)}.");
            }

            if (!_TransitionProperty.IsValid())
                _PreviousSelection = Selection.objects;
            Selection.activeObject = this;

            DestroyTransitionProperty();

            _TransitionProperty = property;
            _Scene.OnTargetPropertyChanged();
        }

        /************************************************************************************************************************/

        private void DestroyTransitionProperty()
        {
            if (_TransitionProperty == null)
                return;

            _Scene.DestroyModelInstance();

            _TransitionProperty.Dispose();
            _TransitionProperty = null;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Error Intercepts
#if !UNITY_2020_1_OR_NEWER
        /************************************************************************************************************************/

        /// <summary>Prevents log messages between <see cref="Activate"/> and <see cref="IDisposable.Dispose"/>.</summary>
        private class BlockAllLogs : IDisposable, ILogHandler
        {
            private static readonly BlockAllLogs Instance = new BlockAllLogs();

            private ILogHandler _PreviousHandler;

            public static IDisposable Activate()
            {
                AnimancerUtilities.Assert(Instance._PreviousHandler == null,
                    $"{nameof(BlockAllLogs)} can't be used recursively.");

                Instance._PreviousHandler = Debug.unityLogger.logHandler;
                Debug.unityLogger.logHandler = Instance;
                return Instance;
            }

            void IDisposable.Dispose()
            {
                Debug.unityLogger.logHandler = _PreviousHandler;
                _PreviousHandler = null;
            }

            void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args) { }

            void ILogHandler.LogException(Exception exception, Object context) { }
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

