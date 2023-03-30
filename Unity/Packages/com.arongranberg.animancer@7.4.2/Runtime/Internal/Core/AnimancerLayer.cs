// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// A layer on which animations can play with their states managed independantly of other layers while blending the
    /// output with those layers.
    /// </summary>
    ///
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">Layers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerLayer
    /// 
    public sealed class AnimancerLayer : AnimancerNode, IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>[Internal] Creates a new <see cref="AnimancerLayer"/>.</summary>
        internal AnimancerLayer(AnimancerPlayable root, int index)
        {

            Root = root;
            Index = index;
            CreatePlayable();

            if (ApplyParentAnimatorIK)
                _ApplyAnimatorIK = root.ApplyAnimatorIK;
            if (ApplyParentFootIK)
                _ApplyFootIK = root.ApplyFootIK;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationMixerPlayable"/> managed by this layer.</summary>
        protected override void CreatePlayable(out Playable playable)
            => playable = AnimationMixerPlayable.Create(Root._Graph);

        /************************************************************************************************************************/

        /// <summary>A layer is its own root.</summary>
        public override AnimancerLayer Layer => this;

        /// <summary>The <see cref="AnimancerNode.Root"/> receives the output of the <see cref="Playable"/>.</summary>
        public override IPlayableWrapper Parent => Root;

        /// <summary>Indicates whether child playables should stay connected to this layer at all times.</summary>
        public override bool KeepChildrenConnected => Root.KeepChildrenConnected;

        /************************************************************************************************************************/

        /// <summary>All of the animation states connected to this layer.</summary>
        private readonly List<AnimancerState> States = new List<AnimancerState>();

        /************************************************************************************************************************/

        private AnimancerState _CurrentState;

        /// <summary>The state of the animation currently being played.</summary>
        /// <remarks>
        /// Specifically, this is the state that was most recently started using any of the Play or CrossFade methods
        /// on this layer. States controlled individually via methods in the <see cref="AnimancerState"/> itself will
        /// not register in this property.
        /// <para></para>
        /// Each time this property changes, the <see cref="CommandCount"/> is incremented.
        /// </remarks>
        public AnimancerState CurrentState
        {
            get => _CurrentState;
            private set
            {
                _CurrentState = value;
                CommandCount++;
            }
        }

        /// <summary>
        /// The number of times the <see cref="CurrentState"/> has changed. By storing this value and later comparing
        /// the stored value to the current value, you can determine whether the state has been changed since then,
        /// even it has changed back to the same state.
        /// </summary>
        public int CommandCount { get; private set; }

#if UNITY_EDITOR
        /// <summary>[Editor-Only] [Internal] Increases the <see cref="CommandCount"/> by 1.</summary>
        internal void IncrementCommandCount() => CommandCount++;
#endif

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Determines whether this layer is set to additive blending. Otherwise it will override any earlier layers.
        /// </summary>
        public bool IsAdditive
        {
            get => Root.Layers.IsAdditive(Index);
            set => Root.Layers.SetAdditive(Index, value);
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Sets an <see cref="AvatarMask"/> to determine which bones this layer will affect.
        /// </summary>
        public void SetMask(AvatarMask mask)
        {
            Root.Layers.SetMask(Index, mask);
        }

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] The <see cref="AvatarMask"/> that determines which bones this layer will affect.</summary>
        internal AvatarMask _Mask;
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// The average velocity of the root motion of all currently playing animations, taking their current
        /// <see cref="AnimancerNode.Weight"/> into account.
        /// </summary>
        public Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = States.Count - 1; i >= 0; i--)
                {
                    var state = States[i];
                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Child States
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int ChildCount => States.Count;

        /// <summary>Returns the state connected to the specified `index` as a child of this layer.</summary>
        /// <remarks>This method is identical to <see cref="this[int]"/>.</remarks>
        public override AnimancerState GetChild(int index) => States[index];

        /// <summary>Returns the state connected to the specified `index` as a child of this layer.</summary>
        /// <remarks>This indexer is identical to <see cref="GetChild(int)"/>.</remarks>
        public AnimancerState this[int index] => States[index];

        /************************************************************************************************************************/

        /// <summary>Adds a new port and uses <see cref="AnimancerState.SetParent"/> to connect the `state` to it.</summary>
        public void AddChild(AnimancerState state)
        {
            if (state.Parent == this)
                return;

            // Set the root before expanding the States list in case it throws an exception.
            state.SetRoot(Root);

            var index = States.Count;
            States.Add(null);// OnAddChild will assign the state.
            _Playable.SetInputCount(index + 1);
            state.SetParent(this, index);
        }

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this layer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnAddChild(AnimancerState state) => OnAddChild(States, state);

        /************************************************************************************************************************/

        /// <summary>Disconnects the `state` from this layer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnRemoveChild(AnimancerState state)
        {
            var index = state.Index;
            Validate.AssertCanRemoveChild(state, States, States.Count);

            if (_Playable.GetInput(index).IsValid())
                Root._Graph.Disconnect(_Playable, index);

            // Swap the last state into the place of the one that was just removed.
            var last = States.Count - 1;
            if (index < last)
            {
                state = States[last];
                state.DisconnectFromGraph();

                States[index] = state;
                state.Index = index;

                if (state.Weight != 0 || Root.KeepChildrenConnected)
                    state.ConnectToGraph();
            }

            States.RemoveAt(last);
            _Playable.SetInputCount(last);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override FastEnumerator<AnimancerState> GetEnumerator()
            => new FastEnumerator<AnimancerState>(States);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Create State
        /************************************************************************************************************************/

        /// <summary>Creates and returns a new <see cref="ClipState"/> to play the `clip`.</summary>
        /// <remarks>
        /// <see cref="AnimancerPlayable.GetKey"/> is used to determine the <see cref="AnimancerState.Key"/>.
        /// </remarks>
        public ClipState CreateState(AnimationClip clip)
            => CreateState(Root.GetKey(clip), clip);

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` and registers it with the `key`.
        /// </summary>
        public ClipState CreateState(object key, AnimationClip clip)
        {
            var state = new ClipState(clip)
            {
                _Key = key,
            };
            AddChild(state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a state registered with the `key` and attached to this layer or null if none exist.</summary>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <remarks>
        /// If a state is registered with the `key` but on a different layer, this method will use that state as the
        /// key and try to look up another state with it. This allows it to associate multiple states with the same
        /// original key.
        /// </remarks>
        public AnimancerState GetState(ref object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // Check through any states backwards in the key chain.
            var earlierKey = key;
            while (earlierKey is AnimancerState keyState)
            {
                if (keyState.Parent == this)// If the state is on this layer, return it.
                {
                    key = keyState.Key;
                    return keyState;
                }
                else if (keyState.Parent == null)// If the state is on no layer, attach it to this one and return it.
                {
                    key = keyState.Key;
                    AddChild(keyState);
                    return keyState;
                }
                else// Otherwise the state is on a different layer.
                {
                    earlierKey = keyState.Key;
                }
            }

            while (true)
            {
                // If no state is registered with the key, return null.
                if (!Root.States.TryGet(key, out var state))
                    return null;

                if (state.Parent == this)// If the state is on this layer, return it.
                {
                    return state;
                }
                else if (state.Parent == null)// If the state is on no layer, attach it to this one and return it.
                {
                    AddChild(state);
                    return state;
                }
                else// Otherwise the state is on a different layer.
                {
                    // Use it as the key and try to look up the next state in a chain.
                    key = state;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
            GetOrCreateState(clip3);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(params AnimationClip[] clips)
        {
            if (clips == null)
                return;

            var count = clips.Length;
            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    GetOrCreateState(clip);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerPlayable.GetKey"/> and returns the state registered with that key or
        /// creates one if it doesn't exist.
        /// <para></para>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance to use with caution.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public AnimancerState GetOrCreateState(AnimationClip clip, bool allowSetClip = false)
        {
            return GetOrCreateState(Root.GetKey(clip), clip, allowSetClip);
        }

        /// <summary>
        /// Returns the state registered with the <see cref="IHasKey.Key"/> if there is one. Otherwise
        /// this method uses <see cref="ITransition.CreateState"/> to create a new one and registers it with
        /// that key before returning it.
        /// </summary>
        public AnimancerState GetOrCreateState(ITransition transition)
        {
            var key = transition.Key;
            var state = GetState(ref key);

            if (state == null)
            {
                state = transition.CreateState();
                state.Key = key;
                AddChild(state);
            }

            return state;
        }

        /// <summary>Returns the state registered with the `key` or creates one if it doesn't exist.</summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <remarks>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance to use with caution.
        /// <para></para>
        /// See also: <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(object, AnimationClip, bool)"/>.
        /// </remarks>
        public AnimancerState GetOrCreateState(object key, AnimationClip clip, bool allowSetClip = false)
        {
            var state = GetState(ref key);
            if (state == null)
                return CreateState(key, clip);

            // If a state exists but has the wrong clip, either change it or complain.
            if (!ReferenceEquals(state.Clip, clip))
            {
                if (allowSetClip)
                {
                    state.Clip = clip;
                }
                else
                {
                    throw new ArgumentException(
                        AnimancerPlayable.StateDictionary.GetClipMismatchError(key, state.Clip, clip));
                }
            }

            return state;
        }

        /// <summary>Returns the `state` if it's a child of this layer. Otherwise makes a clone of it.</summary>
        public AnimancerState GetOrCreateState(AnimancerState state)
        {
            if (state.Parent == this)
                return state;

            if (state.Parent == null)
            {
                AddChild(state);
                return state;
            }

            var key = state.Key;
            if (key == null)
                key = state;

            var stateOnThisLayer = GetState(ref key);

            if (stateOnThisLayer == null)
            {
                stateOnThisLayer = state.Clone(Root);
                stateOnThisLayer.Key = key;
                AddChild(stateOnThisLayer);
            }

            return stateOnThisLayer;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The maximum <see cref="AnimancerNode.Weight"/> that <see cref="GetOrCreateWeightlessState"/> will treat as
        /// being weightless. Default = 0.1.
        /// </summary>
        /// <remarks>This allows states with very small weights to be reused instead of needing to create new ones.</remarks>
        public static float WeightlessThreshold { get; set; } = 0.1f;

        /// <summary>
        /// The maximum number of duplicate states that can be created for a single clip when trying to get a
        /// weightless state. Exceeding this limit will cause it to just use the state with the lowest weight.
        /// Default = 3.
        /// </summary>
        public static int MaxCloneCount { get; private set; } = 3;

        /// <summary>
        /// If the `state`'s <see cref="AnimancerNode.Weight"/> is not currently low, this method finds or creates a
        /// copy of it which is low. he returned <see cref="AnimancerState.Time"/> is also set to 0.
        /// </summary>
        /// <remarks>
        /// If this method would exceed the <see cref="MaxCloneCount"/>, it returns the clone with the lowest weight.
        /// <para></para>
        /// "Low" weight is defined as less than or equal to the <see cref="WeightlessThreshold"/>.
        /// <para></para>
        /// The <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading/modes">Fade Modes</see> page
        /// explains why clones are created.
        /// </remarks>
        public AnimancerState GetOrCreateWeightlessState(AnimancerState state)
        {
            if (state.Parent == null)
            {
                state.Weight = 0;
                goto GotState;
            }

            if (state.Parent == this &&
                state.Weight <= WeightlessThreshold)
                goto GotState;

            float lowestWeight = float.PositiveInfinity;
            AnimancerState lowestWeightState = null;

            int cloneCount = 0;

            // Use any earlier state that is weightless.
            var keyState = state;
            while (true)
            {
                keyState = keyState.Key as AnimancerState;
                if (keyState == null)
                {
                    break;
                }
                else if (keyState.Parent == this)
                {
                    if (keyState.Weight <= WeightlessThreshold)
                    {
                        state = keyState;
                        goto GotState;
                    }
                    else if (lowestWeight > keyState.Weight)
                    {
                        lowestWeight = keyState.Weight;
                        lowestWeightState = keyState;
                    }
                }
                else if (keyState.Parent == null)
                {
                    AddChild(keyState);
                    goto GotState;
                }

                cloneCount++;
            }

            if (state.Parent == this)
            {
                lowestWeight = state.Weight;
                lowestWeightState = state;
            }

            keyState = state;

            // If that state is not at low weight, get or create another state registered using the previous state as a key.
            // Keep going through states in this manner until you find one at low weight.
            while (true)
            {
                var key = (object)state;
                if (!Root.States.TryGet(key, out state))
                {
                    if (cloneCount >= MaxCloneCount && lowestWeightState != null)
                    {
                        state = lowestWeightState;
                        goto GotState;
                    }
                    else
                    {
#if UNITY_ASSERTIONS
                        var cloneTimer = OptionalWarning.CloneComplexState.IsEnabled() && !(keyState is ClipState)
                            ? SimpleTimer.Start()
                            : default;
#endif

                        state = keyState.Clone(Root);
                        state.SetDebugName($"[{cloneCount + 1}] {keyState}");
                        state.Weight = 0;
                        state._Key = key;
                        Root.States.Register(state);
                        AddChild(state);

#if UNITY_ASSERTIONS
                        if (cloneTimer.Stop())
                        {
                            OptionalWarning.CloneComplexState.Log(
                                $"A {keyState.GetType().Name} was cloned in {cloneTimer.total * 1000} milliseconds." +
                                $" This performance cost may be notable and complex states generally have parameters" +
                                $" that need to be controlled which may result in undesired behaviour if your scripts" +
                                $" are only expecting to have one state to control so you may wish to avoid cloning." +
                                $"\n\nThe Fade Modes page explains why clones are created: {Strings.DocsURLs.FadeModes}",
                                Root?.Component);
                        }
#endif

                        goto GotState;
                    }
                }
                else if (state.Parent == this)
                {
                    if (state.Weight <= WeightlessThreshold)
                    {
                        goto GotState;
                    }
                    else if (lowestWeight > state.Weight)
                    {
                        lowestWeight = state.Weight;
                        lowestWeightState = state;
                    }
                }
                else if (state.Parent == null)
                {
                    AddChild(state);
                    goto GotState;
                }

                cloneCount++;
            }

            GotState:

            state.TimeD = 0;

            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Destroys all states connected to this layer.</summary>
        /// <remarks>This operation cannot be undone.</remarks>
        public void DestroyStates()
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                States[i].Destroy();
            }

            States.Clear();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play Management
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void OnStartFade()
        {
            for (int i = States.Count - 1; i >= 0; i--)
                States[i].OnStartFade();
        }

        /************************************************************************************************************************/
        // Play Immediately.
        /************************************************************************************************************************/

        /// <summary>Stops all other animations on this layer, plays the `clip`, and returns its state.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip).Time = 0;</c>.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `clip` was already playing.
        /// </remarks>
        public AnimancerState Play(AnimationClip clip)
            => Play(GetOrCreateState(clip));

        /// <summary>Stops all other animations on the same layer, plays the `state`, and returns it.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(state).Time = 0;</c>.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="AnimancerState.Parent"/> is another state (likely a <see cref="ManualMixerState"/>).
        /// It must be either null or a layer.
        /// </exception>
        public AnimancerState Play(AnimancerState state)
        {
#if UNITY_ASSERTIONS
            if (state.Parent is AnimancerState)
                throw new InvalidOperationException(
                    $"A layer can't Play a state which is the child of another state." +
                    $"\n- State: {state}" +
                    $"\n- Parent: {state.Parent}" +
                    $"\n- Layer: {this}");
#endif

            if (Weight == 0 && TargetWeight == 0)
                Weight = 1;

            state = GetOrCreateState(state);

            CurrentState = state;

            state.Play();

            for (int i = States.Count - 1; i >= 0; i--)
            {
                var otherState = States[i];
                if (otherState != state)
                    otherState.Stop();
            }

            return state;
        }

        /************************************************************************************************************************/
        // Cross Fade.
        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` over the course of the `fadeDuration` while fading out all others in the same
        /// layer. Returns its state.
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
            => Play(GetOrCreateState(clip), fadeDuration, mode);

        /// <summary>
        /// Starts fading in the `state` over the course of the `fadeDuration` while fading out all others in this
        /// layer. Returns the `state`.
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
            // Skip the fade if:
            if (fadeDuration <= 0 ||// There is no duration.
                (Root.SkipFirstFade && Index == 0 && Weight == 0))// Or this is Layer 0 and it has no weight.
            {
                Weight = 1;
                state = Play(state);

                if (mode == FadeMode.FromStart || mode == FadeMode.NormalizedFromStart)
                    state.TimeD = 0;

                return state;
            }

            EvaluateFadeMode(mode, ref state, ref fadeDuration, out var layerFadeDuration);

            StartFade(1, layerFadeDuration);
            if (Weight == 0)
                return Play(state);

            state = GetOrCreateState(state);

            CurrentState = state;

            // If the state is already playing or will finish fading in faster than this new fade,
            // continue the existing fade but still pretend it was restarted.
            if (state.IsPlaying && state.TargetWeight == 1 &&
                (state.Weight == 1 || state.FadeSpeed * fadeDuration > Math.Abs(1 - state.Weight)))
            {
                OnStartFade();
            }
            else// Otherwise fade in the target state and fade out all others.
            {
                state.IsPlaying = true;
                state.StartFade(1, fadeDuration);

                for (int i = States.Count - 1; i >= 0; i--)
                {
                    var otherState = States[i];
                    if (otherState != state)
                        otherState.StartFade(0, fadeDuration);
                }
            }

            return state;
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
            var state = GetOrCreateState(transition);
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
        /// To restart it from the beginning you can simply set the returned state's time to 0.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// </remarks>
        public AnimancerState TryPlay(object key)
            => Root.States.TryGet(key, out var state) ? Play(state) : null;

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
        public AnimancerState TryPlay(object key, float fadeDuration, FadeMode mode = default)
            => Root.States.TryGet(key, out var state) ? Play(state, fadeDuration, mode) : null;

        /************************************************************************************************************************/

        /// <summary>Manipulates the other parameters according to the `mode`.</summary>
        /// <exception cref="ArgumentException">
        /// The <see cref="AnimancerState.Clip"/> is null when using <see cref="FadeMode.FromStart"/> or
        /// <see cref="FadeMode.NormalizedFromStart"/>.
        /// </exception>
        private void EvaluateFadeMode(FadeMode mode, ref AnimancerState state, ref float fadeDuration, out float layerFadeDuration)
        {
            layerFadeDuration = fadeDuration;

            switch (mode)
            {
                case FadeMode.FixedSpeed:
                    fadeDuration *= Math.Abs(1 - state.Weight);
                    layerFadeDuration *= Math.Abs(1 - Weight);
                    break;

                case FadeMode.FixedDuration:
                    break;

                case FadeMode.FromStart:
                    state = GetOrCreateWeightlessState(state);
                    break;

                case FadeMode.NormalizedSpeed:
                    {
                        var length = state.Length;
                        fadeDuration *= Math.Abs(1 - state.Weight) * length;
                        layerFadeDuration *= Math.Abs(1 - Weight) * length;
                    }
                    break;

                case FadeMode.NormalizedDuration:
                    {
                        var length = state.Length;
                        fadeDuration *= length;
                        layerFadeDuration *= length;
                    }
                    break;

                case FadeMode.NormalizedFromStart:
                    {
                        state = GetOrCreateWeightlessState(state);

                        var length = state.Length;
                        fadeDuration *= length;
                        layerFadeDuration *= length;
                    }
                    break;

                default:
                    throw AnimancerUtilities.CreateUnsupportedArgumentException(mode);
            }
        }

        /************************************************************************************************************************/
        // Stopping
        /************************************************************************************************************************/

        /// <summary>
        /// Sets <see cref="AnimancerNode.Weight"/> = 0 and calls <see cref="AnimancerState.Stop"/> on all animations
        /// to stop them from playing and rewind them to the start.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            CurrentState = null;

            for (int i = States.Count - 1; i >= 0; i--)
                States[i].Stop();
        }

        /************************************************************************************************************************/
        // Checking
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the `clip` is currently being played by at least one state.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip)
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                var state = States[i];
                if (state.Clip == clip && state.IsPlaying)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if at least one animation is being played.
        /// </summary>
        public bool IsAnyStatePlaying()
        {
            for (int i = States.Count - 1; i >= 0; i--)
                if (States[i].IsPlaying)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the <see cref="CurrentState"/> is playing and hasn't yet reached its end.
        /// <para></para>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </summary>
        public override bool IsPlayingAndNotEnding()
            => _CurrentState != null && _CurrentState.IsPlayingAndNotEnding();

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the total <see cref="AnimancerNode.Weight"/> of all states in this layer.
        /// </summary>
        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = States.Count - 1; i >= 0; i--)
            {
                weight += States[i].Weight;
            }

            return weight;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        private bool _ApplyAnimatorIK;

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set => base.ApplyAnimatorIK = _ApplyAnimatorIK = value;
        }

        /************************************************************************************************************************/

        private bool _ApplyFootIK;

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set => base.ApplyFootIK = _ApplyFootIK = value;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// Gathers all the animations in this layer.
        /// </summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(States);

        /************************************************************************************************************************/

        /// <summary>The Inspector display name of this layer.</summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (string.IsNullOrEmpty(DebugName))
            {
                if (_Mask != null)
                    return _Mask.name;

                SetDebugName(Index == 0 ? "Base Layer" : "Layer " + Index);
            }

            return base.ToString();
#else
            return "Layer " + Index;
#endif
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.Append(separator).Append($"{nameof(CurrentState)}: ").Append(CurrentState);
            text.Append(separator).Append($"{nameof(CommandCount)}: ").Append(CommandCount);
            text.Append(separator).Append($"{nameof(IsAdditive)}: ").Append(IsAdditive);

#if UNITY_ASSERTIONS
            text.Append(separator).Append($"{nameof(AvatarMask)}: ").Append(AnimancerUtilities.ToStringOrNull(_Mask));
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

