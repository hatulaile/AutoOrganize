using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Models.FileMetadataModels.SuccessMetadata;

public sealed class FileSeriesMetadata : FileMetadataBase, ISubheading, IFileMetadata<SeriesMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override FileMetadataType Type => FileMetadataType.TvSeries;

    public SeriesMetadata Metadata { get; }

    public string Subheading => Metadata.OriginalName ?? string.Empty;

    public override bool HasChildren => true;

    public FileSeriesMetadata(SeriesMetadata seasonMetadata)
    {
        Metadata = seasonMetadata;
    }
}