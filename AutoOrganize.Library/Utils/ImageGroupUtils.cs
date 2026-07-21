using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;

namespace AutoOrganize.Library.Utils;

public static class ImageGroupUtils
{
    public static ImageGroup? Coalesce(ImageGroup? first, ImageGroup? second)
    {
        if (first is null) return second;
        if (second is not null) first.AddRange(second);
        return first;
    }
}