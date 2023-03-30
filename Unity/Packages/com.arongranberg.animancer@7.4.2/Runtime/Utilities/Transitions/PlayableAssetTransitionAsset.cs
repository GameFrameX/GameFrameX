// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using Animancer.Editor;
using UnityEditor;
#endif

namespace Animancer
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/PlayableAssetTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Playable Asset Transition", order = Strings.AssetMenuOrder + 9)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(PlayableAssetTransitionAsset))]
    public class PlayableAssetTransitionAsset : AnimancerTransitionAsset<PlayableAssetTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<PlayableAssetTransitionAsset, PlayableAssetTransition, PlayableAssetState>,
            PlayableAssetState.ITransition
        { }
    }

    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/PlayableAssetTransition
    [Serializable]
    public class PlayableAssetTransition : AnimancerTransition<PlayableAssetState>,
        PlayableAssetState.ITransition, IAnimationClipCollection, ICopyable<PlayableAssetTransition>
    {
        /************************************************************************************************************************/

        [SerializeField, Tooltip("The asset to play")]
        private PlayableAsset _Asset;

        /// <summary>[<see cref="SerializeField"/>] The asset to play.</summary>
        public ref PlayableAsset Asset => ref _Asset;

        /// <inheritdoc/>
        public override Object MainObject => _Asset;

        /// <summary>
        /// The <see cref="Asset"/> will be used as the <see cref="AnimancerState.Key"/> for the created state to
        /// be registered with.
        /// </summary>
        public override object Key => _Asset;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(Strings.Tooltips.OptionalSpeed)]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float _Speed = 1;

        /// <summary>[<see cref="SerializeField"/>]
        /// Determines how fast the animation plays (1x = normal speed, 2x = double speed).
        /// </summary>
        public override float Speed
        {
            get => _Speed;
            set => _Speed = value;
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(Strings.Tooltips.NormalizedStartTime)]
        [AnimationTime(AnimationTimeAttribute.Units.Normalized)]
        [DefaultValue(float.NaN, 0f)]
        private float _NormalizedStartTime = float.NaN;

        /// <inheritdoc/>
        public override float NormalizedStartTime
        {
            get => _NormalizedStartTime;
            set => _NormalizedStartTime = value;
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("The objects controlled by each of the tracks in the Asset")]
#if UNITY_2020_2_OR_NEWER
        [NonReorderable]
#endif
        private Object[] _Bindings;

        /// <summary>[<see cref="SerializeField"/>] The objects controlled by each of the tracks in the Asset.</summary>
        public ref Object[] Bindings => ref _Bindings;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float MaximumDuration => _Asset != null ? (float)_Asset.duration : 0;

        /// <inheritdoc/>
        public override bool IsValid => _Asset != null;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override PlayableAssetState CreateState()
        {
            State = new PlayableAssetState(_Asset);
            State.SetBindings(_Bindings);
            return State;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Apply(AnimancerState state)
        {
            ApplyDetails(state, _Speed, _NormalizedStartTime);
            base.Apply(state);
        }

        /************************************************************************************************************************/

        /// <summary>Gathers all the animations associated with this object.</summary>
        void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromAsset(_Asset);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(PlayableAssetTransition copyFrom)
        {
            CopyFrom((AnimancerTransition<PlayableAssetState>)copyFrom);

            if (copyFrom == null)
            {
                _Asset = default;
                _Speed = 1;
                _NormalizedStartTime = float.NaN;
                _Bindings = default;
                return;
            }

            _Asset = copyFrom._Asset;
            _Speed = copyFrom._Speed;
            _NormalizedStartTime = copyFrom._NormalizedStartTime;
            AnimancerUtilities.CopyExactArray(copyFrom._Bindings, ref _Bindings);
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        [CustomPropertyDrawer(typeof(PlayableAssetTransition), true)]
        public class Drawer : TransitionDrawer
        {
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Drawer"/>.</summary>
            public Drawer() : base(nameof(_Asset)) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                _CurrentAsset = null;

                var height = base.GetPropertyHeight(property, label);

                if (property.isExpanded)
                {
                    var bindings = property.FindPropertyRelative(nameof(_Bindings));
                    if (bindings != null)
                    {
                        bindings.isExpanded = true;
                        height -= AnimancerGUI.StandardSpacing + AnimancerGUI.LineHeight;
                    }
                }

                return height;
            }

            /************************************************************************************************************************/

            private PlayableAsset _CurrentAsset;

            /// <inheritdoc/>
            protected override void DoMainPropertyGUI(Rect area, out Rect labelArea,
                SerializedProperty rootProperty, SerializedProperty mainProperty)
            {
                _CurrentAsset = mainProperty.objectReferenceValue as PlayableAsset;
                base.DoMainPropertyGUI(area, out labelArea, rootProperty, mainProperty);
            }

            /// <inheritdoc/>
            public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            {
                base.OnGUI(area, property, label);
                _CurrentAsset = null;
            }

            /// <inheritdoc/>
            protected override void DoChildPropertyGUI(ref Rect area, SerializedProperty rootProperty,
                SerializedProperty property, GUIContent label)
            {
                var path = property.propertyPath;
                if (path.EndsWith($".{nameof(_Bindings)}"))
                {
                    DoBindingsGUI(ref area, property, label);
                    return;
                }

                base.DoChildPropertyGUI(ref area, rootProperty, property, label);
            }

            /************************************************************************************************************************/

            private void DoBindingsGUI(ref Rect area, SerializedProperty property, GUIContent label)
            {
                var outputCount = GetOutputCount(out var outputEnumerator, out var firstBindingIsAnimation);

                // Bindings.
                property.Next(true);
                // Array.
                property.Next(true);
                // Array Size.
                DoBindingsCountGUI(area, property, label, outputCount, firstBindingIsAnimation, out var bindingCount);

                EditorGUI.indentLevel++;

                for (int i = 0; i < bindingCount; i++)
                {
                    AnimancerGUI.NextVerticalArea(ref area);

                    if (!property.Next(false))
                    {
                        EditorGUI.LabelField(area, "Binding Count Mismatch");
                        break;
                    }
                    // First Array Item.

                    if (outputEnumerator != null && outputEnumerator.MoveNext())
                    {
                        DoBindingGUI(area, property, label, outputEnumerator, i);
                    }
                    else
                    {
                        var color = GUI.color;
                        GUI.color = AnimancerGUI.WarningFieldColor;

                        EditorGUI.PropertyField(area, property, false);

                        GUI.color = color;
                    }
                }

                EditorGUI.indentLevel--;
            }

            /************************************************************************************************************************/

            private int GetOutputCount(out IEnumerator<PlayableBinding> outputEnumerator, out bool firstBindingIsAnimation)
            {
                var outputCount = 0;

                firstBindingIsAnimation = false;
                if (_CurrentAsset != null)
                {
                    var outputs = _CurrentAsset.outputs;
                    _CurrentAsset = null;
                    outputEnumerator = outputs.GetEnumerator();

                    while (outputEnumerator.MoveNext())
                    {
                        PlayableAssetState.GetBindingDetails(
                            outputEnumerator.Current, out var _, out var _, out var isMarkers);
                        if (isMarkers)
                            continue;

                        if (outputCount == 0 && outputEnumerator.Current.outputTargetType == typeof(Animator))
                            firstBindingIsAnimation = true;

                        outputCount++;
                    }

                    outputEnumerator = outputs.GetEnumerator();
                }
                else outputEnumerator = null;

                return outputCount;
            }

            /************************************************************************************************************************/

            private void DoBindingsCountGUI(Rect area, SerializedProperty property, GUIContent label,
                int outputCount, bool firstBindingIsAnimation, out int bindingCount)
            {
                var color = GUI.color;

                var sizeArea = area;
                bindingCount = property.intValue;

                // Button to fix the number of bindings in the array.
                if (bindingCount != outputCount && !(bindingCount == 0 && outputCount == 1 && firstBindingIsAnimation))
                {
                    GUI.color = AnimancerGUI.WarningFieldColor;

                    var labelText = label.text;
                    var style = AnimancerGUI.MiniButton;

                    var countLabel = outputCount.ToString();
                    var fixSizeWidth = AnimancerGUI.CalculateWidth(style, countLabel);
                    var fixSizeArea = AnimancerGUI.StealFromRight(
                        ref sizeArea, fixSizeWidth, AnimancerGUI.StandardSpacing);
                    if (GUI.Button(fixSizeArea, countLabel, style))
                        property.intValue = bindingCount = outputCount;

                    label.text = labelText;
                }

                EditorGUI.PropertyField(sizeArea, property, label, false);

                GUI.color = color;
            }

            /************************************************************************************************************************/

            private void DoBindingGUI(Rect area, SerializedProperty property, GUIContent label,
                IEnumerator<PlayableBinding> outputEnumerator, int trackIndex)
            {
                CheckIfSkip:
                PlayableAssetState.GetBindingDetails(
                    outputEnumerator.Current, out var name, out var bindingType, out var isMarkers);

                if (isMarkers)
                {
                    outputEnumerator.MoveNext();
                    goto CheckIfSkip;
                }

                label.text = name;

                var targetObject = property.serializedObject.targetObject;
                var allowSceneObjects =
                    targetObject != null &&
                    !EditorUtility.IsPersistent(targetObject);

                label = EditorGUI.BeginProperty(area, label, property);
                var fieldArea = area;
                var obj = property.objectReferenceValue;
                var objExists = obj != null;

                if (objExists)
                    DoRemoveButtonIfNecessary(ref fieldArea, label, property, trackIndex, ref bindingType, ref obj);

                if (bindingType != null || objExists)
                {
                    property.objectReferenceValue =
                        EditorGUI.ObjectField(fieldArea, label, obj, bindingType, allowSceneObjects);
                }
                else
                {
                    EditorGUI.LabelField(fieldArea, label);
                }

                EditorGUI.EndProperty();
            }

            /************************************************************************************************************************/

            private static void DoRemoveButtonIfNecessary(ref Rect area, GUIContent label, SerializedProperty property,
                 int trackIndex, ref Type bindingType, ref Object obj)
            {
                if (trackIndex == 0 && bindingType == typeof(Animator))
                {
                    DoRemoveButton(ref area, label, property, ref obj,
                        "This Animation Track is the first Track" +
                        " so it will automatically control the Animancer output" +
                        " and likely doesn't need a binding.");
                }
                else if (bindingType == null)
                {
                    DoRemoveButton(ref area, label, property, ref obj,
                        "This Track doesn't need a binding.");
                    bindingType = typeof(Object);
                }
                else if (!bindingType.IsAssignableFrom(obj.GetType()))
                {
                    DoRemoveButton(ref area, label, property, ref obj,
                        "This binding has the wrong type for this Track.");
                }
            }

            /************************************************************************************************************************/

            private static void DoRemoveButton(ref Rect area, GUIContent label, SerializedProperty property,
                ref Object obj, string tooltip)
            {
                label.tooltip = tooltip;
                GUI.color = AnimancerGUI.WarningFieldColor;
                var miniButton = AnimancerGUI.MiniButton;

                var text = label.text;
                label.text = "x";

                var xWidth = AnimancerGUI.CalculateWidth(miniButton, label);
                var xArea = AnimancerGUI.StealFromRight(
                    ref area, xWidth, AnimancerGUI.StandardSpacing);
                if (GUI.Button(xArea, label, miniButton))
                    property.objectReferenceValue = obj = null;

                label.text = text;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
