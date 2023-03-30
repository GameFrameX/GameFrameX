// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Reflection;

#if UNITY_EDITOR
using Animancer.Editor;
using UnityEditor;
#endif

namespace Animancer
{
    /// <summary>[Editor-Conditional] Specifies the default value of a field and a secondary fallback.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/DefaultValueAttribute
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public class DefaultValueAttribute : Attribute
    {
        /************************************************************************************************************************/

        /// <summary>The main default value.</summary>
        public virtual object Primary { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>The fallback value to use if the target value was already equal to the <see cref="Primary"/>.</summary>
        public virtual object Secondary { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="DefaultValueAttribute"/>.</summary>
        public DefaultValueAttribute(object primary, object secondary = null)
        {
            Primary = primary;
            Secondary = secondary;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="DefaultValueAttribute"/>.</summary>
        protected DefaultValueAttribute() { }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// If the field represented by the `property` has a <see cref="DefaultValueAttribute"/>, this method sets
        /// the `value` to its <see cref="Primary"/> value. If it was already at the value, it sets it to the
        /// <see cref="Secondary"/> value instead. And if the field has no attribute, it uses the default for the type.
        /// </summary>
        public static void SetToDefault<T>(ref T value, SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            var field = accessor.GetField(property);
            if (field == null)
                accessor.SetValue(property, null);
            else
                SetToDefault(ref value, field);
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// If the field represented by the `property` has a <see cref="DefaultValueAttribute"/>, this method sets
        /// the `value` to its <see cref="Primary"/> value. If it was already at the value, it sets it to the
        /// <see cref="Secondary"/> value instead. And if the field has no attribute, it uses the default for the type.
        /// </summary>
        public static void SetToDefault<T>(ref T value, FieldInfo field)
        {
            var defaults = field.GetAttribute<DefaultValueAttribute>();
            if (defaults != null)
                defaults.SetToDefault(ref value);
            else
                value = default;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Sets the `value` equal to the <see cref="Primary"/> value. If it was already at the value, it sets it equal
        /// to the <see cref="Secondary"/> value instead.
        /// </summary>
        public void SetToDefault<T>(ref T value)
        {
            var primary = Primary;
            if (!Equals(value, primary))
            {
                value = (T)primary;
                return;
            }

            var secondary = Secondary;
            if (secondary != null || !typeof(T).IsValueType)
            {
                value = (T)secondary;
                return;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Sets the `value` equal to the `primary` value. If it was already at the value, it sets it equal to the
        /// `secondary` value instead.
        /// </summary>
        public static void SetToDefault<T>(ref T value, T primary, T secondary)
        {
            if (!Equals(value, primary))
                value = primary;
            else
                value = secondary;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

