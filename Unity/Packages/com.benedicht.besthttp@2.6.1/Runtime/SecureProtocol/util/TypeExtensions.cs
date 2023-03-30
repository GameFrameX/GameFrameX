#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
#if NEW_REFLECTION || NETFX_CORE

using System;
using System.Reflection;

namespace Org.BouncyCastle
{
    internal static class TypeExtensions
    {
        public static bool IsInstanceOfType(this Type type, object instance)
        {
            return instance != null && type.GetTypeInfo().IsAssignableFrom(instance.GetType().GetTypeInfo());
        }
    }
}

#endif
#pragma warning restore
#endif
