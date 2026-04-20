using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

public sealed class FileSeasonMetadata : FileMetadataBase, ISubheading, IFileMetadata<SeasonMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override FileMetadataType Type => FileMetadataType.TvSeason;

    public SeasonMetadata Metadata { get; }

    public string Subheading => $"Season {Metadata.SeasonNumber}";

    public override bool HasChildren => true;

    public FileSeasonMetadata(SeasonMetadata seasonMetadata)
    {
        Metadata = seasonMetadata;
    }
}