// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A <see cref="PlayableBehaviour"/> which can be used as a substitute for the
    /// <see cref="RuntimeAnimatorController"/> normally used to control an <see cref="Animator"/>.
    /// </summary>
    /// 
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// The most common way to access this class is via <see cref="AnimancerComponent.Playable"/>.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing">Playing Animations</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerPlayable
    /// 
    public partial class AnimancerPlayable : PlayableBehaviour,
        IEnumerator, IPlayableWrapper, IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        private static float _DefaultFadeDuration = 0.25f;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// The namespace that should be used for a class which sets the <see cref="DefaultFadeDuration"/>.
        /// </summary>
        public const string DefaultFadeDurationNamespace = nameof(Animancer);

        /// <summary>[Editor-Only]
        /// The name that should be used for a class which sets the <see cref="DefaultFadeDuration"/>.
        /// </summary>
        public const string DefaultFadeDurationClass = nameof(DefaultFadeDuration);

        /// <summary>[Editor-Only]
        /// Initializes the <see cref="DefaultFadeDuration"/> (see its example for more information).
        /// </summary>
        /// <remarks>
        /// This method takes about 2 milliseconds if a <see cref="DefaultFadeDuration"/> class exists, or 0 if it
        /// doesn't (less than 0.5 rounded off according to a <see cref="System.Diagnostics.Stopwatch"/>).
        /// <para></para>
        /// The <see cref="DefaultFadeDuration"/> can't simply be stored in the
        /// <see cref="Editor.AnimancerSettings"/> because it needs to be initialized before Unity is able to load
        /// <see cref="ScriptableObject"/>s.
        /// </remarks>
        static AnimancerPlayable()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Iterate backwards since it's more likely to be towards the end.
            for (int iAssembly = assemblies.Length - 1; iAssembly >= 0; iAssembly--)
            {
                var type = assemblies[iAssembly].GetType(DefaultFadeDurationNamespace + "." + DefaultFadeDurationClass);
                if (type != null)
                {
                    var methods = type.GetMethods(Editor.AnimancerEditorUtilities.StaticBindings);
                    for (int iMethod = 0; iMethod < methods.Length; iMethod++)
                    {
                        var method = methods[iMethod];
                        if (method.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute), false))
                        {
                            method.Invoke(null, null);
                            return;
                        }
                    }
                }
            }
        }
#endif

        /************************************************************************************************************************/

        /// <summary>The fade duration to use if not specified. Default is 0.25.</summary>
        /// <exception cref="UnityEngine.Assertions.AssertionException">The value is negative or infinity.</exception>
        /// <remarks><em>Animancer Lite doesn't allow this value to be changed in runtime builds (except to 0).</em></remarks>
        /// <example>
        /// <see cref="Sprite"/> based games often have no use for fading so you could set this value to 0 using the
        /// following script so that you don't need to manually set the <see cref="ITransition.FadeDuration"/> of all
        /// your transitions.
        /// <para></para>
        /// To set this value automatically on startup, put the following class into any script:
        /// <para></para><code>
        /// namespace Animancer
        /// {
        ///     internal static class DefaultFadeDuration
        ///     {
        ///         [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        ///         private static void Initialize() => AnimancerPlayable.DefaultFadeDuration = 0;
        ///     }
        /// }
        /// </code>
        /// Using that specific namespace (<see cref="DefaultFadeDurationNamespace"/>) and class name
        /// (<see cref="DefaultFadeDurationClass"/>) allows Animancer to find and run it immediately in the Unity
        /// Editor so that newly created transition fields can start with the correct value (using a
        /// <c>[UnityEditor.InitializeOnLoadMethod]</c> attribute would run it too late).
        /// </example>
        public static float DefaultFadeDuration
        {
            get => _DefaultFadeDuration;
            set
            {
                AnimancerUtilities.Assert(value >= 0 && value < float.PositiveInfinity,
                    $"{nameof(AnimancerPlayable)}.{nameof(DefaultFadeDuration)} must not be negative or infinity.");

                _DefaultFadeDuration = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] The <see cref="PlayableGraph"/> containing this <see cref="AnimancerPlayable"/>.</summary>
        internal PlayableGraph _Graph;

        /// <summary>[Pro-Only] The <see cref="PlayableGraph"/> containing this <see cref="AnimancerPlayable"/>.</summary>
        public PlayableGraph Graph => _Graph;

        /// <summary>[Internal] The <see cref="Playable"/> containing this <see cref="AnimancerPlayable"/>.</summary>
        internal Playable _RootPlayable;

        /// <summary>[Internal] The <see cref="Playable"/> which layers connect to.</summary>
        internal Playable _LayerMixer;

        /************************************************************************************************************************/

        /// <summary>[Internal] The <see cref="Playable"/> which layers connect to.</summary>
        Playable IPlayableWrapper.Playable => _LayerMixer;

        /// <summary>[Internal] An <see cref="AnimancerPlayable"/> is the root of the graph so it has no parent.</summary>
        IPlayableWrapper IPlayableWrapper.Parent => null;

        /// <summary>[Internal] The current blend weight of this node which determines how much it affects the final output.</summary>
        float IPlayableWrapper.Weight => 1;

        /// <summary>[Internal] The <see cref="LayerList.Count"/>.</summary>
        int IPlayableWrapper.ChildCount => Layers.Count;

        /// <summary>[Internal] Returns the layer at the specified `index`.</summary>
        AnimancerNode IPlayableWrapper.GetChild(int index) => Layers[index];

        /************************************************************************************************************************/
        // These collections can't be readonly because when Unity clones the Template it copies the memory without running the
        // field initializers on the new clone so everything would be referencing the same collections.
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerLayer"/>s which each manage their own set of animations.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">Layers</see>
        /// </remarks>
        public LayerList Layers { get; private set; }

        /// <summary>The <see cref="AnimancerState"/>s managed by this playable.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">States</see>
        /// </remarks>
        public StateDictionary States { get; private set; }

        /// <summary>All of the nodes that need to be updated.</summary>
        private Key.KeyedList<IUpdatable> _PreUpdatables;

        /// <summary>All of the objects that need to be updated early.</summary>
        private Key.KeyedList<IUpdatable> _PostUpdatables;

        /// <summary>A <see cref="PlayableBehaviour"/> that updates the <see cref="_PostUpdatables"/>.</summary>
        private PostUpdate _PostUpdate;

        /************************************************************************************************************************/

        /// <summary>The component that is playing this <see cref="AnimancerPlayable"/>.</summary>
        public IAnimancerComponent Component { get; private set; }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of times the <see cref="AnimancerLayer.CurrentState"/> has changed on layer 0. By storing this
        /// value and later comparing the stored value to the current value, you can determine whether the state has
        /// been changed since then, even it has changed back to the same state.
        /// </summary>
        public int CommandCount => Layers[0].CommandCount;

        /************************************************************************************************************************/

        /// <summary>Determines what time source is used to update the <see cref="PlayableGraph"/>.</summary>
        public DirectorUpdateMode UpdateMode
        {
            get => _Graph.GetTimeUpdateMode();
            set => _Graph.SetTimeUpdateMode(value);
        }

        /************************************************************************************************************************/

        private float _Speed = 1;

        /// <summary>How fast the <see cref="AnimancerState.Time"/> of all animations is advancing every frame.</summary>
        /// 
        /// <remarks>
        /// 1 is the normal speed.
        /// <para></para>
        /// A negative value will play the animations backwards.
        /// <para></para>
        /// Setting this value to 0 would pause all animations, but calling <see cref="PauseGraph"/> is more efficient.
        /// <para></para>
        /// <em>Animancer Lite does not allow this value to be changed in runtime builds.</em>
        /// </remarks>
        ///
        /// <example><code>
        /// void SetSpeed(AnimancerComponent animancer)
        /// {
        ///     animancer.Playable.Speed = 1;// Normal speed.
        ///     animancer.Playable.Speed = 2;// Double speed.
        ///     animancer.Playable.Speed = 0.5f;// Half speed.
        ///     animancer.Playable.Speed = -1;// Normal speed playing backwards.
        /// }
        /// </code></example>
        public float Speed
        {
            get => _Speed;
            set => _LayerMixer.SetSpeed(_Speed = value);
        }

        /************************************************************************************************************************/

        private bool _KeepChildrenConnected;

        /// <summary>
        /// Should playables stay connected to the graph at all times?
        /// Otherwise they will be disconnected when their  <see cref="AnimancerNode.Weight"/> is 0.
        /// </summary>
        /// 
        /// <remarks>
        /// Humanoid Rigs default this value to <c>false</c> so that playables will be disconnected from the graph
        /// while they are at 0 weight which stops it from evaluating them every frame.
        /// <para></para>
        /// Generic Rigs default this value to <c>true</c> because they do not always animate the same standard set of
        /// values so every connection change has a higher performance cost than with Humanoid Rigs which is generally
        /// more significant than the gains for having fewer playables connected at a time.
        /// <para></para>
        /// The default is set by <see cref="CreateOutput(Animator, IAnimancerComponent)"/>.
        /// </remarks>
        /// 
        /// <example><code>
        /// [SerializeField]
        /// private AnimancerComponent _Animancer;
        /// 
        /// public void Initialize()
        /// {
        ///     _Animancer.Playable.KeepChildrenConnected = true;
        /// }
        /// </code></example>
        public bool KeepChildrenConnected
        {
            get => _KeepChildrenConnected;
            set
            {
                if (_KeepChildrenConnected == value)
                    return;

                _KeepChildrenConnected = value;

                if (value)
                {
                    _PostUpdate.IsConnected = true;

                    for (int i = Layers.Count - 1; i >= 0; i--)
                        Layers.GetLayer(i).ConnectAllChildrenToGraph();
                }
                else
                {
                    for (int i = Layers.Count - 1; i >= 0; i--)
                        Layers.GetLayer(i).DisconnectWeightlessChildrenFromGraph();
                }
            }
        }

        /************************************************************************************************************************/

        private bool _SkipFirstFade;

        /// <summary>
        /// Normally the first animation on the Base Layer should not fade in because there is nothing fading out. But
        /// sometimes that is undesirable, such as if the <see cref="Animator.runtimeAnimatorController"/> is assigned
        /// since Animancer can blend with that.
        /// </summary>
        /// <remarks>
        /// Setting this value to false ensures that the <see cref="AnimationLayerMixerPlayable"/> has at least two
        /// inputs because it ignores the <see cref="AnimancerNode.Weight"/> of the layer when there is only one.
        /// </remarks>
        public bool SkipFirstFade
        {
            get => _SkipFirstFade;
            set
            {
                _SkipFirstFade = value;

                if (!value && Layers.Count < 2)
                {
                    Layers.Count = 1;
                    _LayerMixer.SetInputCount(2);
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="PlayableGraph"/> containing an <see cref="AnimancerPlayable"/>.
        /// <para></para>
        /// The caller is responsible for calling <see cref="DestroyGraph()"/> on the returned object, except in Edit Mode
        /// where it will be called automatically.
        /// <para></para>
        /// Consider calling <see cref="SetNextGraphName"/> before this method to give it a name.
        /// </summary>
        public static AnimancerPlayable Create()
        {
#if UNITY_EDITOR
            var name = _NextGraphName;
            _NextGraphName = null;

            var graph = name != null ?
                PlayableGraph.Create(name) :
                PlayableGraph.Create();
#else
            var graph = PlayableGraph.Create();
#endif

            return Create(graph);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Since <see cref="ScriptPlayable{T}.Create(PlayableGraph, int)"/> needs to clone an existing instance, we
        /// keep a static template to avoid allocating an extra garbage one every time. This is why the fields are
        /// assigned in <see cref="OnPlayableCreate"/> rather than being readonly with field initializers.
        /// </summary>
        private static readonly AnimancerPlayable Template = new AnimancerPlayable();

        /// <summary>Creates an <see cref="AnimancerPlayable"/> in an existing <see cref="PlayableGraph"/>.</summary>
        public static AnimancerPlayable Create(PlayableGraph graph)
            => Create(graph, Template);

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerPlayable"/> in an existing <see cref="PlayableGraph"/>.</summary>
        /// <example>
        /// When inheriting from <see cref="AnimancerPlayable"/>, it is recommended to give your class a field like the
        /// following to use as the `template` for this method:
        /// <code>
        /// private static readonly MyAnimancerPlayable Template = new MyAnimancerPlayable();
        /// </code></example>
        protected static T Create<T>(PlayableGraph graph, T template)
            where T : AnimancerPlayable, new()
            => ScriptPlayable<T>.Create(graph, template, 2)
                .GetBehaviour();

        /************************************************************************************************************************/

        /// <summary>[Internal] Called by Unity when it creates this <see cref="AnimancerPlayable"/>.</summary>
        public override void OnPlayableCreate(Playable playable)
        {
            _RootPlayable = playable;
            _Graph = playable.GetGraph();

            _PostUpdatables = new Key.KeyedList<IUpdatable>();
            _PreUpdatables = new Key.KeyedList<IUpdatable>();
            _PostUpdate = PostUpdate.Create(this);
            Layers = new LayerList(this, out _LayerMixer);
            States = new StateDictionary(this);

            playable.SetInputWeight(0, 1);

#if UNITY_EDITOR
            RegisterInstance();
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        private static string _NextGraphName;
#endif

        /// <summary>[Editor-Conditional]
        /// Sets the display name for the next <see cref="Create()"/> call to give its <see cref="PlayableGraph"/>.
        /// </summary>
        /// <remarks>
        /// Having this method separate from <see cref="Create()"/> allows the
        /// <see cref="System.Diagnostics.ConditionalAttribute"/> to compile it out of runtime builds which would
        /// otherwise require #ifs on the caller side.
        /// </remarks>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetNextGraphName(string name)
        {
#if UNITY_EDITOR
            _NextGraphName = name;
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] Returns "AnimancerPlayable (Graph Name)".</summary>
        public override string ToString()
            => $"{nameof(AnimancerPlayable)} ({(_Graph.IsValid() ? _Graph.GetEditorName() : "Graph Not Initialized")})";
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// Outputs the <see cref="PlayableOutput"/> connected to the <see cref="AnimancerPlayable"/> and returns true
        /// if it was found. Otherwise returns false.
        /// </summary>
        public bool TryGetOutput(out PlayableOutput output)
        {
            var outputCount = _Graph.GetOutputCount();
            for (int i = 0; i < outputCount; i++)
            {
                output = _Graph.GetOutput(i);
                if (output.GetSourcePlayable().Equals(_RootPlayable))
                    return true;
            }

            output = default;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Plays this playable on the <see cref="IAnimancerComponent.Animator"/> and sets the
        /// <see cref="Component"/>.
        /// </summary>
        public void CreateOutput(IAnimancerComponent animancer)
            => CreateOutput(animancer.Animator, animancer);

        /// <summary>Plays this playable on the specified `animator` and sets the <see cref="Component"/>.</summary>
        public void CreateOutput(Animator animator, IAnimancerComponent animancer)
        {
#if UNITY_ASSERTIONS
            if (animator == null)
                throw new ArgumentNullException(nameof(animator),
                    $"An {nameof(Animator)} component is required to play animations.");

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(animator))
                throw new ArgumentException(
                    $"The specified {nameof(Animator)} component is a prefab which means it cannot play animations.",
                    nameof(animator));
#endif

            if (animancer != null)
            {
                Debug.Assert(animancer.IsPlayableInitialized && animancer.Playable == this,
                    $"{nameof(CreateOutput)} was called on an {nameof(AnimancerPlayable)} which does not match the" +
                    $" {nameof(IAnimancerComponent)}.{nameof(IAnimancerComponent.Playable)}.");
                Debug.Assert(animator == animancer.Animator,
                    $"{nameof(CreateOutput)} was called with an {nameof(Animator)} which does not match the" +
                    $" {nameof(IAnimancerComponent)}.{nameof(IAnimancerComponent.Animator)}.");
            }

            if (TryGetOutput(out var output))
            {
                Debug.LogWarning(
                    $"A {nameof(PlayableGraph)} output is already connected to the {nameof(AnimancerPlayable)}." +
                    $" The old output should be destroyed using `animancerComponent.Playable.DestroyOutput();`" +
                    $" before calling {nameof(CreateOutput)}.", animator);
            }
#endif

            Component = animancer;

            var isHumanoid = animator.isHuman;

            // Generic Rigs get better performance by keeping children connected but Humanoids don't.
            KeepChildrenConnected = !isHumanoid;

            // Generic Rigs can blend with an underlying Animator Controller but Humanoids can't.
            SkipFirstFade = isHumanoid || animator.runtimeAnimatorController == null;

#pragma warning disable CS0618 // Type or member is obsolete.
            // Unity 2022 marked this method as [Obsolete] even though it's the only way to use Animate Physics mode.
            AnimationPlayableUtilities.Play(animator, _RootPlayable, _Graph);
#pragma warning restore CS0618 // Type or member is obsolete.

            _IsGraphPlaying = true;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Inserts a `playable` after the root of the <see cref="Graph"/> so that it can modify the final output.
        /// </summary>
        /// <remarks>It can be removed using <see cref="AnimancerUtilities.RemovePlayable"/>.</remarks>
        public void InsertOutputPlayable(Playable playable)
        {
            var output = _Graph.GetOutput(0);
            _Graph.Connect(output.GetSourcePlayable(), 0, playable, 0);
            playable.SetInputWeight(0, 1);
            output.SetSourcePlayable(playable);
        }

        /// <summary>[Pro-Only]
        /// Inserts an animation job after the root of the <see cref="Graph"/> so that it can modify the final output.
        /// </summary>
        /// <remarks>
        /// It can can be removed by passing the returned value into <see cref="AnimancerUtilities.RemovePlayable"/>.
        /// </remarks>
        public AnimationScriptPlayable InsertOutputJob<T>(T data) where T : struct, IAnimationJob
        {
            var playable = AnimationScriptPlayable.Create(_Graph, data, 1);
            var output = _Graph.GetOutput(0);
            _Graph.Connect(output.GetSourcePlayable(), 0, playable, 0);
            playable.SetInputWeight(0, 1);
            output.SetSourcePlayable(playable);
            return playable;
        }

        /************************************************************************************************************************/

        #endregion
        /************************************************************************************************************************/
        #region Cleanup
        /************************************************************************************************************************/

        /// <summary>Is this <see cref="AnimancerPlayable"/> currently usable (not destroyed)?</summary>
        public bool IsValid => _Graph.IsValid();

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Graph"/>. This operation cannot be undone.</summary>
        public void DestroyGraph()
        {
            if (_Graph.IsValid())
                _Graph.Destroy();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Destroys the <see cref="PlayableOutput"/> connected to this <see cref="AnimancerPlayable"/> and returns
        /// true if it was found. Otherwise returns false.
        /// </summary>
        public bool DestroyOutput()
        {
            if (TryGetOutput(out var output))
            {
                _Graph.DestroyOutput(output);
                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up the resources managed by this <see cref="AnimancerPlayable"/>.</summary>
        public override void OnPlayableDestroy(Playable playable)
        {
            var previous = Current;
            Current = this;

            DisposeAll();
            GC.SuppressFinalize(this);

            // No need to destroy every layer and state individually because destroying the graph will do so anyway.

            Layers = null;
            States = null;

            Current = previous;
        }

        /************************************************************************************************************************/

        private List<IDisposable> _Disposables;

        /// <summary>A list of objects that need to be disposed when this <see cref="AnimancerPlayable"/> is destroyed.</summary>
        /// <remarks>This list is primarily used to dispose native arrays used in Animation Jobs.</remarks>
        public List<IDisposable> Disposables => _Disposables ?? (_Disposables = new List<IDisposable>());

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IDisposable.Dispose"/> on all the <see cref="Disposables"/>.</summary>
        ~AnimancerPlayable() => DisposeAll();

        /// <summary>Calls <see cref="IDisposable.Dispose"/> on all the <see cref="Disposables"/>.</summary>
        private void DisposeAll()
        {
            if (_Disposables == null)
                return;

            var i = _Disposables.Count;
            DisposeNext:
            try
            {
                while (--i >= 0)
                {
                    _Disposables[i].Dispose();
                }

                _Disposables.Clear();
                _Disposables = null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto DisposeNext;
            }
        }

        /************************************************************************************************************************/
        #region Inverse Kinematics
        // These fields are stored here but accessed via the LayerList.
        /************************************************************************************************************************/

        private bool _ApplyAnimatorIK;

        /// <inheritdoc/>
        public bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set
            {
                _ApplyAnimatorIK = value;

                for (int i = Layers.Count - 1; i >= 0; i--)
                    Layers.GetLayer(i).ApplyAnimatorIK = value;
            }
        }

        /************************************************************************************************************************/

        private bool _ApplyFootIK;

        /// <inheritdoc/>
        public bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set
            {
                _ApplyFootIK = value;

                for (int i = Layers.Count - 1; i >= 0; i--)
                    Layers.GetLayer(i).ApplyFootIK = value;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Playing
        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IAnimancerComponent.GetKey"/> on the <see cref="Component"/>.</summary>
        /// <remarks>If the <see cref="Component"/> is null, this method returns the `clip` itself.</remarks>
        public object GetKey(AnimationClip clip) => Component != null ? Component.GetKey(clip) : clip;

        /************************************************************************************************************************/
        // Play Immediately.
        /************************************************************************************************************************/

        /// <summary>Stops all other animations on the same layer, plays the `clip`, and returns its state.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip).Time = 0;</c>.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `clip` was already playing.
        /// </remarks>
        public AnimancerState Play(AnimationClip clip)
            => Play(States.GetOrCreate(clip));

        /// <summary>Stops all other animations on the same layer, plays the `state`, and returns it.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(state).Time = 0;</c>.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// </remarks>
        public AnimancerState Play(AnimancerState state)
            => GetLocalLayer(state).Play(state);

        /************************************************************************************************************************/
        // Cross Fade.
        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` while fading out all other states in the same layer over the course of the
        /// `fadeDuration`. Returns its state.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState Play(AnimationClip clip, float fadeDuration, FadeMode mode = default)
            => Play(States.GetOrCreate(clip), fadeDuration, mode);

        /// <summary>
        /// Starts fading in the `state` while fading out all others in the same layer over the course of the
        /// `fadeDuration`. Returns the `state`.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState Play(AnimancerState state, float fadeDuration, FadeMode mode = default)
        {
            return GetLocalLayer(state).Play(state, fadeDuration, mode);
        }

        /************************************************************************************************************************/
        // Transition.
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on the <see cref="ITransition.FadeDuration"/>.
        /// </summary>
        /// <remarks>
        /// This method is safe to call repeatedly without checking whether the `transition` was already playing.
        /// </remarks>
        public AnimancerState Play(ITransition transition)
            => Play(transition, transition.FadeDuration, transition.FadeMode);

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on the <see cref="ITransition.FadeDuration"/>.
        /// </summary>
        /// <remarks>
        /// This method is safe to call repeatedly without checking whether the `transition` was already playing.
        /// </remarks>
        public AnimancerState Play(ITransition transition, float fadeDuration, FadeMode mode = default)
        {
            var state = States.GetOrCreate(transition);
            state = Play(state, fadeDuration, mode);
            transition.Apply(state);
            return state;
        }

        /************************************************************************************************************************/
        // Try Play.
        /************************************************************************************************************************/

        /// <summary>
        /// Stops all other animations on the same layer, plays the animation registered with the `key`, and returns
        /// that state. Or if no state is registered with that `key`, this method does nothing and returns null.
        /// </summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// If you wish to force it back to the start, you can simply set the returned state's time to 0.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        public AnimancerState TryPlay(object key)
            => States.TryGet(key, out var state) ? Play(state) : null;

        /// <summary>
        /// Starts fading in the animation registered with the `key` while fading out all others in the same layer
        /// over the course of the `fadeDuration`. Or if no state is registered with that `key`, this method does
        /// nothing and returns null.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        public AnimancerState TryPlay(object key, float fadeDuration, FadeMode mode = default)
            => States.TryGet(key, out var state) ? Play(state, fadeDuration, mode) : null;

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="AnimancerNode.Layer"/> if the <see cref="AnimancerNode.Root"/> is this.
        /// Otherwise returns the first layer in this graph.
        /// </summary>
        private AnimancerLayer GetLocalLayer(AnimancerState state)
        {
            if (state.Root == this)
            {
                var layer = state.Layer;
                if (layer != null)
                    return layer;
            }

            return Layers[0];
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the state registered with the <see cref="IHasKey.Key"/>, stops and rewinds it to the start, then
        /// returns it.
        /// </summary>
        public AnimancerState Stop(IHasKey hasKey) => Stop(hasKey.Key);

        /// <summary>
        /// Calls <see cref="AnimancerState.Stop"/> on the state registered with the `key` to stop it from playing and
        /// rewind it to the start.
        /// </summary>
        public AnimancerState Stop(object key)
        {
            if (States.TryGet(key, out var state))
                state.Stop();

            return state;
        }

        /// <summary>
        /// Calls <see cref="AnimancerState.Stop"/> on all animations to stop them from playing and rewind them to the
        /// start.
        /// </summary>
        public void Stop()
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
                Layers.GetLayer(i).Stop();
        }

        /************************************************************************************************************************/

        /// <summary>Is a state registered with the <see cref="IHasKey.Key"/> and currently playing?</summary>
        public bool IsPlaying(IHasKey hasKey) => IsPlaying(hasKey.Key);

        /// <summary>Is a state registered with the `key` and currently playing?</summary>
        public bool IsPlaying(object key) => States.TryGet(key, out var state) && state.IsPlaying;

        /// <summary>Is least one animation being played?</summary>
        public bool IsPlaying()
        {
            if (!_IsGraphPlaying)
                return false;

            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                if (Layers.GetLayer(i).IsAnyStatePlaying())
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the `clip` is currently being played by at least one state in the specified layer.
        /// <para></para>
        /// This method is inefficient because it searches through every state to find any that are playing the `clip`,
        /// unlike <see cref="IsPlaying(object)"/> which only checks the state registered using the specified key.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip)
        {
            if (!_IsGraphPlaying)
                return false;

            for (int i = Layers.Count - 1; i >= 0; i--)
                if (Layers.GetLayer(i).IsPlayingClip(clip))
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the total <see cref="AnimancerNode.Weight"/> of all states in all layers.</summary>
        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = Layers.Count - 1; i >= 0; i--)
                weight += Layers.GetLayer(i).GetTotalWeight();

            return weight;
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in all layers.</summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips) => Layers.GatherAnimationClips(clips);

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
        /************************************************************************************************************************/

        /// <summary>Are any animations playing?</summary>
        /// <remarks>This allows this object to be used as a custom yield instruction.</remarks>
        bool IEnumerator.MoveNext()
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
                if (Layers.GetLayer(i).IsPlayingAndNotEnding())
                    return true;

            return false;
        }

        /// <summary>Returns null.</summary>
        object IEnumerator.Current => null;

        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }

        /************************************************************************************************************************/
        #region Key Error Methods
#if UNITY_EDITOR
        /************************************************************************************************************************/
        // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
        // AnimancerState as a key, since the whole point of a key is to identify a state in the first place.
        /************************************************************************************************************************/

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just call <see cref="AnimancerState.Stop"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Stop().", true)]
        public AnimancerState Stop(AnimancerState key)
        {
            key.Stop();
            return key;
        }

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just check <see cref="AnimancerState.IsPlaying"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just check AnimancerState.IsPlaying.", true)]
        public bool IsPlaying(AnimancerState key) => key.IsPlaying;

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Evaluation
        /************************************************************************************************************************/

        private bool _IsGraphPlaying = true;

        /// <summary>Indicates whether the <see cref="PlayableGraph"/> is currently playing.</summary>
        public bool IsGraphPlaying
        {
            get => _IsGraphPlaying;
            set
            {
                if (value)
                    UnpauseGraph();
                else
                    PauseGraph();
            }
        }

        /// <summary>
        /// Resumes playing the <see cref="PlayableGraph"/> if <see cref="PauseGraph"/> was called previously.
        /// </summary>
        public void UnpauseGraph()
        {
            if (!_IsGraphPlaying)
            {
                _Graph.Play();
                _IsGraphPlaying = true;

#if UNITY_EDITOR
                // In Edit Mode, unpausing the graph does not work properly unless we force it to change.
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    Evaluate(Time.maximumDeltaTime);
#endif
            }
        }

        /// <summary>
        /// Freezes the <see cref="PlayableGraph"/> at its current state.
        /// <para></para>
        /// If you call this method, you are responsible for calling <see cref="UnpauseGraph"/> to resume playing.
        /// </summary>
        public void PauseGraph()
        {
            if (_IsGraphPlaying)
            {
                _Graph.Stop();
                _IsGraphPlaying = false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Evaluates all of the currently playing animations to apply their states to the animated objects.
        /// </summary>
        public void Evaluate() => _Graph.Evaluate();

        /// <summary>
        /// Advances all currently playing animations by the specified amount of time (in seconds) and evaluates the
        /// graph to apply their states to the animated objects.
        /// </summary>
        public void Evaluate(float deltaTime) => _Graph.Evaluate(deltaTime);

        /************************************************************************************************************************/

        /// <summary>Returns a detailed descrption of all currently playing states and other registered states.</summary>
        public string GetDescription()
        {
            var text = ObjectPool.AcquireStringBuilder();
            AppendDescription(text);
            return text.ReleaseToString();
        }

        /// <summary>Appends a detailed descrption of all currently playing states and other registered states.</summary>
        public void AppendDescription(StringBuilder text)
        {
            text.Append($"{nameof(AnimancerPlayable)} (")
                .Append(Component)
                .Append(") Layer Count: ")
                .Append(Layers.Count);

            const string separator = "\n    ";
            AnimancerNode.AppendIKDetails(text, separator, this);

            var count = Layers.Count;
            for (int i = 0; i < count; i++)
            {
                text.Append(separator);
                Layers[i].AppendDescription(text, separator);
            }

            text.AppendLine();
            AppendInternalDetails(text, Strings.Indent, Strings.Indent + Strings.Indent);
        }

        /// <summary>Appends all registered <see cref="IUpdatable"/>s and <see cref="IDisposable"/>s.</summary>
        public void AppendInternalDetails(StringBuilder text, string sectionPrefix, string itemPrefix)
        {
            AppendAll(text, sectionPrefix, itemPrefix, _PreUpdatables, "Pre Updatables");
            text.AppendLine();
            AppendAll(text, sectionPrefix, itemPrefix, _PostUpdatables, "Post Updatables");
            text.AppendLine();
            AppendAll(text, sectionPrefix, itemPrefix, _Disposables, "Disposables");
        }

        private static void AppendAll(StringBuilder text, string sectionPrefix, string itemPrefix, ICollection collection, string name)
        {
            var count = collection != null ? collection.Count : 0;
            text.Append(sectionPrefix).Append(name).Append(": ").Append(count);
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    text.AppendLine().Append(itemPrefix).Append(item);
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Update
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Adds the `updatable` to the list that need to be updated before the playables if it was not there already.
        /// </summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The execution order is non-deterministic. Specifically, the most recently added will be updated first and
        /// <see cref="CancelPreUpdate"/> will change the order by swapping the last one into the place of the removed
        /// object.
        /// </remarks>
        public void RequirePreUpdate(IUpdatable updatable)
        {
#if UNITY_ASSERTIONS
            if (updatable is AnimancerNode node)
            {
                Validate.AssertPlayable(node);
                Validate.AssertRoot(node, this);
            }
#endif

            _PreUpdatables.AddNew(updatable);
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Adds the `updatable` to the list that need to be updated after the playables if it was not there already.
        /// </summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The execution order is non-deterministic. Specifically, the most recently added will be updated first and
        /// <see cref="CancelPostUpdate"/> will change the order by swapping the last one into the place of the removed
        /// object.
        /// </remarks>
        public void RequirePostUpdate(IUpdatable updatable)
        {
#if UNITY_ASSERTIONS
            if (updatable is AnimancerNode node)
            {
                Validate.AssertPlayable(node);
                Validate.AssertRoot(node, this);
            }
#endif

            _PostUpdatables.AddNew(updatable);
        }

        /************************************************************************************************************************/

        /// <summary>Removes the `updatable` from the `updatables`.</summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed so that the rest of them do not need to
        /// be moved down one place to fill the gap. This is more efficient, but means that the update order can change.
        /// </remarks>
        private void CancelUpdate(Key.KeyedList<IUpdatable> updatables, IUpdatable updatable)
        {
            var index = updatables.IndexOf(updatable);
            if (index < 0)
                return;

            updatables.RemoveAtSwap(index);

            if (_CurrentUpdatable < index && updatables == _CurrentUpdatables)
                _CurrentUpdatable--;
        }

        /// <summary>Removes the `updatable` from the list of objects that need to be updated before the playables.</summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed so that the rest of them do not need to
        /// be moved down one place to fill the gap. This is more efficient, but means that the update order can change.
        /// </remarks>
        public void CancelPreUpdate(IUpdatable updatable) => CancelUpdate(_PreUpdatables, updatable);

        /// <summary>Removes the `updatable` from the list of objects that need to be updated after the playebles.</summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed so that the rest of them do not need to
        /// be moved down one place to fill the gap. This is more efficient, but means that the update order can change.
        /// </remarks>
        public void CancelPostUpdate(IUpdatable updatable) => CancelUpdate(_PostUpdatables, updatable);

        /************************************************************************************************************************/

        /// <summary>The number of objects that have been registered by <see cref="RequirePreUpdate"/>.</summary>
        public int PreUpdatableCount => _PreUpdatables.Count;

        /// <summary>The number of objects that have been registered by <see cref="RequirePostUpdate"/>.</summary>
        public int PostUpdatableCount => _PostUpdatables.Count;

        /************************************************************************************************************************/

        /// <summary>Returns the object registered by <see cref="RequirePreUpdate"/> at the specified `index`.</summary>
        public IUpdatable GetPreUpdatable(int index) => _PreUpdatables[index];

        /// <summary>Returns the object registered by <see cref="RequirePostUpdate"/> at the specified `index`.</summary>
        public IUpdatable GetPostUpdatable(int index) => _PostUpdatables[index];

        /************************************************************************************************************************/

        /// <summary>The object currently executing <see cref="PrepareFrame"/>.</summary>
        public static AnimancerPlayable Current { get; private set; }

        /// <summary>The current <see cref="FrameData.deltaTime"/>.</summary>
        /// <remarks>After <see cref="PrepareFrame"/>, this property will be left at its most recent value.</remarks>
        public static float DeltaTime { get; private set; }

        /// <summary>The current <see cref="FrameData.frameId"/>.</summary>
        /// <remarks>
        /// After <see cref="PrepareFrame"/>, this property will be left at its most recent value.
        /// <para></para>
        /// <see cref="AnimancerState.Time"/> uses this value to determine whether it has accessed the playable's time
        /// since it was last updated in order to cache its value.
        /// </remarks>
        public ulong FrameID { get; private set; }

        /// <summary>The list <see cref="IUpdatable"/>s currently being updated.</summary>
        private static Key.KeyedList<IUpdatable> _CurrentUpdatables;

        /// <summary>The index of the <see cref="IUpdatable"/> currently being updated.</summary>
        private static int _CurrentUpdatable = -1;

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Calls <see cref="IUpdatable.Update"/> on everything registered using <see cref="RequirePreUpdate"/>.
        /// </summary>
        /// <remarks>
        /// Called by the <see cref="PlayableGraph"/> before the rest of the <see cref="Playable"/>s are evaluated.
        /// </remarks>
        public override void PrepareFrame(Playable playable, FrameData info)
        {
#if UNITY_ASSERTIONS
            if (OptionalWarning.AnimatorSpeed.IsEnabled() && Component != null)
            {
                var animator = Component.Animator;
                if (animator != null &&
                    animator.speed != 1 &&
                    animator.runtimeAnimatorController == null)
                {
                    animator.speed = 1;
                    OptionalWarning.AnimatorSpeed.Log(
                        $"{nameof(Animator)}.{nameof(Animator.speed)} does not affect {nameof(Animancer)}." +
                        $" Use {nameof(AnimancerPlayable)}.{nameof(Speed)} instead.", animator);
                }
            }
#endif

            UpdateAll(_PreUpdatables, info.deltaTime * info.effectiveParentSpeed);

            if (!_KeepChildrenConnected)
                _PostUpdate.IsConnected = _PostUpdatables.Count != 0;

            // Any time before or during this method will still have all Playables at their time from last frame, so we
            // don't want them to think their time is dirty until we are done.
            FrameID = info.frameId;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IUpdatable.Update"/> on each of the updatables`.</summary>
        private void UpdateAll(Key.KeyedList<IUpdatable> updatables, float deltaTime)
        {
            var previous = Current;
            Current = this;

            var previousUpdatables = _CurrentUpdatables;
            _CurrentUpdatables = updatables;

            DeltaTime = deltaTime;

            var previousUpdatable = _CurrentUpdatable;
            _CurrentUpdatable = updatables.Count;
            ContinueNodeLoop:
            try
            {
                while (--_CurrentUpdatable >= 0)
                {
                    updatables[_CurrentUpdatable].Update();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto ContinueNodeLoop;
            }
            _CurrentUpdatable = previousUpdatable;

            _CurrentUpdatables = previousUpdatables;
            Current = previous;
        }

        /************************************************************************************************************************/
        #region Post Update
        /************************************************************************************************************************/

        /// <summary>Indicates whether the internal <see cref="PostUpdate"/> is currently executing.</summary>
        public static bool IsRunningPostUpdate(AnimancerPlayable animancer) => _CurrentUpdatables == animancer._PostUpdatables;

        /************************************************************************************************************************/

        /// <summary>
        /// A <see cref="PlayableBehaviour"/> which connects to a later port than the main layer mixer so that its
        /// <see cref="PrepareFrame"/> method gets called after all other playables are updated in order to call
        /// <see cref="IUpdatable.Update"/> on the <see cref="_PostUpdatables"/>.
        /// </summary>
        private class PostUpdate : PlayableBehaviour
        {
            /************************************************************************************************************************/

            /// <summary>See <see cref="AnimancerPlayable.Template"/>.</summary>
            private static readonly PostUpdate Template = new PostUpdate();

            /// <summary>The <see cref="AnimancerPlayable"/> this behaviour is connected to.</summary>
            private AnimancerPlayable _Root;

            /// <summary>The underlying <see cref="Playable"/> of this behaviour.</summary>
            private Playable _Playable;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="PostUpdate"/> for the `root`.</summary>
            public static PostUpdate Create(AnimancerPlayable root)
            {
                var instance = ScriptPlayable<PostUpdate>.Create(root._Graph, Template, 0)
                    .GetBehaviour();
                instance._Root = root;
                return instance;
            }

            /************************************************************************************************************************/

            /// <summary>[Internal] Called by Unity when it creates this <see cref="AnimancerPlayable"/>.</summary>
            public override void OnPlayableCreate(Playable playable) => _Playable = playable;

            /************************************************************************************************************************/

            private bool _IsConnected;

            /// <summary>
            /// Indicates whether this behaviour is connected to the <see cref="PlayableGraph"/> and thus, whether it
            /// will receive <see cref="PrepareFrame"/> calls.
            /// </summary>
            public bool IsConnected
            {
                get => _IsConnected;
                set
                {
                    if (value)
                    {
                        if (!_IsConnected)
                        {
                            _IsConnected = true;
                            _Root._Graph.Connect(_Playable, 0, _Root._RootPlayable, 1);
                        }
                    }
                    else
                    {
                        if (_IsConnected)
                        {
                            _IsConnected = false;
                            _Root._Graph.Disconnect(_Root._RootPlayable, 1);
                        }
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Internal]
            /// Calls <see cref="IUpdatable.Update"/> on everything registered using <see cref="RequirePostUpdate"/>.
            /// </summary>
            /// <remarks>
            /// Called by the <see cref="PlayableGraph"/> after the rest of the <see cref="Playable"/>s are evaluated.
            /// </remarks>
            public override void PrepareFrame(Playable playable, FrameData info)
            {
                _Root.UpdateAll(_Root._PostUpdatables, info.deltaTime * info.effectiveParentSpeed);

                // Ideally we would be able to update the dirty nodes here instead of in the early update so that they
                // can respond immediately to the effects of the post update.

                // However, doing that with KeepChildrenConnected == false causes problems where states that aren't
                // connected early (before they update) don't affect the output even though weight changes do apply. So
                // in the first frame when cross fading to a new animation it will lower the weight of the previous
                // state a bit without the corresponding increase to the new animation's weight having any effect,
                // giving a total weight less than 1 and thus an incorrect output.
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Editor
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private static List<AnimancerPlayable> _AllInstances;

        /// <summary>[Editor-Only]
        /// Registers this object in the list of things that need to be cleaned up in Edit Mode.
        /// </summary>
        private void RegisterInstance()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (_AllInstances == null)
            {
                _AllInstances = new List<AnimancerPlayable>();
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += () =>
                {
                    for (int i = _AllInstances.Count - 1; i >= 0; i--)
                    {
                        var playable = _AllInstances[i];
                        if (playable.IsValid)
                            playable.DestroyGraph();
                    }

                    _AllInstances.Clear();
                };
            }
            else// Clear out any old instances.
            {
                for (int i = _AllInstances.Count - 1; i >= 0; i--)
                {
                    var playable = _AllInstances[i];
                    if (!playable.ShouldStayAlive())
                    {
                        if (playable.IsValid)
                            playable.DestroyGraph();

                        _AllInstances.RemoveAt(i);
                    }
                }
            }

            _AllInstances.Add(this);
        }

        /************************************************************************************************************************/

        /// <summary>Should this playable should stay alive instead of being destroyed?</summary>
        private bool ShouldStayAlive()
        {
            if (!IsValid)
                return false;

            if (Component == null)
                return true;

            if (Component is Object obj && obj == null)
                return false;

            if (Component.Animator == null)
                return false;

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Returns true if the `initial` mode was <see cref="AnimatorUpdateMode.AnimatePhysics"/> and the `current`
        /// has changed to another mode or if the `initial` mode was something else and the `current` has changed to
        /// <see cref="AnimatorUpdateMode.AnimatePhysics"/>.
        /// </summary>
        public static bool HasChangedToOrFromAnimatePhysics(AnimatorUpdateMode? initial, AnimatorUpdateMode current)
        {
            if (initial == null)
                return false;

#if UNITY_2023_1_OR_NEWER
            var wasAnimatePhysics = initial.Value == AnimatorUpdateMode.Fixed;
            var isAnimatePhysics = current == AnimatorUpdateMode.Fixed;
#else
            var wasAnimatePhysics = initial.Value == AnimatorUpdateMode.AnimatePhysics;
            var isAnimatePhysics = current == AnimatorUpdateMode.AnimatePhysics;
#endif

            return wasAnimatePhysics != isAnimatePhysics;
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

