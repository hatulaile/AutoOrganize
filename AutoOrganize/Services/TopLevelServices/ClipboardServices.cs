using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace AutoOrganize.Services.TopLevelServices;

public sealed class ClipboardServices : TopLevelServicesBase<IClipboard>, IClipboardServices
{
    public async Task<string?> GetTextAsync()
        => await Default.TryGetTextAsync();

    public async Task SetTextAsync(string? text)
        => await Default.SetTextAsync(text);

    public async Task ClearAsync()
        => await Default.ClearAsync();

    protected override IClipboard GetProvider(TopLevel topLevel)
        => topLevel.Clipboard ?? throw new NotSupportedException();
}