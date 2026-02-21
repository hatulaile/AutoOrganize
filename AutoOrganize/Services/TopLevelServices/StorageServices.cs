using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AutoOrganize.Services.TopLevelServices;

public sealed class StorageServices : TopLevelServicesBase<IStorageProvider>, IStorageServices
{
    public async Task<IReadOnlyList<IStorageFile>> GetSelectFilesAsync(FilePickerOpenOptions options, Visual? visual)
    {
        IStorageProvider provider = visual is null ? Default : GetProvider(visual);
        return await provider.OpenFilePickerAsync(options);
    }

    public async Task<IReadOnlyList<IStorageFile>> GetSelectFilesAsync(FilePickerOpenOptions options,
        object? datadataContext = null)
    {
        IStorageProvider provider = GetProviderOrDefault(datadataContext);
        return await provider.OpenFilePickerAsync(options);
    }

    public async Task<IStorageFile?> GetSaveFileAsync(FilePickerSaveOptions options, Visual? visual)
    {
        IStorageProvider provider = visual is null ? Default : GetProvider(visual);
        return await provider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> GetSaveFileAsync(FilePickerSaveOptions options, object? dataContext = null)
    {
        IStorageProvider provider = GetProviderOrDefault(dataContext);
        return await provider.SaveFilePickerAsync(options);
    }

    public async Task<SaveFilePickerResult> GetSaveFilesWithResultAsync(FilePickerSaveOptions options, Visual? visual)
    {
        IStorageProvider provider = visual is null ? Default : GetProvider(visual);
        return await provider.SaveFilePickerWithResultAsync(options);
    }

    public async Task<SaveFilePickerResult> GetSaveFilesWithResultAsync(FilePickerSaveOptions options,
        object? dataContext = null)
    {
        IStorageProvider provider = GetProviderOrDefault(dataContext);
        return await provider.SaveFilePickerWithResultAsync(options);
    }

    public async Task<IReadOnlyList<IStorageFolder>> GetSelectFoldersAsync(FolderPickerOpenOptions options,
        Visual? visual)
    {
        IStorageProvider provider = visual is null ? Default : GetProvider(visual);
        return await provider.OpenFolderPickerAsync(options);
    }

    public async Task<IReadOnlyList<IStorageFolder>> GetSelectFoldersAsync(FolderPickerOpenOptions options,
        object? dataContext = null)
    {
        IStorageProvider provider = GetProviderOrDefault(dataContext);
        return await provider.OpenFolderPickerAsync(options);
    }


    protected override IStorageProvider GetProvider(TopLevel topLevel)
        => topLevel.StorageProvider;
}