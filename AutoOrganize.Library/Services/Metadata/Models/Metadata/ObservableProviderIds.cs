using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata;

public sealed class ObservableProviderIds : IProviderIds, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly Dictionary<string, string> _items;

    public int Count => _items.Count;

    public string this[string key]
    {
        get => _items[key];
        set
        {
            if (_items.TryGetValue(key, out string? oldValue))
            {
                _items[key] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<string, string>(key, value),
                    new KeyValuePair<string, string>(key, oldValue)));
                return;
            }

            _items[key] = value;
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                new KeyValuePair<string, string>(key, value)));
        }
    }

    public ObservableProviderIds()
    {
        _items = new Dictionary<string, string>();
    }

    public ObservableProviderIds(int capacity)
    {
        _items = new Dictionary<string, string>(capacity);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        => _items.TryGetValue(key, out value);

    public void Add(string key, string value)
    {
        _items.Add(key, value);
        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            new KeyValuePair<string, string>(key, value)));
    }

    public bool TryAdd(string key, string value)
    {
        if (!_items.TryAdd(key, value))
            return false;

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            new KeyValuePair<string, string>(key, value)));
        return true;
    }

    public bool ContainsKey(string key) => _items.ContainsKey(key);

    public bool Remove(string key)
    {
        if (!_items.Remove(key, out string? value))
            return false;

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            new KeyValuePair<string, string>(key, value)));
        return true;
    }

    public void Clear() =>
        _items.Clear();

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
