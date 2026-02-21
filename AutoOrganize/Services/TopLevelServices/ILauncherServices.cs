using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;

namespace AutoOrganize.Services.TopLevelServices;

public interface ILauncherServices
{
    Task<bool> LaunchUriAsync(Uri uri, Visual? visual);

    Task<bool> LaunchUriAsync(Uri uri, object? dataContext = null);

    Task<bool> LaunchFileAsync(IStorageItem info, Visual? visual);

    Task<bool> LaunchFileAsync(IStorageItem info, object? dataContext = null);

    Task<bool> LaunchFileInfoAsync(FileInfo info, Visual? visual);

    Task<bool> LaunchFileInfoAsync(FileInfo info, object? visual);

    Task<bool> LaunchDirectoryInfoAsync(DirectoryInfo info, Visual? visual);

    Task<bool> LaunchDirectoryInfoAsync(DirectoryInfo info, object? visual);
}