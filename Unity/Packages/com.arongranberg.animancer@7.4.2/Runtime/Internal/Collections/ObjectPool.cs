// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

//#define ANIMANCER_LOG_OBJECT_POOLING

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>Convenience methods for accessing <see cref="ObjectPool{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ObjectPool
    /// 
    public static class ObjectPool
    {
        /************************************************************************************************************************/

        /// <summary>Returns a spare item if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(T)"/> it when you are done.</remarks>
        public static T Acquire<T>()
            where T : class, new()
            => ObjectPool<T>.Acquire();

        /// <summary>Returns a spare `item` if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(T)"/> it when you are done.</remarks>
        public static void Acquire<T>(out T item)
            where T : class, new()
            => item = ObjectPool<T>.Acquire();

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the list of spares so it can be reused.</summary>
        public static void Release<T>(T item)
            where T : class, new()
            => ObjectPool<T>.Release(item);

        /// <summary>Adds the `item` to the list of spares so it can be reused and sets it to <c>null</c>.</summary>
        public static void Release<T>(ref T item) where T : class, new()
        {
            ObjectPool<T>.Release(item);
            item = null;
        }

        /************************************************************************************************************************/

        /// <summary>An error message for when something has been modified after being released to the pool.</summary>
        public const string
            NotClearError = " They must be cleared before being released to the pool and not modified after that.";

        /************************************************************************************************************************/

        /// <summary>Returns a spare <see cref="List{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(List{T})"/> it when you are done.</remarks>
        public static List<T> AcquireList<T>()
        {
            var list = ObjectPool<List<T>>.Acquire();
            AnimancerUtilities.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
            return list;
        }

        /// <summary>Returns a spare <see cref="List{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(List{T})"/> it when you are done.</remarks>
        public static void Acquire<T>(out List<T> list)
            => list = AcquireList<T>();

        /// <summary>Clears the `list` and adds it to the list of spares so it can be reused.</summary>
        public static void Release<T>(List<T> list)
        {
            list.Clear();
            ObjectPool<List<T>>.Release(list);
        }

        /// <summary>Clears the `list`, adds it to the list of spares so it can be reused, and sets it to <c>null</c>.</summary>
        public static void Release<T>(ref List<T> list)
        {
            Release(list);
            list = null;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a spare <see cref="HashSet{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(HashSet{T})"/> it when you are done.</remarks>
        public static HashSet<T> AcquireSet<T>()
        {
            var set = ObjectPool<HashSet<T>>.Acquire();
            AnimancerUtilities.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
            return set;
        }

        /// <summary>Returns a spare <see cref="HashSet{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(HashSet{T})"/> it when you are done.</remarks>
        public static void Acquire<T>(out HashSet<T> set)
            => set = AcquireSet<T>();

        /// <summary>Clears the `set` and adds it to the list of spares so it can be reused.</summary>
        public static void Release<T>(HashSet<T> set)
        {
            set.Clear();
            ObjectPool<HashSet<T>>.Release(set);
        }

        /// <summary>Clears the `set`, adds it to the list of spares so it can be reused, and sets it to <c>null</c>.</summary>
        public static void Release<T>(ref HashSet<T> set)
        {
            Release(set);
            set = null;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a spare <see cref="StringBuilder"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release(StringBuilder)"/> it when you are done.</remarks>
        public static StringBuilder AcquireStringBuilder()
        {
            var builder = ObjectPool<StringBuilder>.Acquire();
            AnimancerUtilities.Assert(builder.Length == 0, $"A pooled {nameof(StringBuilder)} is not empty." + NotClearError);
            return builder;
        }

        /// <summary>Sets the <see cref="StringBuilder.Length"/> = 0 and adds it to the list of spares so it can be reused.</summary>
        public static void Release(StringBuilder builder)
        {
            builder.Length = 0;
            ObjectPool<StringBuilder>.Release(builder);
        }

        /// <summary>[Animancer Extension] Calls <see cref="StringBuilder.ToString()"/> and <see cref="Release(StringBuilder)"/>.</summary>
        public static string ReleaseToString(this StringBuilder builder)
        {
            var result = builder.ToString();
            Release(builder);
            return result;
        }

        /************************************************************************************************************************/

        /// <summary>Convenience wrappers for <see cref="ObjectPool{T}.Disposable"/>.</summary>
        public static class Disposable
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ObjectPool{T}.Disposable"/> and calls <see cref="ObjectPool{T}.Acquire"/> to set the
            /// <see cref="ObjectPool{T}.Disposable.Item"/> and `item`.
            /// </summary>
            public static ObjectPool<T>.Disposable Acquire<T>(out T item)
                where T : class, new()
                => new ObjectPool<T>.Disposable(out item);

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ObjectPool{T}.Disposable"/> and calls <see cref="ObjectPool{T}.Acquire"/> to set the
            /// <see cref="ObjectPool{T}.Disposable.Item"/> and `item`.
            /// </summary>
            public static ObjectPool<List<T>>.Disposable AcquireList<T>(out List<T> list)
            {
                var disposable = new ObjectPool<List<T>>.Disposable(out list, onRelease: (l) => l.Clear());
                AnimancerUtilities.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
                return disposable;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ObjectPool{T}.Disposable"/> and calls <see cref="ObjectPool{T}.Acquire"/> to set the
            /// <see cref="ObjectPool{T}.Disposable.Item"/> and `item`.
            /// </summary>
            public static ObjectPool<HashSet<T>>.Disposable AcquireSet<T>(out HashSet<T> set)
            {
                var disposable = new ObjectPool<HashSet<T>>.Disposable(out set, onRelease: (s) => s.Clear());
                AnimancerUtilities.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
                return disposable;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="ObjectPool{T}.Disposable"/> and calls <see cref="ObjectPool{T}.Acquire"/> to set the
            /// <see cref="ObjectPool{T}.Disposable.Item"/> and `item`.
            /// </summary>
            public static ObjectPool<GUIContent>.Disposable AcquireContent(out GUIContent content,
                string text = null, string tooltip = null, bool narrowText = true)
            {
                var disposable = new ObjectPool<GUIContent>.Disposable(out content, onRelease: c =>
                {
                    c.text = null;
                    c.tooltip = null;
                    c.image = null;
                });

#if UNITY_ASSERTIONS
                if (!string.IsNullOrEmpty(content.text) ||
                    !string.IsNullOrEmpty(content.tooltip) ||
                    content.image != null)
                {
                    throw new UnityEngine.Assertions.AssertionException(
                        $"A pooled {nameof(GUIContent)} is not cleared." + NotClearError,
                        $"- {nameof(content.text)} = '{content.text}'" +
                        $"\n- {nameof(content.tooltip)} = '{content.tooltip}'" +
                        $"\n- {nameof(content.image)} = '{content.image}'");
                }
#endif

                content.text = text;
                content.tooltip = tooltip;
                content.image = null;
                return disposable;
            }

            /************************************************************************************************************************/

#if UNITY_EDITOR
            /// <summary>[Editor-Only]
            /// Creates a new <see cref="ObjectPool{T}.Disposable"/> and calls <see cref="ObjectPool{T}.Acquire"/> to set the
            /// <see cref="ObjectPool{T}.Disposable.Item"/> and `item`.
            /// </summary>
            public static ObjectPool<GUIContent>.Disposable AcquireContent(out GUIContent content,
                UnityEditor.SerializedProperty property, bool narrowText = true)
                => AcquireContent(out content, property.displayName, property.tooltip, narrowText);
#endif

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// <summary>A simple object pooling system.</summary>
    /// <remarks><typeparamref name="T"/> must not inherit from <see cref="Component"/> or <see cref="ScriptableObject"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ObjectPool_1
    /// 
    public static class ObjectPool<T> where T : class, new()
    {
        /************************************************************************************************************************/

        private static readonly List<T>
            Items = new List<T>();

        /************************************************************************************************************************/

        /// <summary>The number of spare items currently in the pool.</summary>
        public static int Count
        {
            get => Items.Count;
            set
            {
                var count = Items.Count;
                if (count < value)
                {
                    if (Items.Capacity < value)
                        Items.Capacity = Mathf.NextPowerOfTwo(value);

                    do
                    {
                        Items.Add(new T());
                        count++;
                    }
                    while (count < value);

                }
                else if (count > value)
                {
                    Items.RemoveRange(value, count - value);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Increases the <see cref="Count"/> to equal the `count` if it was lower.</summary>
        public static void IncreaseCountTo(int count)
        {
            if (Count < count)
                Count = count;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="List{T}.Capacity"/> of the internal list of spare items.</summary>
        public static int Capacity
        {
            get => Items.Capacity;
            set
            {
                if (Items.Count > value)
                    Items.RemoveRange(value, Items.Count - value);
                Items.Capacity = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Increases the <see cref="Capacity"/> to equal the `capacity` if it was lower.</summary>
        public static void IncreaseCapacityTo(int capacity)
        {
            if (Capacity < capacity)
                Capacity = capacity;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a spare item if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release(T)"/> it when you are done.</remarks>
        public static T Acquire()
        {
            var count = Items.Count;
            if (count == 0)
            {
                return new T();
            }
            else
            {
                count--;
                var item = Items[count];
                Items.RemoveAt(count);

                return item;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the list of spares so it can be reused.</summary>
        public static void Release(T item)
        {
            AnimancerUtilities.Assert(item != null,
                $"Null objects must not be released into an {nameof(ObjectPool<T>)}.");

            Items.Add(item);

        }

        /************************************************************************************************************************/

        /// <summary>Returns a description of the state of this pool.</summary>
        public static string GetDetails()
        {
            return
                $"{typeof(T).Name}" +
                $" ({nameof(Count)} = {Items.Count}" +
                $", {nameof(Capacity)} = {Items.Capacity}" +
                ")";
        }

        /************************************************************************************************************************/

        /// <summary>
        /// An <see cref="IDisposable"/> to allow pooled objects to be acquired and released within <c>using</c>
        /// statements instead of needing to manually release everything.
        /// </summary>
        public readonly struct Disposable : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The object acquired from the <see cref="ObjectPool{T}"/>.</summary>
            public readonly T Item;

            /// <summary>Called by <see cref="IDisposable.Dispose"/>.</summary>
            public readonly Action<T> OnRelease;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Disposable"/> and calls <see cref="ObjectPool{T}.Acquire"/> to set the
            /// <see cref="Item"/> and `item`.
            /// </summary>
            public Disposable(out T item, Action<T> onRelease = null)
            {
                Item = item = Acquire();
                OnRelease = onRelease;
            }

            /************************************************************************************************************************/

            void IDisposable.Dispose()
            {
                OnRelease?.Invoke(Item);
                Release(Item);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

