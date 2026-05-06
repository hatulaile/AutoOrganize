using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

[AutoConfig]
public sealed partial class ThemoviedbMetadataProviderConfig :
    ConfigBase<ThemoviedbMetadataProviderConfig>, IMetadataProviderConfig
{
    [ObservableProperty]
    public partial int Priority { get; set; }
}