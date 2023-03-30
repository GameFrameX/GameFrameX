// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR
using Animancer.Editor;
using UnityEditor;
using UnityEngine;

#endif

namespace Animancer.Units
{
    /// <summary>[Editor-Conditional] Applies a different GUI for an animation speed field.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/AnimationSpeedAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class AnimationSpeedAttribute : UnitsAttribute
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimationTimeAttribute"/>.</summary>
        public AnimationSpeedAttribute()
        {
#if UNITY_EDITOR
            SetUnits(Multipliers, DisplayConverters);
            Rule = Validate.Value.IsFiniteOrNaN;
            IsOptional = true;
            DefaultValue = 1;
#endif
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private static new readonly float[]
            Multipliers = { 1 };

        /// <summary>A converter that adds an <c>x</c> suffix.</summary>
        public static new readonly CompactUnitConversionCache[]
            DisplayConverters = { AnimationTimeAttribute.XSuffix, };

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int GetLineCount(SerializedProperty property, GUIContent label) => 1;

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

