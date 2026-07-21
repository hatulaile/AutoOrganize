using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;

public sealed class ImageGroup : IReadOnlyDictionary<string, ImageDataList>
{
    private readonly Dictionary<string, ImageDataList> _items = new();

    public ImageGroup()
    {
    }

    public ImageGroup(params IEnumerable<ImageDataList> lists)
    {
        foreach (ImageDataList list in lists)
            Add(list);
    }

    public void Add(ImageDataList list)
    {
        if (_items.TryGetValue(list.Id, out var existingList))
        {
            existingList.AddRange(list);
            return;
        }

        _items[list.Id] = list;
    }

    public void AddRange(ImageGroup group)
    {
        foreach (ImageDataList list in group.Values)
            Add(list);
    }

    public void AddRange(params IEnumerable<ImageDataList> lists)
    {
        foreach (ImageDataList list in lists)
            Add(list);
    }

    public ImageDataList this[string key] => _items[key];
    public IEnumerable<string> Keys => _items.Keys;
    public IEnumerable<ImageDataList> Values => _items.Values;
    public int Count => _items.Count;
    public bool ContainsKey(string key) => _items.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out ImageDataList? value) => _items.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, ImageDataList>> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
