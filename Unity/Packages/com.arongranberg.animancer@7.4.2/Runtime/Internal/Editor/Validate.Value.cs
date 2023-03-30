// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/Validate
    public static partial class Validate
    {
        /************************************************************************************************************************/

        /// <summary>A rule that defines which values are valid.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Value
        public enum Value
        {
            /// <summary>Any value is allowed.</summary>
            Any,

            /// <summary>Only values between 0 (inclusive) and 1 (inclusive) are allowed.</summary>
            ZeroToOne,

            /// <summary>Only 0 or positive values are allowed.</summary>
            IsNotNegative,

            /// <summary>Infinity and NaN are not allowed.</summary>
            IsFinite,

            /// <summary>Infinity is not allowed.</summary>
            IsFiniteOrNaN,
        }

        /************************************************************************************************************************/

        /// <summary>Enforces the `rule` on the `value`.</summary>
        public static void ValueRule(ref float value, Value rule)
        {
            switch (rule)
            {
                case Value.Any:
                default:
                    return;

                case Value.ZeroToOne:
                    if (!(value >= 0))// Reversed comparison to include NaN.
                        value = 0;
                    else if (value > 1)
                        value = 1;
                    break;

                case Value.IsNotNegative:
                    if (!(value >= 0))// Reversed comparison to include NaN.
                        value = 0;
                    break;

                case Value.IsFinite:
                    if (float.IsNaN(value))
                        value = 0;
                    else if (float.IsPositiveInfinity(value))
                        value = float.MaxValue;
                    else if (float.IsNegativeInfinity(value))
                        value = float.MinValue;
                    break;

                case Value.IsFiniteOrNaN:
                    if (float.IsPositiveInfinity(value))
                        value = float.MaxValue;
                    else if (float.IsNegativeInfinity(value))
                        value = float.MinValue;
                    break;
            }
        }

        /************************************************************************************************************************/
    }
}

