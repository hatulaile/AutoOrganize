using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

[AutoConfig]
public sealed partial class ThemoviedbMetadataProviderConfig :
    ConfigBase<ThemoviedbMetadataProviderConfig>, IMetadataProviderConfig
{
    [ObservableProperty]
    public partial string ApiKey { get; set; } = TmdbConstants.TMDB_API_KEY;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial int Priority { get; set; }
}