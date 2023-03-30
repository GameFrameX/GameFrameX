// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;

namespace Animancer
{
    /// <summary>Stores the index of an object in a <see cref="KeyedList{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/Key
    /// 
    public class Key : Key.IListItem
    {
        /************************************************************************************************************************/

        /// <summary>An object with a <see cref="Animancer.Key"/> so it can be used in a <see cref="KeyedList{T}"/>.</summary>
        /// <example>
        /// It's usually easiest to just inherit from <see cref="Animancer.Key"/>, but otherwise the recommended
        /// implementation looks like this:
        /// <para></para><code>
        /// class MyClass : Key.IListItem
        /// {
        ///     Key Key.IListItem.Key { get; } = new Key();
        ///     // Don't use expression bodied ...Key => new... because that would create a new one every time.
        /// }
        /// </code></example>
        /// https://kybernetik.com.au/animancer/api/Animancer/IListItem
        /// 
        public interface IListItem
        {
            /// <summary>
            /// The <see cref="Animancer.Key"/> which stores the <see cref="KeyedList{T}"/> index of this object.
            /// </summary>
            Key Key { get; }
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="_Index"/> which indicates that an item isn't in a list.</summary>
        public const int NotInList = -1;

        /// <summary>The current position of this key in the list.</summary>
        private int _Index = -1;

        /// <summary>Returns location of this object in the list (or <c>-1</c> if it is not currently in a keyed list).</summary>
        public static int IndexOf(Key key) => key._Index;

        /// <summary>Is the `key` currently in a keyed list?</summary>
        public static bool IsInList(Key key) => key._Index != NotInList;

        /************************************************************************************************************************/

        /// <summary>A <see cref="Key"/> is its own <see cref="IListItem.Key"/>.</summary>
        Key IListItem.Key => this;

        /************************************************************************************************************************/

        /// <summary>A <see cref="List{T}"/> which can remove items without needing to search the entire collection.</summary>
        /// <remarks>
        /// This implementation has several restrictions compared to a regular <see cref="List{T}"/>:
        /// <list type="bullet">
        /// <item>Items must implement <see cref="IListItem"/> or inherit from <see cref="Key"/>.</item>
        /// <item>Items cannot be <c>null</c>.</item>
        /// <item>Items can only be in one <see cref="KeyedList{T}"/> at a time and cannot appear multiple times in it.</item>
        /// </list>
        /// This class is nested inside <see cref="Key"/> so it can modify the private <see cref="_Index"/> without
        /// exposing that capability to anything else.
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/KeyedList_1
        /// 
        public class KeyedList<T> : IList<T>, ICollection where T : class, IListItem
        {
            /************************************************************************************************************************/

            private const string
                SingleUse = "Each item can only be used in one " + nameof(KeyedList<T>) + " at a time.",
                NotFound = "The specified item does not exist in this " + nameof(KeyedList<T>) + ".";

            /************************************************************************************************************************/

            private readonly List<T> Items;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="KeyedList{T}"/> using the default <see cref="List{T}"/> constructor.</summary>
            public KeyedList() => Items = new List<T>();

            /// <summary>Creates a new <see cref="KeyedList{T}"/> with the specified initial `capacity`.</summary>
            public KeyedList(int capacity) => Items = new List<T>(capacity);

            // No copy constructor because the keys will not work if they are used in multiple lists at once.

            /************************************************************************************************************************/

            /// <summary>The number of items currently in the list.</summary>
            public int Count => Items.Count;

            /// <summary>The number of items that this list can contain before resizing is required.</summary>
            public int Capacity
            {
                get => Items.Capacity;
                set => Items.Capacity = value;
            }

            /************************************************************************************************************************/

            /// <summary>The item at the specified `index`.</summary>
            /// <exception cref="ArgumentException">The `value` was already in a keyed list (setter only).</exception>
            public T this[int index]
            {
                get => Items[index];
                set
                {
                    var key = value.Key;

                    // Make sure it isn't already in a list.
                    if (key._Index != NotInList)
                        throw new ArgumentException(SingleUse);

                    // Remove the old item at that index.
                    Items[index].Key._Index = NotInList;

                    // Set the index of the new item and add it at that index.
                    key._Index = index;
                    Items[index] = value;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Indicates whether the `item` is currently in this list.</summary>
            public bool Contains(T item)
            {
                if (item == null)
                    return false;

                var index = item.Key._Index;
                return
                    (uint)index < (uint)Items.Count &&
                    Items[index] == item;
            }

            /************************************************************************************************************************/

            /// <summary>Returns the index of the `item` in this list or <c>-1</c> if it is not in this list.</summary>
            public int IndexOf(T item)
            {
                if (item == null)
                    return NotInList;

                var index = item.Key._Index;
                if ((uint)index < (uint)Items.Count &&
                    Items[index] == item)
                    return index;
                else
                    return NotInList;
            }

            /************************************************************************************************************************/

            /// <summary>Adds the `item` to the end of this list.</summary>
            /// <exception cref="ArgumentException">The `item` was already in a keyed list.</exception>
            public void Add(T item)
            {
                var key = item.Key;

                // Make sure it isn't already in a list.
                if (key._Index != NotInList)
                    throw new ArgumentException(SingleUse);

                // Set the index of the new item and add it to the list.
                key._Index = Items.Count;
                Items.Add(item);
            }

            /// <summary>Adds the `item` to the end of this list if it wasn't already in it.</summary>
            public void AddNew(T item)
            {
                if (!Contains(item))
                    Add(item);
            }

            /************************************************************************************************************************/

            /// <summary>Adds the `item` to this list at the specified `index`.</summary>
            public void Insert(int index, T item)
            {
                for (int i = index; i < Items.Count; i++)
                    Items[i].Key._Index++;

                item.Key._Index = index;
                Items.Insert(index, item);
            }

            /************************************************************************************************************************/

            /// <summary>Removes the item at the specified `index`.</summary>
            public void RemoveAt(int index)
            {
                // Adjust the indices of all items after the target.
                for (int i = index + 1; i < Items.Count; i++)
                    Items[i].Key._Index--;

                // Mark the key as removed and remove the item.
                Items[index].Key._Index = NotInList;
                Items.RemoveAt(index);
            }

            /// <summary>Removes the item at the specified `index` by swapping the last item in this list into its place.</summary>
            /// <remarks>
            /// This does not maintain the order of items, but is more efficient than <see cref="RemoveAt"/> because
            /// it avoids the need to move every item after the target down one place.
            /// </remarks>
            public void RemoveAtSwap(int index)
            {
                // Mark the item as removed.
                Items[index].Key._Index = NotInList;

                // If it wasn't the last item, move the last item over it.
                var lastIndex = Items.Count - 1;
                if (lastIndex > index)
                {
                    var lastItem = Items[lastIndex];
                    lastItem.Key._Index = index;
                    Items[index] = lastItem;
                }

                // Remove the last item from the list.
                Items.RemoveAt(lastIndex);
            }

            /************************************************************************************************************************/

            /// <summary>Removes the `item` from this list.</summary>
            /// <exception cref="ArgumentException">The `item` is not in this list.</exception>
            public bool Remove(T item)
            {
                var key = item.Key;
                var index = key._Index;

                // If it isn't in a list, do nothing.
                if (index == NotInList)
                    return false;

                // Make sure the item is actually in this list at the index it says.
                // Otherwise it must be in a different list.
                if (Items[index] != item)
                    throw new ArgumentException(NotFound, nameof(item));

                // Remove the item.
                RemoveAt(index);
                return true;
            }

            /************************************************************************************************************************/

            /// <summary>Removes the `item` by swapping the last item in this list into its place.</summary>
            /// <remarks>
            /// This does not maintain the order of items, but is more efficient than <see cref="Remove"/> because
            /// it avoids the need to move every item after the target down one place.
            /// </remarks>
            /// <exception cref="ArgumentException">The `item` is not in this list.</exception>
            public bool RemoveSwap(T item)
            {
                var key = item.Key;
                var index = key._Index;

                // If it isn't in a list, do nothing.
                if (index == NotInList)
                    return false;

                // Make sure the item is actually in this list at the index it says.
                // Otherwise it must be in a different list.
                if (Items[index] != item)
                    throw new ArgumentException(NotFound, nameof(item));

                // Remove the item.
                RemoveAtSwap(index);
                return true;
            }

            /************************************************************************************************************************/

            /// <summary>Removes all items from this list.</summary>
            public void Clear()
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                    Items[i].Key._Index = NotInList;

                Items.Clear();
            }

            /************************************************************************************************************************/

            /// <summary>Copies all the items from this list into the `array`, starting at the specified `index`.</summary>
            public void CopyTo(T[] array, int index) => Items.CopyTo(array, index);

            /// <summary>Copies all the items from this list into the `array`, starting at the specified `index`.</summary>
            void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

            /// <summary>Returns false.</summary>
            bool ICollection<T>.IsReadOnly => false;

            /// <summary>Returns an enumerator that iterates through this list.</summary>
            public List<T>.Enumerator GetEnumerator() => Items.GetEnumerator();

            /// <inheritdoc/>
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /************************************************************************************************************************/

            /// <summary>Is this list thread safe?</summary>
            bool ICollection.IsSynchronized => ((ICollection)Items).IsSynchronized;

            /// <summary>An object that can be used to synchronize access to this <see cref="ICollection"/>.</summary>
            object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

