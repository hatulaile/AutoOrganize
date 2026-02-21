using System.Globalization;

namespace AutoOrganize.Library.Models.Metadata.Interfaces;

public interface ICountries
{
    List<RegionInfo>? Countries { get; }
}