using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;

namespace AutoOrganize.Services.TopLevelServices;

public interface IStorageServices
{
    Task<IReadOnlyList<IStorageFile>> GetSelectFilesAsync(FilePickerOpenOptions options, Visual? visual);
    Task<IReadOnlyList<IStorageFile>> GetSelectFilesAsync(FilePickerOpenOptions options, object? dataContext = null);

    Task<IStorageFile?> GetSaveFileAsync(FilePickerSaveOptions options, Visual? visual);
    Task<IStorageFile?> GetSaveFileAsync(FilePickerSaveOptions options, object? dataContext = null);

    Task<SaveFilePickerResult> GetSaveFilesWithResultAsync(FilePickerSaveOptions options, Visual? visual);
    Task<SaveFilePickerResult> GetSaveFilesWithResultAsync(FilePickerSaveOptions options, object? dataContext = null);

    Task<IReadOnlyList<IStorageFolder>> GetSelectFoldersAsync(FolderPickerOpenOptions options, Visual? visual);
    Task<IReadOnlyList<IStorageFolder>> GetSelectFoldersAsync(FolderPickerOpenOptions options, object? dataContext = null);
}