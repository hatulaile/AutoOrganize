using AutoOrganize.Library.Models.Metadata.Movie;

namespace AutoOrganize.Models.MetadataViewModels.Metadata;

public sealed class FileMovieMetadata : FileMetadataBase, ISubheading, IFileMetadata<MovieMetadata>
{
    public override string Title => Metadata.Name ?? string.Empty;

    public override FileMetadataType Type => FileMetadataType.Movie;

    public MovieMetadata Metadata { get; }

    public override bool HasChildren => true;

    public string Subheading => Metadata.OriginalName ?? string.Empty;

    public FileMovieMetadata(MovieMetadata metadata)
    {
        Metadata = metadata;
    }
}