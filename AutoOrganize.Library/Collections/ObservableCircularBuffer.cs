using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AutoOrganize.Library.Collections;

public class ObservableCircularBuffer<T> :
    IList<T>,
    IList,
    IReadOnlyList<T>,
    INotifyCollectionChanged,
    INotifyPropertyChanged
{
    private int _cacheHead;
    private int _cacheTail;
    private bool _cacheIsFull;
    private bool _cacheIsEmpty;

    private readonly CircularBuffer<T> _circularBuffer;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Head => _circularBuffer.Head;

    public int Tail => _circularBuffer.Tail;

    public bool IsEmpty => _circularBuffer.IsEmpty;

    public bool IsFull => _circularBuffer.IsFull;

    public int Count => _circularBuffer.Count;

    public T this[int index]
    {
        get => _circularBuffer[index];
        set
        {
            _circularBuffer[index] = value;
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
        }
    }

    public bool AllowOverwrite
    {
        get => _circularBuffer.AllowOverwrite;
        set
        {
            if (_circularBuffer.AllowOverwrite.Equals(value))
                return;

            _circularBuffer.AllowOverwrite = value;
            OnPropertyChanged();
        }
    }

    public int Capacity
    {
        get => _circularBuffer.Capacity;
        set
        {
            if (_circularBuffer.Capacity.Equals(value))
                return;

            _circularBuffer.Capacity = value;
            OnPropertyChanged();
        }
    }

    public ObservableCircularBuffer(int capacity)
    {
        _circularBuffer = new CircularBuffer<T>(capacity);
    }

    public void Put(T item)
    {
        T? oldItem = default;
        if (Capacity == Count) oldItem = _circularBuffer.Peek();
        _circularBuffer.Put(item);
        if (oldItem is not null)
            OnCollectionRemove(oldItem, 0);
        OnCollectionAdd(item, Count - 1);
    }

    public int Put(T[] array)
    {
        IList<T>? oldItems = null;
        if (Count + array.Length > Capacity)
            oldItems = Peek(Count + array.Length - Capacity);
        int count = _circularBuffer.Put(array);
        if (oldItems is not null)
            OnCollectionRemove(oldItems, 0);
        OnCollectionAdd(array, Count - array.Length - 1);
        return count;
    }

    public int Put(T[] array, int arrayIndex, int count)
    {
        IList<T>? oldItems = null;
        if (Count + array.Length > Capacity)
            oldItems = Peek(Count + array.Length - Capacity);
        int actualCount = _circularBuffer.Put(array, arrayIndex, count);
        if (oldItems is not null)
            OnCollectionRemove(oldItems, 0);
        OnCollectionAdd(array, count - array.Length - arrayIndex);
        return actualCount;
    }

    public void Insert(int index, T item)
    {
        T? oldItem = default;
        if (Capacity == Count)
            oldItem = _circularBuffer.Peek();
        _circularBuffer.Insert(index, item);
        if (oldItem is not null)
            OnCollectionRemove(oldItem, 0);
        OnCollectionAdd(item, index);
    }

    public bool Remove(T item)
    {
        int index = this.IndexOf(item);
        if (index < 0)
            return false;
        this.RemoveAt(index);
        OnCollectionRemove(item, index);
        return true;
    }

    public void RemoveAt(int index)
    {
        T item = _circularBuffer.PeekAt(index);
        _circularBuffer.RemoveAt(index);
        OnCollectionRemove(item, index);
    }

    public void Skip(int count)
    {
        if (count == 1)
        {
            T[] item = _circularBuffer.Peek(1);
            _circularBuffer.Skip(1);
            OnCollectionRemove(item, 0);
            return;
        }

        T[] items = _circularBuffer.Peek(count);
        _circularBuffer.Skip(count);
        OnCollectionRemove(items, 0);
    }

    public void Clear()
    {
        _circularBuffer.Clear();
        OnCollectionReset();
    }

    public T Get()
    {
        T item = _circularBuffer.Get();
        OnCollectionRemove(item, 0);
        return item;
    }

    public T[] Get(int count)
    {
        T[] item = _circularBuffer.Get(count);
        OnCollectionRemove(item, 0);
        return item;
    }

    public int Get(T[] array)
    {
        int count = _circularBuffer.Get(array);
        OnCollectionRemove(array, 0);
        return count;
    }

    public int Get(T[] array, int arrayIndex, int count)
    {
        var c = _circularBuffer.Get(array, arrayIndex, count);
        OnCollectionRemove(array, arrayIndex);
        return c;
    }

    public T GetLast()
    {
        T item = _circularBuffer.GetLast();
        OnCollectionRemove(item, Count);
        return item;
    }

    public T[] GetLast(int count)
    {
        T[] item = _circularBuffer.GetLast(count);
        OnCollectionRemove(item, Count - count);
        return item;
    }

    public int GetLast(T[] array)
    {
        int count = _circularBuffer.GetLast(array);
        OnCollectionRemove(array, Count - array.Length);
        return count;
    }

    public int GetLast(T[] array, int arrayIndex, int count)
    {
        var c = _circularBuffer.Get(array, arrayIndex, count);
        OnCollectionRemove(array, Count - count);
        return c;
    }

    public T Peek() =>
        _circularBuffer.Peek();

    public T[] Peek(int count) =>
        _circularBuffer.Peek(count);

    public T PeekAt(int index) =>
        _circularBuffer.PeekAt(index);

    public T PeekLast() =>
        _circularBuffer.Peek();

    public T[] PeekLast(int count) =>
        _circularBuffer.PeekLast(count);

    public int PeekLast(T[] array) =>
        _circularBuffer.PeekLast(array);

    public int PeekLast(T[] array, int arrayIndex, int count) =>
        _circularBuffer.PeekLast(array, arrayIndex, count);

    public bool Contains(T item) =>
        _circularBuffer.Contains(item);

    public int IndexOf(T item) =>
        _circularBuffer.IndexOf(item);

    public void CopyTo(T[] array) =>
        _circularBuffer.CopyTo(array);

    public void CopyTo(T[] array, int arrayIndex) =>
        _circularBuffer.CopyTo(array, arrayIndex);

    public void CopyTo(int index, T[] array, int arrayIndex, int count) =>
        _circularBuffer.CopyTo(index, array, arrayIndex, count);

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

    bool IList.IsFixedSize => ((IList)_circularBuffer).IsFixedSize;

    bool IList.IsReadOnly => ((IList)_circularBuffer).IsReadOnly;

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return _circularBuffer.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_circularBuffer).GetEnumerator();
    }

    void ICollection<T>.Add(T item)
    {
        Put(item);
    }

    int IList.Add(object? value)
    {
        if (!IsCompatibleObject(value))
            return -1;

        if (!IsCompatibleObject(value))
            return -1;
        this.Put((T)value);
        return Count - 1;
    }

    bool IList.Contains(object? value) =>
        IsCompatibleObject(value) && Contains((T)value);

    int IList.IndexOf(object? value)
    {
        if (!IsCompatibleObject(value))
            return -1;

        return IndexOf((T)value);
    }

    void IList.Insert(int index, object? value)
    {
        if (!IsCompatibleObject(value))
            return;

        Insert(index, (T)value);
    }

    void IList.Remove(object? value)
    {
        if (!IsCompatibleObject(value))
            return;

        Remove((T)value);
    }

    void ICollection<T>.Clear() => Clear();

    void ICollection.CopyTo(Array array, int index) =>
        this.CopyTo((T[])array, index);

    bool ICollection.IsSynchronized => ((ICollection)_circularBuffer).IsSynchronized;

    object ICollection.SyncRoot => ((ICollection)_circularBuffer).SyncRoot;

    bool ICollection<T>.IsReadOnly => ((ICollection<T>)_circularBuffer).IsReadOnly;

    private void OnCollectionAdd(T item)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionAdd(T item, int index)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionAdd(IList<T> items)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionAdd(IList<T> item, int startIndex)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, startIndex));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionRemove(T item)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionRemove(T item, int index)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionRemove(IList<T> items)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionRemove(IList<T> item, int startIndex)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, startIndex));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionReset()
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnCountPropertyChanged();
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionReplace(T oldItem, T newItem)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
        RaisePropertyChangedIfNeeded();
    }

    private void OnCollectionReplace(T oldItem, T newItem, int index)
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        RaisePropertyChangedIfNeeded();
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnCountPropertyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    private void RaisePropertyChangedIfNeeded()
    {
        if (_cacheHead != Head)
        {
            _cacheHead = Head;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Head)));
        }

        if (_cacheTail != Tail)
        {
            _cacheTail = Tail;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tail)));
        }

        if (_cacheIsEmpty != IsEmpty)
        {
            _cacheIsEmpty = IsEmpty;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
        }

        if (_cacheIsFull != IsFull)
        {
            _cacheIsFull = IsFull;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFull)));
        }
    }

    private static bool IsCompatibleObject([NotNullWhen(true)] object? value)
    {
        if (value is T)
            return true;
        return value is null && default(T) is null;
    }
}