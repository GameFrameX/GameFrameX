// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using Animancer.Editor;
using UnityEditor;
using UnityEditorInternal;
using static Animancer.Editor.AnimancerGUI;
#endif

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerTransition
    [Serializable]
    public class ManualMixerTransition : ManualMixerTransition<ManualMixerState>,
        ManualMixerState.ITransition, ICopyable<ManualMixerTransition>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override ManualMixerState CreateState()
        {
            State = new ManualMixerState();
            InitializeState();
            return State;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(ManualMixerTransition copyFrom)
        {
            CopyFrom((ManualMixerTransition<ManualMixerState>)copyFrom);
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [CustomPropertyDrawer(typeof(ManualMixerTransition), true)]
        public class Drawer : TransitionDrawer
        {
            /************************************************************************************************************************/

            /// <summary>Should two lines be used to draw each child?</summary>
            public static readonly BoolPref
                TwoLineMode = new BoolPref(
                    nameof(ManualMixerTransition) + "." + nameof(Drawer) + "." + nameof(TwoLineMode),
                    "Two Line Mode",
                    true);

            /************************************************************************************************************************/

            /// <summary>The property this drawer is currently drawing.</summary>
            /// <remarks>Normally each property has its own drawer, but arrays share a single drawer for all elements.</remarks>
            public static SerializedProperty CurrentProperty { get; private set; }

            /// <summary>The <see cref="ManualMixerTransition{TState}.Animations"/> field.</summary>
            public static SerializedProperty CurrentAnimations { get; private set; }

            /// <summary>The <see cref="ManualMixerTransition{TState}.Speeds"/> field.</summary>
            public static SerializedProperty CurrentSpeeds { get; private set; }

            /// <summary>The <see cref="ManualMixerTransition{TState}.SynchronizeChildren"/> field.</summary>
            public static SerializedProperty CurrentSynchronizeChildren { get; private set; }

            private readonly Dictionary<string, ReorderableList>
                PropertyPathToStates = new Dictionary<string, ReorderableList>();

            private ReorderableList _MultiSelectDummyList;

            /************************************************************************************************************************/

            /// <summary>Gather the details of the `property`.</summary>
            /// <remarks>
            /// This method gets called by every <see cref="GetPropertyHeight"/> and <see cref="OnGUI"/> call since
            /// Unity uses the same <see cref="PropertyDrawer"/> instance for each element in a collection, so it
            /// needs to gather the details associated with the current property.
            /// </remarks>
            protected virtual ReorderableList GatherDetails(SerializedProperty property)
            {
                InitializeMode(property);
                GatherSubProperties(property);

                if (property.hasMultipleDifferentValues)
                {
                    if (_MultiSelectDummyList == null)
                    {
                        _MultiSelectDummyList = new ReorderableList(new List<Object>(), typeof(Object))
                        {
                            elementHeight = LineHeight,
                            displayAdd = false,
                            displayRemove = false,
                            footerHeight = 0,
                            drawHeaderCallback = DoAnimationHeaderGUI,
                            drawNoneElementCallback = area => EditorGUI.LabelField(area,
                                "Multi-editing animations is not supported"),
                        };
                    }

                    return _MultiSelectDummyList;
                }

                if (CurrentAnimations == null)
                    return null;

                var path = property.propertyPath;

                if (!PropertyPathToStates.TryGetValue(path, out var states))
                {
                    states = new ReorderableList(CurrentAnimations.serializedObject, CurrentAnimations)
                    {
                        drawHeaderCallback = DoChildListHeaderGUI,
                        elementHeightCallback = GetElementHeight,
                        drawElementCallback = DoElementGUI,
                        onAddCallback = OnAddElement,
                        onRemoveCallback = OnRemoveElement,
                        onReorderCallbackWithDetails = OnReorderList,
                        drawFooterCallback = DoChildListFooterGUI,
                    };

                    PropertyPathToStates.Add(path, states);
                }

                states.serializedProperty = CurrentAnimations;

                return states;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Called every time a `property` is drawn to find the relevant child properties and store them to be
            /// used in <see cref="GetPropertyHeight"/> and <see cref="OnGUI"/>.
            /// </summary>
            protected virtual void GatherSubProperties(SerializedProperty property)
            {
                CurrentProperty = property;
                CurrentAnimations = property.FindPropertyRelative(AnimationsField);
                CurrentSpeeds = property.FindPropertyRelative(SpeedsField);
                CurrentSynchronizeChildren = property.FindPropertyRelative(SynchronizeChildrenField);

                if (!property.hasMultipleDifferentValues &&
                    CurrentAnimations != null &&
                    CurrentSpeeds != null &&
                    CurrentSpeeds.arraySize != 0)
                    CurrentSpeeds.arraySize = CurrentAnimations.arraySize;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Adds a menu item that will call <see cref="GatherSubProperties"/> then run the specified
            /// `function`.
            /// </summary>
            protected void AddPropertyModifierFunction(GenericMenu menu, string label,
                MenuFunctionState state, Action<SerializedProperty> function)
            {
                Serialization.AddPropertyModifierFunction(menu, CurrentProperty, label, state, (property) =>
                {
                    GatherSubProperties(property);
                    function(property);
                });
            }

            /// <summary>
            /// Adds a menu item that will call <see cref="GatherSubProperties"/> then run the specified
            /// `function`.
            /// </summary>
            protected void AddPropertyModifierFunction(GenericMenu menu, string label,
                Action<SerializedProperty> function)
            {
                Serialization.AddPropertyModifierFunction(menu, CurrentProperty, label, (property) =>
                {
                    GatherSubProperties(property);
                    function(property);
                });
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var height = EditorGUI.GetPropertyHeight(property, label);

                if (property.isExpanded)
                {
                    var states = GatherDetails(property);
                    if (states != null)
                        height += StandardSpacing +
                            states.GetHeight();

                    if (CurrentAnimations != null)
                        height -= StandardSpacing +
                            EditorGUI.GetPropertyHeight(CurrentAnimations, label);

                    if (CurrentSpeeds != null)
                        height -= StandardSpacing +
                            EditorGUI.GetPropertyHeight(CurrentSpeeds, label);

                    if (CurrentSynchronizeChildren != null)
                        height -= StandardSpacing +
                            EditorGUI.GetPropertyHeight(CurrentSynchronizeChildren, label);
                }

                return height;
            }

            /************************************************************************************************************************/

            private SerializedProperty _RootProperty;
            private ReorderableList _CurrentChildList;

            /// <inheritdoc/>
            public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            {
                _RootProperty = null;

                base.OnGUI(area, property, label);

                if (_RootProperty == null ||
                    !_RootProperty.isExpanded)
                    return;

                using (DrawerContext.Get(_RootProperty))
                {
                    if (Context.Transition == null)
                        return;

                    _CurrentChildList = GatherDetails(_RootProperty);
                    if (_CurrentChildList == null)
                        return;

                    var indentLevel = EditorGUI.indentLevel;

                    area.yMin = area.yMax - _CurrentChildList.GetHeight();

                    EditorGUI.indentLevel++;
                    area = EditorGUI.IndentedRect(area);

                    EditorGUI.indentLevel = 0;
                    _CurrentChildList.DoList(area);

                    EditorGUI.indentLevel = indentLevel;

                    TryCollapseArrays();
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void DoChildPropertyGUI(ref Rect area,
                SerializedProperty rootProperty, SerializedProperty property, GUIContent label)
            {
                if (Context?.Transition != null)
                {
                    area.height = 0;

                    // If we find the Animations property, hide it to draw it last.

                    var path = property.propertyPath;
                    if (path.EndsWith("." + AnimationsField))
                    {
                        _RootProperty = rootProperty;
                        return;
                    }
                    else if (_RootProperty != null)
                    {
                        // If we already found the Animations property, also hide Speeds and Synchronize Children.
                        if (path.EndsWith("." + SpeedsField) ||
                            path.EndsWith("." + SynchronizeChildrenField))
                            return;
                    }
                }

                base.DoChildPropertyGUI(ref area, rootProperty, property, label);
            }

            /************************************************************************************************************************/

            private static float _SpeedLabelWidth;
            private static float _SyncLabelWidth;

            /// <summary>Splits the specified `area` into separate sections.</summary>
            protected static void SplitListRect(Rect area, bool isHeader,
                out Rect animation, out Rect speed, out Rect sync)
            {
                if (_SpeedLabelWidth == 0)
                    _SpeedLabelWidth = AnimancerGUI.CalculateWidth(EditorStyles.popup, "Speed");

                if (_SyncLabelWidth == 0)
                    _SyncLabelWidth = AnimancerGUI.CalculateWidth(EditorStyles.popup, "Sync");

                var spacing = StandardSpacing;

                var syncWidth = isHeader ?
                    _SyncLabelWidth :
                    ToggleWidth - spacing;

                var speedWidth = _SpeedLabelWidth + _SyncLabelWidth - syncWidth;
                if (!isHeader)
                {
                    // Don't use Clamp because the max might be smaller than the min.
                    var max = Math.Max(area.height, area.width * 0.25f - 30);
                    speedWidth = Math.Min(speedWidth, max);
                }

                area.width += spacing;
                if (TwoLineMode && !isHeader)
                {
                    animation = area;
                    area.y += area.height;
                    sync = StealFromRight(ref area, syncWidth, spacing);
                    speed = area;
                }
                else
                {
                    sync = StealFromRight(ref area, syncWidth, spacing);
                    speed = StealFromRight(ref area, speedWidth, spacing);
                    animation = area;
                }
            }

            /************************************************************************************************************************/
            #region Headers
            /************************************************************************************************************************/

            /// <summary>Draws the headdings of the child list.</summary>
            protected virtual void DoChildListHeaderGUI(Rect area)
            {
                SplitListRect(area, true, out var animationArea, out var speedArea, out var syncArea);

                DoAnimationHeaderGUI(animationArea);
                DoSpeedHeaderGUI(speedArea);
                DoSyncHeaderGUI(syncArea);
            }

            /************************************************************************************************************************/

            /// <summary>Draws an "Animation" header.</summary>
            protected void DoAnimationHeaderGUI(Rect area)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, "Animation",
                    $"The animations that will be used for each child state" +
                    $"\n\nCtrl + Click to allow picking Transition Assets (or anything that implements {nameof(ITransition)})"))
                {
                    DoHeaderDropdownGUI(area, CurrentAnimations, label, menu =>
                    {
                        menu.AddItem(new GUIContent(TwoLineMode.MenuItem), TwoLineMode.Value, () =>
                        {
                            TwoLineMode.Value = !TwoLineMode.Value;
                            ReSelectCurrentObjects();
                        });
                    });
                }
            }

            /************************************************************************************************************************/
            #region Speeds
            /************************************************************************************************************************/

            /// <summary>Draws a "Speed" header.</summary>
            protected void DoSpeedHeaderGUI(Rect area)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, "Speed", Strings.Tooltips.Speed))
                {
                    DoHeaderDropdownGUI(area, CurrentSpeeds, label, menu =>
                    {
                        AddPropertyModifierFunction(menu, "Reset All to 1",
                            CurrentSpeeds.arraySize == 0 ? MenuFunctionState.Selected : MenuFunctionState.Normal,
                            (_) => CurrentSpeeds.arraySize = 0);

                        AddPropertyModifierFunction(menu, "Normalize Durations", MenuFunctionState.Normal, NormalizeDurations);
                    });
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Recalculates the <see cref="CurrentSpeeds"/> depending on the <see cref="AnimationClip.length"/> of
            /// their animations so that they all take the same amount of time to play fully.
            /// </summary>
            private static void NormalizeDurations(SerializedProperty property)
            {
                var speedCount = CurrentSpeeds.arraySize;

                var lengths = new float[CurrentAnimations.arraySize];
                if (lengths.Length <= 1)
                    return;

                int nonZeroLengths = 0;
                float totalLength = 0;
                float totalSpeed = 0;
                for (int i = 0; i < lengths.Length; i++)
                {
                    var state = CurrentAnimations.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (AnimancerUtilities.TryGetLength(state, out var length) &&
                        length > 0)
                    {
                        nonZeroLengths++;
                        totalLength += length;
                        lengths[i] = length;

                        if (speedCount > 0)
                            totalSpeed += CurrentSpeeds.GetArrayElementAtIndex(i).floatValue;
                    }
                }

                if (nonZeroLengths == 0)
                    return;

                var averageLength = totalLength / nonZeroLengths;
                var averageSpeed = speedCount > 0 ? totalSpeed / nonZeroLengths : 1;

                CurrentSpeeds.arraySize = lengths.Length;
                InitializeSpeeds(speedCount);

                for (int i = 0; i < lengths.Length; i++)
                {
                    if (lengths[i] == 0)
                        continue;

                    CurrentSpeeds.GetArrayElementAtIndex(i).floatValue = averageSpeed * lengths[i] / averageLength;
                }

                TryCollapseArrays();
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Initializes every element in the <see cref="CurrentSpeeds"/> array from the `start` to the end of
            /// the array to contain a value of 1.
            /// </summary>
            public static void InitializeSpeeds(int start)
            {
                var count = CurrentSpeeds.arraySize;
                while (start < count)
                    CurrentSpeeds.GetArrayElementAtIndex(start++).floatValue = 1;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Sync
            /************************************************************************************************************************/

            /// <summary>Draws a "Sync" header.</summary>
            protected void DoSyncHeaderGUI(Rect area)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, "Sync",
                    "Determines which child states have their normalized times constantly synchronized"))
                {
                    DoHeaderDropdownGUI(area, CurrentSpeeds, label, menu =>
                    {
                        var syncCount = CurrentSynchronizeChildren.arraySize;

                        var allState = syncCount == 0 ? MenuFunctionState.Selected : MenuFunctionState.Normal;
                        AddPropertyModifierFunction(menu, "All", allState,
                            (_) => CurrentSynchronizeChildren.arraySize = 0);

                        var syncNone = syncCount == CurrentAnimations.arraySize;
                        if (syncNone)
                        {
                            for (int i = 0; i < syncCount; i++)
                            {
                                if (CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue)
                                {
                                    syncNone = false;
                                    break;
                                }
                            }
                        }
                        var noneState = syncNone ? MenuFunctionState.Selected : MenuFunctionState.Normal;
                        AddPropertyModifierFunction(menu, "None", noneState, (_) =>
                        {
                            var count = CurrentSynchronizeChildren.arraySize = CurrentAnimations.arraySize;
                            for (int i = 0; i < count; i++)
                                CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue = false;
                        });

                        AddPropertyModifierFunction(menu, "Invert", MenuFunctionState.Normal, (_) =>
                        {
                            var count = CurrentSynchronizeChildren.arraySize;
                            for (int i = 0; i < count; i++)
                            {
                                var property = CurrentSynchronizeChildren.GetArrayElementAtIndex(i);
                                property.boolValue = !property.boolValue;
                            }

                            var newCount = CurrentSynchronizeChildren.arraySize = CurrentAnimations.arraySize;
                            for (int i = count; i < newCount; i++)
                                CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue = false;
                        });

                        AddPropertyModifierFunction(menu, "Non-Stationary", MenuFunctionState.Normal, (_) =>
                        {
                            var count = CurrentAnimations.arraySize;

                            for (int i = 0; i < count; i++)
                            {
                                var state = CurrentAnimations.GetArrayElementAtIndex(i).objectReferenceValue;
                                if (state == null)
                                    continue;

                                if (i >= syncCount)
                                {
                                    CurrentSynchronizeChildren.arraySize = i + 1;
                                    for (int j = syncCount; j < i; j++)
                                        CurrentSynchronizeChildren.GetArrayElementAtIndex(j).boolValue = true;
                                    syncCount = i + 1;
                                }

                                CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue =
                                    AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) &&
                                    velocity != default;
                            }

                            TryCollapseSync();
                        });
                    });
                }
            }

            /************************************************************************************************************************/

            private static void SyncNone()
            {
                var count = CurrentSynchronizeChildren.arraySize = CurrentAnimations.arraySize;
                for (int i = 0; i < count; i++)
                    CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue = false;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/

            /// <summary>Draws the GUI for a header dropdown button.</summary>
            public static void DoHeaderDropdownGUI(Rect area, SerializedProperty property, GUIContent content,
                Action<GenericMenu> populateMenu)
            {
                if (property != null)
                    EditorGUI.BeginProperty(area, GUIContent.none, property);

                if (populateMenu != null)
                {
                    if (EditorGUI.DropdownButton(area, content, FocusType.Passive))
                    {
                        var menu = new GenericMenu();
                        populateMenu(menu);
                        menu.ShowAsContext();
                    }
                }
                else
                {
                    GUI.Label(area, content);
                }

                if (property != null)
                    EditorGUI.EndProperty();
            }

            /************************************************************************************************************************/

            /// <summary>Draws the footer of the child list.</summary>
            protected virtual void DoChildListFooterGUI(Rect area)
            {
                ReorderableList.defaultBehaviours.DrawFooter(area, _CurrentChildList);

                EditorGUI.BeginChangeCheck();

                area.xMax = EditorGUIUtility.labelWidth + IndentSize;

                area.y++;
                area.height = LineHeight;

                using (ObjectPool.Disposable.AcquireContent(out var label, "Count"))
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    var labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = CalculateLabelWidth(label.text);

                    var count = EditorGUI.DelayedIntField(area, label, _CurrentChildList.count);

                    if (EditorGUI.EndChangeCheck())
                        ResizeList(count);

                    EditorGUIUtility.labelWidth = labelWidth;

                    EditorGUI.indentLevel = indentLevel;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/

            /// <summary>Calculates the height of the state at the specified `index`.</summary>
            protected virtual float GetElementHeight(int index)
                => TwoLineMode
                ? LineHeight * 2
                : LineHeight;

            /************************************************************************************************************************/

            /// <summary>Draws the GUI of the state at the specified `index`.</summary>
            private void DoElementGUI(Rect area, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || index > CurrentAnimations.arraySize)
                    return;

                area.height = LineHeight;

                var state = CurrentAnimations.GetArrayElementAtIndex(index);
                var speed = CurrentSpeeds.arraySize > 0
                    ? CurrentSpeeds.GetArrayElementAtIndex(index)
                    : null;
                DoElementGUI(area, index, state, speed);
            }

            /************************************************************************************************************************/

            /// <summary>Draws the GUI of the animation at the specified `index`.</summary>
            protected virtual void DoElementGUI(Rect area, int index,
                SerializedProperty animation, SerializedProperty speed)
            {
                SplitListRect(area, false, out var animationArea, out var speedArea, out var syncArea);

                DoAnimationField(animationArea, animation);
                DoSpeedFieldGUI(speedArea, speed, index);
                DoSyncToggleGUI(syncArea, index);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Draws an <see cref="EditorGUI.ObjectField(Rect, GUIContent, Object, Type, bool)"/> that accepts
            /// <see cref="AnimationClip"/>s and <see cref="ITransition"/>s
            /// </summary>
            public static void DoAnimationField(Rect area, SerializedProperty property)
            {
                EditorGUI.BeginProperty(area, GUIContent.none, property);

                var targetObject = property.serializedObject.targetObject;
                var oldReference = property.objectReferenceValue;

                var currentEvent = Event.current;
                var isDrag =
                    currentEvent.type == EventType.DragUpdated ||
                    currentEvent.type == EventType.DragPerform;
                var type =
                    isDrag ||
                    currentEvent.control ||
                    currentEvent.commandName == "ObjectSelectorUpdated" ?
                    typeof(Object) : typeof(AnimationClip);

                var allowSceneObjects = targetObject != null && !EditorUtility.IsPersistent(targetObject);

                EditorGUI.BeginChangeCheck();
                var newReference = EditorGUI.ObjectField(area, GUIContent.none, oldReference, type, allowSceneObjects);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newReference == null || (IsClipOrTransition(newReference) && newReference != targetObject))
                        property.objectReferenceValue = newReference;
                }

                if (isDrag && area.Contains(currentEvent.mousePosition))
                {
                    var objects = DragAndDrop.objectReferences;
                    if (objects.Length != 1 ||
                        !IsClipOrTransition(objects[0]) ||
                        objects[0] == targetObject)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }

                EditorGUI.EndProperty();
            }

            /// <summary>Is the `clipOrTransition` an <see cref="AnimationClip"/> or <see cref="ITransition"/>?</summary>
            public static bool IsClipOrTransition(object clipOrTransition)
                => clipOrTransition is AnimationClip || clipOrTransition is ITransition;

            /************************************************************************************************************************/

            /// <summary>
            /// Draws a toggle to enable or disable <see cref="ManualMixerState.SynchronizedChildren"/> for the child at
            /// the specified `index`.
            /// </summary>
            protected void DoSpeedFieldGUI(Rect area, SerializedProperty speed, int index)
            {
                if (speed != null)
                {
                    EditorGUI.PropertyField(area, speed, GUIContent.none);
                }
                else// If this element doesn't have its own speed property, just show 1.
                {
                    EditorGUI.BeginProperty(area, GUIContent.none, CurrentSpeeds);

                    var value = Units.UnitsAttribute.DoSpecialFloatField(
                        area, null, 1, Units.AnimationSpeedAttribute.DisplayConverters[0]);

                    // Middle Click toggles from 1 to -1.
                    if (TryUseClickEvent(area, 2))
                        value = -1;

                    if (value != 1)
                    {
                        CurrentSpeeds.InsertArrayElementAtIndex(0);
                        CurrentSpeeds.GetArrayElementAtIndex(0).floatValue = 1;
                        CurrentSpeeds.arraySize = CurrentAnimations.arraySize;
                        CurrentSpeeds.GetArrayElementAtIndex(index).floatValue = value;
                    }

                    EditorGUI.EndProperty();
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Draws a toggle to enable or disable <see cref="ManualMixerState.SynchronizedChildren"/> for the child at
            /// the specified `index`.
            /// </summary>
            protected void DoSyncToggleGUI(Rect area, int index)
            {
                var syncProperty = CurrentSynchronizeChildren;
                var syncFlagCount = syncProperty.arraySize;

                var enabled = true;

                if (index < syncFlagCount)
                {
                    syncProperty = syncProperty.GetArrayElementAtIndex(index);
                    enabled = syncProperty.boolValue;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginProperty(area, GUIContent.none, syncProperty);

                enabled = GUI.Toggle(area, enabled, GUIContent.none);

                EditorGUI.EndProperty();
                if (EditorGUI.EndChangeCheck())
                {
                    if (index < syncFlagCount)
                    {
                        syncProperty.boolValue = enabled;
                    }
                    else
                    {
                        syncProperty.arraySize = index + 1;

                        for (int i = syncFlagCount; i < index; i++)
                        {
                            syncProperty.GetArrayElementAtIndex(i).boolValue = true;
                        }

                        syncProperty.GetArrayElementAtIndex(index).boolValue = enabled;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Called when adding a new state to the list to ensure that any other relevant arrays have new
            /// elements added as well.
            /// </summary>
            private void OnAddElement(ReorderableList list)
            {
                var index = list.index;
                if (index < 0 || Event.current.button == 1)// Right Click to add at the end.
                {
                    index = CurrentAnimations.arraySize - 1;
                    if (index < 0)
                        index = 0;
                }

                OnAddElement(index);
            }

            /// <summary>
            /// Called when adding a new state to the list to ensure that any other relevant arrays have new
            /// elements added as well.
            /// </summary>
            protected virtual void OnAddElement(int index)
            {
                CurrentAnimations.InsertArrayElementAtIndex(index);

                if (CurrentSpeeds.arraySize > 0)
                    CurrentSpeeds.InsertArrayElementAtIndex(index);

                if (CurrentSynchronizeChildren.arraySize > index)
                    CurrentSynchronizeChildren.InsertArrayElementAtIndex(index);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Called when removing a state from the list to ensure that any other relevant arrays have elements
            /// removed as well.
            /// </summary>
            protected virtual void OnRemoveElement(ReorderableList list)
            {
                var index = list.index;

                Serialization.RemoveArrayElement(CurrentAnimations, index);

                if (CurrentSpeeds.arraySize > index)
                    Serialization.RemoveArrayElement(CurrentSpeeds, index);

                if (CurrentSynchronizeChildren.arraySize > index)
                    Serialization.RemoveArrayElement(CurrentSynchronizeChildren, index);
            }

            /************************************************************************************************************************/

            /// <summary>Sets the number of items in the child list.</summary>
            protected virtual void ResizeList(int size)
            {
                CurrentAnimations.arraySize = size;

                if (CurrentSpeeds.arraySize > size)
                    CurrentSpeeds.arraySize = size;

                if (CurrentSynchronizeChildren.arraySize > size)
                    CurrentSynchronizeChildren.arraySize = size;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Called when reordering states in the list to ensure that any other relevant arrays have their
            /// corresponding elements reordered as well.
            /// </summary>
            protected virtual void OnReorderList(ReorderableList list, int oldIndex, int newIndex)
            {
                CurrentSpeeds.MoveArrayElement(oldIndex, newIndex);

                var syncCount = CurrentSynchronizeChildren.arraySize;
                if (Math.Max(oldIndex, newIndex) >= syncCount)
                {
                    CurrentSynchronizeChildren.arraySize++;
                    CurrentSynchronizeChildren.GetArrayElementAtIndex(syncCount).boolValue = true;
                    CurrentSynchronizeChildren.arraySize = newIndex + 1;
                }

                CurrentSynchronizeChildren.MoveArrayElement(oldIndex, newIndex);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="TryCollapseSpeeds"/> and <see cref="TryCollapseSync"/>.
            /// </summary>
            public static void TryCollapseArrays()
            {
                if (CurrentProperty == null ||
                    CurrentProperty.hasMultipleDifferentValues)
                    return;

                TryCollapseSpeeds();
                TryCollapseSync();
            }

            /************************************************************************************************************************/

            /// <summary>
            /// If every element in the <see cref="CurrentSpeeds"/> array is 1, this method sets the array size to 0.
            /// </summary>
            public static void TryCollapseSpeeds()
            {
                var property = CurrentSpeeds;
                if (property == null)
                    return;

                var speedCount = property.arraySize;
                if (speedCount <= 0)
                    return;

                for (int i = 0; i < speedCount; i++)
                {
                    if (property.GetArrayElementAtIndex(i).floatValue != 1)
                        return;
                }

                property.arraySize = 0;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Removes any true elements from the end of the <see cref="CurrentSynchronizeChildren"/> array.
            /// </summary>
            public static void TryCollapseSync()
            {
                var property = CurrentSynchronizeChildren;
                if (property == null)
                    return;

                var count = property.arraySize;
                var changed = false;

                for (int i = count - 1; i >= 0; i--)
                {
                    if (property.GetArrayElementAtIndex(i).boolValue)
                    {
                        count = i;
                        changed = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (changed)
                    property.arraySize = count;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
