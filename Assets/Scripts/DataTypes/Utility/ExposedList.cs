using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fire_Pixel.Utility
{
    /// <summary>
    /// Custom list datatype providing a bunc of built in utility methods. Also exposes the internal array.
    /// </summary>
    [System.Serializable]
    public class ExposedList<T>
    {
        [SerializeField] private T[] array;
        public T[] InternalArray => array;
        public bool IsCreated => array != null;
        public int Capacity => array?.Length ?? 0;

        [field: SerializeField, InspectorReadOnly] public int Count { get; private set; }


        public ExposedList() : this(0) { }
        public ExposedList(int capacity)
        {
            DebugLogger.LogAssertion("ExposedList created with negative capacity, this is not allowed", capacity < 0);

            array = new T[capacity];
            Count = 0;
        }
        public ExposedList(T[] array, int count)
        {
            DebugLogger.LogAssertion("ExposedList created with null array, this is not allowed", array == null);
            DebugLogger.LogAssertion("ExposedList created with negative count, this is not allowed", count < 0);

            this.array = array ?? Array.Empty<T>();
            Count = Math.Clamp(count, 0, this.array.Length);
        }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfRangeException();

                return array[index];
            }
            set
            {
                if ((uint)index >= (uint)Count)
                    throw new IndexOutOfRangeException();

                array[index] = value;
            }
        }


        #region Entry Addition

        /// <summary>
        /// Add <paramref name="item"/> to the list in the first free slot. (Resize list to make space if needed)
        /// </summary>
        public void Add(T item)
        {
            if (Count >= Capacity)
            {
                Resize(Capacity == 0 ? 4 : Capacity * 2);
            }

            array[Count++] = item;
        }
        /// <summary>
        /// Add <paramref name="item"/> to the list in the first free slot. (Warning: an error will be thrown when exceeding the list capacity.)
        /// </summary>
        public void AddNoResize(T item)
        {
            DebugLogger.LogAssertion("List is out of space!", Count >= Capacity);

            array[Count++] = item;
        }

        /// <summary>
        /// Add an entire array of entries to the list in the first free slots. (Resizes list to make space if needed.)
        /// </summary>
        public void AddRange(ReadOnlySpan<T> items)
        {
            int requiredCapacity = Count + items.Length;

            if (requiredCapacity > Capacity)
            {
                int newCapacity = Math.Max(requiredCapacity, Capacity == 0 ? 4 : Capacity * 2);
                Resize(newCapacity);
            }

            items.CopyTo(array.AsSpan(Count));
            Count += items.Length;
        }
        /// <summary>
        /// Add an entire array of entries to the list in the first free slots. (Warning: allocates an array when passing multiple individual parameters.) (Resizes list to make space if needed.)
        /// </summary>
        public void AddRange(params T[] items) => AddRange((ReadOnlySpan<T>)items);

        /// <summary>
        /// Add an entire array of entries to the list in the first free slots. (Resizes list to make space if needed.)
        /// </summary>
        public void AddRangeNoResize(ReadOnlySpan<T> items)
        {
            DebugLogger.LogAssertion("List is out of space!", Count + items.Length > Capacity);

            items.CopyTo(array.AsSpan(Count));
            Count += items.Length;
        }
        /// <summary>
        /// Add an entire array of entries to the list in the first free slots. (Warning: allocates an array when passing multiple individual parameters.) (Warning: an error will be thrown when exceeding the list capacity.)
        /// </summary>
        public void AddRangeNoResize(params T[] items) => AddRangeNoResize((ReadOnlySpan<T>)items);

        #endregion


        #region Capacity Handling

        /// <summary>
        /// Resize list capacity to <paramref name="newCapacity"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newCapacity)
        {
            T[] newArray = new T[newCapacity];
            Array.Copy(array, newArray, Count);
            array = newArray;
        }

        /// <summary>
        /// Check if lists current capacity is less then <paramref name="capacity"/>, if so, resize it to new capacity.
        /// </summary>
        public void EnsureCapacity(int capacity)
        {
            if (capacity <= Capacity)
                return;

            Resize(capacity);
        }

        /// <summary>
        /// Resize list to match its capacity equal to the current entry count to spare memory usage. (Does nothing if the list is already completely full)
        /// </summary>
        public void TrimExcess()
        {
            if (Count == Capacity) return;

            Resize(Count);
        }

        #endregion


        #region Entry Removal

        /// <summary>
        /// Remove an entry from the list at <paramref name="index"/> and swap all entries above it one downwards to maintain the list order. O(n)
        /// </summary>
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
                throw new IndexOutOfRangeException();

            int newCount = --Count;

            for (int i = index; i < newCount; i++)
            {
                array[i] = array[i + 1];
            }

            array[newCount] = default;
        }
        /// <summary>
        /// Remove an entry from the list at <paramref name="index"/> and swap the last entry to that position. Order is not maintained. O(1)
        /// </summary>
        public void RemoveAtSwapBack(int index)
        {
            if ((uint)index >= (uint)Count)
                throw new IndexOutOfRangeException();

            Count--;

            array[index] = array[Count];
            array[Count] = default;
        }

        /// <summary>
        /// Remove an entry from the list equal to <paramref name="value"/> and swap all entries above it one downwards to maintain the list order. O(n)
        /// </summary>
        public void Remove(T value)
        {
            int currentCount = Count;
            for (int i = 0; i < currentCount; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(array[i], value)) continue;

                for (int i2 = i; i2 < currentCount - 1; i2++)
                {
                    array[i2] = array[i2 + 1];
                }
                Count--;
                array[Count] = default;

                return;
            }

            DebugLogger.LogWarning($"Value not found for exposedlist removal, value: '{value}'");
        }
        /// <summary>
        /// Remove an entry from the list equal to <paramref name="value"/> and swap all entries above it one downwards to maintain the list order. O(n)
        /// </summary>
        public bool TryRemove(T value)
        {
            int currentCount = Count;
            for (int i = 0; i < currentCount; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(array[i], value)) continue;

                for (int i2 = i; i2 < currentCount - 1; i2++)
                {
                    array[i2] = array[i2 + 1];
                }
                Count--;
                array[Count] = default;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove an entry from the list equal to <paramref name="value"/> and swap the last entry to that position. Order is not maintained. O(n)
        /// </summary>
        public void RemoveSwapBack(T value)
        {
            if (!TryGetIndexOf(value, out int index)) return;

            RemoveAtSwapBack(index);
        }
        /// <summary>
        /// Remove an entry from the list equal to <paramref name="value"/> and swap the last entry to that position. Order is not maintained. O(n)
        /// </summary>
        public bool TryRemoveSwapBack(T value)
        {
            if (!TryGetIndexOf(value, out int index)) return false;

            RemoveAtSwapBack(index);
            return true;
        }

        /// <summary>
        /// Remove a range of entries from the list and swap all entries above it downwards to maintain the list order. O(n)
        /// </summary>
        public void RemoveRangeAt(int index, int count)
        {
            if ((uint)index >= (uint)Count || count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException();

            int remaining = Count - (index + count);

            if (remaining > 0)
            {
                Array.Copy(array, index + count, array, index, remaining);
            }

            Count -= count;
            Array.Clear(array, Count, count);
        }
        /// <summary>
        /// Remove a range of entries from the list and swap the last entries to the removed positions. Order is not maintained. O(count)
        /// </summary>
        public void RemoveRangeAtSwapBack(int index, int count)
        {
            if ((uint)index >= (uint)Count || count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException();

            int oldCount = Count;
            int newCount = oldCount - count;

            for (int i = 0; i < count; i++)
            {
                int destination = index + i;
                int source = oldCount - 1 - i;

                if (destination >= source)
                    break;

                array[destination] = array[source];
            }

            Array.Clear(array, newCount, count);
            Count = newCount;
        }

        #endregion


        #region Data Lookup

        /// <summary>
        /// Check if <paramref name="value"/> is present in the list
        /// </summary>
        /// <returns>True if <paramref name="value"/> was found in the list</returns>
        public bool Contains(T value)
        {
            int currentCount = Count;
            for (int i = 0; i < currentCount; i++)
            {
                if (EqualityComparer<T>.Default.Equals(array[i], value)) return true;
            }

            return false;
        }

        /// <summary>
        /// Get the position (index) of <paramref name="value"/> in the list.
        /// </summary>
        /// <returns>The position (index) of <paramref name="value"/> in the list. (Returns -1 if it isnt found)</returns>
        public int IndexOf(T value)
        {
            int currentCount = Count;
            for (int i = 0; i < currentCount; i++)
            {
                if (EqualityComparer<T>.Default.Equals(array[i], value)) return i;
            }

            return -1;
        }
        /// <summary>
        /// Try to get the position (index) of <paramref name="value"/> in the list.
        /// </summary>
        /// <returns>The position (index) of <paramref name="value"/> in the list if it was found (Returns -1 if it isnt found)</returns>
        public bool TryGetIndexOf(T value, out int index)
        {
            int currentCount = Count;
            for (int i = 0; i < currentCount; i++)
            {
                if (EqualityComparer<T>.Default.Equals(array[i], value))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        #endregion


        #region List Clear/Disposal

        /// <summary>
        /// Set internal list.count to zero. Optionally clear all entries in the array if <paramref name="wipeListData"/> is true
        /// </summary>
        public void Clear(bool wipeListData = false)
        {
            if (wipeListData)
            {
                Array.Clear(array, 0, Count);
            }

            Count = 0;
        }
        /// <summary>
        /// Set internal list.count to zero. Set internal array to null.
        /// </summary>
        public void Release()
        {
            Count = 0;
            array = null;
        }

        #endregion
    }
}