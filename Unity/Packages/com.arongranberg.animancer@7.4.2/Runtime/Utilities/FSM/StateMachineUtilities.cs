// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.FSM
{
    /// <summary>[Editor-Only] Utilities used by the <see cref="FSM"/> system.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateMachineUtilities
    /// 
    public static class StateMachineUtilities
    {
        /************************************************************************************************************************/

        /// <summary>Draws a GUI field for the `value`.</summary>
        public static T DoGenericField<T>(Rect area, string label, T value)
        {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)EditorGUI.ObjectField(
                    area, label, value as Object, typeof(T), true);
            }

            var stateName = value != null ? value.ToString() : "Null";
            EditorGUI.LabelField(area, label, stateName);
            return value;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Rect.height"/> is positive, this method moves the <see cref="Rect.y"/> by that amount and
        /// adds the <see cref="EditorGUIUtility.standardVerticalSpacing"/>.
        /// </summary>
        public static void NextVerticalArea(ref Rect area)
        {
            if (area.height > 0)
                area.y += area.height + EditorGUIUtility.standardVerticalSpacing;
        }

        /************************************************************************************************************************/
    }
}

#endif
