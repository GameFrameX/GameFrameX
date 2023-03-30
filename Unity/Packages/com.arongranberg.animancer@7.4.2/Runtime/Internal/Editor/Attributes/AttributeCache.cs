// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A cache to optimize repeated attribute access.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AttributeCache_1
    /// 
    public static class AttributeCache<TAttribute> where TAttribute : class
    {
        /************************************************************************************************************************/

        private static readonly Dictionary<MemberInfo, TAttribute>
            MemberToAttribute = new Dictionary<MemberInfo, TAttribute>();

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <typeparamref name="TAttribute"/> attribute on the specified `member` (if there is one).
        /// </summary>
        public static TAttribute GetAttribute(MemberInfo member)
        {
            if (!MemberToAttribute.TryGetValue(member, out var attribute))
            {
                try
                {
                    attribute = member.GetAttribute<TAttribute>();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }

                MemberToAttribute.Add(member, attribute);
            }

            return attribute;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <typeparamref name="TAttribute"/> attribute (if any) on the specified `type` or its
        /// <see cref="Type.BaseType"/> (recursively).
        /// </summary>
        public static TAttribute GetAttribute(Type type)
        {
            if (type == null)
                return null;

            var attribute = GetAttribute((MemberInfo)type);
            if (attribute != null)
                return attribute;

            return MemberToAttribute[type] = GetAttribute(type.BaseType);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <typeparamref name="TAttribute"/> attribute on the specified `field` or its
        /// <see cref="FieldInfo.FieldType"/> or <see cref="MemberInfo.DeclaringType"/>.
        /// </summary>
        public static TAttribute FindAttribute(FieldInfo field)
        {
            var attribute = GetAttribute(field);
            if (attribute != null)
                return attribute;

            attribute = GetAttribute(field.FieldType);
            if (attribute != null)
                return MemberToAttribute[field] = attribute;

            attribute = GetAttribute(field.DeclaringType);
            if (attribute != null)
                return MemberToAttribute[field] = attribute;

            return attribute;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Returns the <typeparamref name="TAttribute"/> attribute on the underlying field of the `property` or its
        /// <see cref="FieldInfo.FieldType"/> or <see cref="MemberInfo.DeclaringType"/> or any of the parent properties
        /// or the type of the <see cref="SerializedObject.targetObject"/>.
        /// </summary>
        public static TAttribute FindAttribute(SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            while (accessor != null)
            {
                var field = accessor.GetField(property);
                var attribute = GetAttribute(field);
                if (attribute != null)
                    return attribute;

                var value = accessor.GetValue(property);
                if (value != null)
                {
                    attribute = GetAttribute(value.GetType());
                    if (attribute != null)
                        return attribute;
                }

                accessor = accessor.Parent;
            }

            // If none of the fields of types they are declared in have names, try the actual type of the target.
            {
                var attribute = GetAttribute(property.serializedObject.targetObject.GetType());
                if (attribute != null)
                    return attribute;
            }

            return null;
        }

        /************************************************************************************************************************/
    }
}

#endif

