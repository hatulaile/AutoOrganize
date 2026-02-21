using System.Threading.Tasks;

namespace AutoOrganize.Services.TopLevelServices;

public interface IClipboardServices
{
    Task<string?> GetTextAsync();

    Task SetTextAsync(string? text);

    Task ClearAsync();
}