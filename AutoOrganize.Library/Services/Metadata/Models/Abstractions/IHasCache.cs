namespace AutoOrganize.Library.Services.Metadata.Models.Abstractions;

public interface IHasCache
{
    IEnumerable<string> GetCacheNames();
}