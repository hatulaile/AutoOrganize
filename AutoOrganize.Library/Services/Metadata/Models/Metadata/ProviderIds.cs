using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata;

public sealed class ProviderIds : IProviderIds
{
    private readonly Dictionary<string, string> _items;

    public int Count => _items.Count;

    public string this[string key]
    {
        get => _items[key];
        set => _items[key] = value;
    }

    public ProviderIds()
    {
        _items = new Dictionary<string, string>();
    }

    public ProviderIds(int capacity)
    {
        _items = new Dictionary<string, string>(capacity);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        => _items.TryGetValue(key, out value);

    public void Add(string key, string value) =>
        _items.Add(key, value);

    public bool TryAdd(string key, string value)
        => _items.TryAdd(key, value);

    public bool ContainsKey(string key) => _items.ContainsKey(key);

    public bool Remove(string key) => _items.Remove(key);

    public void Clear() =>
        _items.Clear();

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
