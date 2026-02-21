using AutoOrganize.Library.Models.Metadata.Images;

namespace AutoOrganize.Library.Models.Metadata.Interfaces;

public interface IPosters
{
    ImageGroup? Posters { get; set; }
}