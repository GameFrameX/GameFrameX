// Animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A button that allows the user to select an object type for a [<see cref="SerializeReference"/>] field.</summary>
    /// <example><code>
    /// public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
    /// {
    ///     using (new TypeSelectionButton(area, property, label, true))
    ///     {
    ///         EditorGUI.PropertyField(area, property, label, true);
    ///     }
    /// }
    /// </code></example>
    public readonly struct TypeSelectionButton : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The pixel area occupied by the button.</summary>
        public readonly Rect Area;

        /// <summary>The <see cref="SerializedProperty"/> representing the attributed field.</summary>
        public readonly SerializedProperty Property;

        /// <summary>The original <see cref="Event.type"/> from when this button was initialized.</summary>
        public readonly EventType EventType;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TypeSelectionButton"/>.</summary>
        public TypeSelectionButton(Rect area, SerializedProperty property, bool hasLabel)
        {
            area.height = AnimancerGUI.LineHeight;

            if (hasLabel)
                area.xMin += EditorGUIUtility.labelWidth + AnimancerGUI.StandardSpacing;

            var currentEvent = Event.current;

            Area = area;
            Property = property;
            EventType = currentEvent.type;

            if (Property.propertyType != SerializedPropertyType.ManagedReference)
                return;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                    if (area.Contains(currentEvent.mousePosition))
                        currentEvent.type = EventType.Ignore;
                    break;
            }
        }

        /************************************************************************************************************************/

        void IDisposable.Dispose() => DoGUI();

        /// <summary>Draws this button's GUI.</summary>
        /// <remarks>Run this method after drawing the target property so the button draws on top of its label.</remarks>
        public void DoGUI()
        {
            if (Property.propertyType != SerializedPropertyType.ManagedReference)
                return;

            var currentEvent = Event.current;
            var eventType = currentEvent.type;

            using (ObjectPool.Disposable.AcquireContent(out var label))
            {
                switch (EventType)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                        currentEvent.type = EventType;
                        break;

                    case EventType.Layout:
                        break;

                    // Only Repaint events actually care what the label is.
                    case EventType.Repaint:
                        var accessor = Property.GetAccessor();
                        var valueType = accessor.GetValue(Property)?.GetType();
                        if (valueType == null)
                        {
                            label.text = "Null";
                            label.tooltip = "Nothing is assigned";
                        }
                        else
                        {
                            label.text = valueType.GetNameCS(false);
                            label.tooltip = valueType.GetNameCS(true);
                        }
                        break;

                    default:
                        return;
                }

                if (GUI.Button(Area, label, EditorStyles.popup))
                    ShowTypeSelectorMenu(Property);
            }

            if (currentEvent.type == EventType)
                currentEvent.type = eventType;
        }

        /************************************************************************************************************************/

        /// <summary>Shows a menu to select which type of object to assign to the `property`.</summary>
        private void ShowTypeSelectorMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();

            UseFullNames.AddToggleFunction(menu);
            UseTypeHierarchy.AddToggleFunction(menu);

            var accessor = Property.GetAccessor();
            var fieldType = accessor.GetFieldElementType(property);
            var selectedType = accessor.GetValue(Property)?.GetType();

            AddTypeSelector(menu, property, fieldType, selectedType, null);

            var inheritors = GetDerivedTypes(fieldType);
            for (int i = 0; i < inheritors.Count; i++)
                AddTypeSelector(menu, property, fieldType, selectedType, inheritors[i]);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        /// <summary>Adds a menu function to assign a new instance of the `newType` to the `property`.</summary>
        private static void AddTypeSelector(
            GenericMenu menu, SerializedProperty property, Type fieldType, Type selectedType, Type newType)
        {
            var label = GetSelectorLabel(fieldType, newType);
            var state = selectedType == newType ? MenuFunctionState.Selected : MenuFunctionState.Normal;
            menu.AddPropertyModifierFunction(property, label, state, (targetProperty) =>
            {
                var oldValue = property.GetValue();
                var newValue = CreateDefaultInstance(newType);

                if (newValue is IPolymorphicReset reset)
                    reset.Reset();

                CopyCommonFields(oldValue, newValue);
                targetProperty.managedReferenceValue = newValue;
                targetProperty.isExpanded = true;
            });
        }

        /************************************************************************************************************************/

        private const string
            PrefKeyPrefix = nameof(TypeSelectionButton) + ".",
            PrefMenuPrefix = "Display Options/";

        private static readonly BoolPref
            UseFullNames = new BoolPref(PrefKeyPrefix + nameof(UseFullNames), PrefMenuPrefix + "Show Full Names", false),
            UseTypeHierarchy = new BoolPref(PrefKeyPrefix + nameof(UseTypeHierarchy), PrefMenuPrefix + "Show Type Hierarchy", false);

        private static string GetSelectorLabel(Type fieldType, Type newType)
        {
            if (newType == null)
                return "Null";

            if (!UseTypeHierarchy)
                return newType.GetNameCS(UseFullNames);

            var label = ObjectPool.AcquireStringBuilder();

            if (fieldType.IsInterface)// Interface.
            {
                while (true)
                {
                    if (label.Length > 0)
                        label.Insert(0, '/');

                    var displayType = newType.IsGenericType ?
                        newType.GetGenericTypeDefinition() :
                        newType;
                    label.Insert(0, displayType.GetNameCS(UseFullNames));

                    newType = newType.BaseType;

                    if (newType == null ||
                        !fieldType.IsAssignableFrom(newType))
                        break;
                }
            }
            else// Base Class.
            {
                while (true)
                {
                    if (label.Length > 0)
                        label.Insert(0, '/');

                    label.Insert(0, newType.GetNameCS(UseFullNames));

                    newType = newType.BaseType;

                    if (newType == null)
                        break;

                    if (fieldType.IsAbstract)
                    {
                        if (newType == fieldType)
                            break;
                    }
                    else
                    {
                        if (newType == fieldType.BaseType)
                            break;
                    }
                }
            }

            return label.ReleaseToString();
        }

        /************************************************************************************************************************/

        private static readonly List<Type>
            AllTypes = new List<Type>(1024);
        private static readonly Dictionary<Type, List<Type>>
            TypeToDerived = new Dictionary<Type, List<Type>>();

        /// <summary>Returns a list of all types that inherit from the `baseType`.</summary>
        public static List<Type> GetDerivedTypes(Type baseType)
        {
            if (!TypeToDerived.TryGetValue(baseType, out var derivedTypes))
            {
                if (AllTypes.Count == 0)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
                    {
                        var types = assemblies[iAssembly].GetTypes();
                        for (int iType = 0; iType < types.Length; iType++)
                        {
                            var type = types[iType];
                            if (IsViableType(type))
                                AllTypes.Add(type);
                        }
                    }

                    AllTypes.Sort((a, b) => a.FullName.CompareTo(b.FullName));
                }

                derivedTypes = new List<Type>();
                for (int i = 0; i < AllTypes.Count; i++)
                {
                    var type = AllTypes[i];
                    if (baseType.IsAssignableFrom(type))
                        derivedTypes.Add(type);
                }
                TypeToDerived.Add(baseType, derivedTypes);
            }

            return derivedTypes;
        }

        /************************************************************************************************************************/

        /// <summary>Is the `type` supported by <see cref="SerializeReference"/> fields?</summary>
        public static bool IsViableType(Type type) =>
            !type.IsAbstract &&
            !type.IsEnum &&
            !type.IsGenericTypeDefinition &&
            !type.IsInterface &&
            !type.IsPrimitive &&
            !type.IsSpecialName &&
            type.Name[0] != '<' &&
            type.IsDefined(typeof(SerializableAttribute), false) &&
            !type.IsDefined(typeof(ObsoleteAttribute), true) &&
            !typeof(Object).IsAssignableFrom(type) &&
            type.GetConstructor(AnimancerEditorUtilities.InstanceBindings, null, Type.EmptyTypes, null) != null;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new instance of the `type` using its parameterless constructor if it has one or a fully
        /// uninitialized object if it doesn't. Or returns <c>null</c> if the <see cref="Type.IsAbstract"/>.
        /// </summary>
        public static object CreateDefaultInstance(Type type)
        {
            if (type == null ||
                type.IsAbstract)
                return default;

            var constructor = type.GetConstructor(AnimancerEditorUtilities.InstanceBindings, null, Type.EmptyTypes, null);
            if (constructor != null)
                return constructor.Invoke(null);

            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// Creates a <typeparamref name="T"/> using its parameterless constructor if it has one or a fully
        /// uninitialized object if it doesn't. Or returns <c>null</c> if the <see cref="Type.IsAbstract"/>.
        /// </summary>
        public static T CreateDefaultInstance<T>() => (T)CreateDefaultInstance(typeof(T));

        /************************************************************************************************************************/

        /// <summary>
        /// Copies the values of all fields in `from` into corresponding fields in `to` as long as they have the same
        /// name and compatible types.
        /// </summary>
        public static void CopyCommonFields(object from, object to)
        {
            if (from == null ||
                to == null)
                return;

            var nameToFromField = new Dictionary<string, FieldInfo>();
            var fromType = from.GetType();
            do
            {
                var fromFields = fromType.GetFields(AnimancerEditorUtilities.InstanceBindings);

                for (int i = 0; i < fromFields.Length; i++)
                {
                    var field = fromFields[i];
                    nameToFromField[field.Name] = field;
                }

                fromType = fromType.BaseType;
            }
            while (fromType != null);

            var toType = to.GetType();
            do
            {
                var toFields = toType.GetFields(AnimancerEditorUtilities.InstanceBindings);

                for (int i = 0; i < toFields.Length; i++)
                {
                    var toField = toFields[i];
                    if (nameToFromField.TryGetValue(toField.Name, out var fromField))
                    {
                        var fromValue = fromField.GetValue(from);
                        if (fromValue == null || toField.FieldType.IsAssignableFrom(fromValue.GetType()))
                            toField.SetValue(to, fromValue);
                    }
                }

                toType = toType.BaseType;
            }
            while (toType != null);
        }

        /************************************************************************************************************************/
    }
}

#endif
