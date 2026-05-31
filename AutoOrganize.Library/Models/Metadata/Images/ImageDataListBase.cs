using System.Collections;

namespace AutoOrganize.Library.Models.Metadata.Images;

public abstract class ImageDataListBase : List<ImageData>
{
    public abstract string Id { get; }

    protected ImageDataListBase() : base(0)
    {

    }

    protected ImageDataListBase(int capacity) : base(capacity)
    {

    }
}

public sealed class MetadataProviderImageDataList : ImageDataListBase
{
    public override string Id => ProviderType.ToString();

    public MetadataProviderType ProviderType { get; }

    public MetadataProviderImageDataList(MetadataProviderType type)
    {
        ProviderType = type;
    }

    public MetadataProviderImageDataList(MetadataProviderType type, IEnumerable<ImageData> imageData)
    {
        ProviderType = type;
        AddRange(imageData);
    }
}