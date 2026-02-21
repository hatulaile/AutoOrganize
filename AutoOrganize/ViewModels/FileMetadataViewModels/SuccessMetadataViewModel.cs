using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoOrganize.Converters;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Models.Metadata.Interfaces;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Providers;
using AutoOrganize.Services.TopLevelServices;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.FileMetadataViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class SuccessMetadataViewModel : MetadataViewModelBase<MetadataBase>
{
    private readonly ILauncherServices _launcherServices;
    private readonly IClipboardServices _clipboardServices;
    private readonly INotificationServices _notificationServices;

    public string? Title
    {
        get
        {
            if (Metadata is null)
                return null;

            return Metadata switch
            {
                EpisodeMetadata episode => episode.Series?.Name ?? Metadata.Name,
                SeasonMetadata season => season.Series?.Name ?? Metadata.Name,
                _ => Metadata.Name
            };
        }
    }

    public string? Subheading
    {
        get
        {
            if (Metadata is null)
                return null;

            return Metadata switch
            {
                IOriginalName originalName => originalName.OriginalName,
                EpisodeMetadata episode => $"S{episode.SeasonNumber:00}E{episode.EpisodeNumber:00} - {episode.Name}",
                SeasonMetadata season => $"S{season.SeasonNumber:00} - {season.Name}",
                _ => null
            };
        }
    }

    public string? Language => IfCastOrNull<ILanguages, string?>(Metadata,
        language => language.Languages is not null
            ? string.Join(", ", language.Languages.Select(x => x.DisplayName))
            : null);

    public string? Countries => IfCastOrNull<ICountries, string?>(Metadata,
        country => country.Countries is not null
            ? string.Join(", ", country.Countries.Select(x => x.DisplayName))
            : null);

    public string? Runtime => IfCastOrNull<IRuntime, string?>(Metadata, runtime => runtime.Runtime.ToString());

    public string? Revenue => IfCastOrNull<IRevenue, string?>(Metadata, revenue => revenue.Revenue.ToString());

    public string? Logo =>
        IfCastOrNull<ILogos, string?>(Metadata, logos => MetadataConverters.GetImageGroupUrl(logos.Logos));

    public string? Poster =>
        IfCastOrNull<IPosters, string?>(Metadata, posters => MetadataConverters.GetImageGroupUrl(posters.Posters));

    public string? Backdrop =>
        IfCastOrNull<IBackdrops, string?>(Metadata,
            backdrops => MetadataConverters.GetImageGroupUrl(backdrops.Backdrops));

    [RelayCommand(CanExecute = nameof(CanOpenMetadataInBrowser))]
    private async Task OpenMetadataInBrowser(KeyValuePair<string, string> parm)
    {
        (string providerId, string id) = parm;

        var info = (IUrlMetadataProviderInfo?)App.Current.ServiceProvider.GetKeyedService<IMetadataProvider>(providerId)
            ?.Info;
        if (info is null || Metadata is null)
            return;

        if (!info.TryGetUrl(id, Metadata.Type, out var uriStr) ||
            !Uri.TryCreate(uriStr, UriKind.Absolute, out var uri))
            return;

        await _launcherServices.LaunchUriAsync(uri, this);
    }

    [RelayCommand(CanExecute = nameof(CanOpenProviderHomeInBrowser))]
    private async Task OpenProviderHomeInBrowser(string providerId)
    {
        var info = (IUrlMetadataProviderInfo?)App.Current.ServiceProvider.GetKeyedService<IMetadataProvider>(providerId)
            ?.Info;
        if (info is null || Metadata is null)
            return;

        if (!Uri.TryCreate(info.HomeUrl, UriKind.Absolute, out var uri))
            return;

        await _launcherServices.LaunchUriAsync(uri, this);
    }

    private bool CanOpenMetadataInBrowser(KeyValuePair<string, string> parm)
    {
        if (Metadata is null)
            return false;

        (string providerId, string id) = parm;

        //其实这里最好能直接把所有 provider 写在 vm 内的, 但是 key services 不能使用一个 Dictionary 依赖注入, 所以暂时这样写
        if (App.Current.ServiceProvider.GetKeyedService<IMetadataProvider>(providerId)?.Info is not
            IUrlMetadataProviderInfo info)
        {
            return false;
        }

        return info.TryGetUrl(id, Metadata.Type, out _);
    }

    private bool CanOpenProviderHomeInBrowser(string providerId)
    {
        if (Metadata is null)
            return false;

        if (App.Current.ServiceProvider.GetKeyedService<IMetadataProvider>(providerId)?.Info is not
            IUrlMetadataProviderInfo)
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task CopyMetadataId(KeyValuePair<string, string> parm)
    {
        (string providerId, string id) = parm;
        await _clipboardServices.SetTextAsync(id);
        _notificationServices.Show(new Notification("复制", $"复制 {providerId} 的 id:{id}"), this);
    }

    public SuccessMetadataViewModel(ILauncherServices launcherServices, IClipboardServices clipboardServices,
        INotificationServices notificationServices)
    {
        _launcherServices = launcherServices;
        _clipboardServices = clipboardServices;
        _notificationServices = notificationServices;
    }

    protected override void MetadataChanging(MetadataBase? value)
    {
        base.MetadataChanging(value);
        OnPropertyChanging(nameof(Title));
        OnPropertyChanging(nameof(Subheading));
        OnPropertyChanging(nameof(Language));
        OnPropertyChanging(nameof(Countries));
        OnPropertyChanging(nameof(Runtime));
        OnPropertyChanging(nameof(Revenue));
        OnPropertyChanging(nameof(Logo));
        OnPropertyChanging(nameof(Poster));
        OnPropertyChanging(nameof(Backdrop));
    }

    protected override void MetadataChanged(MetadataBase? value)
    {
        base.MetadataChanged(value);
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subheading));
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(Countries));
        OnPropertyChanged(nameof(Runtime));
        OnPropertyChanged(nameof(Revenue));
        OnPropertyChanged(nameof(Logo));
        OnPropertyChanged(nameof(Poster));
        OnPropertyChanged(nameof(Backdrop));
    }

    private static T? IfCastOrNull<T>(object? obj) => obj is T t ? t : default;

    private static TResult? IfCastOrNull<T, TResult>(object? obj, Func<T, TResult> selector) =>
        obj is T t ? selector(t) : default;
}