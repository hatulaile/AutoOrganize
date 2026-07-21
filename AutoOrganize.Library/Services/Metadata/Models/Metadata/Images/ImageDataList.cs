using System.Collections;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;

public abstract class ImageDataList : IReadOnlyList<ImageData>
{
    private readonly List<ImageData> _items;

    public abstract string Id { get; }

    public ImageData this[int index] => _items[index];

    public int Count => _items.Count;

    public void Add(ImageData item) => _items.Add(item);

    public void AddRange(IEnumerable<ImageData> items) => _items.AddRange(items);

    public IEnumerator<ImageData> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected ImageDataList()
    {
        _items = [];
    }

    protected ImageDataList(int capacity)
    {
        _items = new List<ImageData>(capacity);
    }

    protected ImageDataList(IEnumerable<ImageData> images)
    {
        _items = [.. images];
    }
}