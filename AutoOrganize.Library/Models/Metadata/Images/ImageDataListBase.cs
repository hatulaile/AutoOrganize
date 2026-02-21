namespace AutoOrganize.Library.Models.Metadata.Images;

public abstract class ImageDataListBase
{
    public abstract string Id { get; }

    public List<ImageData> ImageData => field ??= new List<ImageData>(16);
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
        ImageData.AddRange(imageData);
    }
}