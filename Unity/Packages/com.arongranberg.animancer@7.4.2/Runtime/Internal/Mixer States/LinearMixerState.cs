// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together using linear interpolation
    /// between the specified thresholds.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 1D Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/LinearMixerState
    /// 
    public class LinearMixerState : MixerState<float>, ICopyable<LinearMixerState>
    {
        /************************************************************************************************************************/

        /// <summary>An <see cref="ITransition{TState}"/> that creates a <see cref="LinearMixerState"/>.</summary>
        public new interface ITransition : ITransition<LinearMixerState> { }

        /************************************************************************************************************************/

        private bool _ExtrapolateSpeed = true;

        /// <summary>
        /// Should setting the <see cref="MixerState{TParameter}.Parameter"/> above the highest threshold increase the
        /// <see cref="AnimancerNode.Speed"/> of this mixer proportionally?
        /// </summary>
        public bool ExtrapolateSpeed
        {
            get => _ExtrapolateSpeed;
            set
            {
                if (_ExtrapolateSpeed == value)
                    return;

                _ExtrapolateSpeed = value;

                if (!_Playable.IsValid())
                    return;

                var speed = Speed;

                var childCount = ChildCount;
                if (value && childCount > 0)
                {
                    var threshold = GetThreshold(childCount - 1);
                    if (Parameter > threshold)
                        speed *= Parameter / threshold;
                }

                _Playable.SetSpeed(speed);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetParameterError(float value)
            => value.IsFinite() ? null : Strings.MustBeFinite;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(AnimancerPlayable root)
        {
            var clone = new LinearMixerState();
            clone.SetNewCloneRoot(root);
            ((ICopyable<LinearMixerState>)clone).CopyFrom(this);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void ICopyable<LinearMixerState>.CopyFrom(LinearMixerState copyFrom)
        {
            _ExtrapolateSpeed = copyFrom._ExtrapolateSpeed;

            ((ICopyable<MixerState<float>>)this).CopyFrom(copyFrom);
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        private bool _NeedToCheckThresholdSorting;

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that <see cref="AssertThresholdsSorted"/> needs to
        /// be called by the next <see cref="ForceRecalculateWeights"/> if UNITY_ASSERTIONS is defined, then calls
        /// <see cref="MixerState{TParameter}.OnThresholdsChanged"/>.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _NeedToCheckThresholdSorting = true;

            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the thresholds are not sorted from lowest to highest without
        /// any duplicates.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidOperationException">The thresholds have not been initialized.</exception>
        public void AssertThresholdsSorted()
        {
#if UNITY_ASSERTIONS
            _NeedToCheckThresholdSorting = false;
#endif

            if (!HasThresholds)
                throw new InvalidOperationException("Thresholds have not been initialized");

            var previous = float.NegativeInfinity;

            var childCount = ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var state = ChildStates[i];
                if (state == null)
                    continue;

                var next = GetThreshold(i);
                if (next > previous)
                    previous = next;
                else
                    throw new ArgumentException(
                        (next == previous ? "Mixer has multiple identical thresholds." : "Mixer has thresholds out of order.") +
                        " They must be sorted from lowest to highest with no equal values." +
                        "\n" + GetDescription());
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all <see cref="ManualMixerState.ChildStates"/> based on the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the thresholds.
        /// </summary>
        protected override void ForceRecalculateWeights()
        {
            WeightsAreDirty = false;

#if UNITY_ASSERTIONS
            if (_NeedToCheckThresholdSorting)
                AssertThresholdsSorted();
#endif

            // Go through all states, figure out how much weight to give those with thresholds adjacent to the
            // current parameter value using linear interpolation, and set all others to 0 weight.

            var childCount = ChildCount;
            if (childCount == 0)
                goto ResetExtrapolatedSpeed;

            var index = 0;
            var previousState = ChildStates[index];

            var parameter = Parameter;
            var previousThreshold = GetThreshold(index);

            if (parameter <= previousThreshold)
            {
                DisableRemainingStates(index);

                if (previousThreshold >= 0)
                {
                    previousState.Weight = 1;
                    goto ResetExtrapolatedSpeed;
                }
            }
            else
            {
                while (++index < childCount)
                {
                    var nextState = ChildStates[index];
                    var nextThreshold = GetThreshold(index);

                    if (parameter > previousThreshold && parameter <= nextThreshold)
                    {
                        var t = (parameter - previousThreshold) / (nextThreshold - previousThreshold);
                        previousState.Weight = 1 - t;
                        nextState.Weight = t;
                        DisableRemainingStates(index);
                        goto ResetExtrapolatedSpeed;
                    }
                    else
                    {
                        previousState.Weight = 0;
                    }

                    previousState = nextState;
                    previousThreshold = nextThreshold;
                }
            }

            previousState.Weight = 1;

            if (ExtrapolateSpeed)
                _Playable.SetSpeed(Speed * (parameter / previousThreshold));

            return;

            ResetExtrapolatedSpeed:
            if (ExtrapolateSpeed && _Playable.IsValid())
                _Playable.SetSpeed(Speed);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the thresholds to be evenly spaced between the specified min and max (inclusive).
        /// </summary>
        public LinearMixerState AssignLinearThresholds(float min = 0, float max = 1)
        {
#if UNITY_ASSERTIONS
            if (min >= max)
                throw new ArgumentException($"{nameof(min)} must be less than {nameof(max)}");
#endif
            var childCount = ChildCount;

            var thresholds = new float[childCount];

            var increment = (max - min) / (childCount - 1);

            for (int i = 0; i < childCount; i++)
            {
                thresholds[i] =
                    i < childCount - 1 ?
                    min + i * increment :// Assign each threshold linearly spaced between the min and max.
                    max;// and ensure that the last one is exactly at the max (to avoid floating-point error).
            }

            SetThresholds(thresholds);

            return this;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            text.Append(separator)
                .Append($"{nameof(ExtrapolateSpeed)}: ")
                .Append(ExtrapolateSpeed);

            base.AppendDetails(text, separator);
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int ParameterCount => 1;

        /// <inheritdoc/>
        protected override string GetParameterName(int index) => "Parameter";

        /// <inheritdoc/>
        protected override AnimatorControllerParameterType GetParameterType(int index) => AnimatorControllerParameterType.Float;

        /// <inheritdoc/>
        protected override object GetParameterValue(int index) => Parameter;

        /// <inheritdoc/>
        protected override void SetParameterValue(int index, object value) => Parameter = (float)value;

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Returns a <see cref="Drawer"/> for this state.</summary>
        protected internal override Editor.IAnimancerNodeDrawer CreateDrawer() => new Drawer(this);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public class Drawer : Drawer<LinearMixerState>
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> to manage the Inspector GUI for the `state`.
            /// </summary>
            public Drawer(LinearMixerState state) : base(state) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddContextMenuFunctions(UnityEditor.GenericMenu menu)
            {
                base.AddContextMenuFunctions(menu);

                menu.AddItem(new GUIContent("Extrapolate Speed"), Target.ExtrapolateSpeed, () =>
                {
                    Target.ExtrapolateSpeed = !Target.ExtrapolateSpeed;
                });

            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

