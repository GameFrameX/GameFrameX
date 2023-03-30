// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Various extension methods and utilities.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    /// 
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        /// <summary>This is Animancer Pro.</summary>
        public const bool IsAnimancerPro = true;

        /************************************************************************************************************************/

        /// <summary>Loops the `value` so that <c>0 &lt;= value &lt; 1</c>.</summary>
        /// <remarks>This is more efficient than using <see cref="Wrap"/> with a <c>length</c> of 1.</remarks>
        public static float Wrap01(float value)
        {
            var valueAsDouble = (double)value;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble));
            return value < 1 ? value : 0;
        }

        /// <summary>Loops the `value` so that <c>0 &lt;= value &lt; length</c>.</summary>
        /// <remarks>Unike <see cref="Mathf.Repeat"/>, this method will never return the `length`.</remarks>
        public static float Wrap(float value, float length)
        {
            var valueAsDouble = (double)value;
            var lengthAsDouble = (double)length;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble / lengthAsDouble) * lengthAsDouble);
            return value < length ? value : 0;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Rounds the `value` to the nearest integer using <see cref="MidpointRounding.AwayFromZero"/>.
        /// </summary>
        public static float Round(float value)
            => (float)Math.Round(value, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Rounds the `value` to be a multiple of the `multiple` using <see cref="MidpointRounding.AwayFromZero"/>.
        /// </summary>
        public static float Round(float value, float multiple)
            => Round(value / multiple) * multiple;

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Is the `value` not NaN or Infinity?</summary>
        /// <remarks>Newer versions of the .NET framework apparently have a <c>float.IsFinite</c> method.</remarks>
        public static bool IsFinite(this float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        /// <summary>[Animancer Extension] Is the `value` not NaN or Infinity?</summary>
        /// <remarks>Newer versions of the .NET framework apparently have a <c>double.IsFinite</c> method.</remarks>
        public static bool IsFinite(this double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        /// <summary>[Animancer Extension] Are all components of the `value` not NaN or Infinity?</summary>
        public static bool IsFinite(this Vector2 value) => value.x.IsFinite() && value.y.IsFinite();

        /************************************************************************************************************************/

        /// <summary>
        /// If `obj` exists, this method returns <see cref="object.ToString"/>.
        /// Or if it is <c>null</c>, this method returns <c>"Null"</c>.
        /// Or if it is an <see cref="Object"/> that has been destroyed, this method returns <c>"Null (ObjectType)"</c>.
        /// </summary>
        public static string ToStringOrNull(object obj)
        {
            if (obj is null)
                return "Null";

            if (obj is Object unityObject && unityObject == null)
                return $"Null ({obj.GetType()})";

            return obj.ToString();
        }

        /************************************************************************************************************************/

        /// <summary>Ensures that the length and contents of `copyTo` match `copyFrom`.</summary>
        public static void CopyExactArray<T>(T[] copyFrom, ref T[] copyTo)
        {
            if (copyFrom == null)
            {
                copyTo = null;
                return;
            }

            var length = copyFrom.Length;
            SetLength(ref copyTo, length);
            Array.Copy(copyFrom, copyTo, length);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Swaps <c>array[a]</c> with <c>array[b]</c>.</summary>
        public static void Swap<T>(this T[] array, int a, int b)
        {
            var temp = array[a];
            array[a] = array[b];
            array[b] = temp;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Is the `array` <c>null</c> or its <see cref="Array.Length"/> <c>0</c>?
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length == 0;

        /************************************************************************************************************************/

        /// <summary>
        /// If the `array` is <c>null</c> or its <see cref="Array.Length"/> isn't equal to the specified `length`, this
        /// method creates a new array with that `length` and returns <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Array.Resize{T}(ref T[], int)"/>, this method doesn't copy over the contents of the old
        /// `array` into the new one.
        /// </remarks>
        public static bool SetLength<T>(ref T[] array, int length)
        {
            if (array == null || array.Length != length)
            {
                array = new T[length];
                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Is the `node` is not null and <see cref="AnimancerNode.IsValid"/>?</summary>
        public static bool IsValid(this AnimancerNode node) => node != null && node.IsValid;

        /// <summary>[Animancer Extension] Is the `transition` not null and <see cref="ITransitionDetailed.IsValid"/>?</summary>
        public static bool IsValid(this ITransitionDetailed transition) => transition != null && transition.IsValid;

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Calls <see cref="ITransition.CreateState"/> and <see cref="ITransition.Apply"/>.</summary>
        public static AnimancerState CreateStateAndApply(this ITransition transition, AnimancerPlayable root = null)
        {
            var state = transition.CreateState();
            state.SetRoot(root);
            transition.Apply(state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Reconnects the input of the specified `playable` to its output.</summary>
        public static void RemovePlayable(Playable playable, bool destroy = true)
        {
            if (!playable.IsValid())
                return;

            Assert(playable.GetInputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 input.");
            Assert(playable.GetOutputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 output.");

            var input = playable.GetInput(0);
            if (!input.IsValid())
            {
                if (destroy)
                    playable.Destroy();
                return;
            }

            var graph = playable.GetGraph();
            var output = playable.GetOutput(0);

            if (output.IsValid())// Connected to another Playable.
            {
                if (destroy)
                {
                    playable.Destroy();
                }
                else
                {
                    Assert(output.GetInputCount() == 1,
                        $"{nameof(RemovePlayable)} can only be used on playables connected to a playable with 1 input.");
                    graph.Disconnect(output, 0);
                    graph.Disconnect(playable, 0);
                }

                graph.Connect(input, 0, output, 0);
            }
            else// Connected to the graph output.
            {
                Assert(graph.GetOutput(0).GetSourcePlayable().Equals(playable),
                    $"{nameof(RemovePlayable)} can only be used on playables connected to another playable or to the graph output.");

                if (destroy)
                    playable.Destroy();
                else
                    graph.Disconnect(playable, 0);

                graph.GetOutput(0).SetSourcePlayable(input);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if any <see cref="AnimationClip"/> in the `source` has an animation event with the specified
        /// `functionName`.
        /// </summary>
        public static bool HasEvent(IAnimationClipCollection source, string functionName)
        {
            var clips = ObjectPool.AcquireSet<AnimationClip>();
            source.GatherAnimationClips(clips);

            foreach (var clip in clips)
            {
                if (HasEvent(clip, functionName))
                {
                    ObjectPool.Release(clips);
                    return true;
                }
            }

            ObjectPool.Release(clips);
            return false;
        }

        /// <summary>Checks if the `clip` has an animation event with the specified `functionName`.</summary>
        public static bool HasEvent(AnimationClip clip, string functionName)
        {
            var events = clip.events;
            for (int i = events.Length - 1; i >= 0; i--)
            {
                if (events[i].functionName == functionName)
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Pro-Only]
        /// Calculates all thresholds in the `mixer` using the <see cref="AnimancerState.AverageVelocity"/> of each
        /// state on the X and Z axes.
        /// <para></para>
        /// Note that this method requires the <c>Root Transform Position (XZ) -> Bake Into Pose</c> toggle to be
        /// disabled in the Import Settings of each <see cref="AnimationClip"/> in the mixer.
        /// </summary>
        public static void CalculateThresholdsFromAverageVelocityXZ(this MixerState<Vector2> mixer)
        {
            mixer.ValidateThresholdCount();

            for (int i = mixer.ChildCount - 1; i >= 0; i--)
            {
                var state = mixer.GetChild(i);
                if (state == null)
                    continue;

                var averageVelocity = state.AverageVelocity;
                mixer.SetThreshold(i, new Vector2(averageVelocity.x, averageVelocity.z));
            }
        }

        /************************************************************************************************************************/

        /// <summary>Copies the value of the `parameter` from `copyFrom` to `copyTo`.</summary>
        public static void CopyParameterValue(Animator copyFrom, Animator copyTo, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    copyTo.SetFloat(parameter.nameHash, copyFrom.GetFloat(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Int:
                    copyTo.SetInteger(parameter.nameHash, copyFrom.GetInteger(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    copyTo.SetBool(parameter.nameHash, copyFrom.GetBool(parameter.nameHash));
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /// <summary>Copies the value of the `parameter` from `copyFrom` to `copyTo`.</summary>
        public static void CopyParameterValue(AnimatorControllerPlayable copyFrom, AnimatorControllerPlayable copyTo, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    copyTo.SetFloat(parameter.nameHash, copyFrom.GetFloat(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Int:
                    copyTo.SetInteger(parameter.nameHash, copyFrom.GetInteger(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    copyTo.SetBool(parameter.nameHash, copyFrom.GetBool(parameter.nameHash));
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gets the value of the `parameter` in the `animator`.</summary>
        public static object GetParameterValue(Animator animator, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    return animator.GetFloat(parameter.nameHash);

                case AnimatorControllerParameterType.Int:
                    return animator.GetInteger(parameter.nameHash);

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return animator.GetBool(parameter.nameHash);

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /// <summary>Gets the value of the `parameter` in the `playable`.</summary>
        public static object GetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    return playable.GetFloat(parameter.nameHash);

                case AnimatorControllerParameterType.Int:
                    return playable.GetInteger(parameter.nameHash);

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return playable.GetBool(parameter.nameHash);

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the `value` of the `parameter` in the `animator`.</summary>
        public static void SetParameterValue(Animator animator, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        animator.SetTrigger(parameter.nameHash);
                    else
                        animator.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /// <summary>Sets the `value` of the `parameter` in the `playable`.</summary>
        public static void SetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    playable.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    playable.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    playable.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        playable.SetTrigger(parameter.nameHash);
                    else
                        playable.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="NativeArray{T}"/> containing a single element so that it can be used like a reference
        /// in Unity's C# Job system which does not allow regular reference types.
        /// </summary>
        /// <remarks>Note that you must call <see cref="NativeArray{T}.Dispose()"/> when you're done with the array.</remarks>
        public static NativeArray<T> CreateNativeReference<T>() where T : struct
        {
            return new NativeArray<T>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="NativeArray{T}"/> of <see cref="TransformStreamHandle"/>s for each of the `transforms`.
        /// </summary>
        /// <remarks>Note that you must call <see cref="NativeArray{T}.Dispose()"/> when you're done with the array.</remarks>
        public static NativeArray<TransformStreamHandle> ConvertToTransformStreamHandles(
            IList<Transform> transforms, Animator animator)
        {
            var count = transforms.Count;

            var boneHandles = new NativeArray<TransformStreamHandle>(
                count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < count; i++)
                boneHandles[i] = animator.BindStreamTransform(transforms[i]);

            return boneHandles;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string stating that the `value` is unsupported.</summary>
        public static string GetUnsupportedMessage<T>(T value)
            => $"Unsupported {typeof(T).FullName}: {value}";

        /// <summary>Returns an exception stating that the `value` is unsupported.</summary>
        public static ArgumentException CreateUnsupportedArgumentException<T>(T value)
            => new ArgumentException(GetUnsupportedMessage(value));

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Components
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Adds the specified type of <see cref="IAnimancerComponent"/>, links it to the `animator`, and returns it.
        /// </summary>
        public static T AddAnimancerComponent<T>(this Animator animator) where T : Component, IAnimancerComponent
        {
            var animancer = animator.gameObject.AddComponent<T>();
            animancer.Animator = animator;
            return animancer;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Returns the <see cref="IAnimancerComponent"/> on the same <see cref="GameObject"/> as the `animator` if
        /// there is one. Otherwise this method adds a new one and returns it.
        /// </summary>
        public static T GetOrAddAnimancerComponent<T>(this Animator animator) where T : Component, IAnimancerComponent
        {
            if (animator.TryGetComponent<T>(out var component))
                return component;
            else
                return animator.AddAnimancerComponent<T>();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the first <typeparamref name="T"/> component on the `gameObject` or its parents or children (in
        /// that order).
        /// </summary>
        public static T GetComponentInParentOrChildren<T>(this GameObject gameObject) where T : class
        {
            var component = gameObject.GetComponentInParent<T>();
            if (component != null)
                return component;

            return gameObject.GetComponentInChildren<T>();
        }

        /// <summary>
        /// If the `component` is <c>null</c>, this method tries to find one on the `gameObject` or its parents or
        /// children (in that order).
        /// </summary>
        public static bool GetComponentInParentOrChildren<T>(this GameObject gameObject, ref T component) where T : class
        {
            if (component != null &&
                (!(component is Object obj) || obj != null))
                return false;

            component = gameObject.GetComponentInParentOrChildren<T>();
            return !(component is null);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Editor
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Throws an <see cref="UnityEngine.Assertions.AssertionException"/> if the `condition` is false.
        /// </summary>
        /// <remarks>
        /// This method is similar to <see cref="Debug.Assert(bool, object)"/>, but it throws an exception instead of
        /// just logging the `message`.
        /// </remarks>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Assert(bool condition, object message)
        {
#if UNITY_ASSERTIONS
            if (!condition)
                throw new UnityEngine.Assertions.AssertionException(message != null ? message.ToString() : "Assertion failed.", null);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Indicates that the `target` needs to be re-serialized.</summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional]
        /// Applies the effects of the animation `clip` to the <see cref="Component.gameObject"/>.
        /// </summary>
        /// <remarks>This method is safe to call during <see cref="MonoBehaviour"/><c>.OnValidate</c>.</remarks>
        /// <param name="clip">The animation to apply. If <c>null</c>, this method does nothing.</param>
        /// <param name="component">
        /// The animation will be applied to an <see cref="Animator"/> or <see cref="Animation"/> component on the same
        /// object as this or on any of its parents or children. If <c>null</c>, this method does nothing.
        /// </param>
        /// <param name="time">Determines which part of the animation to apply (in seconds).</param>
        /// <seealso cref="EditModePlay"/>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModeSampleAnimation(this AnimationClip clip, Component component, float time = 0)
        {
#if UNITY_EDITOR
            if (!ShouldEditModeSample(clip, component))
                return;

            var gameObject = component.gameObject;
            component = gameObject.GetComponentInParentOrChildren<Animator>();
            if (component == null)
            {
                component = gameObject.GetComponentInParentOrChildren<Animation>();
                if (component == null)
                    return;
            }

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (!ShouldEditModeSample(clip, component))
                    return;

                clip.SampleAnimation(component.gameObject, time);
            };
        }

        private static bool ShouldEditModeSample(AnimationClip clip, Component component)
        {
            return
                !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode &&
                clip != null &&
                component != null &&
                !UnityEditor.EditorUtility.IsPersistent(component);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Plays the specified `clip` if called in Edit Mode.</summary>
        /// <remarks>This method is safe to call during <see cref="MonoBehaviour"/><c>.OnValidate</c>.</remarks>
        /// <param name="clip">The animation to apply. If <c>null</c>, this method does nothing.</param>
        /// <param name="component">
        /// The animation will be played on an <see cref="IAnimancerComponent"/> on the same object as this or on any
        /// of its parents or children. If <c>null</c>, this method does nothing.
        /// </param>
        /// <seealso cref="EditModeSampleAnimation"/>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModePlay(this AnimationClip clip, Component component)
        {
#if UNITY_EDITOR
            if (!ShouldEditModeSample(clip, component))
                return;

            var animancer = component as IAnimancerComponent;
            if (animancer == null)
                animancer = component.gameObject.GetComponentInParentOrChildren<IAnimancerComponent>();

            if (!ShouldEditModePlay(animancer, clip))
                return;

            // If it's already initialized, play immediately.
            if (animancer.IsPlayableInitialized)
            {
                animancer.Playable.Play(clip);
                return;
            }

            // Otherwise, delay it in case this was called at a bad time (such as during OnValidate).
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (ShouldEditModePlay(animancer, clip))
                    animancer.Playable.Play(clip);
            };
        }

        private static bool ShouldEditModePlay(IAnimancerComponent animancer, AnimationClip clip)
        {
            return
                ShouldEditModeSample(clip, animancer?.Animator) &&
                (!(animancer is Object obj) || obj != null);
#endif
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        private static System.Reflection.FieldInfo _DelegatesField;
        private static bool _GotDelegatesField;

        /// <summary>[Assert-Only]
        /// Uses reflection to achieve the same as <see cref="Delegate.GetInvocationList"/> without allocating
        /// garbage every time.
        /// <list type="bullet">
        /// <item>If the delegate is <c>null</c> or , this method returns <c>false</c> and outputs <c>null</c>.</item>
        /// <item>If the underlying <c>delegate</c> field was not found, this method returns <c>false</c> and outputs <c>null</c>.</item>
        /// <item>If the delegate is not multicast, this method this method returns <c>true</c> and outputs <c>null</c>.</item>
        /// <item>If the delegate is multicast, this method this method returns <c>true</c> and outputs its invocation list.</item>
        /// </list>
        /// </summary>
        public static bool TryGetInvocationListNonAlloc(MulticastDelegate multicast, out Delegate[] delegates)
        {
            if (multicast == null)
            {
                delegates = null;
                return false;
            }

            if (!_GotDelegatesField)
            {
                const string FieldName = "delegates";

                _GotDelegatesField = true;
                _DelegatesField = typeof(MulticastDelegate).GetField("delegates",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (_DelegatesField != null && _DelegatesField.FieldType != typeof(Delegate[]))
                    _DelegatesField = null;

                if (_DelegatesField == null)
                    Debug.LogError($"Unable to find {nameof(MulticastDelegate)}.{FieldName} field.");
            }

            if (_DelegatesField == null)
            {
                delegates = null;
                return false;
            }
            else
            {
                delegates = (Delegate[])_DelegatesField.GetValue(multicast);
                return true;
            }
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

