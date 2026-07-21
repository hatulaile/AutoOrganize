using System.Globalization;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

public interface ILanguages
{
    List<CultureInfo>? Languages { get; }
}