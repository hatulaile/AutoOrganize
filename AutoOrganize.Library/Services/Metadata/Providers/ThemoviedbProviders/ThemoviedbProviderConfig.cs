using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

[AutoConfig]
public sealed partial class ThemoviedbProviderConfig : ConfigBase<ThemoviedbProviderConfig>, IProviderConfig
{
    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial int Priority { get; set; }

    [ObservableProperty]
    public partial string ApiKey { get; set; } = "a68ae3528e2875c12cca9e924c5483b5";
}