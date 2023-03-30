// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

#if UNITY_EDITOR
using Animancer.Editor;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerTransition_2
    [Serializable]
    public abstract class MixerTransition<TMixer, TParameter> : ManualMixerTransition<TMixer>,
        ICopyable<MixerTransition<TMixer, TParameter>>
        where TMixer : MixerState<TParameter>
    {
        /************************************************************************************************************************/

        [SerializeField]
        private TParameter[] _Thresholds;

        /// <summary>[<see cref="SerializeField"/>]
        /// The parameter values at which each of the states are used and blended.
        /// </summary>
        public ref TParameter[] Thresholds => ref _Thresholds;

        /// <summary>The name of the serialized backing field of <see cref="Thresholds"/>.</summary>
        public const string ThresholdsField = nameof(_Thresholds);

        /************************************************************************************************************************/

        [SerializeField]
        private TParameter _DefaultParameter;

        /// <summary>[<see cref="SerializeField"/>]
        /// The initial parameter value to give the mixer when it is first created.
        /// </summary>
        public ref TParameter DefaultParameter => ref _DefaultParameter;

        /// <summary>The name of the serialized backing field of <see cref="DefaultParameter"/>.</summary>
        public const string DefaultParameterField = nameof(_DefaultParameter);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void InitializeState()
        {
            base.InitializeState();

            State.SetThresholds(_Thresholds);
            State.Parameter = _DefaultParameter;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(MixerTransition<TMixer, TParameter> copyFrom)
        {
            CopyFrom((ManualMixerTransition<TMixer>)copyFrom);

            if (copyFrom == null)
            {
                _DefaultParameter = default;
                _Thresholds = default;
                return;
            }

            _DefaultParameter = copyFrom._DefaultParameter;
            AnimancerUtilities.CopyExactArray(copyFrom._Thresholds, ref _Thresholds);
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

#if UNITY_EDITOR
    /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition{TMixer, TParameter}"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
    /// and <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerTransitionDrawer
    /// 
    public class MixerTransitionDrawer : ManualMixerTransition.Drawer
    {
        /************************************************************************************************************************/

        /// <summary>The number of horizontal pixels the "Threshold" label occupies.</summary>
        private readonly float ThresholdWidth;

        /************************************************************************************************************************/

        private static float _StandardThresholdWidth;

        /// <summary>
        /// The number of horizontal pixels the word "Threshold" occupies when drawn with the
        /// <see cref="EditorStyles.popup"/> style.
        /// </summary>
        protected static float StandardThresholdWidth
        {
            get
            {
                if (_StandardThresholdWidth == 0)
                    _StandardThresholdWidth = AnimancerGUI.CalculateWidth(EditorStyles.popup, "Threshold");
                return _StandardThresholdWidth;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="MixerTransitionDrawer"/> using the default <see cref="StandardThresholdWidth"/>.
        /// </summary>
        public MixerTransitionDrawer()
            : this(StandardThresholdWidth)
        { }

        /// <summary>
        /// Creates a new <see cref="MixerTransitionDrawer"/> using a custom width for its threshold labels.
        /// </summary>
        protected MixerTransitionDrawer(float thresholdWidth)
            => ThresholdWidth = thresholdWidth;

        /************************************************************************************************************************/

        /// <summary>
        /// The serialized <see cref="MixerTransition{TMixer, TParameter}.Thresholds"/> of the
        /// <see cref="ManualMixerTransition.Drawer.CurrentProperty"/>.
        /// </summary>
        protected static SerializedProperty CurrentThresholds { get; private set; }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void GatherSubProperties(SerializedProperty property)
        {
            base.GatherSubProperties(property);

            CurrentThresholds = property.FindPropertyRelative(MixerTransition2D.ThresholdsField);

            if (CurrentAnimations == null ||
                CurrentThresholds == null ||
                property.hasMultipleDifferentValues)
                return;

            var count = Math.Max(CurrentAnimations.arraySize, CurrentThresholds.arraySize);
            CurrentAnimations.arraySize = count;
            CurrentThresholds.arraySize = count;
            if (CurrentSpeeds != null &&
                CurrentSpeeds.arraySize != 0)
                CurrentSpeeds.arraySize = count;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);

            if (property.isExpanded)
            {
                if (CurrentThresholds != null)
                {
                    height -= AnimancerGUI.StandardSpacing +
                        EditorGUI.GetPropertyHeight(CurrentThresholds, label);
                }
            }

            return height;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoChildPropertyGUI(ref Rect area, SerializedProperty rootProperty, SerializedProperty property, GUIContent label)
        {
            if (property.propertyPath.EndsWith($".{MixerTransition2D.ThresholdsField}"))
                return;

            base.DoChildPropertyGUI(ref area, rootProperty, property, label);
        }

        /************************************************************************************************************************/

        /// <summary>Splits the specified `area` into separate sections.</summary>
        protected void SplitListRect(Rect area, bool isHeader,
            out Rect animation, out Rect threshold, out Rect speed, out Rect sync)
        {
            SplitListRect(area, isHeader, out animation, out speed, out sync);

            if (TwoLineMode && !isHeader)
            {
                threshold = AnimancerGUI.StealFromLeft(ref speed, ThresholdWidth, AnimancerGUI.StandardSpacing);
            }
            else
            {
                threshold = animation;

                var xMin = threshold.xMin = EditorGUIUtility.labelWidth + AnimancerGUI.IndentSize;

                animation.xMax = xMin - AnimancerGUI.StandardSpacing;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoChildListHeaderGUI(Rect area)
        {
            SplitListRect(area, true, out var animationArea, out var thresholdArea, out var speedArea, out var syncArea);

            DoAnimationHeaderGUI(animationArea);

            var attribute = AttributeCache<ThresholdLabelAttribute>.FindAttribute(CurrentThresholds);
            var text = attribute != null
                ? attribute.Label
                : "Threshold";

            using (ObjectPool.Disposable.AcquireContent(out var label, text,
                "The parameter values at which each child state will be fully active"))
                DoHeaderDropdownGUI(thresholdArea, CurrentThresholds, label, AddThresholdFunctionsToMenu);

            DoSpeedHeaderGUI(speedArea);

            DoSyncHeaderGUI(syncArea);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoElementGUI(Rect area, int index,
            SerializedProperty animation, SerializedProperty speed)
        {
            SplitListRect(area, false,
                out var animationArea, out var thresholdArea, out var speedArea, out var syncArea);

            DoAnimationField(animationArea, animation);
            DoThresholdGUI(thresholdArea, index);
            DoSpeedFieldGUI(speedArea, speed, index);
            DoSyncToggleGUI(syncArea, index);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of the threshold at the specified `index`.</summary>
        protected virtual void DoThresholdGUI(Rect area, int index)
        {
            var threshold = CurrentThresholds.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(area, threshold, GUIContent.none);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnAddElement(int index)
        {
            base.OnAddElement(index);

            if (CurrentThresholds.arraySize > 0)
                CurrentThresholds.InsertArrayElementAtIndex(index);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnRemoveElement(ReorderableList list)
        {
            base.OnRemoveElement(list);
            Serialization.RemoveArrayElement(CurrentThresholds, list.index);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void ResizeList(int size)
        {
            base.ResizeList(size);
            CurrentThresholds.arraySize = size;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnReorderList(ReorderableList list, int oldIndex, int newIndex)
        {
            base.OnReorderList(list, oldIndex, newIndex);
            CurrentThresholds.MoveArrayElement(oldIndex, newIndex);
        }

        /************************************************************************************************************************/

        /// <summary>Adds functions to the `menu` relating to the thresholds.</summary>
        protected virtual void AddThresholdFunctionsToMenu(GenericMenu menu) { }

        /************************************************************************************************************************/
    }
#endif
}
