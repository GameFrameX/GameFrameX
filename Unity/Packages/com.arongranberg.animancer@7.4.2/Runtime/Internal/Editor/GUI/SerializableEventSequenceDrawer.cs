// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;
using Sequence = Animancer.AnimancerEvent.Sequence.Serializable;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Sequence"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SerializableEventSequenceDrawer
    /// 
    [CustomPropertyDrawer(typeof(Sequence), true)]
    public class SerializableEventSequenceDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <summary><see cref="AnimancerGUI.RepaintEverything"/></summary>
        public static UnityAction Repaint = AnimancerGUI.RepaintEverything;

        private readonly Dictionary<string, List<AnimBool>>
            EventVisibility = new Dictionary<string, List<AnimBool>>();

        private AnimBool GetVisibility(Context context, int index)
        {
            var path = context.Property.propertyPath;
            if (!EventVisibility.TryGetValue(path, out var list))
                EventVisibility.Add(path, list = new List<AnimBool>());

            while (list.Count <= index)
            {
                var visible = context.Property.isExpanded || context.SelectedEvent == index;
                list.Add(new AnimBool(visible, Repaint));
            }

            return list[index];
        }

        /************************************************************************************************************************/

        /// <summary>Can't cache because it breaks the <see cref="TimelineGUI"/>.</summary>
        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the number of vertical pixels the `property` will occupy when it is drawn.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.hasMultipleDifferentValues)
                return AnimancerGUI.LineHeight;

            using (var context = Context.Get(property))
            {
                var height = AnimancerGUI.LineHeight;

                var count = Math.Max(1, context.Times.Count);
                for (int i = 0; i < count; i++)
                {
                    height += CalculateEventHeight(context, i) * GetVisibility(context, i).faded;
                }

                var events = context.Sequence?.InitializedEvents;
                if (events != null)
                    height += EventSequenceDrawer.Get(events).CalculateHeight(events) + AnimancerGUI.StandardSpacing;

                return height;
            }
        }

        /************************************************************************************************************************/

        private float CalculateEventHeight(Context context, int index)
        {
            // Name.
            var height = index < context.Times.Count - 1 ?
                AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing :
                0;// End Events don't have a Name.

            // Time.
            height += EventTimeAttribute.GetPropertyHeight(null, null) + AnimancerGUI.StandardSpacing;

            // Callback.
            if (!AnimancerSettings.HideEventCallbacks || context.Callbacks.Count > 0)
            {
                height += index < context.Callbacks.Count ?
                    EditorGUI.GetPropertyHeight(context.Callbacks.GetElement(index), null, false) :
                    DummySerializableCallback.Height;
                height += AnimancerGUI.StandardSpacing;
            }

            return height;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the `property`.</summary>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            using (var context = Context.Get(property))
            {
                DoHeaderGUI(ref area, label, context);

                if (property.hasMultipleDifferentValues)
                    return;

                EditorGUI.indentLevel++;
                DoAllEventsGUI(ref area, context);
                EditorGUI.indentLevel--;

                var sequence = context.Sequence?.InitializedEvents;
                if (sequence != null)
                {
                    using (ObjectPool.Disposable.AcquireContent(out label, "Runtime Events",
                        $"The runtime {nameof(AnimancerEvent)}.{nameof(Sequence)} created from the serialized data above"))
                    {
                        EventSequenceDrawer.Get(sequence).Draw(ref area, sequence, label);
                    }
                }
            }
        }

        /************************************************************************************************************************/

        private void DoHeaderGUI(ref Rect area, GUIContent label, Context context)
        {
#if UNITY_2022_2_OR_NEWER
            if (!EditorGUIUtility.hierarchyMode)
                EditorGUI.indentLevel--;
#endif

            area.height = AnimancerGUI.LineHeight;
            var headerArea = area;
            AnimancerGUI.NextVerticalArea(ref area);

            label = EditorGUI.BeginProperty(headerArea, label, context.Property);

            if (!context.Property.hasMultipleDifferentValues)
            {
                var addEventArea = AnimancerGUI.StealFromRight(ref headerArea, headerArea.height, AnimancerGUI.StandardSpacing);
                DoAddRemoveEventButtonGUI(addEventArea, context);
            }

            if (context.TransitionContext != null && context.TransitionContext.Transition != null)
            {
                EditorGUI.EndProperty();

                TimelineGUI.DoGUI(headerArea, context, out var addEventNormalizedTime);

                if (!float.IsNaN(addEventNormalizedTime))
                {
                    AddEvent(context, addEventNormalizedTime);
                }
            }
            else
            {
                label.text = AnimancerGUI.GetNarrowText(label.text);

                string summary;
                if (context.Times.Count == 0)
                {
                    summary = "[0] End Time 1";
                }
                else
                {
                    var index = context.Times.Count - 1;
                    var endTime = context.Times.GetElement(index).floatValue;
                    summary = $"[{index}] End Time {endTime:G3}";
                }

                using (ObjectPool.Disposable.AcquireContent(out var content, summary))
                    EditorGUI.LabelField(headerArea, label, content);

                EditorGUI.EndProperty();
            }

            EditorGUI.BeginChangeCheck();
            context.Property.isExpanded =
                EditorGUI.Foldout(headerArea, context.Property.isExpanded, GUIContent.none, true);
            if (EditorGUI.EndChangeCheck())
                context.SelectedEvent = -1;

#if UNITY_2022_2_OR_NEWER
            if (!EditorGUIUtility.hierarchyMode)
                EditorGUI.indentLevel++;
#endif
        }

        /************************************************************************************************************************/

        private static readonly int EventTimeHash = "EventTime".GetHashCode();

        private static int _HotControlAdjustRoot;
        private static int _SelectedEventToHotControl;

        private void DoAllEventsGUI(ref Rect area, Context context)
        {
            var currentEvent = Event.current;
            var originalEventType = currentEvent.type;
            if (originalEventType == EventType.Used)
                return;

            var rootControlID = GUIUtility.GetControlID(EventTimeHash - 1, FocusType.Passive);

            var warnings = OptionalWarning.LockedEvents.DisableTemporarily();

            var eventCount = Mathf.Max(1, context.Times.Count);
            for (int i = 0; i < eventCount; i++)
            {
                var controlID = GUIUtility.GetControlID(EventTimeHash + i, FocusType.Passive);

                if (rootControlID == _HotControlAdjustRoot &&
                    _SelectedEventToHotControl > 0 &&
                    i == context.SelectedEvent)
                {
                    GUIUtility.hotControl = GUIUtility.keyboardControl = controlID + _SelectedEventToHotControl;
                    _SelectedEventToHotControl = 0;
                    _HotControlAdjustRoot = -1;
                }

                DoEventGUI(ref area, context, i, false);

                if (currentEvent.type == EventType.Used && originalEventType == EventType.MouseUp)
                {
                    context.SelectedEvent = i;

                    if (SortEvents(context))
                    {
                        _SelectedEventToHotControl = GUIUtility.keyboardControl - controlID;
                        _HotControlAdjustRoot = rootControlID;
                        AnimancerGUI.Deselect();
                    }

                    GUIUtility.ExitGUI();
                }
            }

            warnings.Enable();
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI fields for the event at the specified `index`.</summary>
        public void DoEventGUI(ref Rect area, Context context, int index, bool autoSort)
        {
            GetEventLabels(index, context,
                out var nameLabel, out var timeLabel, out var callbackLabel, out var defaultTime, out var isEndEvent);

            var y = area.y;

            var visibility = GetVisibility(context, index);
            visibility.target = context.Property.isExpanded || context.SelectedEvent == index;

            var x = area.xMin;
            area.xMin = 0;

            area.height = CalculateEventHeight(context, index) * visibility.faded;
            GUI.BeginGroup(area, GUIStyle.none);

            if (visibility.faded > 0)
            {
                area.xMin = x;
                area.y = 0;

                DoNameGUI(ref area, context, index, nameLabel);
                DoTimeGUI(ref area, context, index, autoSort, timeLabel, defaultTime, isEndEvent);
                DoCallbackGUI(ref area, context, index, autoSort, callbackLabel);

                area.y = area.y * visibility.faded + y;
                area.height *= visibility.faded;
            }

            GUI.EndGroup();

            area.xMin = x;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the time field for the event at the specified `index`.</summary>
        public static void DoNameGUI(ref Rect area, Context context, int index, string nameLabel)
        {
            if (nameLabel == null)
                return;

            EditorGUI.BeginChangeCheck();
            string name;

            area.height = AnimancerGUI.LineHeight;
            var fieldArea = area;
            AnimancerGUI.NextVerticalArea(ref area);

            using (ObjectPool.Disposable.AcquireContent(out var label, nameLabel,
                "An optional name which can be used to identify the event in code." +
                " Leaving all names blank is recommended if you are not using them."))
            {
                fieldArea = EditorGUI.PrefixLabel(fieldArea, label);
            }

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (index < context.Names.Count)
            {
                var nameProperty = context.Names.GetElement(index);

                EditorGUI.BeginProperty(fieldArea, GUIContent.none, nameProperty);

                EditorGUI.BeginChangeCheck();
                name = DoEventNameTextField(fieldArea, context, nameProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                    nameProperty.stringValue = name;

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginProperty(fieldArea, GUIContent.none, context.Names.Property);
                name = DoEventNameTextField(fieldArea, context, "");
                EditorGUI.EndProperty();

                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(name))
                {
                    context.Names.Count++;
                    if (context.Names.Count < index + 1)
                    {
                        var nextProperty = context.Names.GetElement(context.Names.Count - 1);
                        nextProperty.stringValue = "";
                        context.Names.Count = index + 1;
                    }

                    var nameProperty = context.Names.GetElement(index);
                    nameProperty.stringValue = name;
                }
            }

            EditorGUI.indentLevel = indentLevel;

            if (EditorGUI.EndChangeCheck())
            {
                var events = context.Sequence?.InitializedEvents;
                if (events != null)
                    events.SetName(index, name);
            }
        }

        private static string DoEventNameTextField(Rect area, Context context, string text)
        {
            var eventNames = AttributeCache<EventNamesAttribute>.FindAttribute(context.TransitionContext.Property)?.Names;
            if (eventNames.IsNullOrEmpty())
                return EditorGUI.TextField(area, text);

            var index = text == "" ? 0 : Array.IndexOf(eventNames, text, 1);

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(area, index, eventNames);
            if (EditorGUI.EndChangeCheck())
                return index > 0 ? eventNames[index] : "";
            else
                return text;
        }

        /************************************************************************************************************************/

        private static readonly AnimationTimeAttribute
            EventTimeAttribute = new AnimationTimeAttribute(AnimationTimeAttribute.Units.Normalized);

        private static float _PreviousTime = float.NaN;

        /// <summary>Draws the time field for the event at the specified `index`.</summary>
        public static void DoTimeGUI(ref Rect area, Context context, int index, bool autoSort,
            string timeLabel, float defaultTime, bool isEndEvent)
        {
            EditorGUI.BeginChangeCheck();

            area.height = EventTimeAttribute.GetPropertyHeight(null, null);
            var timeArea = area;
            AnimancerGUI.NextVerticalArea(ref area);

            float normalizedTime;

            using (ObjectPool.Disposable.AcquireContent(out var label, timeLabel,
                isEndEvent ? Strings.Tooltips.EndTime : Strings.Tooltips.CallbackTime))
            {
                var length = context.TransitionContext?.MaximumDuration ?? float.NaN;

                if (index < context.Times.Count)
                {
                    var timeProperty = context.Times.GetElement(index);
                    if (timeProperty == null)// Multi-selection screwed up the property retrieval.
                    {
                        EditorGUI.BeginChangeCheck();

                        label = EditorGUI.BeginProperty(timeArea, label, context.Times.Property);
                        if (isEndEvent)
                            AnimationTimeAttribute.nextDefaultValue = defaultTime;
                        normalizedTime = float.NaN;
                        EventTimeAttribute.OnGUI(timeArea, label, ref normalizedTime);

                        EditorGUI.EndProperty();

                        if (EditorGUI.EndChangeCheck())
                        {
                            context.Times.Count = context.Times.Count;
                            timeProperty = context.Times.GetElement(index);
                            timeProperty.floatValue = normalizedTime;
                            SyncEventTimeChange(context, index, normalizedTime);
                        }
                    }
                    else// Event time property was correctly retrieved.
                    {
                        var wasEditingTextField = EditorGUIUtility.editingTextField;
                        if (!wasEditingTextField)
                            _PreviousTime = float.NaN;

                        EditorGUI.BeginChangeCheck();

                        label = EditorGUI.BeginProperty(timeArea, label, timeProperty);

                        if (isEndEvent)
                            AnimationTimeAttribute.nextDefaultValue = defaultTime;
                        normalizedTime = timeProperty.floatValue;
                        EventTimeAttribute.OnGUI(timeArea, label, ref normalizedTime);

                        EditorGUI.EndProperty();

                        if (AnimancerGUI.TryUseClickEvent(timeArea, 2))
                            normalizedTime = float.NaN;

                        var isEditingTextField = EditorGUIUtility.editingTextField;
                        if (EditorGUI.EndChangeCheck() || (wasEditingTextField && !isEditingTextField))
                        {
                            if (float.IsNaN(normalizedTime))
                            {
                                RemoveEvent(context, index);
                                AnimancerGUI.Deselect();
                            }
                            else if (isEndEvent)
                            {
                                timeProperty.floatValue = normalizedTime;
                                SyncEventTimeChange(context, index, normalizedTime);
                            }
                            else if (!autoSort && isEditingTextField)
                            {
                                _PreviousTime = normalizedTime;
                            }
                            else
                            {
                                if (!float.IsNaN(_PreviousTime))
                                {
                                    if (Event.current.keyCode != KeyCode.Escape)
                                    {
                                        normalizedTime = _PreviousTime;
                                        AnimancerGUI.Deselect();
                                    }

                                    _PreviousTime = float.NaN;
                                }

                                WrapEventTime(context, ref normalizedTime);

                                timeProperty.floatValue = normalizedTime;
                                SyncEventTimeChange(context, index, normalizedTime);

                                if (autoSort)
                                    SortEvents(context);
                            }

                            GUI.changed = true;
                        }
                    }
                }
                else// Dummy End Event (when there are no event times).
                {
                    AnimancerUtilities.Assert(index == 0, "Dummy end event index != 0");
                    EditorGUI.BeginChangeCheck();

                    EditorGUI.BeginProperty(timeArea, GUIContent.none, context.Times.Property);

                    AnimationTimeAttribute.nextDefaultValue = defaultTime;
                    normalizedTime = float.NaN;
                    EventTimeAttribute.OnGUI(timeArea, label, ref normalizedTime);

                    EditorGUI.EndProperty();

                    if (EditorGUI.EndChangeCheck() && !float.IsNaN(normalizedTime))
                    {
                        context.Times.Count = 1;
                        var timeProperty = context.Times.GetElement(0);
                        timeProperty.floatValue = normalizedTime;
                        SyncEventTimeChange(context, 0, normalizedTime);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                var eventType = Event.current.type;
                if (eventType == EventType.Layout)
                    return;

                if (eventType == EventType.Used)
                {
                    normalizedTime = UnitsAttribute.GetDisplayValue(normalizedTime, defaultTime);
                    TransitionPreviewWindow.PreviewNormalizedTime = normalizedTime;
                }

                GUIUtility.ExitGUI();
            }
        }

        /// <summary>Draws the time field for the event at the specified `index`.</summary>
        public static void DoTimeGUI(ref Rect area, Context context, int index, bool autoSort)
        {
            GetEventLabels(index, context, out var _, out var timeLabel, out var _, out var defaultTime, out var isEndEvent);
            DoTimeGUI(ref area, context, index, autoSort, timeLabel, defaultTime, isEndEvent);
        }

        /************************************************************************************************************************/

        /// <summary>Updates the <see cref="Sequence.Events"/> to accomodate a changed event time.</summary>
        public static void SyncEventTimeChange(Context context, int index, float normalizedTime)
        {
            var events = context.Sequence?.InitializedEvents;
            if (events == null)
                return;

            if (index == events.Count)// End Event.
            {
                events.NormalizedEndTime = normalizedTime;
            }
            else// Regular Event.
            {
                events.SetNormalizedTime(index, normalizedTime);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI fields for the event at the specified `index`.</summary>
        public static void DoCallbackGUI(ref Rect area, Context context, int index, bool autoSort, string callbackLabel)
        {
            if (AnimancerSettings.HideEventCallbacks && context.Callbacks.Count == 0)
                return;

            EditorGUI.BeginChangeCheck();

            using (ObjectPool.Disposable.AcquireContent(out var label, callbackLabel))
            {
                if (index < context.Callbacks.Count)
                {
                    var callback = context.Callbacks.GetElement(index);
                    area.height = EditorGUI.GetPropertyHeight(callback, false);
                    EditorGUI.BeginProperty(area, GUIContent.none, callback);

                    // UnityEvents ignore the proper indentation which makes them look terrible in a list.
                    // So we force the area to be indented.
                    var indentedArea = EditorGUI.IndentedRect(area);
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    EditorGUI.PropertyField(indentedArea, callback, label, false);

                    EditorGUI.indentLevel = indentLevel;
                    EditorGUI.EndProperty();
                }
                else if (DummySerializableCallback.DoCallbackGUI(ref area, label, context.Callbacks.Property, out var callback))
                {
                    try
                    {
                        Sequence.DisableCompactArrays = true;

                        if (index >= context.Times.Count)
                        {
                            context.Times.Property.InsertArrayElementAtIndex(index);
                            context.Times.Count++;
                            context.Times.GetElement(index).floatValue = float.NaN;
                            context.Times.Property.serializedObject.ApplyModifiedProperties();
                        }

                        context.Callbacks.Property.ForEachTarget((callbacksProperty) =>
                        {
                            var accessor = callbacksProperty.GetAccessor();
                            var oldCallbacks = (Array)accessor.GetValue(callbacksProperty.serializedObject.targetObject);

                            Array newCallbacks;
                            if (oldCallbacks == null)
                            {
                                var elementType = accessor.GetFieldElementType(callbacksProperty);
                                newCallbacks = Array.CreateInstance(elementType, 1);
                            }
                            else
                            {
                                var elementType = oldCallbacks.GetType().GetElementType();
                                newCallbacks = Array.CreateInstance(elementType, index + 1);
                                Array.Copy(oldCallbacks, newCallbacks, oldCallbacks.Length);
                            }

                            newCallbacks.SetValue(callback, index);
                            accessor.SetValue(callbacksProperty, newCallbacks);
                        });

                        context.Callbacks.Property.OnPropertyChanged();
                        context.Callbacks.Refresh();
                    }
                    finally
                    {
                        Sequence.DisableCompactArrays = false;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (index < context.Callbacks.Count)
                {
                    var events = context.Sequence?.InitializedEvents;
                    if (events != null)
                    {
                        var animancerEvent = index < events.Count ? events[index] : events.EndEvent;
                        if (AnimancerEvent.IsNullOrDummy(animancerEvent.callback))
                        {
                            context.Callbacks.Property.serializedObject.ApplyModifiedProperties();
                            var property = context.Callbacks.GetElement(index);
                            var callback = property.GetValue();
                            var invoker = Sequence.GetInvoker(callback);
                            if (index < events.Count)
                                events.SetCallback(index, invoker);
                            else
                                events.OnEnd = invoker;
                        }
                    }
                }
            }

            AnimancerGUI.NextVerticalArea(ref area);
        }

        /************************************************************************************************************************/

        private static ConversionCache<int, string> _NameLabelCache, _TimeLabelCache, _CallbackLabelCache;

        private static void GetEventLabels(int index, Context context,
            out string nameLabel, out string timeLabel, out string callbackLabel, out float defaultTime, out bool isEndEvent)
        {
            if (index >= context.Times.Count - 1)
            {
                nameLabel = null;
                timeLabel = "End Time";
                callbackLabel = "End Callback";

                defaultTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(
                    context.TransitionContext?.Transition?.Speed ?? 1);
                isEndEvent = true;
            }
            else
            {
                if (_NameLabelCache == null)
                {
                    _NameLabelCache = new ConversionCache<int, string>((i) => $"Event {i} Name");
                    _TimeLabelCache = new ConversionCache<int, string>((i) => $"Event {i} Time");
                    _CallbackLabelCache = new ConversionCache<int, string>((i) => $"Event {i} Callback");
                }

                nameLabel = _NameLabelCache.Convert(index);
                timeLabel = _TimeLabelCache.Convert(index);
                callbackLabel = _CallbackLabelCache.Convert(index);

                defaultTime = 0;
                isEndEvent = false;
            }
        }

        /************************************************************************************************************************/

        private static void WrapEventTime(Context context, ref float normalizedTime)
        {
            var transition = context.TransitionContext?.Transition;
            if (transition != null && transition.IsLooping)
            {
                if (normalizedTime == 0)
                    return;
                else if (normalizedTime % 1 == 0)
                    normalizedTime = AnimancerEvent.AlmostOne;
                else
                    normalizedTime = AnimancerUtilities.Wrap01(normalizedTime);
            }
        }

        /************************************************************************************************************************/
        #region Event Modification
        /************************************************************************************************************************/

        private static GUIStyle _AddRemoveEventStyle;
        private static GUIContent _AddEventContent;

        /// <summary>Draws a button to add a new event or remove the selected one.</summary>
        public void DoAddRemoveEventButtonGUI(Rect area, Context context)
        {
            if (_AddRemoveEventStyle == null)
                _AddRemoveEventStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    fixedHeight = 0,
                };

            if (ShowAddButton(context))
            {
                if (_AddEventContent == null)
                    _AddEventContent = EditorGUIUtility.IconContent("Animation.AddEvent", Strings.ProOnlyTag + "Add event");

                _AddRemoveEventStyle.padding = new RectOffset(-1, 1, 0, 0);

                if (GUI.Button(area, _AddEventContent, _AddRemoveEventStyle))
                {
                    // If the target is currently being previewed, add the event at the currently selected time.
                    var state = TransitionPreviewWindow.GetCurrentState();
                    var normalizedTime = state != null ? state.NormalizedTime : float.NaN;
                    AddEvent(context, normalizedTime);
                }
            }
            else
            {
                _AddRemoveEventStyle.padding = new RectOffset(1, 1, 0, 0);

                using (ObjectPool.Disposable.AcquireContent(out var content, "X", "Remove selected event"))
                {
                    if (GUI.Button(area, content, _AddRemoveEventStyle))
                    {
                        RemoveEvent(context, context.SelectedEvent);
                    }
                }
            }
        }

        /************************************************************************************************************************/

        private static bool ShowAddButton(Context context)
        {
            // Nothing selected = Add.
            if (context.SelectedEvent < 0)
                return true;

            // No times means no events exist = Add.
            if (context.Times.Count == 0)
                return true;

            // Regular event selected = Remove.
            if (context.SelectedEvent < context.Times.Count - 1)
                return false;

            // End has non-default time = Remove.
            if (!float.IsNaN(context.Times.GetElement(context.SelectedEvent).floatValue))
                return false;

            // End has non-empty callback = Remove.
            // If the end callback was empty, the array would have been compacted.
            if (context.Callbacks.Count == context.Times.Count)
                return false;

            // End has empty callback = Add.
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Adds an event to the sequence represented by the given `context`.</summary>
        public static void AddEvent(Context context, float normalizedTime)
        {
            // If the time is NaN, add it halfway between the last event and the end.

            if (context.Times.Count == 0)
            {
                // Having any events means we need the end time too.
                context.Times.Count = 2;
                context.Times.GetElement(1).floatValue = float.NaN;
                if (float.IsNaN(normalizedTime))
                    normalizedTime = 0.5f;
            }
            else
            {
                context.Times.Property.InsertArrayElementAtIndex(context.Times.Count - 1);
                context.Times.Count++;

                if (float.IsNaN(normalizedTime))
                {
                    var transition = context.TransitionContext.Transition;

                    var previousTime = context.Times.Count >= 3 ?
                        context.Times.GetElement(context.Times.Count - 3).floatValue :
                        AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(transition.Speed);

                    var endTime = context.Times.GetElement(context.Times.Count - 1).floatValue;
                    if (float.IsNaN(endTime))
                        endTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(transition.Speed);

                    normalizedTime = previousTime < endTime ?
                        (previousTime + endTime) * 0.5f :
                        previousTime;
                }
            }

            WrapEventTime(context, ref normalizedTime);

            var newEvent = context.Times.Count - 2;
            context.Times.GetElement(newEvent).floatValue = normalizedTime;
            context.SelectedEvent = newEvent;

            if (context.Callbacks.Count > newEvent)
            {
                context.Callbacks.Property.InsertArrayElementAtIndex(newEvent);
                context.Callbacks.Property.serializedObject.ApplyModifiedProperties();

                // Make sure the callback starts empty rather than copying an existing value.
                var callback = context.Callbacks.GetElement(newEvent);
                callback.SetValue(null);
                context.Callbacks.Property.OnPropertyChanged();
            }

            // Update the runtime sequence accordingly.
            var events = context.Sequence?.InitializedEvents;
            if (events != null)
                events.Add(normalizedTime, AnimancerEvent.DummyCallback);

            OptionalWarning.UselessEvent.Disable();

            if (Event.current != null)
            {
                GUI.changed = true;
                GUIUtility.ExitGUI();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Removes the event at the specified `index`.</summary>
        public static void RemoveEvent(Context context, int index)
        {
            // If it's an End Event, set it to NaN.
            if (index >= context.Times.Count - 1)
            {
                context.Times.GetElement(index).floatValue = float.NaN;

                if (context.Callbacks.Count > index)
                    context.Callbacks.Count--;

                AnimancerGUI.Deselect();

                // Update the runtime sequence accordingly.
                var events = context.Sequence?.InitializedEvents;
                if (events != null)
                {
                    events.EndEvent = new AnimancerEvent(float.NaN, null);
                }
            }
            else// Otherwise remove it.
            {
                context.Times.Property.DeleteArrayElementAtIndex(index);
                context.Times.Count--;

                // Update the runtime sequence accordingly.
                var events = context.Sequence?.InitializedEvents;
                if (events != null)
                {
                    events.Remove(index);
                }

                if (index < context.Names.Count)
                {
                    context.Names.Property.DeleteArrayElementAtIndex(index);
                    context.Names.Count--;
                }

                if (index < context.Callbacks.Count)
                {
                    context.Callbacks.Property.DeleteArrayElementAtIndex(index);
                    context.Callbacks.Count--;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sorts the events in the `context` according to their times.</summary>
        private static bool SortEvents(Context context)
        {
            if (context.Times.Count <= 2)
                return false;

            // The serializable sequence sorts itself in ISerializationCallbackReceiver.OnBeforeSerialize.
            var selectedEvent = context.SelectedEvent;
            var sorted = context.Property.serializedObject.ApplyModifiedProperties();
            if (!sorted)
                return false;

            context.Property.serializedObject.Update();
            context.Times.Refresh();
            context.Names.Refresh();
            context.Callbacks.Refresh();
            return context.SelectedEvent != selectedEvent;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Context
        /************************************************************************************************************************/

        /// <summary>Details of an <see cref="AnimancerEvent.Sequence.Serializable"/>.</summary>
        public class Context : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The main property representing the <see cref="Sequence"/> field.</summary>
            public SerializedProperty Property { get; private set; }

            private Sequence _Sequence;

            /// <summary>Underlying value of the <see cref="Property"/>.</summary>
            public Sequence Sequence
            {
                get
                {
                    if (_Sequence == null && Property.serializedObject.targetObjects.Length == 1)
                        _Sequence = Property.GetValue<Sequence>();
                    return _Sequence;
                }
            }

            /// <summary>The property representing the <see cref="Sequence._NormalizedTimes"/> field.</summary>
            public readonly SerializedArrayProperty Times = new SerializedArrayProperty();

            /// <summary>The property representing the <see cref="Sequence._Names"/> field.</summary>
            public readonly SerializedArrayProperty Names = new SerializedArrayProperty();

            /// <summary>The property representing the <see cref="Sequence._Callbacks"/> field.</summary>
            public readonly SerializedArrayProperty Callbacks = new SerializedArrayProperty();

            /************************************************************************************************************************/

            private int _SelectedEvent;

            /// <summary>The index of the currently selected event.</summary>
            public int SelectedEvent
            {
                get => _SelectedEvent;
                set
                {
                    if (Times != null && value >= 0 && (value < Times.Count || Times.Count == 0))
                    {
                        float normalizedTime;
                        if (Times.Count > 0)
                        {
                            normalizedTime = Times.GetElement(value).floatValue;
                        }
                        else
                        {
                            var transition = TransitionContext?.Transition;
                            var speed = transition != null ? transition.Speed : 1;
                            normalizedTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(speed);
                        }

                        TransitionPreviewWindow.PreviewNormalizedTime = normalizedTime;
                    }

                    if (_SelectedEvent == value &&
                        Callbacks != null)
                        return;

                    _SelectedEvent = value;
                    TemporarySettings.SetSelectedEvent(Callbacks.Property, value);
                }
            }

            /************************************************************************************************************************/

            /// <summary>The stack of active contexts.</summary>
            private static readonly LazyStack<Context> Stack = new LazyStack<Context>();

            /// <summary>The currently active instance.</summary>
            public static Context Current { get; private set; }

            /************************************************************************************************************************/

            /// <summary>Adds a new <see cref="Context"/> representing the `property` to the stack and returns it.</summary>
            public static Context Get(SerializedProperty property)
            {
                Current = Stack.Increment();
                Current.Initialize(property);
                EditorGUI.BeginChangeCheck();
                return Current;
            }

            /// <summary>Sets this <see cref="Context"/> as the <see cref="Current"/> and returns it.</summary>
            public Context SetAsCurrent()
            {
                Current = this;
                EditorGUI.BeginChangeCheck();
                return this;
            }

            /************************************************************************************************************************/

            private void Initialize(SerializedProperty property)
            {
                if (Property == property)
                    return;

                Property = property;
                _Sequence = null;

                Times.Property = property.FindPropertyRelative(Sequence.NormalizedTimesField);
                Names.Property = property.FindPropertyRelative(Sequence.NamesField);
                Callbacks.Property = property.FindPropertyRelative(Sequence.CallbacksField);

                if (Names.Count > Times.Count)
                    Names.Count = Times.Count;
                if (Callbacks.Count > Times.Count)
                    Callbacks.Count = Times.Count;

                _SelectedEvent = TemporarySettings.GetSelectedEvent(Callbacks.Property);
                _SelectedEvent = Mathf.Min(_SelectedEvent, Mathf.Max(Times.Count - 1, 0));
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IDisposable"/>] Calls <see cref="SerializedObject.ApplyModifiedProperties"/>.</summary>
            public void Dispose()
            {
                if (EditorGUI.EndChangeCheck())
                    Property.serializedObject.ApplyModifiedProperties();

                Property = null;
                _Sequence = null;

                if (this == Stack.Current)
                    Stack.Decrement();
                Current = Stack.Current;
            }

            /************************************************************************************************************************/

            /// <summary>Shorthand for <see cref="TransitionDrawer.Context"/>.</summary>
            public TransitionDrawer.DrawerContext TransitionContext => TransitionDrawer.Context;

            /************************************************************************************************************************/

            /// <summary>Creates a copy of this <see cref="Context"/>.</summary>
            public Context Copy()
            {
                var copy = new Context
                {
                    Property = Property,
                    _SelectedEvent = _SelectedEvent,
                };

                copy.Times.Property = Times.Property;
                copy.Names.Property = Names.Property;
                copy.Callbacks.Property = Callbacks.Property;

                return copy;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

