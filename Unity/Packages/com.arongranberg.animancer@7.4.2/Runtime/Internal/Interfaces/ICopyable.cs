// Shared File Last Modified: 2022-12-05.
// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //
// FlexiMotion // https://kybernetik.com.au/flexi-motion // Copyright 2018-2023 Kybernetik //

#define ANIMANCER
// #define FLEXI_MOTION

namespace Animancer
{
    /// <summary>Interface for objects that can be copied.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ICopyable_1
    /// https://kybernetik.com.au/flexi-motion/api/FlexiMotion/ICopyable_1
    /// 
    public interface ICopyable<T>
    {
        /************************************************************************************************************************/

        /// <summary>Copies the contents of `copyFrom` into this object, replacing its previous contents.</summary>
        /// <remarks><see cref="AnimancerUtilities.Clone{T}(T)"/> uses this method internally.</remarks>
        void CopyFrom(T copyFrom);

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <typeparamref name="T"/> and calls <see cref="ICopyable{T}.CopyFrom(T)"/> on it.</summary>
        public static T Clone<T>(this T original) where T : class, ICopyable<T>, new()
        {
            if (original == null)
                return null;

            var clone = new T();
            clone.CopyFrom(original);
            return clone;
        }

        /************************************************************************************************************************/
    }
}

