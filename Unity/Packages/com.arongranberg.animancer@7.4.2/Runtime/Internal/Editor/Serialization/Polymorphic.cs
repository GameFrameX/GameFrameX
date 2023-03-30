// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /************************************************************************************************************************/

    /// <summary>An object that will be drawn by a <see cref="Editor.PolymorphicDrawer"/>.</summary>
    public interface IPolymorphic { }

    /************************************************************************************************************************/

    /// <summary>An <see cref="IPolymorphic"/> with a <see cref="Reset"/> method.</summary>
    public interface IPolymorphicReset : IPolymorphic
    {
        /// <summary>Called when an instance of this type is created in a [<see cref="SerializeReference"/>] field.</summary>
        void Reset();
    }

    /************************************************************************************************************************/

    /// <summary>The attributed field will be drawn by a <see cref="Editor.PolymorphicDrawer"/>.</summary>
    public sealed class PolymorphicAttribute : PropertyAttribute { }

    /************************************************************************************************************************/
}

#if UNITY_EDITOR

namespace Animancer.Editor
{
    using UnityEditor;

    /// <summary>[Editor-Only]
    /// A <see cref="PropertyDrawer"/> for <see cref="IPolymorphic"/> and <see cref="PolymorphicAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(IPolymorphic), true)]
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), true)]
    public class PolymorphicDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            using (new TypeSelectionButton(area, property, true))
                EditorGUI.PropertyField(area, property, label, true);
        }

        /************************************************************************************************************************/
    }
}

#endif
