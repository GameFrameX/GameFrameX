// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>
    /// Context menu functions for generating <see cref="AnimancerTransitionAssetBase"/>s based on the contents of
    /// Animator Controllers.
    /// </summary>
    internal static class TransitionGenerator
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Creates an appropriate type of <see cref="AnimancerTransitionAssetBase"/> from the
        /// <see cref="MenuCommand.context"/>.
        /// </summary>
        [MenuItem("CONTEXT/" + nameof(AnimatorState) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(BlendTree) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(AnimatorStateTransition) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(AnimatorStateMachine) + "/Generate Transitions")]
        private static void GenerateTransition(MenuCommand command)
        {
            var context = command.context;
            if (context is AnimatorState state)
            {
                Selection.activeObject = GenerateTransition(state);
            }
            else if (context is BlendTree blendTree)
            {
                Selection.activeObject = GenerateTransition(null, blendTree);
            }
            else if (context is AnimatorStateTransition transition)
            {
                Selection.activeObject = GenerateTransition(transition);
            }
            else if (context is AnimatorStateMachine stateMachine)// Layer or Sub-State Machine.
            {
                Selection.activeObject = GenerateTransitions(stateMachine);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransitionAssetBase"/> from the `state`.</summary>
        private static Object GenerateTransition(AnimatorState state)
            => GenerateTransition(state, state.motion);

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransitionAssetBase"/> from the `motion`.</summary>
        private static Object GenerateTransition(Object originalAsset, Motion motion)
        {
            if (motion is BlendTree blendTree)
            {
                return GenerateTransition(originalAsset as AnimatorState, blendTree);
            }
            else if (motion is AnimationClip || motion == null)
            {
                var asset = ScriptableObject.CreateInstance<ClipTransitionAsset>();
                asset.Transition = new ClipTransition
                {
                    Clip = (AnimationClip)motion,
                };

                GetDetailsFromState(originalAsset as AnimatorState, asset.Transition);
                SaveTransition(originalAsset, asset);
                return asset;
            }
            else
            {
                Debug.LogError($"Unsupported {nameof(Motion)} Type: {motion.GetType()}");
                return null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the `transition` based on the `state`.</summary>
        private static void GetDetailsFromState(AnimatorState state, ITransitionDetailed transition)
        {
            if (state == null ||
                transition == null)
                return;

            transition.Speed = state.speed;

            var isForwards = state.speed >= 0;
            var defaultEndTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(state.speed);
            var endTime = defaultEndTime;

            var exitTransitions = state.transitions;
            for (int i = 0; i < exitTransitions.Length; i++)
            {
                var exitTransition = exitTransitions[i];
                if (exitTransition.hasExitTime)
                {
                    if (isForwards)
                    {
                        if (endTime > exitTransition.exitTime)
                            endTime = exitTransition.exitTime;
                    }
                    else
                    {
                        if (endTime < exitTransition.exitTime)
                            endTime = exitTransition.exitTime;
                    }
                }
            }

            if (endTime != defaultEndTime && AnimancerUtilities.TryGetWrappedObject(transition, out IHasEvents events))
            {
                if (events.SerializedEvents == null)
                    events.SerializedEvents = new AnimancerEvent.Sequence.Serializable();
                events.SerializedEvents.SetNormalizedEndTime(endTime);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransitionAssetBase"/> from the `blendTree`.</summary>
        private static Object GenerateTransition(AnimatorState state, BlendTree blendTree)
        {
            var asset = CreateTransition(blendTree);
            if (asset == null)
                return null;

            if (state != null)
                asset.name = state.name;

            AnimancerUtilities.TryGetWrappedObject(asset, out ITransitionDetailed detailed);
            GetDetailsFromState(state, detailed);
            SaveTransition(blendTree, asset);
            return asset;
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransitionAssetBase"/> from the `transition`.</summary>
        private static Object GenerateTransition(AnimatorStateTransition transition)
        {
            Object animancerTransition = null;

            if (transition.destinationStateMachine != null)
                animancerTransition = GenerateTransitions(transition.destinationStateMachine);

            if (transition.destinationState != null)
                animancerTransition = GenerateTransition(transition.destinationState);

            return animancerTransition;
        }

        /************************************************************************************************************************/

        /// <summary>Creates <see cref="AnimancerTransitionAssetBase"/>s from all states in the `stateMachine`.</summary>
        private static Object GenerateTransitions(AnimatorStateMachine stateMachine)
        {
            Object transition = null;

            foreach (var child in stateMachine.stateMachines)
                transition = GenerateTransitions(child.stateMachine);

            foreach (var child in stateMachine.states)
                transition = GenerateTransition(child.state);

            return transition;
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="AnimancerTransitionAssetBase"/> from the `blendTree`.</summary>
        private static Object CreateTransition(BlendTree blendTree)
        {
            switch (blendTree.blendType)
            {
                case BlendTreeType.Simple1D:
                    var linearAsset = ScriptableObject.CreateInstance<LinearMixerTransitionAsset>();
                    linearAsset.Transition = InitializeChildren1D(blendTree);
                    return linearAsset;

                case BlendTreeType.SimpleDirectional2D:
                case BlendTreeType.FreeformDirectional2D:
                    var directionalAsset = ScriptableObject.CreateInstance<MixerTransition2DAsset>();
                    directionalAsset.Transition = new MixerTransition2D
                    {
                        Type = MixerTransition2D.MixerType.Directional
                    };
                    directionalAsset.Transition = InitializeChildren2D(blendTree);
                    return directionalAsset;

                case BlendTreeType.FreeformCartesian2D:
                    var cartesianAsset = ScriptableObject.CreateInstance<MixerTransition2DAsset>();
                    cartesianAsset.Transition = new MixerTransition2D
                    {
                        Type = MixerTransition2D.MixerType.Cartesian
                    };
                    cartesianAsset.Transition = InitializeChildren2D(blendTree);
                    return cartesianAsset;

                case BlendTreeType.Direct:
                    var manualAsset = ScriptableObject.CreateInstance<ManualMixerTransitionAsset>();
                    InitializeChildren<ManualMixerTransition, ManualMixerState>(out var transition, blendTree);
                    manualAsset.Transition = transition;
                    return manualAsset;

                default:
                    Debug.LogError($"Unsupported {nameof(BlendTreeType)}: {blendTree.blendType}");
                    return null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the `transition` based on the <see cref="BlendTree.children"/>.</summary>
        private static LinearMixerTransition InitializeChildren1D(BlendTree blendTree)
        {
            var children = InitializeChildren<LinearMixerTransition, LinearMixerState>(out var transition, blendTree);
            transition.Thresholds = new float[children.Length];
            for (int i = 0; i < children.Length; i++)
                transition.Thresholds[i] = children[i].threshold;

            return transition;
        }

        /// <summary>Initializes the `transition` based on the <see cref="BlendTree.children"/>.</summary>
        private static MixerTransition2D InitializeChildren2D(BlendTree blendTree)
        {
            var children = InitializeChildren<MixerTransition2D, MixerState<Vector2>>(out var transition, blendTree);
            transition.Thresholds = new Vector2[children.Length];
            for (int i = 0; i < children.Length; i++)
                transition.Thresholds[i] = children[i].position;

            return transition;
        }

        /// <summary>Initializes the `transition` based on the <see cref="BlendTree.children"/>.</summary>
        private static ChildMotion[] InitializeChildren<TTransition, TState>(out TTransition transition, BlendTree blendTree)
            where TTransition : ManualMixerTransition<TState>, new()
            where TState : ManualMixerState
        {
            transition = new TTransition();

            var children = blendTree.children;
            transition.Animations = new Object[children.Length];
            float[] speeds = new float[children.Length];
            var hasCustomSpeeds = false;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                transition.Animations[i] = child.motion is AnimationClip
                    ? child.motion
                    : GenerateTransition(blendTree, child.motion);

                if ((speeds[i] = child.timeScale) != 1)
                    hasCustomSpeeds = true;
            }

            if (hasCustomSpeeds)
                transition.Speeds = speeds;

            return children;
        }

        /************************************************************************************************************************/

        /// <summary>Saves the `transition` in the same folder as the `originalAsset`.</summary>
        private static void SaveTransition(Object originalAsset, Object transition)
        {
            if (string.IsNullOrEmpty(transition.name))
                transition.name = originalAsset.name;

            var path = AssetDatabase.GetAssetPath(originalAsset);
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, transition.name + ".asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(transition, path);

            Debug.Log($"Saved {path}", transition);
        }

        /************************************************************************************************************************/
    }
}

#endif
