using System.Globalization;

namespace AutoOrganize.Library.Models.Metadata.Interfaces;

public interface ILanguages
{
    List<CultureInfo>? Languages { get; }
}