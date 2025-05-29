using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoOrganize.Library.Exceptions.ConfigExceptions;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Config;

public class FileConfigManager : IFileConfigManager
{
    public string ConfigDirectory { get; }

    private readonly ConcurrentDictionary<Type, ConfigInfo> _configs;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public TConfig? GetConfig<TConfig>() where TConfig : IConfig
    {
        return (TConfig?)_configs.GetValueOrDefault(typeof(TConfig))?.Config;
    }

    public TConfig GetRequiredConfig<TConfig>() where TConfig : IConfig
    {
        if (!_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            throw new ConfigNotFoundException(typeof(TConfig));
        }

        return (TConfig)info.Config;
    }

    public TConfig GetConfigOrLoad<TConfig>() where TConfig : IConfig, new()
    {
        if (_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
            return (TConfig)info.Config;

        return LoadConfigOrNew<TConfig>();
    }

    public bool TryGetConfig<TConfig>([NotNullWhen(true)] out TConfig? config) where TConfig : IConfig
    {
        config = default;
        if (_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            config = (TConfig)info.Config;
            return true;
        }

        return false;
    }

    public void SetConfig<TConfig>(TConfig config) where TConfig : IConfig
    {
        if (!_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            var newConfig = new ConfigInfo(typeof(TConfig), config, GetConfigPath<TConfig>());
            _configs[typeof(TConfig)] = newConfig;
            SaveConfig(newConfig);
            return;
        }

        if (ReferenceEquals(info.Config, config))
        {
            SaveConfig(info);
            return;
        }

        if (!info.Config.CanCopy(config))
        {
            throw new ConfigCopyNotSupportedException(typeof(TConfig));
        }

        info.Config.CopyFrom(config);
        SaveConfig(info);
    }

    public void RemoveConfig<TConfig>() where TConfig : IConfig
    {
        if (_configs.TryRemove(typeof(TConfig), out ConfigInfo? info))
        {
            File.Delete(info.Path);
        }

        string path = GetConfigPath<TConfig>();
        if (File.Exists(path)) File.Delete(path);
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "因为这里使用了Config, 所以会警告, 但是 Config 一定会设置 TypeInfoResolverChain, 所以忽略")]
    public void SaveConfig(IConfig config)
    {
        if (!_configs.TryGetValue(config.GetType(), out var info))
        {
            SetConfig(config);
            return;
        }

        File.WriteAllText(info.Path,
            JsonSerializer.Serialize(info.Config, info.ConfigType, _jsonSerializerOptions));
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "因为这里使用了Config, 所以会警告, 但是 Config 一定会设置 TypeInfoResolverChain, 所以忽略")]
    public void SaveConfig(ConfigInfo config)
    {
        File.WriteAllText(config.Path,
            JsonSerializer.Serialize(config.Config, config.ConfigType, _jsonSerializerOptions));
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "因为这里使用了Config, 所以会警告, 但是 Config 一定会设置 TypeInfoResolverChain, 所以忽略")]
    private TConfig LoadConfigOrNew<TConfig>() where TConfig : IConfig, new()
    {
        string path = GetConfigPath<TConfig>();

        TConfig config;
        if (File.Exists(path))
        {
            config = JsonSerializer.Deserialize<TConfig>(File.ReadAllText(path), _jsonSerializerOptions) ?? new TConfig();
        }
        else
        {
            config = new TConfig();
        }

        var info = new ConfigInfo(typeof(TConfig), config, path);
        SaveConfig(info);
        _configs.TryAdd(info.ConfigType, info);
        return config;
    }

    public void SaveAllConfigs()
    {
        foreach (var info in _configs.Values)
        {
            try
            {
                SaveConfig(info);
            }
            catch (Exception e)
            {
                //todo: logger
            }
        }
    }

    public Task<TConfig?> GetConfigAsync<TConfig>(CancellationToken token = default) where TConfig : IConfig
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<TConfig?>(token);

        return _configs.TryGetValue(typeof(TConfig), out ConfigInfo? info)
            ? Task.FromResult((TConfig?)info.Config)
            : Task.FromResult(default(TConfig));
    }

    public Task<TConfig> GetRequiredConfigAsync<TConfig>(CancellationToken token = default) where TConfig : IConfig
    {
        if (!_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            throw new ConfigNotFoundException(typeof(TConfig));
        }

        return Task.FromResult((TConfig)info.Config);
    }

    public async Task<TConfig> GetConfigOrLoadAsync<TConfig>(CancellationToken token = default)
        where TConfig : IConfig, new()
    {
        token.ThrowIfCancellationRequested();
        if (_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            return (TConfig)info.Config;
        }

        return await LoadConfigOrNewAsync<TConfig>(token).ConfigureAwait(false);
    }

    public Task TryGetConfigAsync<TConfig>([NotNullWhen(true)] out TConfig? config, CancellationToken token = default)
        where TConfig : IConfig
    {
        config = default;
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        if (_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            config = (TConfig)info.Config;
            return Task.CompletedTask;
        }

        return Task.FromResult(false);
    }

    public async Task SetConfigAsync<TConfig>(TConfig config, CancellationToken token = default) where TConfig : IConfig
    {
        token.ThrowIfCancellationRequested();

        if (!_configs.TryGetValue(typeof(TConfig), out ConfigInfo? info))
        {
            var newConfig = new ConfigInfo(typeof(TConfig), config, GetConfigPath<TConfig>());
            _configs[typeof(TConfig)] = newConfig;
            await SaveConfigAsync(newConfig, token).ConfigureAwait(false);
            return;
        }

        if (ReferenceEquals(info.Config, config))
        {
            await SaveConfigAsync(info, token).ConfigureAwait(false);
            return;
        }

        if (!info.Config.CanCopy(config))
        {
            throw new ConfigCopyNotSupportedException(typeof(TConfig));
        }

        info.Config.CopyFrom(config);
        await SaveConfigAsync(info, token).ConfigureAwait(false);
    }

    public Task RemoveConfigAsync<TConfig>(CancellationToken token = default) where TConfig : IConfig
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        if (_configs.TryRemove(typeof(TConfig), out ConfigInfo? info))
        {
            File.Delete(info.Path);
            return Task.CompletedTask;
        }

        string path = GetConfigPath<TConfig>();
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "因为这里使用了Config, 所以会警告, 但是 Config一定会设置 TypeInfoResolverChain, 所以忽略")]
    public async Task SaveConfigAsync(ConfigInfo info, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await File.WriteAllTextAsync(info.Path,
            JsonSerializer.Serialize(info.Config, info.ConfigType, _jsonSerializerOptions),
            token).ConfigureAwait(false);
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "因为这里使用了Config, 所以会警告, 但是 Config一定会设置 TypeInfoResolverChain, 所以忽略")]
    public async Task SaveConfigAsync(IConfig config, CancellationToken token = default)
    {
        if (!_configs.TryGetValue(config.GetType(), out var info))
        {
            await SetConfigAsync(config, token).ConfigureAwait(false);
            return;
        }

        await File.WriteAllTextAsync(info.Path,
            JsonSerializer.Serialize(info.Config, info.ConfigType, _jsonSerializerOptions),
            token).ConfigureAwait(false);
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "因为这里使用了Config, 所以会警告, 但是 Config一定会设置 TypeInfoResolverChain, 所以忽略")]
    private async Task<TConfig> LoadConfigOrNewAsync<TConfig>(CancellationToken token = default)
        where TConfig : IConfig, new()
    {
        token.ThrowIfCancellationRequested();
        string path = GetConfigPath<TConfig>();

        TConfig config;
        if (File.Exists(path))
        {
            config = JsonSerializer.Deserialize<TConfig>(await File.ReadAllTextAsync(path, token).ConfigureAwait(false), _jsonSerializerOptions) ?? new TConfig();
        }
        else
        {
            config = new TConfig();
        }

        var info = new ConfigInfo(typeof(TConfig), config, path);
        await SaveConfigAsync(info, token).ConfigureAwait(false);
        _configs.TryAdd(info.ConfigType, info);
        return config;
    }

    public async Task SaveAllConfigsAsync(CancellationToken token = default)
    {
        foreach (var info in _configs.Values)
        {
            try
            {
                await SaveConfigAsync(info, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                //todo: logger
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetConfigPath<TConfig>() where TConfig : IConfig =>
        Path.Combine(ConfigDirectory, GetConfigFileName<TConfig>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetConfigFileName<TConfig>() where TConfig : IConfig =>
        $"{typeof(TConfig).Name}.json";

    public FileConfigManager(string? configDirectory = null, params Span<JsonSerializerContext> contexts)
    {
        _configs = [];

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolverChain =
            {
                ConfigJsonSourceGenerationContext.Default,
            }
        };

        foreach (JsonSerializerContext context in contexts)
        {
            _jsonSerializerOptions.TypeInfoResolverChain.Add(context);
        }

        if (string.IsNullOrWhiteSpace(configDirectory) || !Path.IsPathFullyQualified(configDirectory))
            ConfigDirectory = Path.Join(PathUtils.GetDefaultAppdataPath(), "Config");
        else
            ConfigDirectory = configDirectory;

        if (!Directory.Exists(ConfigDirectory))
            Directory.CreateDirectory(ConfigDirectory);
    }

    public sealed record ConfigInfo(Type ConfigType, IConfig Config, string Path);
}