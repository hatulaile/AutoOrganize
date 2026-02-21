using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AutoOrganize.Services.TopLevelServices;

public sealed class LauncherServices : TopLevelServicesBase<ILauncher>, ILauncherServices
{
    public async Task<bool> LaunchUriAsync(Uri uri, Visual? visual)
    {
        ILauncher launcher = visual is null ? Default : GetProvider(visual);
        return await launcher.LaunchUriAsync(uri);
    }

    public async Task<bool> LaunchUriAsync(Uri uri, object? dataContext = null)
    {
        ILauncher launcher = GetProviderOrDefault(dataContext);
        return await launcher.LaunchUriAsync(uri);
    }

    public async Task<bool> LaunchFileAsync(IStorageItem info, Visual? visual)
    {
        ILauncher launcher = visual is null ? Default : GetProvider(visual);
        return await launcher.LaunchFileAsync(info);
    }

    public async Task<bool> LaunchFileAsync(IStorageItem info, object? dataContext = null)
    {
        ILauncher launcher = GetProviderOrDefault(dataContext);
        return await launcher.LaunchFileAsync(info);
    }

    public async Task<bool> LaunchFileInfoAsync(FileInfo info, Visual? visual)
    {
        ILauncher launcher = visual is null ? Default : GetProvider(visual);
        return await launcher.LaunchFileInfoAsync(info);
    }

    public async Task<bool> LaunchFileInfoAsync(FileInfo info, object? visual)
    {
        ILauncher launcher = GetProviderOrDefault(visual);
        return await launcher.LaunchFileInfoAsync(info);
    }

    public async Task<bool> LaunchDirectoryInfoAsync(DirectoryInfo info, Visual? visual)
    {
        ILauncher launcher = visual is null ? Default : GetProvider(visual);
        return await launcher.LaunchDirectoryInfoAsync(info);
    }

    public async Task<bool> LaunchDirectoryInfoAsync(DirectoryInfo info, object? visual)
    {
        ILauncher launcher = GetProviderOrDefault(visual);
        return await launcher.LaunchDirectoryInfoAsync(info);
    }

    protected override ILauncher GetProvider(TopLevel topLevel)
        => topLevel.Launcher;
}