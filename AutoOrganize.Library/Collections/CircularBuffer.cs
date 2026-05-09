/*
New BSD License (BSD)

Copyright (c) 2014-2025 Cyotek Ltd
Copyright (c) 2012, Alex Regueiro
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the copyright holder nor the names of its
      contributors may be used to endorse or promote products derived from
      this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

// Based on: https://github.com/cyotek/Cyotek.Collections.Generic.CircularBuffer

// based on http://circularbuffer.codeplex.com/
// http://en.wikipedia.org/wiki/Circular_buffer

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AutoOrganize.Library.Collections;

public class CircularBuffer<T> : IList<T>, IList, IReadOnlyList<T>
{
    private T[] _buffer = [];
    private int _capacity;

    public int Head { get; private set; }

    public int Tail { get; private set; }

    public bool IsEmpty => Count == 0;

    public bool IsFull => !AllowOverwrite && Count == _capacity;

    public int Count { get; private set; }

    bool ICollection<T>.IsReadOnly => false;

    bool IList.IsReadOnly => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    bool IList.IsFixedSize => true;

    public bool AllowOverwrite { get; set; }

    public int Capacity
    {
        get => _capacity;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value != _capacity)
            {
                if (value < Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "The new capacity must be greater than or equal to the buffer size.");
                }

                var newBuffer = new T[value];
                if (Count > 0)
                    this.CopyTo(newBuffer);


                _buffer = newBuffer;
                Tail = Count;
                Head = 0;

                _capacity = value;
            }
        }
    }

    public T this[int index]
    {
        get => PeekAt(index);
        set
        {
            if (index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _buffer[GetHeadIndex(index)] = value;
        }
    }

    object? IList.this[int index]
    {
        get => this[index];
        set
        {
            if (!IsCompatibleObject(value))
                return;
            this[index] = (T)value;
        }
    }

    T IReadOnlyList<T>.this[int index] => this[index];

    public CircularBuffer(int capacity, bool allowOverwrite = true)
    {
        if (capacity < 0)
        {
            throw new ArgumentException("The buffer capacity must be greater than or equal to zero.",
                nameof(capacity));
        }

        Count = 0;
        this.Capacity = capacity;
        AllowOverwrite = allowOverwrite;
    }

    void ICollection<T>.Add(T item)
    {
        this.Put(item);
    }

    int IList.Add(object? value)
    {
        if (!IsCompatibleObject(value))
            return -1;
        this.Put((T)value);
        return Count - 1;
    }

    public void Put(T item)
    {
        if (!AllowOverwrite && Count == _capacity)
        {
            throw new InvalidOperationException("The buffer does not have sufficient capacity to put new items.");
        }

        _buffer[Tail] = item;

        Tail = WrapIndex(Tail + 1);
        if (Count == _capacity)
        {
            Head++;
            if (Head >= _capacity)
            {
                Head -= _capacity;
            }
        }

        if (Count != _capacity)
        {
            Count++;
        }
    }

    public int Put(T[] array)
    {
        return this.Put(array, 0, array.Length);
    }

    public int Put(T[] array, int arrayIndex, int count)
    {
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (arrayIndex + count == array.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "The specified range exceeds the source array.");

        if (!AllowOverwrite && count > _capacity - Count)
            throw new InvalidOperationException("The buffer does not have sufficient capacity to put new items.");

        int i;
        for (i = 0; i < count; i++)
        {
            this.Put(array[arrayIndex + i]);
        }

        return i;
    }

    void IList.Insert(int index, object? value)
    {
        if (!IsCompatibleObject(value))
            return;
        Insert(index, (T)value);
    }

    public void Insert(int index, T item)
    {
        if (index > Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Count == _capacity)
            Head = GetHeadIndex(1);

        int itemIndex = GetHeadIndex(index);
        for (int i = Tail; i != itemIndex; i--)
        {
            if (i == 0)
                i = _capacity - 1;

            int last = i - 1;
            if (last < 0)
                last = _capacity - 1;

            _buffer[i] = _buffer[last];
        }

        _buffer[itemIndex] = item;

        Tail = WrapIndex(Tail + 1);
        Count++;
    }

    void IList.Remove(object? value)
    {
        if (!IsCompatibleObject(value))
            return;
        Remove((T)value);
    }

    public bool Remove(T item)
    {
        int index = this.IndexOf(item);
        if (index < 0)
            return false;
        this.RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        int removeIndex = GetHeadIndex(index);
        Count--;
        Tail = GetTailIndex(0);
        if (index != Count)
        {
            int lastIndex = Tail - 1;
            if (lastIndex < 0)
                lastIndex = _capacity - 1;
            for (int nowHead = removeIndex; nowHead != lastIndex; nowHead++)
            {
                if (nowHead == _capacity)
                    nowHead = 0;

                int next = nowHead + 1;
                if (next == _capacity)
                    next = 0;

                _buffer[nowHead] = _buffer[next];
            }
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            this._buffer[Tail] = default!;
    }

    public void Skip(int count)
    {
        if (count < 0 || count > Count)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == Count)
        {
            Clear();
            return;
        }

        int oldHead = Head;
        Head = this.GetHeadIndex(count);
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) return;
        for (int i = 0; i < count; i++)
        {
            if (oldHead == _capacity)
                oldHead = 0;

            _buffer[oldHead++] = default!;
        }

        Count -= count;
    }

    public void Clear()
    {
        Count = 0;
        Head = 0;
        Tail = 0;
        Array.Clear(_buffer);
    }

    public T Get()
    {
        if (this.IsEmpty)
            throw new InvalidOperationException("The buffer is empty.");

        T item = _buffer[Head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _buffer[Head] = default!;
        if (++Head == _capacity)
            Head = 0;

        Count--;
        return item;
    }

    public T[] Get(int count)
    {
        var result = new T[count];

        this.Get(result);

        return result;
    }

    public int Get(T[] array)
    {
        return this.Get(array, 0, array.Length);
    }

    public int Get(T[] array, int arrayIndex, int count)
    {
        int realCount = Math.Min(count, Count);
        int dstIndex = arrayIndex;

        for (int i = 0; i < realCount; i++, dstIndex++)
        {
            if (Head == _capacity)
                Head = 0;

            array[dstIndex] = _buffer[Head++];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                _buffer[Head - 1] = default!;
        }

        if (Head == _capacity)
            Head = 0;

        Count -= realCount;
        return realCount;
    }

    public T GetLast()
    {
        if (this.IsEmpty)
            throw new InvalidOperationException("The buffer is empty.");

        int index = this.GetTailIndex(0);
        T item = _buffer[index];

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _buffer[index] = default!;

        Tail = WrapIndex(Tail + 1);
        Count--;
        return item;
    }

    public T[] GetLast(int count)
    {
        var result = new T[count];

        this.GetLast(result);

        return result;
    }

    public int GetLast(T[] array)
    {
        return this.GetLast(array, 0, array.Length);
    }

    public int GetLast(T[] array, int arrayIndex, int count)
    {
        int realCount = Math.Min(count, Count);

        for (int i = realCount; i > 0; i--)
        {
            array[arrayIndex + i - 1] = this.GetLast();
        }

        return realCount;
    }

    public T Peek()
    {
        if (this.IsEmpty)
            throw new InvalidOperationException("The buffer is empty.");

        T item = _buffer[Head];

        return item;
    }

    public T[] Peek(int count)
    {
        if (this.IsEmpty)
            throw new InvalidOperationException("The buffer is empty.");

        var items = new T[count];
        this.CopyTo(items);

        return items;
    }

    public T PeekAt(int index)
    {
        if (this.IsEmpty)
            throw new InvalidOperationException("The buffer is empty.");

        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index,
                $"Index must be between 0 and {Count}.");
        }

        return _buffer[this.GetHeadIndex(index)];
    }

    public T PeekLast()
    {
        if (this.IsEmpty)
            throw new InvalidOperationException("The buffer is empty.");

        int index = this.GetTailIndex(0);
        T item = _buffer[index];

        return item;
    }

    public T[] PeekLast(int count)
    {
        var result = new T[count];

        this.PeekLast(result);

        return result;
    }

    public int PeekLast(T[] array)
    {
        return this.PeekLast(array, 0, array.Length);
    }

    public int PeekLast(T[] array, int arrayIndex, int count)
    {
        int realCount = Math.Min(count, Count);

        for (int i = 0; i < realCount; i++)
        {
            array[arrayIndex + (realCount - (i + 1))] = _buffer[this.GetTailIndex(i)];
        }

        return realCount;
    }

    bool IList.Contains(object? value)
    {
        if (!IsCompatibleObject(value))
            return false;
        return Contains((T)value);
    }

    public bool Contains(T item)
    {
        int bufferIndex = Head;
        var comparer = EqualityComparer<T>.Default;

        for (int i = 0; i < Count; i++, bufferIndex++)
        {
            if (bufferIndex == _capacity)
                bufferIndex = 0;

            if (comparer.Equals(_buffer[bufferIndex], item))
                return true;
        }

        return false;
    }

    int IList.IndexOf(object? value)
    {
        if (!IsCompatibleObject(value))
            return -1;
        return IndexOf((T)value);
    }

    public int IndexOf(T item)
    {
        int bufferIndex = Head;
        var comparer = EqualityComparer<T>.Default;

        for (int i = 0; i < Count; i++, bufferIndex++)
        {
            if (bufferIndex == _capacity)
                bufferIndex = 0;

            if (comparer.Equals(_buffer[bufferIndex], item))
                return i;
        }

        return -1;
    }

    public void CopyTo(T[] array)
    {
        this.CopyTo(array, 0);
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
        this.CopyTo((T[])array, arrayIndex);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        this.CopyTo(0, array, arrayIndex, Math.Min(Count, array.Length - arrayIndex));
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        if (count > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count,
                "The read count cannot be greater than the buffer size.");
        }

        int bufferIndex = Head + index;

        for (int i = 0; i < count; i++, bufferIndex++, arrayIndex++)
        {
            if (bufferIndex == _capacity)
                bufferIndex = 0;

            array[arrayIndex] = _buffer[bufferIndex];
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        int bufferIndex = Head;

        for (int i = 0; i < Count; i++, bufferIndex++)
        {
            if (bufferIndex == _capacity)
                bufferIndex = 0;

            yield return _buffer[bufferIndex];
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    private int GetHeadIndex(int index)
    {
        int newIndex = WrapIndex(Head + index);
        return newIndex;
    }

    private int GetTailIndex(int index)
    {
        int bufferIndex = WrapIndex(Tail == 0
            ? Count - (index + 1)
            : Tail - (index + 1));

        return bufferIndex;
    }

    private int WrapIndex(int index)
    {
        if (index >= _capacity)
            index -= _capacity;
        else if (index < 0)
            index += _capacity;

        return index;
    }

    private static bool IsCompatibleObject([NotNullWhen(true)] object? value)
    {
        if (value is T)
            return true;
        return value is null && default(T) is null;
    }
}