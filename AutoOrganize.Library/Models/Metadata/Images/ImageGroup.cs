using System.Collections;

namespace AutoOrganize.Library.Models.Metadata.Images;

public sealed class ImageGroup : IEnumerable<ImageDataListBase>
{
    public Dictionary<string, ImageDataListBase> ImageDataListForId { get; } = new();

    public IReadOnlyCollection<ImageDataListBase> ImageDataList => ImageDataListForId.Values;

    public void Add(ImageDataListBase imageDataList)
    {
        if (ImageDataListForId.TryGetValue(imageDataList.Id, out var list))
        {
            list.ImageData.AddRange(imageDataList.ImageData);
            return;
        }

        ImageDataListForId[imageDataList.Id] = imageDataList;
    }

    public void AddRange(params IEnumerable<ImageDataListBase> imageDataLists)
    {
        foreach (ImageDataListBase imageDataList in imageDataLists)
            Add(imageDataList);
    }

    public void Set(ImageDataListBase imageDataList)
    {
        ImageDataListForId[imageDataList.Id] = imageDataList;
    }

    public ImageGroup()
    {
    }

    public ImageGroup(params IEnumerable<ImageDataListBase> lists)
    {
        foreach (ImageDataListBase list in lists)
        {
            Add(list);
        }
    }

    IEnumerator<ImageDataListBase> IEnumerable<ImageDataListBase>.GetEnumerator()
    {
        return ImageDataList.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return ImageDataList.GetEnumerator();
    }
}