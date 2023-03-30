// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerTransition2DAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Mixer Transition/2D", order = Strings.AssetMenuOrder + 4)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(MixerTransition2DAsset))]
    public class MixerTransition2DAsset : AnimancerTransitionAsset<MixerTransition2D>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<MixerTransition2DAsset, MixerTransition2D, MixerState<Vector2>>,
            ManualMixerState.ITransition2D
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerTransition2D
    [Serializable]
    public class MixerTransition2D : MixerTransition<MixerState<Vector2>, Vector2>,
        ManualMixerState.ITransition2D, ICopyable<MixerTransition2D>
    {
        /************************************************************************************************************************/

        /// <summary>A type of <see cref="ManualMixerState"/> that can be created by a <see cref="MixerTransition2D"/>.</summary>
        public enum MixerType
        {
            /// <summary><see cref="CartesianMixerState"/></summary>
            Cartesian,

            /// <summary><see cref="DirectionalMixerState"/></summary>
            Directional,
        }

        [SerializeField]
        private MixerType _Type;

        /// <summary>[<see cref="SerializeField"/>]
        /// The type of <see cref="ManualMixerState"/> that this transition will create.
        /// </summary>
        public ref MixerType Type => ref _Type;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates and returns a new <see cref="CartesianMixerState"/> or <see cref="DirectionalMixerState"/>
        /// depending on the <see cref="Type"/>.
        /// </summary>
        /// <remarks>
        /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
        /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
        /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
        /// <para></para>
        /// This method also assigns it as the <see cref="AnimancerTransition{TState}.State"/>.
        /// </remarks>
        public override MixerState<Vector2> CreateState()
        {
            switch (_Type)
            {
                case MixerType.Cartesian: State = new CartesianMixerState(); break;
                case MixerType.Directional: State = new DirectionalMixerState(); break;
                default: throw new ArgumentOutOfRangeException(nameof(_Type));
            }
            InitializeState();
            return State;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(MixerTransition2D copyFrom)
        {
            CopyFrom((MixerTransition<MixerState<Vector2>, Vector2>)copyFrom);

            if (copyFrom == null)
            {
                _Type = default;
                return;
            }

            _Type = copyFrom._Type;
        }

        /************************************************************************************************************************/
        #region Drawer
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [CustomPropertyDrawer(typeof(MixerTransition2D), true)]
        public class Drawer : MixerTransitionDrawer
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> using the a wider `thresholdWidth` than usual to accomodate
            /// both the X and Y values.
            /// </summary>
            public Drawer() : base(StandardThresholdWidth * 2 + 20) { }

            /************************************************************************************************************************/
            #region Threshold Calculation Functions
            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddThresholdFunctionsToMenu(GenericMenu menu)
            {
                AddCalculateThresholdsFunction(menu, "From Velocity/XY", (state, threshold) =>
                {
                    if (AnimancerUtilities.TryGetAverageVelocity(state, out var velocity))
                        return new Vector2(velocity.x, velocity.y);
                    else
                        return new Vector2(float.NaN, float.NaN);
                });

                AddCalculateThresholdsFunction(menu, "From Velocity/XZ", (state, threshold) =>
                {
                    if (AnimancerUtilities.TryGetAverageVelocity(state, out var velocity))
                        return new Vector2(velocity.x, velocity.z);
                    else
                        return new Vector2(float.NaN, float.NaN);
                });

                AddCalculateThresholdsFunctionPerAxis(menu, "From Speed",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.magnitude : float.NaN);
                AddCalculateThresholdsFunctionPerAxis(menu, "From Velocity X",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.x : float.NaN);
                AddCalculateThresholdsFunctionPerAxis(menu, "From Velocity Y",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.y : float.NaN);
                AddCalculateThresholdsFunctionPerAxis(menu, "From Velocity Z",
                    (state, threshold) => AnimancerUtilities.TryGetAverageVelocity(state, out var velocity) ? velocity.z : float.NaN);
                AddCalculateThresholdsFunctionPerAxis(menu, "From Angular Speed (Rad)",
                    (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed : float.NaN);
                AddCalculateThresholdsFunctionPerAxis(menu, "From Angular Speed (Deg)",
                    (state, threshold) => AnimancerUtilities.TryGetAverageAngularSpeed(state, out var speed) ? speed * Mathf.Rad2Deg : float.NaN);

                AddPropertyModifierFunction(menu, "Initialize 4 Directions", Initialize4Directions);
                AddPropertyModifierFunction(menu, "Initialize 8 Directions", Initialize8Directions);
            }

            /************************************************************************************************************************/

            private void Initialize4Directions(SerializedProperty property)
            {
                var oldSpeedCount = CurrentSpeeds.arraySize;

                CurrentAnimations.arraySize = CurrentThresholds.arraySize = CurrentSpeeds.arraySize = 5;
                CurrentThresholds.GetArrayElementAtIndex(0).vector2Value = default;
                CurrentThresholds.GetArrayElementAtIndex(1).vector2Value = Vector2.up;
                CurrentThresholds.GetArrayElementAtIndex(2).vector2Value = Vector2.right;
                CurrentThresholds.GetArrayElementAtIndex(3).vector2Value = Vector2.down;
                CurrentThresholds.GetArrayElementAtIndex(4).vector2Value = Vector2.left;

                InitializeSpeeds(oldSpeedCount);

                var type = property.FindPropertyRelative(nameof(_Type));
                type.enumValueIndex = (int)MixerType.Directional;
            }

            /************************************************************************************************************************/

            private void Initialize8Directions(SerializedProperty property)
            {
                var oldSpeedCount = CurrentSpeeds.arraySize;

                CurrentAnimations.arraySize = CurrentThresholds.arraySize = CurrentSpeeds.arraySize = 9;
                CurrentThresholds.GetArrayElementAtIndex(0).vector2Value = default;
                CurrentThresholds.GetArrayElementAtIndex(1).vector2Value = Vector2.up;
                CurrentThresholds.GetArrayElementAtIndex(2).vector2Value = new Vector2(1, 1);
                CurrentThresholds.GetArrayElementAtIndex(3).vector2Value = Vector2.right;
                CurrentThresholds.GetArrayElementAtIndex(4).vector2Value = new Vector2(1, -1);
                CurrentThresholds.GetArrayElementAtIndex(5).vector2Value = Vector2.down;
                CurrentThresholds.GetArrayElementAtIndex(6).vector2Value = new Vector2(-1, -1);
                CurrentThresholds.GetArrayElementAtIndex(7).vector2Value = Vector2.left;
                CurrentThresholds.GetArrayElementAtIndex(8).vector2Value = new Vector2(-1, 1);

                InitializeSpeeds(oldSpeedCount);

                var type = property.FindPropertyRelative(nameof(_Type));
                type.enumValueIndex = (int)MixerType.Directional;
            }

            /************************************************************************************************************************/

            private void AddCalculateThresholdsFunction(GenericMenu menu, string label,
                Func<Object, Vector2, Vector2> calculateThreshold)
            {
                var functionState = CurrentAnimations == null || CurrentThresholds == null
                    ? Editor.MenuFunctionState.Disabled
                    : Editor.MenuFunctionState.Normal;

                AddPropertyModifierFunction(menu, label, functionState, property =>
                {
                    GatherSubProperties(property);

                    if (CurrentAnimations == null ||
                        CurrentThresholds == null)
                        return;

                    var count = CurrentAnimations.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        var state = CurrentAnimations.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (state == null)
                            continue;

                        var threshold = CurrentThresholds.GetArrayElementAtIndex(i);
                        var value = calculateThreshold(state, threshold.vector2Value);
                        if (!Editor.AnimancerEditorUtilities.IsNaN(value))
                            threshold.vector2Value = value;
                    }
                });
            }

            /************************************************************************************************************************/

            private void AddCalculateThresholdsFunctionPerAxis(GenericMenu menu, string label,
                Func<Object, float, float> calculateThreshold)
            {
                AddCalculateThresholdsFunction(menu, "X/" + label, 0, calculateThreshold);
                AddCalculateThresholdsFunction(menu, "Y/" + label, 1, calculateThreshold);
            }

            private void AddCalculateThresholdsFunction(GenericMenu menu, string label, int axis,
                Func<Object, float, float> calculateThreshold)
            {
                AddPropertyModifierFunction(menu, label, (property) =>
                {
                    var count = CurrentAnimations.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        var state = CurrentAnimations.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (state == null)
                            continue;

                        var threshold = CurrentThresholds.GetArrayElementAtIndex(i);

                        var value = threshold.vector2Value;
                        var newValue = calculateThreshold(state, value[axis]);
                        if (!float.IsNaN(newValue))
                            value[axis] = newValue;
                        threshold.vector2Value = value;
                    }
                });
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}
