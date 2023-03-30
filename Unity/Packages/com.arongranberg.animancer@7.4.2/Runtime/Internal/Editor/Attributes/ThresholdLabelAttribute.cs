// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>[Editor-Conditional]
    /// Specifies a custom display label for the <c>Thresholds</c> column of a mixer transition.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ThresholdLabelAttribute
    /// 
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class ThresholdLabelAttribute : Attribute
    {
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The label.</summary>
        public readonly string Label;
#endif

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ThresholdLabelAttribute"/>.</summary>
        public ThresholdLabelAttribute(string label)
        {
#if UNITY_EDITOR
            Label = label;
#endif
        }

        /************************************************************************************************************************/
    }
}

