using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

public interface IPosters
{
    ImageGroup? Posters { get; }
}