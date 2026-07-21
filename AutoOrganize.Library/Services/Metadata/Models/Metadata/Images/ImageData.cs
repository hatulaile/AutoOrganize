using System.Globalization;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;

public sealed class ImageData
{
    public required Uri ImageUrl { get; set; }

    public double AspectRatio { get; set; }

    public int Height { get; set; }

    public int Width { get; set; }

    public CultureInfo? Locale { get; set; }

    public double Priority { get; set; }
}