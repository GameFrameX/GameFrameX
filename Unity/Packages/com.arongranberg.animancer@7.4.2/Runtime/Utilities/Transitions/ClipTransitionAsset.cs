// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Clip Transition", order = Strings.AssetMenuOrder + 1)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ClipTransitionAsset))]
    public class ClipTransitionAsset : AnimancerTransitionAsset<ClipTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<ClipTransitionAsset, ClipTransition, ClipState>,
            ClipState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipTransition
    [Serializable]
    public class ClipTransition : AnimancerTransition<ClipState>,
        ClipState.ITransition, IMotion, IAnimationClipCollection, ICopyable<ClipTransition>
    {
        /************************************************************************************************************************/

        /// <summary>The name of the serialized backing field of <see cref="Clip"/>.</summary>
        public const string ClipFieldName = nameof(_Clip);

        [SerializeField, Tooltip("The animation to play")]
        private AnimationClip _Clip;

        /// <summary>[<see cref="SerializeField"/>] The animation to play.</summary>
        public AnimationClip Clip
        {
            get => _Clip;
            set
            {
#if UNITY_ASSERTIONS
                if (value != null)
                    Validate.AssertNotLegacy(value);
#endif

                _Clip = value;
            }
        }

        /// <inheritdoc/>
        public override Object MainObject => _Clip;

        /// <summary>Returns the <see cref="Clip"/> to use as the <see cref="AnimancerState.Key"/>.</summary>
        public override object Key => _Clip;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(Strings.Tooltips.OptionalSpeed)]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float _Speed = 1;

        /// <inheritdoc/>
        public override float Speed
        {
            get => _Speed;
            set => _Speed = value;
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(Strings.Tooltips.NormalizedStartTime)]
        [AnimationTime(AnimationTimeAttribute.Units.Normalized)]
        [DefaultValue(float.NaN, 0f)]
        private float _NormalizedStartTime = float.NaN;

        /// <inheritdoc/>
        public override float NormalizedStartTime
        {
            get => _NormalizedStartTime;
            set => _NormalizedStartTime = value;
        }

        /// <summary>
        /// If this transition will set the <see cref="AnimancerState.Time"/>, then it needs to use
        /// <see cref="FadeMode.FromStart"/>.
        /// </summary>
        public override FadeMode FadeMode => float.IsNaN(_NormalizedStartTime) ? FadeMode.FixedSpeed : FadeMode.FromStart;

        /************************************************************************************************************************/

        /// <summary>
        /// The length of the <see cref="Clip"/> (in seconds), accounting for the <see cref="NormalizedStartTime"/> and
        /// <see cref="AnimancerEvent.Sequence.NormalizedEndTime"/> (but not <see cref="Speed"/>).
        /// </summary>
        public virtual float Length
        {
            get
            {
                if (!IsValid)
                    return 0;

                var normalizedEndTime = Events.NormalizedEndTime;
                normalizedEndTime = !float.IsNaN(normalizedEndTime)
                    ? normalizedEndTime
                    : AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(_Speed);

                var normalizedStartTime = !float.IsNaN(_NormalizedStartTime)
                    ? _NormalizedStartTime
                    : AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(_Speed);

                return _Clip.length * (normalizedEndTime - normalizedStartTime);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool IsValid => _Clip != null && !_Clip.legacy;

        /// <summary>[<see cref="ITransitionDetailed"/>] Is the <see cref="Clip"/> looping?</summary>
        public override bool IsLooping => _Clip != null && _Clip.isLooping;

        /// <inheritdoc/>
        public override float MaximumDuration => _Clip != null ? _Clip.length : 0;

        /// <inheritdoc/>
        public virtual float AverageAngularSpeed => _Clip != null ? _Clip.averageAngularSpeed : default;

        /// <inheritdoc/>
        public virtual Vector3 AverageVelocity => _Clip != null ? _Clip.averageSpeed : default;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override ClipState CreateState()
        {
#if UNITY_ASSERTIONS
            if (_Clip == null)
                throw new ArgumentException(
                    $"Unable to create {nameof(ClipState)} because the {nameof(ClipTransition)}.{nameof(Clip)} is null.");
#endif

            return State = new ClipState(_Clip);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Apply(AnimancerState state)
        {
            ApplyDetails(state, _Speed, _NormalizedStartTime);
            base.Apply(state);
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Adds the <see cref="Clip"/> to the collection.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.Gather(_Clip);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(ClipTransition copyFrom)
        {
            CopyFrom((AnimancerTransition<ClipState>)copyFrom);

            if (copyFrom == null)
            {
                _Clip = default;
                _Speed = 1;
                _NormalizedStartTime = float.NaN;
                return;
            }

            _Clip = copyFrom._Clip;
            _Speed = copyFrom._Speed;
            _NormalizedStartTime = copyFrom._NormalizedStartTime;
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [UnityEditor.CustomPropertyDrawer(typeof(ClipTransition), true)]
        public class Drawer : Editor.TransitionDrawer
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Drawer"/>.</summary>
            public Drawer() : base(ClipFieldName) { }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
