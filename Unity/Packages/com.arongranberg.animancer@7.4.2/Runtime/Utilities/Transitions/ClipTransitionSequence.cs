// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// <summary>A group of <see cref="ClipTransition"/>s which play one after the other.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipTransitionSequence
    /// 
    [Serializable]
    public class ClipTransitionSequence : ClipTransition,
        ISerializationCallbackReceiver, ICopyable<ClipTransitionSequence>
    {
        /************************************************************************************************************************/

        [DrawAfterEvents]
        [SerializeField]
        [Tooltip("The other transitions to play in order after the first one.")]
        private ClipTransition[] _Others = Array.Empty<ClipTransition>();

        /// <summary>[<see cref="SerializeField"/>] The transitions to play in order after the first one.</summary>
        public ref ClipTransition[] Others => ref _Others;

        /// <summary>The last of the <see cref="Others"/> (or <c>this</c> if there are none).</summary>
        public ClipTransition LastTransition => _Others.Length > 0 ? _Others[_Others.Length - 1] : this;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        /// <inheritdoc/>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_Others.Length <= 1)
                return;

            // Assign each of the other end events, but this first one will be set by Apply.

            var previous = _Others[0];
            for (int i = 1; i < _Others.Length; i++)
            {
                var next = _Others[i];
                previous.Events.OnEnd = () => AnimancerEvent.CurrentState.Layer.Play(next);
                previous = next;
            }
        }

        /************************************************************************************************************************/

        private Action _OnEnd;

        /// <inheritdoc/>
        public override void Apply(AnimancerState state)
        {
            // If an end event is assigned other than the one to play the next transition,
            // replace it and move it to be the end event of the last transition instead.
            if (_Others.Length > 0)
            {
                if (_OnEnd == null)
                    _OnEnd = () => AnimancerEvent.CurrentState.Layer.Play(_Others[0]);

                var onEnd = Events.OnEnd;
                if (onEnd != _OnEnd)
                {
                    Events.OnEnd = _OnEnd;
                    onEnd -= _OnEnd;
                    _Others[_Others.Length - 1].Events.OnEnd = onEnd;
                }
            }

            base.Apply(state);
        }

        /************************************************************************************************************************/

        /// <summary>Is everything in this sequence valid?</summary>
        public override bool IsValid
        {
            get
            {
                if (!base.IsValid)
                    return false;

                for (int i = 0; i < _Others.Length; i++)
                    if (!_Others[i].IsValid)
                        return false;

                return true;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Is the last animation in this sequence looping?</summary>
        public override bool IsLooping => _Others.Length > 0 ? LastTransition.IsLooping : base.IsLooping;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float Length
        {
            get
            {
                var length = base.Length;
                for (int i = 0; i < _Others.Length; i++)
                    length += _Others[i].Length;
                return length;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float MaximumDuration
        {
            get
            {
                var value = base.MaximumDuration;
                for (int i = 0; i < _Others.Length; i++)
                    value += _Others[i].MaximumDuration;
                return value;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float AverageAngularSpeed
        {
            get
            {
                var speed = base.AverageAngularSpeed;
                if (_Others.Length == 0)
                    return speed;

                var duration = base.MaximumDuration;
                speed *= duration;

                for (int i = 0; i < _Others.Length; i++)
                {
                    var other = _Others[i];
                    var otherSpeed = other.AverageAngularSpeed;
                    var otherDuration = other.MaximumDuration;
                    speed += otherSpeed * otherDuration;
                    duration += otherDuration;
                }

                return speed / duration;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity
        {
            get
            {
                var velocity = base.AverageVelocity;

                if (_Others.Length == 0)
                    return velocity;

                var duration = base.MaximumDuration;
                velocity *= duration;

                for (int i = 0; i < _Others.Length; i++)
                {
                    var other = _Others[i];
                    var otherVelocity = other.AverageVelocity;
                    var otherDuration = other.MaximumDuration;
                    velocity += otherVelocity * otherDuration;
                    duration += otherDuration;
                }

                return velocity / duration;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds the <see cref="ClipTransition.Clip"/> of everything in this sequence to the collection.</summary>
        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            base.GatherAnimationClips(clips);
            for (int i = 0; i < _Others.Length; i++)
                _Others[i].GatherAnimationClips(clips);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(ClipTransitionSequence copyFrom)
        {
            CopyFrom((ClipTransition)copyFrom);

            if (copyFrom == null)
            {
                _Others = Array.Empty<ClipTransition>();
                return;
            }

            AnimancerUtilities.CopyExactArray(copyFrom._Others, ref _Others);
        }

        /************************************************************************************************************************/
        #region Events
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerEvent.Sequence.EndEvent"/> of the last transition in this sequence.</summary>
        public AnimancerEvent EndEvent
        {
            get => LastTransition.Events.EndEvent;
            set => LastTransition.Events.EndEvent = value;
        }

        /************************************************************************************************************************/

        /// <summary>Adds an event at the specified time relative to the entire sequence.</summary>
        public void AddEvent(float time, bool normalized, Action callback)
        {
            // Convert time to Seconds.
            if (normalized)
                time *= Length;

            if (TryAddEvent(this, base.Length, ref time, callback))
                return;

            for (int i = 0; i < _Others.Length - 1; i++)
            {
                var other = _Others[i];
                if (TryAddEvent(other, other.Length, ref time, callback))
                    return;
            }

            AddEvent(LastTransition, time, callback);
        }

        /// <summary>
        /// Tries to add the `callback` as an event to the `transition` if the `time` is within the `length` and
        /// returns true if successful. Otherwise subtracts the `length` from the `time` and returns false so it can be
        /// tried in the next transition in the sequence.
        /// </summary>
        private static bool TryAddEvent(ClipTransition transition, float length, ref float time, Action callback)
        {
            if (time > length)
            {
                time -= length;
                return false;
            }

            AddEvent(transition, time, callback);
            return true;
        }

        /// <summary>
        /// Adds the `callback` as an event to the `transition` at the specified `time` (in seconds, starting from the
        /// <see cref="ClipTransition.NormalizedStartTime"/>).
        /// </summary>
        private static void AddEvent(ClipTransition transition, float time, Action callback)
        {
            var start = transition.NormalizedStartTime;
            if (float.IsNaN(start))
                start = AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(start);

            time /= transition.Clip.length * (1 - start);
            time += start;

            transition.Events.Add(time, callback);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
