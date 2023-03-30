// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

namespace Animancer
{
    /// <summary>[Editor-Conditional]
    /// A <see cref="DefaultValueAttribute"/> which uses the <see cref="AnimancerPlayable.DefaultFadeDuration"/> and 0.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/DefaultFadeValueAttribute
    /// 
    public class DefaultFadeValueAttribute : DefaultValueAttribute
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override object Primary => AnimancerPlayable.DefaultFadeDuration;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="DefaultValueAttribute"/>.</summary>
        public DefaultFadeValueAttribute()
        {
            // This won't change so there's no need to box the value every time by overriding the property.
            Secondary = 0f;
        }

        /************************************************************************************************************************/
    }
}

