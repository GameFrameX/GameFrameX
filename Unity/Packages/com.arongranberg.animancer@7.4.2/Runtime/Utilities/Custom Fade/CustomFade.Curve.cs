// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/CustomFade
    /// 
    public partial class CustomFade
    {
        /************************************************************************************************************************/

        /// <summary>Modify the current fade to use the specified `curve` to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        /// <remarks>The `curve` should follow the <see cref="OptionalWarning.CustomFadeBounds"/> guideline.</remarks>
        public static void Apply(AnimancerComponent animancer, AnimationCurve curve)
            => Apply(animancer.States.Current, curve);

        /// <summary>Modify the current fade to use the specified `curve` to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        /// <remarks>The `curve` should follow the <see cref="OptionalWarning.CustomFadeBounds"/> guideline.</remarks>
        public static void Apply(AnimancerPlayable animancer, AnimationCurve curve)
            => Apply(animancer.States.Current, curve);

        /// <summary>Modify the current fade to use the specified `curve` to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        /// <remarks>The `curve` should follow the <see cref="OptionalWarning.CustomFadeBounds"/> guideline.</remarks>
        public static void Apply(AnimancerState state, AnimationCurve curve)
            => Curve.Acquire(curve).Apply(state);

        /// <summary>Modify the current fade to use the specified `curve` to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        /// <remarks>The `curve` should follow the <see cref="OptionalWarning.CustomFadeBounds"/> guideline.</remarks>
        public static void Apply(AnimancerNode node, AnimationCurve curve)
            => Curve.Acquire(curve).Apply(node);

        /************************************************************************************************************************/

        /// <summary>A <see cref="CustomFade"/> which uses an <see cref="AnimationCurve"/> to calculate the weight.</summary>
        /// <example>See <see cref="CustomFade"/>.</example>
        private class Curve : CustomFade
        {
            /************************************************************************************************************************/

            private AnimationCurve _Curve;

            /************************************************************************************************************************/

            public static Curve Acquire(AnimationCurve curve)
            {
                if (curve == null)
                {
                    OptionalWarning.CustomFadeNotNull.Log($"{nameof(curve)} is null.");
                    return null;
                }

                var fade = ObjectPool<Curve>.Acquire();
                fade._Curve = curve;
                return fade;
            }

            /************************************************************************************************************************/

            protected override float CalculateWeight(float progress) => _Curve.Evaluate(progress);

            /************************************************************************************************************************/

            protected override void Release() => ObjectPool<Curve>.Release(this);

            /************************************************************************************************************************/
        }
    }
}
