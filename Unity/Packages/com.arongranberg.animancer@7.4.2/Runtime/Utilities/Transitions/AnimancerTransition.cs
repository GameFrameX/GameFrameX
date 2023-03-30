// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Units;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A serializable <see cref="ITransition"/> which can create a particular type of <see cref="AnimancerState"/>
    /// when passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerTransition_1
    /// 
    [Serializable]
    public abstract class AnimancerTransition<TState> :
        ITransition<TState>, ITransitionDetailed, ITransitionWithEvents, ICopyable<AnimancerTransition<TState>>
        where TState : AnimancerState
    {
        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(Strings.Tooltips.FadeDuration)]
        [AnimationTime(AnimationTimeAttribute.Units.Seconds, Rule = Validate.Value.IsNotNegative)]
        [DefaultFadeValue]
        private float _FadeDuration = AnimancerPlayable.DefaultFadeDuration;

        /// <inheritdoc/>
        /// <remarks>[<see cref="SerializeField"/>]</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a negative number.</exception>
        public float FadeDuration
        {
            get => _FadeDuration;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(FadeDuration)} must not be negative");

                _FadeDuration = value;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        /// <remarks>Returns <c>false</c> unless overridden.</remarks>
        public virtual bool IsLooping => false;

        /// <inheritdoc/>
        public virtual float NormalizedStartTime
        {
            get => float.NaN;
            set { }
        }

        /// <inheritdoc/>
        /// <remarks>Returns <c>1</c> unless overridden.</remarks>
        public virtual float Speed
        {
            get => 1;
            set { }
        }

        /// <inheritdoc/>
        public abstract float MaximumDuration { get; }

        /************************************************************************************************************************/

        [SerializeField, Tooltip(Strings.ProOnlyTag + "Events which will be triggered as the animation plays")]
        private AnimancerEvent.Sequence.Serializable _Events;

        /// <inheritdoc/>
        /// <remarks>This property returns the <see cref="AnimancerEvent.Sequence.Serializable.Events"/>.</remarks>
        /// <exception cref="NullReferenceException">
        /// <see cref="SerializedEvents"/> is null. If this transition was created in code (using the <c>new</c>
        /// keyword rather than being deserialized by Unity) you will need to null-check and/or assign a new
        /// sequence to the <see cref="SerializedEvents"/> before accessing this property.
        /// </exception>
        public AnimancerEvent.Sequence Events
        {
            get
            {
                if (_Events == null)
                    _Events = new AnimancerEvent.Sequence.Serializable();

                return _Events.Events;
            }
        }

        /// <inheritdoc/>
        public ref AnimancerEvent.Sequence.Serializable SerializedEvents => ref _Events;

        /************************************************************************************************************************/

        /// <summary>
        /// The state that was created by this object. Specifically, this is the state that was most recently
        /// passed into <see cref="Apply"/> (usually by <see cref="AnimancerPlayable.Play(ITransition)"/>).
        /// <para></para>
        /// You can use <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(ITransition)"/> or
        /// <see cref="AnimancerLayer.GetOrCreateState(ITransition)"/> to get or create the state for a
        /// specific object.
        /// <para></para>
        /// <see cref="State"/> is simply a shorthand for casting this to <typeparamref name="TState"/>.
        /// </summary>
        public AnimancerState BaseState { get; private set; }

        /************************************************************************************************************************/

        private TState _State;

        /// <summary>
        /// The state that was created by this object. Specifically, this is the state that was most recently
        /// passed into <see cref="Apply"/> (usually by <see cref="AnimancerPlayable.Play(ITransition)"/>).
        /// </summary>
        /// 
        /// <remarks>
        /// You can use <see cref="AnimancerPlayable.StateDictionary.GetOrCreate(ITransition)"/> or
        /// <see cref="AnimancerLayer.GetOrCreateState(ITransition)"/> to get or create the state for a
        /// specific object.
        /// <para></para>
        /// This property is shorthand for casting the <see cref="BaseState"/> to <typeparamref name="TState"/>.
        /// </remarks>
        /// 
        /// <exception cref="InvalidCastException">
        /// The <see cref="BaseState"/> is not actually a <typeparamref name="TState"/>. This should only
        /// happen if a different type of state was created by something else and registered using the
        /// <see cref="Key"/>, causing this <see cref="AnimancerPlayable.Play(ITransition)"/> to pass that
        /// state into <see cref="Apply"/> instead of calling <see cref="CreateState"/> to make the correct type of
        /// state.
        /// </exception>
        public TState State
        {
            get
            {
                if (_State == null)
                    _State = (TState)BaseState;

                return _State;
            }
            protected set
            {
                BaseState = _State = value;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        /// <remarks>Returns <c>true</c> unless overridden.</remarks>
        public virtual bool IsValid => true;

        /// <summary>The <see cref="AnimancerState.Key"/> which the created state will be registered with.</summary>
        /// <remarks>Returns <c>this</c> unless overridden.</remarks>
        public virtual object Key => this;

        /// <inheritdoc/>
        /// <remarks>Returns <see cref="FadeMode.FixedSpeed"/> unless overridden.</remarks>
        public virtual FadeMode FadeMode => FadeMode.FixedSpeed;

        /// <inheritdoc/>
        public abstract TState CreateState();

        /// <inheritdoc/>
        AnimancerState ITransition.CreateState() => CreateState();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void Apply(AnimancerState state)
        {
            state.Events = _Events;

#if UNITY_ASSERTIONS
            if (state.HasEvents)
                state.Events.SetShouldNotModifyReason("it was created by a Transition." +
                    $"\n\nTransitions give the played {nameof(AnimancerState)} a reference to their Events," +
                    $" meaning that any modifications to the state.Events will also affect the transition.Events" +
                    $" and persist when that Transition is played in the future. This is a common source of logic bugs" +
                    $" so when a Transition is played it marks its Events as no longer expecting to be modified.");
#endif

            BaseState = state;

            if (_State != state)
                _State = null;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerState.MainObject"/> that the created state will have.</summary>
        public virtual Object MainObject { get; }

        /// <summary>The display name of this transition.</summary>
        public virtual string Name
        {
            get
            {
                var mainObject = MainObject;
                return mainObject != null ? mainObject.name : null;
            }
        }

        /// <summary>Returns the <see cref="Name"/> and type of this transition.</summary>
        public override string ToString()
        {
            var type = GetType().FullName;

            var name = Name;
            if (name != null)
                return $"{name} ({type})";
            else
                return type;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(AnimancerTransition<TState> copyFrom)
        {
            if (copyFrom == null)
            {
                _FadeDuration = AnimancerPlayable.DefaultFadeDuration;
                _Events = default;
                return;
            }

            _FadeDuration = copyFrom._FadeDuration;
            _Events = copyFrom._Events.Clone();
        }

        /************************************************************************************************************************/

        /// <summary>Applies the given details to the `state`.</summary>
        public static void ApplyDetails(AnimancerState state, float speed, float normalizedStartTime)
        {
            if (!float.IsNaN(speed))
                state.Speed = speed;

            if (!float.IsNaN(normalizedStartTime))
                state.NormalizedTime = normalizedStartTime;
            else if (state.Weight == 0)
                state.NormalizedTime = AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(speed);
        }

        /************************************************************************************************************************/
    }
}
