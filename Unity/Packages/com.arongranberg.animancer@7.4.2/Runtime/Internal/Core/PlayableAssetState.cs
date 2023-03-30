// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only] An <see cref="AnimancerState"/> which plays a <see cref="PlayableAsset"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/timeline">Timeline</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/PlayableAssetState
    /// 
    public class PlayableAssetState : AnimancerState, ICopyable<PlayableAssetState>
    {
        /************************************************************************************************************************/

        /// <summary>An <see cref="ITransition{TState}"/> that creates a <see cref="PlayableAssetState"/>.</summary>
        public interface ITransition : ITransition<PlayableAssetState> { }

        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        private PlayableAsset _Asset;

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        public PlayableAsset Asset
        {
            get => _Asset;
            set => ChangeMainObject(ref _Asset, value);
        }

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => _Asset;
            set => _Asset = (PlayableAsset)value;
        }

        /************************************************************************************************************************/

        private float _Length;

        /// <summary>The <see cref="PlayableAsset.duration"/>.</summary>
        public override float Length => _Length;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            var inputCount = _Playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var playable = _Playable.GetInput(i);
                if (!playable.IsValid())
                    continue;

                if (IsPlaying)
                    playable.Play();
                else
                    playable.Pause();
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override void CopyIKFlags(AnimancerNode copyFrom) { }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Root?.Component);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Root?.Component);
#endif
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="PlayableAssetState"/> to play the `asset`.</summary>
        /// <exception cref="ArgumentNullException">The `asset` is null.</exception>
        public PlayableAssetState(PlayableAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            _Asset = asset;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Asset.CreatePlayable(Root._Graph, Root.Component.gameObject);
            playable.SetDuration(9223372.03685477);// https://github.com/KybernetikGames/animancer/issues/111

            _Length = (float)_Asset.duration;

            if (!_HasInitializedBindings)
                InitializeBindings();
        }

        /************************************************************************************************************************/

        private IList<Object> _Bindings;
        private bool _HasInitializedBindings;

        /************************************************************************************************************************/

        /// <summary>The objects controlled by each track in the asset.</summary>
        public IList<Object> Bindings
        {
            get => _Bindings;
            set
            {
                _Bindings = value;
                InitializeBindings();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="Bindings"/>.</summary>
        public void SetBindings(params Object[] bindings)
        {
            Bindings = bindings;
        }

        /************************************************************************************************************************/

        private void InitializeBindings()
        {
            if (Root == null)
                return;

            _HasInitializedBindings = true;

            Validate.AssertPlayable(this);

            var graph = Root._Graph;

            var bindableIndex = 0;
            var bindableCount = _Bindings != null ? _Bindings.Count : 0;

            foreach (var binding in _Asset.outputs)
            {
                GetBindingDetails(binding, out var trackName, out var trackType, out var isMarkers);

                var bindable = bindableIndex < bindableCount ? _Bindings[bindableIndex] : null;

#if UNITY_ASSERTIONS
                if (!isMarkers &&
                    trackType != null &&
                    bindable != null &&
                    !trackType.IsAssignableFrom(bindable.GetType()))
                {
                    Debug.LogError(
                        $"Binding Type Mismatch: bindings[{bindableIndex}] is '{bindable}'" +
                        $" but should be a {trackType.FullName} for {trackName}",
                        Root.Component as Object);
                    bindableIndex++;
                    continue;
                }
#endif

                var playable = _Playable.GetInput(bindableIndex);

                if (trackType == typeof(Animator))// AnimationTrack.
                {
                    if (bindable != null)
                    {
#if UNITY_ASSERTIONS
                        if (bindable == Root.Component?.Animator)
                            Debug.LogError(
                                $"{nameof(PlayableAsset)} tracks should not be bound to the same {nameof(Animator)} as" +
                                $" Animancer. Leaving the binding of the first Animation Track empty will automatically" +
                                $" apply its animation to the object being controlled by Animancer.",
                                Root.Component as Object);
#endif

                        var playableOutput = AnimationPlayableOutput.Create(graph, trackName, (Animator)bindable);
                        playableOutput.SetReferenceObject(binding.sourceObject);
                        playableOutput.SetSourcePlayable(playable);
                        playableOutput.SetWeight(1);
                    }
                }
                else if (trackType == typeof(AudioSource))// AudioTrack.
                {
                    if (bindable != null)
                    {
                        var playableOutput = AudioPlayableOutput.Create(graph, trackName, (AudioSource)bindable);
                        playableOutput.SetReferenceObject(binding.sourceObject);
                        playableOutput.SetSourcePlayable(playable);
                        playableOutput.SetWeight(1);
                    }
                }
                else if (isMarkers)// Markers.
                {
                    var animancer = Root.Component as Component;
                    var playableOutput = ScriptPlayableOutput.Create(graph, trackName);
                    playableOutput.SetReferenceObject(binding.sourceObject);
                    playableOutput.SetSourcePlayable(playable);
                    playableOutput.SetWeight(1);
                    playableOutput.SetUserData(animancer);

                    var receivers = ObjectPool.AcquireList<INotificationReceiver>();
                    animancer.GetComponents(receivers);
                    for (int i = 0; i < receivers.Count; i++)
                        playableOutput.AddNotificationReceiver(receivers[i]);
                    ObjectPool.Release(receivers);

                    continue;// Don't increment the bindingIndex.
                }
                else// ActivationTrack, ControlTrack, PlayableTrack, SignalTrack.
                {
                    var playableOutput = ScriptPlayableOutput.Create(graph, trackName);
                    playableOutput.SetReferenceObject(binding.sourceObject);
                    playableOutput.SetSourcePlayable(playable);
                    playableOutput.SetWeight(1);
                    playableOutput.SetUserData(bindable);
                    if (bindable is INotificationReceiver receiver)
                        playableOutput.AddNotificationReceiver(receiver);
                }

                bindableIndex++;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Should the `binding` be skipped when determining how to map the <see cref="Bindings"/>?</summary>
        public static void GetBindingDetails(PlayableBinding binding, out string name, out Type type, out bool isMarkers)
        {
            name = binding.streamName;
            type = binding.outputTargetType;
            isMarkers = type == typeof(GameObject) && name == "Markers";
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Asset = null;
            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(AnimancerPlayable root)
        {
            var clone = new PlayableAssetState(_Asset);
            clone.SetNewCloneRoot(root);
            ((ICopyable<PlayableAssetState>)clone).CopyFrom(this);
            return clone;
        }

        /// <inheritdoc/>
        void ICopyable<PlayableAssetState>.CopyFrom(PlayableAssetState copyFrom)
        {
            _Length = copyFrom._Length;

            ((ICopyable<AnimancerState>)this).CopyFrom(copyFrom);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.Append(separator).Append($"{nameof(Bindings)}: ");

            int count;
            if (_Bindings == null)
            {
                text.Append("Null");
                count = 0;
            }
            else
            {
                count = _Bindings.Count;
                text.Append('[')
                    .Append(count)
                    .Append(']');
            }

            text.Append(_HasInitializedBindings ? " (Initialized)" : " (Not Initialized)");

            for (int i = 0; i < count; i++)
            {
                text.Append(separator)
                    .Append($"{nameof(Bindings)}[")
                    .Append(i)
                    .Append("] = ")
                    .Append(AnimancerUtilities.ToStringOrNull(_Bindings[i]));
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

