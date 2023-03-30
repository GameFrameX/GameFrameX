// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="IAnimancerComponent.Playable"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerPlayableDrawer
    /// 
    public class AnimancerPlayableDrawer
    {
        /************************************************************************************************************************/

        /// <summary>A lazy list of information about the layers currently being displayed.</summary>
        private readonly List<AnimancerLayerDrawer>
            LayerInfos = new List<AnimancerLayerDrawer>();

        /// <summary>The number of elements in <see cref="LayerInfos"/> that are currently being used.</summary>
        private int _LayerCount;

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of the <see cref="IAnimancerComponent.Playable"/> if there is only one target.</summary>
        public void DoGUI(IAnimancerComponent[] targets)
        {
            if (targets.Length != 1)
                return;

            DoGUI(targets[0]);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of the <see cref="IAnimancerComponent.Playable"/>.</summary>
        public void DoGUI(IAnimancerComponent target)
        {
            DoNativeAnimatorControllerGUI(target);

            if (!target.IsPlayableInitialized)
            {
                DoPlayableNotInitializedGUI(target);
                return;
            }

            EditorGUI.BeginChangeCheck();

            var playable = target.Playable;

            // Gather the during the layout event and use the same ones during subsequent events to avoid GUI errors
            // in case they change (they shouldn't, but this is also more efficient).
            if (Event.current.type == EventType.Layout)
            {
                AnimancerLayerDrawer.GatherLayerEditors(playable, LayerInfos, out _LayerCount);
            }

            AnimancerGraphControls.DoGraphGUI(playable, out var area);
            CheckContextMenu(area, playable);

            for (int i = 0; i < _LayerCount; i++)
                LayerInfos[i].DoGUI();

            DoLayerWeightWarningGUI(target);
            DoWeightlessPlayWarningGUI(target);

            if (ShowInternalDetails)
                DoInternalDetailsGUI(playable);

            if (EditorGUI.EndChangeCheck() && !playable.IsGraphPlaying)
                playable.Evaluate();

            DoMultipleAnimationSystemWarningGUI(target);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a GUI for the <see cref="Animator.runtimeAnimatorController"/> if there is one.</summary>
        private void DoNativeAnimatorControllerGUI(IAnimancerComponent target)
        {
            if (!EditorApplication.isPlaying &&
                !target.IsPlayableInitialized)
                return;

            var animator = target.Animator;
            if (animator == null)
                return;

            var controller = animator.runtimeAnimatorController;
            if (controller == null)
                return;

            AnimancerGUI.BeginVerticalBox(GUI.skin.box);

            var label = AnimancerGUI.GetNarrowText("Native Animator Controller");

            EditorGUI.BeginChangeCheck();
            controller = EditorGUILayout.ObjectField(
                label, controller, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
            if (EditorGUI.EndChangeCheck())
                animator.runtimeAnimatorController = controller;

            if (controller is AnimatorController editorController)
            {
                var layers = editorController.layers;
                for (int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];

                    var runtimeState = animator.IsInTransition(i) ?
                        animator.GetNextAnimatorStateInfo(i) :
                        animator.GetCurrentAnimatorStateInfo(i);

                    var states = layer.stateMachine.states;
                    var editorState = GetState(states, runtimeState.shortNameHash);

                    var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.Before);

                    var weight = i == 0 ? 1 : animator.GetLayerWeight(i);

                    string stateName;
                    if (editorState != null)
                    {
                        stateName = editorState.name;

                        var isLooping = editorState.motion != null && editorState.motion.isLooping;
                        AnimancerStateDrawer<ClipState>.DoTimeHighlightBarGUI(
                            area, true, weight, runtimeState.normalizedTime * runtimeState.length, runtimeState.length, isLooping);
                    }
                    else
                    {
                        stateName = "State Not Found";
                    }

                    AnimancerGUI.DoWeightLabel(ref area, weight);

                    stateName = AnimancerGUI.GetNarrowText(stateName);

                    EditorGUI.LabelField(area, layer.name, stateName);
                }
            }

            AnimancerGUI.EndVerticalBox(GUI.skin.box);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the state with the specified <see cref="AnimatorState.nameHash"/>.</summary>
        private static AnimatorState GetState(ChildAnimatorState[] states, int nameHash)
        {
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i].state;
                if (state.nameHash == nameHash)
                {
                    return state;
                }
            }

            return null;
        }

        /************************************************************************************************************************/

        private void DoPlayableNotInitializedGUI(IAnimancerComponent target)
        {
            if (!EditorApplication.isPlaying ||
                target.Animator == null ||
                EditorUtility.IsPersistent(target.Animator))
                return;

            EditorGUILayout.HelpBox("Playable is not initialized." +
                " It will be initialized automatically when something needs it, such as playing an animation.",
                 MessageType.Info);

            if (AnimancerGUI.TryUseClickEventInLastRect(1))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Initialize"), false, () => target.Playable.Evaluate());

                AnimancerEditorUtilities.AddDocumentationLink(menu, "Layer Documentation", Strings.DocsURLs.Layers);

                menu.ShowAsContext();
            }
        }

        /************************************************************************************************************************/

        private void DoLayerWeightWarningGUI(IAnimancerComponent target)
        {
            if (_LayerCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No layers have been created, which likely means no animations have been played yet.",
                    MessageType.Warning);
                return;
            }

            if (!target.gameObject.activeInHierarchy ||
                !target.enabled ||
                (target.Animator != null && target.Animator.runtimeAnimatorController != null))
                return;

            if (_LayerCount == 1)
            {
                var layer = LayerInfos[0].Target;
                if (layer.Weight == 0)
                    EditorGUILayout.HelpBox(
                        layer + " is at 0 weight, which likely means no animations have been played yet.",
                        MessageType.Warning);
                return;
            }

            for (int i = 0; i < _LayerCount; i++)
            {
                var layer = LayerInfos[i].Target;
                if (layer.Weight == 1 &&
                    !layer.IsAdditive &&
                    layer._Mask == null &&
                    Mathf.Approximately(layer.GetTotalWeight(), 1))
                    return;
            }

            EditorGUILayout.HelpBox(
                "There are no Override layers at weight 1, which will likely give undesirable results." +
                " Click here for more information.",
                MessageType.Warning);

            if (AnimancerGUI.TryUseClickEventInLastRect())
                EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.Layers + "#blending");
        }

        /************************************************************************************************************************/

        private void DoWeightlessPlayWarningGUI(IAnimancerComponent target)
        {
            if (target.Playable.KeepChildrenConnected)
                return;

            for (int iLayer = 0; iLayer < _LayerCount; iLayer++)
            {
                var layer = LayerInfos[iLayer].Target;
                var stateCount = layer.ChildCount;
                for (int iState = 0; iState < stateCount; iState++)
                {
                    var state = layer[iState];
                    if (state.IsPlaying &&
                        state.Weight == 0 &&
                        state.TargetWeight == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "A state is playing at weight 0, which will not work unless you set" +
                            $" {nameof(AnimancerPlayable)}.{nameof(AnimancerPlayable.KeepChildrenConnected)} = true." +
                            " Click here for more information.",
                            MessageType.Warning);

                        if (AnimancerGUI.TryUseClickEventInLastRect())
                            EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.KeepChildrenConnected);

                        return;
                    }
                }
            }
        }

        /************************************************************************************************************************/

        private void DoMultipleAnimationSystemWarningGUI(IAnimancerComponent target)
        {
            const string OnlyOneSystemWarning =
                "This is not supported. Each object can only be controlled by one system at a time.";

            using (ObjectPool.Disposable.AcquireList<IAnimancerComponent>(out var animancers))
            {
                target.gameObject.GetComponents(animancers);
                if (animancers.Count > 1)
                {
                    for (int i = 0; i < animancers.Count; i++)
                    {
                        var other = animancers[i];
                        if (other != target && other.Animator == target.Animator)
                        {
                            EditorGUILayout.HelpBox(
                                $"There are multiple {nameof(IAnimancerComponent)}s trying to control the target" +
                                $" {nameof(Animator)}. {OnlyOneSystemWarning}",
                                MessageType.Warning);

                            break;
                        }
                    }
                }
            }

            if (target.Animator.TryGetComponent<Animation>(out _))
            {
                EditorGUILayout.HelpBox(
                    $"There is a Legacy {nameof(Animation)} component on the same object as the target" +
                    $" {nameof(Animator)}. {OnlyOneSystemWarning}",
                    MessageType.Warning);
            }
        }

        /************************************************************************************************************************/

        private string _UpdateListLabel;
        private static GUIStyle _InternalDetailsStyle;

        /// <summary>Draws a box describing the internal details of the `playable`.</summary>
        internal void DoInternalDetailsGUI(AnimancerPlayable playable)
        {
            if (Event.current.type == EventType.Layout)
            {
                var text = ObjectPool.AcquireStringBuilder();
                playable.AppendInternalDetails(text, "", " - ");
                _UpdateListLabel = text.ReleaseToString();
            }

            if (_UpdateListLabel == null)
                return;

            if (_InternalDetailsStyle == null)
                _InternalDetailsStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    stretchWidth = true,
                };

            using (ObjectPool.Disposable.AcquireContent(out var content, _UpdateListLabel, null, false))
            {
                var height = _InternalDetailsStyle.CalcHeight(content, 0);
                var area = GUILayoutUtility.GetRect(0, height, _InternalDetailsStyle);
                GUI.Box(area, content, _InternalDetailsStyle);

                CheckContextMenu(area, playable);
            }
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <summary>
        /// Checks if the current event is a context menu click within the `clickArea` and opens a context menu with various
        /// functions for the `playable`.
        /// </summary>
        private void CheckContextMenu(Rect clickArea, AnimancerPlayable playable)
        {
            if (!AnimancerGUI.TryUseClickEvent(clickArea, 1))
                return;

            var menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent(playable.Graph.GetEditorName() ?? "Unnamed Graph"), false);
            menu.AddDisabledItem(new GUIContent("Command Count: " + playable.CommandCount), false);
            menu.AddDisabledItem(new GUIContent("Frame ID: " + playable.FrameID), false);
            AddDisposablesFunctions(menu, playable.Disposables);

            AddUpdateModeFunctions(menu, playable);
            AnimancerEditorUtilities.AddContextMenuIK(menu, playable);
            AnimancerGraphControls.AddAddAnimationFunction(menu);

            AddRootFunctions(menu, playable);

            menu.AddSeparator("");

            AddDisplayOptions(menu);

            menu.AddItem(new GUIContent("Log Details Of Everything"), false,
                () => Debug.Log(playable.GetDescription(), playable.Component as Object));
            AddPlayableGraphVisualizerFunction(menu, "", playable._Graph);

            AnimancerEditorUtilities.AddDocumentationLink(menu, "Inspector Documentation", Strings.DocsURLs.Inspector);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        /// <summary>Adds functions for controlling the `playable`.</summary>
        public static void AddRootFunctions(GenericMenu menu, AnimancerPlayable playable)
        {
            menu.AddFunction("Add Layer",
                playable.Layers.Count < AnimancerPlayable.LayerList.DefaultCapacity,
                () => playable.Layers.Count++);
            menu.AddFunction("Remove Layer",
                playable.Layers.Count > 0,
                () => playable.Layers.Count--);

            menu.AddItem(new GUIContent("Keep Children Connected ?"),
                playable.KeepChildrenConnected,
                () => playable.KeepChildrenConnected = !playable.KeepChildrenConnected);
        }

        /************************************************************************************************************************/

        /// <summary>Adds menu functions to set the <see cref="DirectorUpdateMode"/>.</summary>
        private void AddUpdateModeFunctions(GenericMenu menu, AnimancerPlayable playable)
        {
            var modes = Enum.GetValues(typeof(DirectorUpdateMode));
            for (int i = 0; i < modes.Length; i++)
            {
                var mode = (DirectorUpdateMode)modes.GetValue(i);
                menu.AddItem(new GUIContent("Update Mode/" + mode), playable.UpdateMode == mode,
                    () => playable.UpdateMode = mode);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds disabled items for each disposable.</summary>
        private void AddDisposablesFunctions(GenericMenu menu, List<IDisposable> disposables)
        {
            var prefix = $"{nameof(AnimancerPlayable.Disposables)}: {disposables.Count}";
            if (disposables.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent(prefix), false);
            }
            else
            {
                prefix += "/";
                for (int i = 0; i < disposables.Count; i++)
                {
                    menu.AddDisabledItem(new GUIContent(prefix + disposables[i]), false);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds a menu function to open the Playable Graph Visualiser if it exists in the project.</summary>
        public static void AddPlayableGraphVisualizerFunction(GenericMenu menu, string prefix, PlayableGraph graph)
        {
            var type = Type.GetType("GraphVisualizer.PlayableGraphVisualizerWindow, Unity.PlayableGraphVisualizer.Editor");

            menu.AddFunction(prefix + "Playable Graph Visualizer", type != null, () =>
            {
                var window = EditorWindow.GetWindow(type);

                var field = type.GetField("m_CurrentGraph", AnimancerEditorUtilities.AnyAccessBindings);

                if (field != null)
                    field.SetValue(window, graph);
            });
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Prefs
        /************************************************************************************************************************/

        private const string
            KeyPrefix = "Inspector ",
            MenuPrefix = "Display Options/";

        internal static readonly BoolPref
            SortStatesByName = new BoolPref(KeyPrefix, MenuPrefix + "Sort States By Name", true),
            HideInactiveStates = new BoolPref(KeyPrefix, MenuPrefix + "Hide Inactive States", false),
            HideSingleLayerHeader = new BoolPref(KeyPrefix, MenuPrefix + "Hide Single Layer Header", true),
            RepaintConstantly = new BoolPref(KeyPrefix, MenuPrefix + "Repaint Constantly", true),
            SeparateActiveFromInactiveStates = new BoolPref(KeyPrefix, MenuPrefix + "Separate Active From Inactive States", false),
            ScaleTimeBarByWeight = new BoolPref(KeyPrefix, MenuPrefix + "Scale Time Bar by Weight", true),
            ShowInternalDetails = new BoolPref(KeyPrefix, MenuPrefix + "Show Internal Details", false),
            VerifyAnimationBindings = new BoolPref(KeyPrefix, MenuPrefix + "Verify Animation Bindings", true),
            AutoNormalizeWeights = new BoolPref(KeyPrefix, MenuPrefix + "Auto Normalize Weights", true),
            UseNormalizedTimeSliders = new BoolPref("Inspector", nameof(UseNormalizedTimeSliders), false);

        /************************************************************************************************************************/

        /// <summary>Adds functions to the menu for each of the Display Options.</summary>
        public static void AddDisplayOptions(GenericMenu menu)
        {
            RepaintConstantly.AddToggleFunction(menu);
            SortStatesByName.AddToggleFunction(menu);
            HideSingleLayerHeader.AddToggleFunction(menu);
            HideInactiveStates.AddToggleFunction(menu);
            SeparateActiveFromInactiveStates.AddToggleFunction(menu);
            ScaleTimeBarByWeight.AddToggleFunction(menu);
            VerifyAnimationBindings.AddToggleFunction(menu);
            ShowInternalDetails.AddToggleFunction(menu);
            AutoNormalizeWeights.AddToggleFunction(menu);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

