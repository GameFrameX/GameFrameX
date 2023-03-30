// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Mixer Transition/Manual", order = Strings.AssetMenuOrder + 2)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ManualMixerTransitionAsset))]
    public class ManualMixerTransitionAsset : AnimancerTransitionAsset<ManualMixerTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<ManualMixerTransitionAsset, ManualMixerTransition, ManualMixerState>,
            ManualMixerState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerTransition_1
    [Serializable]
    public abstract class ManualMixerTransition<TMixer> : AnimancerTransition<TMixer>,
        IMotion, IAnimationClipCollection, ICopyable<ManualMixerTransition<TMixer>>
        where TMixer : ManualMixerState
    {
        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(Strings.Tooltips.OptionalSpeed)]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float _Speed = 1;

        /// <summary>[<see cref="SerializeField"/>]
        /// Determines how fast the mixer plays (1x = normal speed, 2x = double speed).
        /// </summary>
        public override float Speed
        {
            get => _Speed;
            set => _Speed = value;
        }

        /************************************************************************************************************************/

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_Clips")]
        [UnityEngine.Serialization.FormerlySerializedAs("_States")]
        private Object[] _Animations;

        /// <summary>[<see cref="SerializeField"/>] Objects that define how to create each state in the mixer.</summary>
        /// <remarks>See <see cref="ManualMixerState.Initialize(Object[])"/> for more information.</remarks>
        public ref Object[] Animations => ref _Animations;

        /// <summary>The name of the serialized backing field of <see cref="Animations"/>.</summary>
        public const string AnimationsField = nameof(_Animations);

        /************************************************************************************************************************/

        [SerializeField]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float[] _Speeds;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="AnimancerNode.Speed"/> to use for each state in the mixer.
        /// </summary>
        /// <remarks>If the size of this array doesn't match the <see cref="Animations"/>, it will be ignored.</remarks>
        public ref float[] Speeds => ref _Speeds;

        /// <summary>The name of the serialized backing field of <see cref="Speeds"/>.</summary>
        public const string SpeedsField = nameof(_Speeds);

        /// <summary>Are there at least enough <see cref="Speeds"/> for each of the<see cref="Animations"/>?</summary>
        public bool HasSpeeds => _Speeds != null && _Speeds.Length >= _Animations.Length;

        /************************************************************************************************************************/

        [SerializeField]
        private bool[] _SynchronizeChildren;

        /// <summary>[<see cref="SerializeField"/>]
        /// The flags to be used in <see cref="ManualMixerState.InitializeSynchronizedChildren"/>.
        /// </summary>
        /// <remarks>The array can be null or empty. Any elements not in the array will be treated as true.</remarks>
        public ref bool[] SynchronizeChildren => ref _SynchronizeChildren;

        /// <summary>The name of the serialized backing field of <see cref="SynchronizeChildren"/>.</summary>
        public const string SynchronizeChildrenField = nameof(_SynchronizeChildren);

        /************************************************************************************************************************/

        /// <summary>[<see cref="ITransitionDetailed"/>] Are any of the <see cref="Animations"/> looping?</summary>
        public override bool IsLooping
        {
            get
            {
                for (int i = _Animations.Length - 1; i >= 0; i--)
                {
                    if (AnimancerUtilities.TryGetIsLooping(_Animations[i], out var isLooping) &&
                        isLooping)
                        return true;
                }

                return false;
            }
        }

        /// <inheritdoc/>
        public override float MaximumDuration
        {
            get
            {
                if (_Animations == null)
                    return 0;

                var duration = 0f;
                var hasSpeeds = HasSpeeds;

                for (int i = _Animations.Length - 1; i >= 0; i--)
                {
                    if (!AnimancerUtilities.TryGetLength(_Animations[i], out var length))
                        continue;

                    if (hasSpeeds)
                        length *= _Speeds[i];

                    if (duration < length)
                        duration = length;
                }

                return duration;
            }
        }

        /// <inheritdoc/>
        public virtual float AverageAngularSpeed
        {
            get
            {
                if (_Animations == null)
                    return default;

                var average = 0f;
                var hasSpeeds = HasSpeeds;

                var count = 0;
                for (int i = _Animations.Length - 1; i >= 0; i--)
                {
                    if (AnimancerUtilities.TryGetAverageAngularSpeed(_Animations[i], out var speed))
                    {
                        if (hasSpeeds)
                            speed *= _Speeds[i];

                        average += speed;
                        count++;
                    }
                }

                return average / count;
            }
        }

        /// <inheritdoc/>
        public virtual Vector3 AverageVelocity
        {
            get
            {
                if (_Animations == null)
                    return default;

                var average = new Vector3();
                var hasSpeeds = HasSpeeds;

                var count = 0;
                for (int i = _Animations.Length - 1; i >= 0; i--)
                {
                    if (AnimancerUtilities.TryGetAverageVelocity(_Animations[i], out var velocity))
                    {
                        if (hasSpeeds)
                            velocity *= _Speeds[i];

                        average += velocity;
                        count++;
                    }
                }

                return average / count;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Are all <see cref="Animations"/> assigned?</summary>
        public override bool IsValid
        {
            get
            {
                if (_Animations == null ||
                    _Animations.Length == 0)
                    return false;

                for (int i = _Animations.Length - 1; i >= 0; i--)
                    if (_Animations[i] == null)
                        return false;

                return true;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="AnimancerTransition{TState}.State"/> immediately after it is created.</summary>
        public virtual void InitializeState()
        {
            var mixer = State;
            var childCount = mixer.ChildCount;

            var auto = ManualMixerState.SynchronizeNewChildren;
            try
            {
                ManualMixerState.SynchronizeNewChildren = false;
                mixer.AddRange(_Animations);
            }
            finally
            {
                ManualMixerState.SynchronizeNewChildren = auto;
            }

            mixer.InitializeSynchronizedChildren(_SynchronizeChildren);

            if (_Speeds != null)
            {
#if UNITY_ASSERTIONS
                if (_Speeds.Length != 0 && _Speeds.Length != _Animations.Length)
                    Debug.LogError(
                        $"The number of serialized {nameof(Speeds)} ({_Speeds.Length})" +
                        $" does not match the number of {nameof(Animations)} ({_Animations.Length}).",
                        mixer.Root?.Component as Object);
#endif

                var count = Math.Min(_Animations.Length, _Speeds.Length);
                for (int i = count - 1; i >= 0; i--)
                    mixer.GetChild(childCount + i).Speed = _Speeds[i];
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Apply(AnimancerState state)
        {
            base.Apply(state);

            if (!float.IsNaN(_Speed))
                state.Speed = _Speed;

            for (int i = 0; i < _Animations.Length; i++)
                if (_Animations[i] is ITransition transition)
                    transition.Apply(state.GetChild(i));
        }

        /************************************************************************************************************************/

        /// <summary>Adds the <see cref="Animations"/> to the collection.</summary>
        void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromSource(_Animations);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(ManualMixerTransition<TMixer> copyFrom)
        {
            CopyFrom((AnimancerTransition<TMixer>)copyFrom);

            if (copyFrom == null)
            {
                _Speed = 1;
                _Animations = default;
                _Speeds = default;
                _SynchronizeChildren = default;
                return;
            }

            _Speed = copyFrom._Speed;
            AnimancerUtilities.CopyExactArray(copyFrom._Animations, ref _Animations);
            AnimancerUtilities.CopyExactArray(copyFrom._Speeds, ref _Speeds);
            AnimancerUtilities.CopyExactArray(copyFrom._SynchronizeChildren, ref _SynchronizeChildren);
        }

        /************************************************************************************************************************/
    }
}
