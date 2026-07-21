using System.Globalization;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;

public interface ICountries
{
    List<RegionInfo>? Countries { get; }
}