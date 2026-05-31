namespace AutoOrganize.Library.Models.Metadata.Images;

public sealed class ImageGroup : Dictionary<string, ImageDataListBase>
{
    public ImageGroup() : base(0)
    {
    }

    public ImageGroup(params IEnumerable<ImageDataListBase> lists)
    {
        foreach (ImageDataListBase list in lists)
        {
            Add(list);
        }
    }

    public void Add(ImageDataListBase imageDataList)
    {
        if (TryGetValue(imageDataList.Id, out var list))
        {
            list.AddRange(imageDataList);
            return;
        }

        this[imageDataList.Id] = imageDataList;
    }

    public void AddRange(ImageGroup group)
    {
        foreach (ImageDataListBase imageDataList in group.Values)
            Add(imageDataList);
    }

    public void AddRange(params IEnumerable<ImageDataListBase> imageDataLists)
    {
        foreach (ImageDataListBase imageDataList in imageDataLists)
            Add(imageDataList);
    }
}