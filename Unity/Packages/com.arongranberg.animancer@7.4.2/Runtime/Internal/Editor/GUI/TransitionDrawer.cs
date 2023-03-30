// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using Animancer.Units;
using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="ITransitionDetailed"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionDrawer
    /// 
    [CustomPropertyDrawer(typeof(ITransitionDetailed), true)]
    public class TransitionDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <summary>The visual state of a drawer.</summary>
        private enum Mode
        {
            Uninitialized,
            Normal,
            AlwaysExpanded,
        }

        /// <summary>The current state of this drawer.</summary>
        private Mode _Mode;

        /************************************************************************************************************************/

        /// <summary>
        /// If set, the field with this name will be drawn on the header line with the foldout arrow instead of in its
        /// regular place.
        /// </summary>
        protected readonly string MainPropertyName;

        /// <summary>"." + <see cref="MainPropertyName"/> (to avoid creating garbage repeatedly).</summary>
        protected readonly string MainPropertyPathSuffix;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionDrawer"/>.</summary>
        public TransitionDrawer() { }

        /// <summary>Creates a new <see cref="TransitionDrawer"/> and sets the <see cref="MainPropertyName"/>.</summary>
        public TransitionDrawer(string mainPropertyName)
        {
            MainPropertyName = mainPropertyName;
            MainPropertyPathSuffix = "." + mainPropertyName;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the property specified by the <see cref="MainPropertyName"/>.</summary>
        private SerializedProperty GetMainProperty(SerializedProperty rootProperty)
        {
            if (MainPropertyName == null)
                return null;
            else
                return rootProperty.FindPropertyRelative(MainPropertyName);
        }

        /************************************************************************************************************************/

        /// <summary>Can't cache because it breaks the <see cref="TimelineGUI"/>.</summary>
        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        /************************************************************************************************************************/

        /// <summary>Returns the number of vertical pixels the `property` will occupy when it is drawn.</summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            using (DrawerContext.Get(property))
            {
                InitializeMode(property);

                var height = EditorGUI.GetPropertyHeight(property, label, true);

                if (property.isExpanded)
                {
                    if (property.propertyType != SerializedPropertyType.ManagedReference)
                    {
                        var mainProperty = GetMainProperty(property);
                        if (mainProperty != null)
                            height -= EditorGUI.GetPropertyHeight(mainProperty) + AnimancerGUI.StandardSpacing;
                    }

                    // The End Time from the Event Sequence is drawn out in the main transition so we need to add it.
                    // But rather than figuring out which array element actually holds the end time, we just use the
                    // Start Time field since it will have the same height.
                    var startTime = property.FindPropertyRelative(NormalizedStartTimeFieldName);
                    if (startTime != null)
                        height += EditorGUI.GetPropertyHeight(startTime) + AnimancerGUI.StandardSpacing;
                }

                return height;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the root `property` GUI and calls <see cref="DoChildPropertyGUI"/> for each of its children.</summary>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            InitializeMode(property);

            // Highlight the whole area if this transition is currently being previewed.
            var isPreviewing = TransitionPreviewWindow.IsPreviewing(property);
            if (isPreviewing)
            {
                var highlightArea = area;
                highlightArea.xMin -= AnimancerGUI.IndentSize;
                EditorGUI.DrawRect(highlightArea, new Color(0.35f, 0.5f, 1, 0.2f));
            }

            var headerArea = area;

            if (property.propertyType == SerializedPropertyType.ManagedReference)
                DoPreviewButtonGUI(ref headerArea, property, isPreviewing);

            using (new TypeSelectionButton(headerArea, property, true))
            {
                DoPropertyGUI(area, property, label, isPreviewing);
            }
        }

        /************************************************************************************************************************/

        private void DoPropertyGUI(Rect area, SerializedProperty property, GUIContent label, bool isPreviewing)
        {
            using (DrawerContext.Get(property))
            {
                if (Context.Transition == null)
                {
                    EditorGUI.PrefixLabel(area, label);
                    return;
                }

                var indent = !string.IsNullOrEmpty(label.text);

                EditorGUI.BeginChangeCheck();

                var mainProperty = GetMainProperty(property);
                DoHeaderGUI(ref area, property, mainProperty, label, isPreviewing);
                DoChildPropertiesGUI(area, property, mainProperty, indent);

                if (EditorGUI.EndChangeCheck() && isPreviewing)
                    TransitionPreviewWindow.PreviewNormalizedTime = TransitionPreviewWindow.PreviewNormalizedTime;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="_Mode"/> is <see cref="Mode.Uninitialized"/>, this method determines how it should start
        /// based on the number of properties in the `serializedObject`. If the only serialized field is an
        /// <see cref="ITransition"/> then it should start expanded.
        /// </summary>
        protected void InitializeMode(SerializedProperty property)
        {
            if (_Mode == Mode.Uninitialized)
            {
                _Mode = Mode.AlwaysExpanded;

                var iterator = property.serializedObject.GetIterator();
                iterator.Next(true);

                var count = 0;
                do
                {
                    switch (iterator.propertyPath)
                    {
                        case "m_ObjectHideFlags":
                        case "m_Script":
                            break;

                        default:
                            count++;
                            if (count > 1)
                            {
                                _Mode = Mode.Normal;
                                return;
                            }
                            break;
                    }
                }
                while (iterator.NextVisible(false));
            }

            if (_Mode == Mode.AlwaysExpanded)
                property.isExpanded = true;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the root property of a transition with an optional main property on the same line.</summary>
        protected virtual void DoHeaderGUI(
            ref Rect area,
            SerializedProperty rootProperty,
            SerializedProperty mainProperty,
            GUIContent label,
            bool isPreviewing)
        {
            area.height = AnimancerGUI.LineHeight;
            var labelArea = area;
            AnimancerGUI.NextVerticalArea(ref area);

#if UNITY_2022_2_OR_NEWER
            EditorGUI.indentLevel++;
            labelArea = EditorGUI.IndentedRect(labelArea);
            EditorGUI.indentLevel--;
#endif

            if (rootProperty.propertyType != SerializedPropertyType.ManagedReference)
                DoPreviewButtonGUI(ref labelArea, rootProperty, isPreviewing);

            // Draw the Root Property after the Main Property to give better spacing between the label and field.

            // Drawing the main property might assign its details to the label so we keep our own copy.
            using (ObjectPool.Disposable.AcquireContent(out var rootLabel, label.text, label.tooltip))
            {
                // Main Property.

                DoMainPropertyGUI(labelArea, out labelArea, rootProperty, mainProperty);

                // Root Property.

                rootLabel = EditorGUI.BeginProperty(labelArea, rootLabel, rootProperty);
                EditorGUI.LabelField(labelArea, rootLabel);
                EditorGUI.EndProperty();

                if (_Mode != Mode.AlwaysExpanded)
                {
                    var hierarchyMode = EditorGUIUtility.hierarchyMode;
                    EditorGUIUtility.hierarchyMode = true;

                    rootProperty.isExpanded =
                        EditorGUI.Foldout(labelArea, rootProperty.isExpanded, GUIContent.none, true);

                    EditorGUIUtility.hierarchyMode = hierarchyMode;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI the the target transition's main property.</summary>
        protected virtual void DoMainPropertyGUI(Rect area, out Rect labelArea,
            SerializedProperty rootProperty, SerializedProperty mainProperty)
        {
            labelArea = area;
            if (mainProperty == null)
                return;

            var fullArea = area;

            var labelWidth = EditorGUIUtility.labelWidth;
#if UNITY_2022_2_OR_NEWER
            EditorGUIUtility.labelWidth -= AnimancerGUI.LineHeight - AnimancerGUI.StandardSpacing - 1;
#endif

            labelArea = AnimancerGUI.StealFromLeft(ref area, EditorGUIUtility.labelWidth, AnimancerGUI.StandardSpacing);

            var mainPropertyReferenceIsMissing =
                mainProperty.propertyType == SerializedPropertyType.ObjectReference &&
                mainProperty.objectReferenceValue == null;

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            if (rootProperty.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (rootProperty.isExpanded || _Mode == Mode.AlwaysExpanded)
                {
#if !UNITY_2022_2_OR_NEWER
                    EditorGUI.indentLevel++;
#endif

                    AnimancerGUI.NextVerticalArea(ref fullArea);
                    using (ObjectPool.Disposable.AcquireContent(out var label, mainProperty))
                        EditorGUI.PropertyField(fullArea, mainProperty, label, true);

#if !UNITY_2022_2_OR_NEWER
                    EditorGUI.indentLevel--;
#endif
                }
            }
            else
            {
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.PropertyField(area, mainProperty, GUIContent.none, true);

                EditorGUI.indentLevel = indentLevel;
            }

#if UNITY_2022_2_OR_NEWER
            EditorGUIUtility.labelWidth = labelWidth;
#endif

            EditorGUIUtility.hierarchyMode = hierarchyMode;

            // If the main Object reference was just assigned and all fields were at their type default,
            // reset the value to run its default constructor and field initializers then reassign the reference.
            var reference = mainProperty.objectReferenceValue;
            if (mainPropertyReferenceIsMissing && reference != null)
            {
                mainProperty.objectReferenceValue = null;
                if (Serialization.IsDefaultValueByType(rootProperty))
                    rootProperty.GetAccessor().ResetValue(rootProperty);
                mainProperty.objectReferenceValue = reference;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws a small button using the <see cref="TransitionPreviewWindow.Icon"/>.</summary>
        private static void DoPreviewButtonGUI(ref Rect area, SerializedProperty property, bool isPreviewing)
        {
            if (property.serializedObject.targetObjects.Length != 1 ||
                !TransitionPreviewWindow.CanBePreviewed(property))
                return;

            var enabled = GUI.enabled;
            var currentEvent = Event.current;
            if (currentEvent.button == 1)// Ignore Right Clicks on the Preview Button.
            {
                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                    case EventType.ContextClick:
                        GUI.enabled = false;
                        break;
                }
            }

            var tooltip = isPreviewing ? TransitionPreviewWindow.Inspector.CloseTooltip : "Preview this transition";

            if (DoPreviewButtonGUI(ref area, isPreviewing, tooltip))
                TransitionPreviewWindow.OpenOrClose(property);

            GUI.enabled = enabled;
        }

        /// <summary>Draws a small button using the <see cref="TransitionPreviewWindow.Icon"/>.</summary>
        public static bool DoPreviewButtonGUI(ref Rect area, bool selected, string tooltip)
        {
            var width = AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing * 2;
            var buttonArea = AnimancerGUI.StealFromRight(ref area, width, AnimancerGUI.StandardSpacing);
            buttonArea.height = AnimancerGUI.LineHeight;

            using (ObjectPool.Disposable.AcquireContent(out var content, "", tooltip))
            {
                content.image = TransitionPreviewWindow.Icon;

                return GUI.Toggle(buttonArea, selected, content, PreviewButtonStyle) != selected;
            }
        }

        /************************************************************************************************************************/

        private static GUIStyle _PreviewButtonStyle;

        /// <summary>The style used for the button that opens the <see cref="TransitionPreviewWindow"/>.</summary>
        public static GUIStyle PreviewButtonStyle
        {
            get
            {
                if (_PreviewButtonStyle == null)
                {
                    _PreviewButtonStyle = new GUIStyle(AnimancerGUI.MiniButton)
                    {
                        padding = new RectOffset(0, 0, 0, 1),
                        fixedWidth = 0,
                        fixedHeight = 0,
                    };
                }

                return _PreviewButtonStyle;
            }
        }

        /************************************************************************************************************************/

        private void DoChildPropertiesGUI(Rect area, SerializedProperty rootProperty, SerializedProperty mainProperty, bool indent)
        {
            if (!rootProperty.isExpanded && _Mode != Mode.AlwaysExpanded)
                return;

            // Skip over the main property if it was already drawn by the header.
            if (rootProperty.propertyType == SerializedPropertyType.ManagedReference &&
                mainProperty != null)
                AnimancerGUI.NextVerticalArea(ref area);

            if (indent)
                EditorGUI.indentLevel++;

            var property = rootProperty.Copy();

            SerializedProperty eventsProperty = null;

            var depth = property.depth;
            property.NextVisible(true);
            while (property.depth > depth)
            {
                // Grab the Events property and draw it last.
                var path = property.propertyPath;
                if (eventsProperty == null && path.EndsWith("._Events"))
                {
                    eventsProperty = property.Copy();
                }
                // Don't draw the main property again.
                else if (mainProperty != null && path.EndsWith(MainPropertyPathSuffix))
                {
                }
                else
                {
                    if (eventsProperty != null)
                    {
                        var type = Context.Transition.GetType();
                        var accessor = property.GetAccessor();
                        var field = Serialization.GetField(type, accessor.Name);
                        if (field != null && field.IsDefined(typeof(DrawAfterEventsAttribute), false))
                        {
                            using (ObjectPool.Disposable.AcquireContent(out var eventsLabel, eventsProperty))
                                DoChildPropertyGUI(ref area, rootProperty, eventsProperty, eventsLabel);
                            AnimancerGUI.NextVerticalArea(ref area);
                            eventsProperty = null;
                        }
                    }

                    using (ObjectPool.Disposable.AcquireContent(out var label, property))
                        DoChildPropertyGUI(ref area, rootProperty, property, label);
                    AnimancerGUI.NextVerticalArea(ref area);
                }

                if (!property.NextVisible(false))
                    break;
            }

            if (eventsProperty != null)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, eventsProperty))
                    DoChildPropertyGUI(ref area, rootProperty, eventsProperty, label);
            }

            if (indent)
                EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the `property` GUI in relation to the `rootProperty` which was passed into <see cref="OnGUI"/>.
        /// </summary>
        protected virtual void DoChildPropertyGUI(ref Rect area, SerializedProperty rootProperty,
            SerializedProperty property, GUIContent label)
        {
            // If we keep using the GUIContent that was passed into OnGUI then GetPropertyHeight will change it to
            // match the 'property' which we don't want.

            using (ObjectPool.Disposable.AcquireContent(out var content, label.text, label.tooltip, false))
            {
                area.height = EditorGUI.GetPropertyHeight(property, content, true);

                if (TryDoStartTimeField(ref area, rootProperty, property, content))
                    return;

#if !UNITY_2022_2_OR_NEWER
                if (!EditorGUIUtility.hierarchyMode)
                    EditorGUI.indentLevel++;
#endif

                EditorGUI.PropertyField(area, property, content, true);

#if !UNITY_2022_2_OR_NEWER
                if (!EditorGUIUtility.hierarchyMode)
                    EditorGUI.indentLevel--;
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>The name of the backing field of <c>ClipTransition.NormalizedStartTime</c>.</summary>
        public const string NormalizedStartTimeFieldName = "_NormalizedStartTime";

        /// <summary>
        /// If the `property` is a "Start Time" field, this method draws it as well as the "End Time" below it and
        /// returns true.
        /// </summary>
        public static bool TryDoStartTimeField(ref Rect area, SerializedProperty rootProperty,
            SerializedProperty property, GUIContent label)
        {
            if (!property.propertyPath.EndsWith("." + NormalizedStartTimeFieldName))
                return false;

            // Start Time.
            label.text = AnimancerGUI.GetNarrowText("Start Time");
            AnimationTimeAttribute.nextDefaultValue = AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(Context.Transition.Speed);
            EditorGUI.PropertyField(area, property, label, false);

            AnimancerGUI.NextVerticalArea(ref area);

            // End Time.
            var events = rootProperty.FindPropertyRelative("_Events");
            using (var context = SerializableEventSequenceDrawer.Context.Get(events))
            {
                var areaCopy = area;
                var index = Mathf.Max(0, context.Times.Count - 1);
                SerializableEventSequenceDrawer.DoTimeGUI(ref areaCopy, context, index, true);
            }

            return true;
        }

        /************************************************************************************************************************/
        #region Context
        /************************************************************************************************************************/

        /// <summary>The current <see cref="DrawerContext"/>.</summary>
        public static DrawerContext Context => DrawerContext.Stack.Current;

        /************************************************************************************************************************/

        /// <summary>Details of an <see cref="ITransition"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer.Editor/DrawerContext
        /// 
        public class DrawerContext : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The main property representing the <see cref="ITransition"/> field.</summary>
            public SerializedProperty Property { get; private set; }

            /// <summary>The actual transition object rerieved from the <see cref="Property"/>.</summary>
            public ITransitionDetailed Transition { get; private set; }

            /// <summary>The cached value of <see cref="ITransitionDetailed.MaximumDuration"/>.</summary>
            public float MaximumDuration { get; private set; }

            /************************************************************************************************************************/

            /// <summary>The stack of active contexts.</summary>
            public static readonly LazyStack<DrawerContext> Stack = new LazyStack<DrawerContext>();

            /// <summary>Returns a disposable <see cref="DrawerContext"/> representing the specified parameters.</summary>
            /// <remarks>
            /// Instances are stored in a <see cref="LazyStack{T}"/> and the current one can be accessed via
            /// <see cref="Context"/>.
            /// </remarks>
            public static IDisposable Get(SerializedProperty transitionProperty)
            {
                var context = Stack.Increment();

                context.Property = transitionProperty;
                context.Transition = transitionProperty.GetValue<ITransitionDetailed>();

                AnimancerUtilities.TryGetLength(context.Transition, out var length);
                context.MaximumDuration = length;

                EditorGUI.BeginChangeCheck();

                return context;
            }

            /************************************************************************************************************************/

            /// <summary>Decrements the <see cref="Stack"/>.</summary>
            public void Dispose()
            {
                var context = Stack.Current;

                if (EditorGUI.EndChangeCheck())
                    context.Property.serializedObject.ApplyModifiedProperties();

                context.Property = null;
                context.Transition = null;

                Stack.Decrement();
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

