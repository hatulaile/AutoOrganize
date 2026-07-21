using System.Collections;
using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata;

public sealed class ProviderIds : IEnumerable<KeyValuePair<string, string>>
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
        _items = new Dictionary<string, string>(0);
    }

    public ProviderIds(int count)
    {
        _items = new Dictionary<string, string>(count);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        => _items.TryGetValue(key, out value);

    public void Add(string key, string value) =>
        _items.Add(key, value);

    public bool TryAdd(string key, string value)
        => _items.TryAdd(key, value);

    public bool ContainsKey(string key) => _items.ContainsKey(key);

    public bool Remove(string key) => _items.Remove(key);


    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}