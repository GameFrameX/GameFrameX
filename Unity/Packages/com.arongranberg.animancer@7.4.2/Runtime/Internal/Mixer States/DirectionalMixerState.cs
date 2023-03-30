// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together based on a two dimensional
    /// parameter and thresholds using Polar Gradient Band Interpolation.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 2D Freeform Directional Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/DirectionalMixerState
    /// 
    public class DirectionalMixerState : MixerState<Vector2>, ICopyable<DirectionalMixerState>
    {
        /************************************************************************************************************************/

        /// <summary><see cref="MixerState{TParameter}.Parameter"/>.x.</summary>
        public float ParameterX
        {
            get => Parameter.x;
            set => Parameter = new Vector2(value, Parameter.y);
        }

        /// <summary><see cref="MixerState{TParameter}.Parameter"/>.y.</summary>
        public float ParameterY
        {
            get => Parameter.y;
            set => Parameter = new Vector2(Parameter.x, value);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetParameterError(Vector2 value)
            => value.IsFinite() ? null : $"value.x and value.y {Strings.MustBeFinite}";

        /************************************************************************************************************************/

        /// <summary>Precalculated magnitudes of all thresholds to speed up the recalculation of weights.</summary>
        private float[] _ThresholdMagnitudes;

        /// <summary>Precalculated values to speed up the recalculation of weights.</summary>
        private Vector2[][] _BlendFactors;

        /// <summary>Indicates whether the <see cref="_BlendFactors"/> need to be recalculated.</summary>
        private bool _BlendFactorsDirty = true;

        /// <summary>The multiplier that controls how much an angle (in radians) is worth compared to normalized distance.</summary>
        private const float AngleFactor = 2;

        /************************************************************************************************************************/

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that the internal blend factors need to be
        /// recalculated and calls <see cref="ForceRecalculateWeights"/>.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _BlendFactorsDirty = true;
            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all <see cref="ManualMixerState.ChildStates"/> based on the current value of the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the thresholds.
        /// </summary>
        protected override void ForceRecalculateWeights()
        {
            WeightsAreDirty = false;

            var childCount = ChildCount;
            if (childCount == 0)
            {
                return;
            }
            else if (childCount == 1)
            {
                var state = GetChild(0);
                state.Weight = 1;
                return;
            }

            CalculateBlendFactors(childCount);

            var parameterMagnitude = Parameter.magnitude;
            float totalWeight = 0;

            for (int i = 0; i < childCount; i++)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                var blendFactors = _BlendFactors[i];

                var thresholdI = GetThreshold(i);
                var magnitudeI = _ThresholdMagnitudes[i];

                // Convert the threshold to polar coordinates (distance, angle) and interpolate the weight based on those.
                var differenceIToParameter = parameterMagnitude - magnitudeI;
                var angleIToParameter = SignedAngle(thresholdI, Parameter) * AngleFactor;

                float weight = 1;

                for (int j = 0; j < childCount; j++)
                {
                    if (j == i || GetChild(j) == null)
                        continue;

                    var magnitudeJ = _ThresholdMagnitudes[j];
                    var averageMagnitude = (magnitudeJ + magnitudeI) * 0.5f;

                    var polarIToParameter = new Vector2(
                        differenceIToParameter / averageMagnitude,
                        angleIToParameter);

                    var newWeight = 1 - Vector2.Dot(polarIToParameter, blendFactors[j]);

                    if (weight > newWeight)
                        weight = newWeight;
                }

                if (weight < 0.01f)
                    weight = 0;

                state.Weight = weight;
                totalWeight += weight;
            }

            NormalizeWeights(totalWeight);
        }

        /************************************************************************************************************************/

        private void CalculateBlendFactors(int childCount)
        {
            if (!_BlendFactorsDirty)
                return;

            _BlendFactorsDirty = false;

            // Resize the precalculated values.
            if (_BlendFactors == null || _BlendFactors.Length != childCount)
            {
                _ThresholdMagnitudes = new float[childCount];

                _BlendFactors = new Vector2[childCount][];
                for (int i = 0; i < childCount; i++)
                    _BlendFactors[i] = new Vector2[childCount];
            }

            // Calculate the magnitude of each threshold.
            for (int i = 0; i < childCount; i++)
            {
                _ThresholdMagnitudes[i] = GetThreshold(i).magnitude;
            }

            // Calculate the blend factors between each combination of thresholds.
            for (int i = 0; i < childCount; i++)
            {
                var blendFactors = _BlendFactors[i];

                var thresholdI = GetThreshold(i);
                var magnitudeI = _ThresholdMagnitudes[i];

                var j = 0;// i + 1;
                for (; j < childCount; j++)
                {
                    if (i == j)
                        continue;

                    var thresholdJ = GetThreshold(j);
                    var magnitudeJ = _ThresholdMagnitudes[j];

#if UNITY_ASSERTIONS
                    if (thresholdI == thresholdJ)
                        throw new ArgumentException(
                            $"Mixer has multiple identical thresholds.\n{GetDescription()}");
#endif

                    var averageMagnitude = (magnitudeI + magnitudeJ) * 0.5f;

                    // Convert the thresholds to polar coordinates (distance, angle) and interpolate the weight based on those.

                    var differenceIToJ = magnitudeJ - magnitudeI;

                    var angleIToJ = SignedAngle(thresholdI, thresholdJ);

                    var polarIToJ = new Vector2(
                        differenceIToJ / averageMagnitude,
                        angleIToJ * AngleFactor);

                    polarIToJ *= 1f / polarIToJ.sqrMagnitude;

                    // Each factor is used in [i][j] with it's opposite in [j][i].
                    blendFactors[j] = polarIToJ;
                    _BlendFactors[j][i] = -polarIToJ;
                }
            }
        }

        /************************************************************************************************************************/

        private static float SignedAngle(Vector2 a, Vector2 b)
        {
            // If either vector is exactly at the origin, the angle is 0.
            if ((a.x == 0 && a.y == 0) || (b.x == 0 && b.y == 0))
            {
                // Due to floating point error the formula below usually gives 0 but sometimes Pi,
                // which screws up our other calculations so we need it to always be 0 properly.
                return 0;
            }

            return Mathf.Atan2(
                a.x * b.y - a.y * b.x,
                a.x * b.x + a.y * b.y);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(AnimancerPlayable root)
        {
            var clone = new DirectionalMixerState();
            clone.SetNewCloneRoot(root);
            ((ICopyable<DirectionalMixerState>)clone).CopyFrom(this);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void ICopyable<DirectionalMixerState>.CopyFrom(DirectionalMixerState copyFrom)
        {
            _ThresholdMagnitudes = copyFrom._ThresholdMagnitudes;
            _BlendFactorsDirty = copyFrom._BlendFactorsDirty;
            if (!_BlendFactorsDirty)
                _BlendFactors = copyFrom._BlendFactors;

            ((ICopyable<MixerState<Vector2>>)this).CopyFrom(copyFrom);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void AppendParameter(StringBuilder text, Vector2 parameter)
        {
            text.Append('(')
                .Append(parameter.x)
                .Append(", ")
                .Append(parameter.y)
                .Append(')');
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int ParameterCount => 2;

        /// <inheritdoc/>
        protected override string GetParameterName(int index)
        {
            switch (index)
            {
                case 0: return "Parameter X";
                case 1: return "Parameter Y";
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <inheritdoc/>
        protected override AnimatorControllerParameterType GetParameterType(int index) => AnimatorControllerParameterType.Float;

        /// <inheritdoc/>
        protected override object GetParameterValue(int index)
        {
            switch (index)
            {
                case 0: return ParameterX;
                case 1: return ParameterY;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <inheritdoc/>
        protected override void SetParameterValue(int index, object value)
        {
            switch (index)
            {
                case 0: ParameterX = (float)value; break;
                case 1: ParameterY = (float)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

