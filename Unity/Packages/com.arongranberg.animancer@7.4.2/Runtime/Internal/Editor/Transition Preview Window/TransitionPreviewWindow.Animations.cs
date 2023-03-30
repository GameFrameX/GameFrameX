// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionPreviewWindow
    partial class TransitionPreviewWindow
    {
        /// <summary>Animation details for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
        /// </remarks>
        [Serializable]
        private class Animations
        {
            /************************************************************************************************************************/

            public const string
                PreviousAnimationKey = "Previous Animation",
                NextAnimationKey = "Next Animation";

            /************************************************************************************************************************/

            [NonSerialized] private AnimationClip[] _OtherAnimations;

            [SerializeField]
            private AnimationClip _PreviousAnimation;
            public AnimationClip PreviousAnimation => _PreviousAnimation;

            [SerializeField]
            private AnimationClip _NextAnimation;
            public AnimationClip NextAnimation => _NextAnimation;

            /************************************************************************************************************************/

            public void DoGUI()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Preview Details", "(Not Serialized)");

                DoModelGUI();
                DoAnimatorSelectorGUI();

                using (ObjectPool.Disposable.AcquireContent(out var label, "Previous Animation",
                    "The animation for the preview to play before the target transition"))
                {
                    DoAnimationFieldGUI(label, ref _PreviousAnimation, (clip) => _PreviousAnimation = clip);
                }

                var animancer = _Instance._Scene.Animancer;
                DoCurrentAnimationGUI(animancer);

                using (ObjectPool.Disposable.AcquireContent(out var label, "Next Animation",
                    "The animation for the preview to play after the target transition"))
                {
                    DoAnimationFieldGUI(label, ref _NextAnimation, (clip) => _NextAnimation = clip);
                }

                if (animancer != null)
                {
                    using (new EditorGUI.DisabledScope(!Transition.IsValid()))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (animancer.IsGraphPlaying)
                        {
                            if (CompactMiniButton(PauseButtonContent))
                                animancer.PauseGraph();
                        }
                        else
                        {
                            if (CompactMiniButton(StepBackwardButtonContent))
                                StepBackward();

                            if (CompactMiniButton(PlayButtonContent))
                                PlaySequence(animancer);

                            if (CompactMiniButton(StepForwardButtonContent))
                                StepForward();
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
            }

            /************************************************************************************************************************/

            private void DoModelGUI()
            {
                var root = _Instance._Scene.OriginalRoot;
                var model = root != null ? root.gameObject : null;

                EditorGUI.BeginChangeCheck();

                var warning = GetModelWarning(model);
                var color = GUI.color;
                if (warning != null)
                    GUI.color = WarningFieldColor;

                using (ObjectPool.Disposable.AcquireContent(out var label, "Model"))
                {
                    if (DoDropdownObjectField(label, true, ref model, SpacingMode.After))
                    {
                        var menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Default Humanoid"), Settings.IsDefaultHumanoid(model),
                            () => _Instance._Scene.OriginalRoot = Settings.DefaultHumanoid.transform);
                        menu.AddItem(new GUIContent("Default Sprite"), Settings.IsDefaultSprite(model),
                            () => _Instance._Scene.OriginalRoot = Settings.DefaultSprite.transform);

                        var persistentModels = Settings.Models;
                        var temporaryModels = TemporarySettings.PreviewModels;
                        if (persistentModels.Count == 0 && temporaryModels.Count == 0)
                        {
                            menu.AddDisabledItem(new GUIContent("No model prefabs have been used yet"));
                        }
                        else
                        {
                            AddModelSelectionFunctions(menu, persistentModels, model);
                            AddModelSelectionFunctions(menu, temporaryModels, model);
                        }

                        menu.ShowAsContext();
                    }
                }

                GUI.color = color;

                if (EditorGUI.EndChangeCheck())
                    _Instance._Scene.OriginalRoot = model != null ? model.transform : null;

                if (warning != null)
                    EditorGUILayout.HelpBox(warning, MessageType.Warning, true);

            }

            /************************************************************************************************************************/

            private static void AddModelSelectionFunctions(GenericMenu menu, List<GameObject> models, GameObject selected)
            {
                for (int i = models.Count - 1; i >= 0; i--)
                {
                    var model = models[i];
                    var path = AssetDatabase.GetAssetPath(model);
                    if (!string.IsNullOrEmpty(path))
                        path = path.Replace('/', '\\');
                    else
                        path = model.name;

                    menu.AddItem(new GUIContent(path), model == selected,
                        () => _Instance._Scene.OriginalRoot = model.transform);
                }
            }

            /************************************************************************************************************************/

            private string GetModelWarning(GameObject model)
            {
                if (model == null)
                    return "No Model is selected so nothing can be previewed.";

                if (_Instance._Scene.Animancer == null)
                    return "The selected Model has no Animator component.";

                return null;
            }

            /************************************************************************************************************************/

            private void DoAnimatorSelectorGUI()
            {
                var instanceAnimators = _Instance._Scene.InstanceAnimators;
                if (instanceAnimators == null ||
                    instanceAnimators.Length <= 1)
                    return;

                var area = LayoutSingleLineRect(SpacingMode.After);
                var labelArea = StealFromLeft(ref area, EditorGUIUtility.labelWidth, StandardSpacing);
                GUI.Label(labelArea, nameof(Animator));

                var selectedAnimator = _Instance._Scene.SelectedInstanceAnimator;
                using (ObjectPool.Disposable.AcquireContent(out var label, selectedAnimator != null ? selectedAnimator.name : "None"))
                {
                    var clicked = EditorGUI.DropdownButton(area, label, FocusType.Passive);

                    if (!clicked)
                        return;

                    var menu = new GenericMenu();

                    for (int i = 0; i < instanceAnimators.Length; i++)
                    {
                        var animator = instanceAnimators[i];
                        var index = i;
                        menu.AddItem(new GUIContent(animator.name), animator == selectedAnimator, () =>
                        {
                            _Instance._Scene.SetSelectedAnimator(index);
                            NormalizedTime = 0;
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            /************************************************************************************************************************/

            public void GatherAnimations()
            {
                AnimationGatherer.GatherFromGameObject(_Instance._Scene.OriginalRoot.gameObject, ref _OtherAnimations, true);

                if (_OtherAnimations.Length > 0 &&
                    (_PreviousAnimation == null || _NextAnimation == null))
                {
                    var defaultClip = _OtherAnimations[0];
                    var defaultClipIsIdle = false;

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var clip = _OtherAnimations[i];

                        if (defaultClipIsIdle && clip.name.Length > defaultClip.name.Length)
                            continue;

                        if (clip.name.IndexOf("idle", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            defaultClip = clip;
                            break;
                        }
                    }

                    if (_PreviousAnimation == null)
                        _PreviousAnimation = defaultClip;
                    if (_NextAnimation == null)
                        _NextAnimation = defaultClip;
                }
            }

            /************************************************************************************************************************/

            private void DoAnimationFieldGUI(GUIContent label, ref AnimationClip clip, Action<AnimationClip> setClip)
            {
                var showDropdown = !_OtherAnimations.IsNullOrEmpty();

                if (DoDropdownObjectField(label, showDropdown, ref clip))
                {
                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("None"), clip == null, () => setClip(null));

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var animation = _OtherAnimations[i];
                        menu.AddItem(new GUIContent(animation.name), animation == clip, () => setClip(animation));
                    }

                    menu.ShowAsContext();
                }
            }

            /************************************************************************************************************************/

            private static bool DoDropdownObjectField<T>(GUIContent label, bool showDropdown, ref T obj,
                SpacingMode spacingMode = SpacingMode.None) where T : Object
            {
                var area = LayoutSingleLineRect(spacingMode);

                var labelWidth = EditorGUIUtility.labelWidth;

                labelWidth += 2;
                area.xMin -= 1;

                var spacing = StandardSpacing;
                var labelArea = StealFromLeft(ref area, labelWidth - spacing, spacing);

                obj = (T)EditorGUI.ObjectField(area, obj, typeof(T), true);

                if (showDropdown)
                {
                    return EditorGUI.DropdownButton(labelArea, label, FocusType.Passive);
                }
                else
                {
                    GUI.Label(labelArea, label);
                    return false;
                }
            }

            /************************************************************************************************************************/

            private void DoCurrentAnimationGUI(AnimancerPlayable animancer)
            {
                string text;

                if (animancer != null)
                {
                    var transition = Transition;
                    if (transition.IsValid && transition.Key != null)
                        text = animancer.States.GetOrCreate(transition).ToString();
                    else
                        text = transition.ToString();
                }
                else
                {
                    text = _Instance._TransitionProperty.Property.GetFriendlyPath();
                }

                if (text != null)
                    EditorGUILayout.LabelField("Current Animation", text);
            }

            /************************************************************************************************************************/

            private void PlaySequence(AnimancerPlayable animancer)
            {
                if (_PreviousAnimation != null && _PreviousAnimation.length > 0)
                {
                    _Instance._Scene.Animancer.Stop();
                    var fromState = animancer.States.GetOrCreate(PreviousAnimationKey, _PreviousAnimation, true);
                    animancer.Play(fromState);
                    OnPlayAnimation();
                    fromState.TimeD = 0;

                    var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                    fromState.Events.EndEvent = new AnimancerEvent(1 / fromState.Length, PlayTransition);
                    warnings.Enable();
                }
                else
                {
                    PlayTransition();
                }

                _Instance._Scene.Animancer.UnpauseGraph();
            }

            private void PlayTransition()
            {
                var transition = Transition;
                var animancer = _Instance._Scene.Animancer;
                animancer.States.TryGet(transition, out var oldState);

                var targetState = animancer.Play(transition);
                OnPlayAnimation();

                if (oldState != null && oldState != targetState)
                    oldState.Destroy();

                var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                targetState.Events.OnEnd = () =>
                {
                    if (_NextAnimation != null)
                    {
                        var fadeDuration = AnimancerEvent.GetFadeOutDuration(targetState, AnimancerPlayable.DefaultFadeDuration);
                        PlayOther(NextAnimationKey, _NextAnimation, 0, fadeDuration);
                        OnPlayAnimation();
                    }
                    else
                    {
                        animancer.Layers[0].IncrementCommandCount();
                    }
                };
                warnings.Enable();
            }

            /************************************************************************************************************************/

            public void OnPlayAnimation()
            {
                var animancer = _Instance._Scene.Animancer;
                if (animancer == null ||
                    animancer.States.Current == null)
                    return;

                var state = animancer.States.Current;

                state.RecreatePlayableRecursive();

                if (state.HasEvents)
                {
                    var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                    var normalizedEndTime = state.Events.NormalizedEndTime;
                    state.Events = null;
                    state.Events.NormalizedEndTime = normalizedEndTime;
                    warnings.Enable();
                }
            }

            /************************************************************************************************************************/

            private void StepBackward()
                => StepTime(-AnimancerSettings.FrameStep);

            private void StepForward()
                => StepTime(AnimancerSettings.FrameStep);

            private void StepTime(float timeOffset)
            {
                if (!TryShowTransitionPaused(out _, out _, out var state))
                    return;

                var length = state.Length;
                if (length != 0)
                    timeOffset /= length;

                NormalizedTime += timeOffset;
            }

            /************************************************************************************************************************/

            [SerializeField]
            private float _NormalizedTime;

            public float NormalizedTime
            {
                get => _NormalizedTime;
                set
                {
                    if (!value.IsFinite())
                        return;

                    _NormalizedTime = value;

                    if (!TryShowTransitionPaused(out var animancer, out var transition, out var state))
                        return;

                    var length = state.Length;
                    var speed = state.Speed;
                    var time = value * length;
                    var fadeDuration = transition.FadeDuration * Math.Abs(speed);

                    var startTime = TimelineGUI.GetStartTime(transition.NormalizedStartTime, speed, length);
                    var normalizedEndTime = state.NormalizedEndTime;
                    var endTime = normalizedEndTime * length;
                    var fadeOutEnd = TimelineGUI.GetFadeOutEnd(speed, endTime, length);

                    if (speed < 0)
                    {
                        time = length - time;
                        startTime = length - startTime;
                        value = 1 - value;
                        normalizedEndTime = 1 - normalizedEndTime;
                        endTime = length - endTime;
                        fadeOutEnd = length - fadeOutEnd;
                    }

                    if (time < startTime)// Previous animation.
                    {
                        if (_PreviousAnimation != null)
                        {
                            PlayOther(PreviousAnimationKey, _PreviousAnimation, value);
                            value = 0;
                        }
                    }
                    else if (time < startTime + fadeDuration)// Fade from previous animation to the target.
                    {
                        if (_PreviousAnimation != null)
                        {
                            var fromState = PlayOther(PreviousAnimationKey, _PreviousAnimation, value);

                            state.IsPlaying = true;
                            state.Weight = (time - startTime) / fadeDuration;
                            fromState.Weight = 1 - state.Weight;
                        }
                    }
                    else if (_NextAnimation != null)
                    {
                        if (value < normalizedEndTime)
                        {
                            // Just the main state.
                        }
                        else
                        {
                            var toState = PlayOther(NextAnimationKey, _NextAnimation, value - normalizedEndTime);

                            if (time < fadeOutEnd)// Fade from the target transition to the next animation.
                            {
                                state.IsPlaying = true;
                                toState.Weight = (time - endTime) / (fadeOutEnd - endTime);
                                state.Weight = 1 - toState.Weight;
                            }
                            // Else just the next animation.
                        }
                    }

                    if (speed < 0)
                        value = 1 - value;

                    state.NormalizedTime = state.Weight > 0 ? value : 0;
                    animancer.Evaluate();

                    RepaintEverything();
                }
            }

            /************************************************************************************************************************/

            private bool TryShowTransitionPaused(
                out AnimancerPlayable animancer, out ITransitionDetailed transition, out AnimancerState state)
            {
                animancer = _Instance._Scene.Animancer;
                transition = Transition;

                if (animancer == null || !transition.IsValid())
                {
                    state = null;
                    return false;
                }

                state = animancer.Play(transition, 0);
                OnPlayAnimation();
                animancer.PauseGraph();
                return true;
            }

            /************************************************************************************************************************/

            private AnimancerState PlayOther(object key, AnimationClip animation, float normalizedTime, float fadeDuration = 0)
            {
                var animancer = _Instance._Scene.Animancer;
                var state = animancer.States.GetOrCreate(key, animation, true);
                state = animancer.Play(state, fadeDuration);
                OnPlayAnimation();

                normalizedTime *= state.Length;
                state.Time = normalizedTime.IsFinite() ? normalizedTime : 0;

                return state;
            }

            /************************************************************************************************************************/

            internal class WindowMatchStateTime : Key, IUpdatable
            {
                /************************************************************************************************************************/

                public static readonly WindowMatchStateTime Instance = new WindowMatchStateTime();

                /************************************************************************************************************************/

                void IUpdatable.Update()
                {
                    if (_Instance == null ||
                        !AnimancerPlayable.Current.IsGraphPlaying)
                        return;

                    var transition = Transition;
                    if (transition == null)
                        return;

                    if (AnimancerPlayable.Current.States.TryGet(transition, out var state))
                        _Instance._Animations._NormalizedTime = state.NormalizedTime;
                }

                /************************************************************************************************************************/

            }

            /************************************************************************************************************************/
        }
    }
}

#endif

