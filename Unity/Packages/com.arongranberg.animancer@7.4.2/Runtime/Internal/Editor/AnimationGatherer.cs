// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A system that procedurally gathers animations throughout the hierarchy without needing explicit references.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimationGatherer
    /// 
    public class AnimationGatherer : IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Recursion Guard
        /************************************************************************************************************************/

        private const int MaxFieldDepth = 7;

        /************************************************************************************************************************/

        private static readonly HashSet<object>
            RecursionGuard = new HashSet<object>();

        private static int _CallCount;

        private static bool BeginRecursionGuard(object obj)
        {
            if (RecursionGuard.Contains(obj))
                return false;

            RecursionGuard.Add(obj);
            return true;
        }

        private static void EndCall()
        {
            if (_CallCount == 0)
                RecursionGuard.Clear();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Fields and Accessors
        /************************************************************************************************************************/

        /// <summary>All the <see cref="AnimationClip"/>s that have been gathered.</summary>
        public readonly HashSet<AnimationClip> Clips = new HashSet<AnimationClip>();

        /// <summary>All the <see cref="ITransition"/>s that have been gathered.</summary>
        public readonly HashSet<ITransition> Transitions = new HashSet<ITransition>();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            try
            {
                foreach (var clip in Clips)
                    clips.Add(clip);

                foreach (var transition in Transitions)
                    clips.GatherFromSource(transition);
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Cache
        /************************************************************************************************************************/

        private static readonly Dictionary<GameObject, AnimationGatherer>
            ObjectToGatherer = new Dictionary<GameObject, AnimationGatherer>();

        /************************************************************************************************************************/

        static AnimationGatherer()
        {
            UnityEditor.EditorApplication.hierarchyChanged += ClearCache;
            UnityEditor.Selection.selectionChanged += ClearCache;
        }

        /************************************************************************************************************************/

        /// <summary>Clears all cached gatherers.</summary>
        public static void ClearCache() => ObjectToGatherer.Clear();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>Should exceptions thrown while gathering animations be logged? Default is false to ignore them.</summary>
        public static bool logExceptions;

        /// <summary>Logs the `exception` if <see cref="logExceptions"/> is true. Otherwise does nothing.</summary>
        private static void HandleException(Exception exception)
        {
            if (logExceptions)
                Debug.LogException(exception);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns a cached <see cref="AnimationGatherer"/> containing any <see cref="AnimationClip"/>s referenced by
        /// components in the same hierarchy as the `gameObject`. See <see cref="ICharacterRoot"/> for details.
        /// </summary>
        public static AnimationGatherer GatherFromGameObject(GameObject gameObject)
        {
            if (!BeginRecursionGuard(gameObject))
                return null;

            try
            {
                _CallCount++;
                if (!ObjectToGatherer.TryGetValue(gameObject, out var gatherer))
                {
                    gatherer = new AnimationGatherer();
                    ObjectToGatherer.Add(gameObject, gatherer);
                    gatherer.GatherFromComponents(gameObject);
                }

                return gatherer;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
            finally
            {
                _CallCount--;
                EndCall();
            }
        }

        /// <summary>
        /// Fills the `clips` with any <see cref="AnimationClip"/>s referenced by components in the same hierarchy as
        /// the `gameObject`. See <see cref="ICharacterRoot"/> for details.
        /// </summary>
        public static void GatherFromGameObject(GameObject gameObject, ICollection<AnimationClip> clips)
        {
            var gatherer = GatherFromGameObject(gameObject);
            gatherer?.GatherAnimationClips(clips);
        }

        /// <summary>
        /// Fills the `clips` with any <see cref="AnimationClip"/>s referenced by components in the same hierarchy as
        /// the `gameObject`. See <see cref="ICharacterRoot"/> for details.
        /// </summary>
        public static void GatherFromGameObject(GameObject gameObject, ref AnimationClip[] clips, bool sort)
        {
            var gatherer = GatherFromGameObject(gameObject);
            if (gatherer == null)
                return;

            using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clipSet))
            {
                gatherer.GatherAnimationClips(clipSet);
                AnimancerUtilities.SetLength(ref clips, clipSet.Count);
                clipSet.CopyTo(clips);
            }

            if (sort)
                Array.Sort(clips, (a, b) => a.name.CompareTo(b.name));
        }

        /************************************************************************************************************************/

        private void GatherFromComponents(GameObject gameObject)
        {
            var root = AnimancerEditorUtilities.FindRoot(gameObject);

            using (ObjectPool.Disposable.AcquireList<MonoBehaviour>(out var components))
            {
                root.GetComponentsInChildren(true, components);
                GatherFromComponents(components);
            }
        }

        /************************************************************************************************************************/

        private void GatherFromComponents(List<MonoBehaviour> components)
        {
            var i = components.Count;
            GatherClips:
            try
            {
                while (--i >= 0)
                {
                    GatherFromObject(components[i], 0);
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
                goto GatherClips;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gathers all animations from the `source`s fields.</summary>
        private void GatherFromObject(object source, int depth)
        {
            if (source == null)
                return;

            if (source is AnimationClip clip)
            {
                Clips.Add(clip);
                return;
            }

            if (!MightContainAnimations(source.GetType()))
                return;

            if (!BeginRecursionGuard(source))
                return;

            try
            {
                if (Clips.GatherFromSource(source))
                    return;
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
            finally
            {
                RecursionGuard.Remove(source);
            }

            GatherFromFields(source, depth);
        }

        /************************************************************************************************************************/

        /// <summary>Types mapped to a delegate that can quickly gather their clips.</summary>
        private static readonly Dictionary<Type, Action<object, AnimationGatherer>>
            TypeToGathererDelegate = new Dictionary<Type, Action<object, AnimationGatherer>>();

        /// <summary>
        /// Uses reflection to gather <see cref="AnimationClip"/>s from fields on the `source` object.
        /// </summary>
        private void GatherFromFields(object source, int depth)
        {
            if (depth >= MaxFieldDepth ||
                source == null ||
                !BeginRecursionGuard(source))
                return;

            var type = source.GetType();

            if (!TypeToGathererDelegate.TryGetValue(type, out var gatherClips))
            {
                gatherClips = BuildClipGathererDelegate(type, depth);
                TypeToGathererDelegate.Add(type, gatherClips);
            }

            gatherClips?.Invoke(source, this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a delegate to gather <see cref="AnimationClip"/>s from all relevant fields in a given `type`.
        /// </summary>
        private static Action<object, AnimationGatherer> BuildClipGathererDelegate(Type type, int depth)
        {
            if (!MightContainAnimations(type))
                return null;

            Action<object, AnimationGatherer> gathererDelegate = null;

            while (type != null)
            {
                var fields = type.GetFields(AnimancerEditorUtilities.InstanceBindings);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var fieldType = field.FieldType;
                    if (!MightContainAnimations(fieldType))
                        continue;

                    if (fieldType == typeof(AnimationClip))
                    {
                        gathererDelegate += (obj, gatherer) =>
                        {
                            var clip = (AnimationClip)field.GetValue(obj);
                            gatherer.Clips.Gather(clip);
                        };
                    }
                    else if (typeof(IAnimationClipSource).IsAssignableFrom(fieldType) ||
                        typeof(IAnimationClipCollection).IsAssignableFrom(fieldType))
                    {
                        gathererDelegate += (obj, gatherer) =>
                        {
                            var source = field.GetValue(obj);
                            gatherer.Clips.GatherFromSource(source);
                        };
                    }
                    else if (typeof(ICollection).IsAssignableFrom(fieldType))
                    {
                        gathererDelegate += (obj, gatherer) =>
                        {
                            var collection = (ICollection)field.GetValue(obj);
                            if (collection != null)
                            {
                                foreach (var item in collection)
                                {
                                    gatherer.GatherFromObject(item, depth + 1);
                                }
                            }
                        };
                    }
                    else
                    {
                        gathererDelegate += (obj, gatherer) =>
                        {
                            var source = field.GetValue(obj);
                            if (source == null ||
                                (source is Object sourceObject && sourceObject == null))
                                return;

                            gatherer.GatherFromObject(source, depth + 1);
                        };
                    }
                }

                type = type.BaseType;
            }

            return gathererDelegate;
        }

        /************************************************************************************************************************/

        private static bool MightContainAnimations(Type type)
        {
            return
                !type.IsPrimitive &&
                !type.IsEnum &&
                !type.IsAutoClass &&
                !type.IsPointer;
        }

        /************************************************************************************************************************/
    }
}

#endif

