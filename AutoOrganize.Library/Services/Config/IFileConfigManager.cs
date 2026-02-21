using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.Config;

public interface IFileConfigManager
{
    TConfig? GetConfig<TConfig>() where TConfig : IConfig;
    TConfig GetRequiredConfig<TConfig>() where TConfig : IConfig;

    TConfig GetConfigOrLoad<TConfig>() where TConfig : IConfig, new();

    bool TryGetConfig<TConfig>([NotNullWhen(true)] out TConfig? config) where TConfig : IConfig;

    void SetConfig<TConfig>(TConfig config) where TConfig : IConfig;

    void RemoveConfig<TConfig>() where TConfig : IConfig;

    void SaveConfig(FileConfigManager.ConfigInfo info);

    TConfig LoadConfigOrNew<TConfig>() where TConfig : IConfig, new();

    void SaveAllConfigs();

    Task<TConfig?> GetConfigAsync<TConfig>(CancellationToken token = default) where TConfig : IConfig;
    Task<TConfig> GetRequiredConfigAsync<TConfig>(CancellationToken token = default) where TConfig : IConfig;

    Task<TConfig> GetConfigOrLoadAsync<TConfig>(CancellationToken token = default)
        where TConfig : IConfig, new();

    Task TryGetConfigAsync<TConfig>([NotNullWhen(true)] out TConfig? config, CancellationToken token = default)
        where TConfig : IConfig;

    Task SetConfigAsync<TConfig>(TConfig config, CancellationToken token = default) where TConfig : IConfig;
    Task RemoveConfigAsync<TConfig>(CancellationToken token = default) where TConfig : IConfig;

    Task SaveConfigAsync(FileConfigManager.ConfigInfo info, CancellationToken token = default);

    Task<TConfig> LoadConfigOrNewAsync<TConfig>(CancellationToken token = default)
        where TConfig : IConfig, new();

    Task SaveAllConfigsAsync(CancellationToken token = default);
}