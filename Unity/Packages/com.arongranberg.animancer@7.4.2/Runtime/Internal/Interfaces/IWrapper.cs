// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

namespace Animancer
{
    /// <summary>An object which wraps a <see cref="WrappedObject"/> object.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IWrapper
    /// 
    public interface IWrapper
    {
        /************************************************************************************************************************/

        /// <summary>The wrapped object.</summary>
        /// <remarks>
        /// Use <see cref="AnimancerUtilities.GetWrappedObject"/> in case the <see cref="WrappedObject"/> is also an
        /// <see cref="IWrapper"/>.
        /// </remarks>
        object WrappedObject { get; }

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="IWrapper.WrappedObject"/> recursively.</summary>
        public static object GetWrappedObject(object wrapper)
        {
            while (wrapper is IWrapper targetWrapper)
                wrapper = targetWrapper.WrappedObject;

            return wrapper;
        }

        /// <summary>
        /// Returns the `wrapper` or first <see cref="IWrapper.WrappedObject"/> which is a <typeparamref name="T"/>.
        /// </summary>
        public static bool TryGetWrappedObject<T>(object wrapper, out T wrapped) where T : class
        {
            while (true)
            {
                wrapped = wrapper as T;
                if (wrapped != null)
                    return true;

                if (wrapper is IWrapper targetWrapper)
                    wrapper = targetWrapper.WrappedObject;
                else
                    return false;
            }
        }

        /************************************************************************************************************************/
    }
}

