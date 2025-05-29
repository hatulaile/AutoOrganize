using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Services.NavigationServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels.SettingsViewModels;

public interface ISettingsViewModel : INavigationViewModel
{
    bool HasConfigChanged();

    void LoadConfig();

    Task LoadConfigAsync(CancellationToken token = default);

    void ApplyConfig();

    Task ApplyConfigAsync(CancellationToken token = default);

    void CancelConfigChange();

    Task CancelConfigChangeAsync(CancellationToken token = default);
}

public abstract partial class SettingsViewModelBase<TConfig> : ViewModelBase, ISettingsViewModel
    where TConfig : IConfig, new()
{
    public TConfig Config { get; private set; }

    [ObservableProperty]
    public partial TConfig NewConfig { get; set; }

    protected readonly IFileConfigManager FileConfigManager;

    protected SettingsViewModelBase(IFileConfigManager configManager)
    {
        FileConfigManager = configManager;
        LoadConfig();
    }

    public bool HasConfigChanged()
    {
        return !NewConfig.Equals(Config);
    }

    [MemberNotNull(nameof(Config), nameof(NewConfig))]
    public virtual void LoadConfig()
    {
        Config = FileConfigManager.GetConfigOrLoad<TConfig>();
        NewConfig = (TConfig)Config.Clone();
    }

    public virtual async Task LoadConfigAsync(CancellationToken token = default)
    {
        Config = await FileConfigManager.GetConfigOrLoadAsync<TConfig>(token);
        NewConfig = (TConfig)Config.Clone();
    }

    public virtual void ApplyConfig()
    {
        if (NewConfig.Equals(Config))
            return;
        NewConfig.CopyTo(Config);
    }

    public virtual Task ApplyConfigAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        ApplyConfig();
        return Task.CompletedTask;
    }

    public virtual void CancelConfigChange()
    {
        Config.CopyTo(NewConfig);
    }

    public virtual Task CancelConfigChangeAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        CancelConfigChange();
        return Task.CompletedTask;
    }
}

public abstract partial class SettingsViewModelBase<TConfig1, TConfig2> : ViewModelBase, ISettingsViewModel
    where TConfig1 : IConfig, new()
    where TConfig2 : IConfig, new()
{
    public TConfig1 Config1 { get; private set; }

    [ObservableProperty]
    public partial TConfig1 NewConfig1 { get; set; }

    public TConfig2 Config2 { get; private set; }

    [ObservableProperty]
    public partial TConfig2 NewConfig2 { get; set; }

    protected readonly IFileConfigManager FileConfigManager;

    protected SettingsViewModelBase(IFileConfigManager configManager)
    {
        FileConfigManager = configManager;
        LoadConfig();
    }

    public bool HasConfigChanged()
    {
        return !NewConfig1.Equals(Config1) || !NewConfig2.Equals(Config2);
    }

    [MemberNotNull(nameof(Config1), nameof(Config2), nameof(NewConfig1), nameof(NewConfig2))]
    public virtual void LoadConfig()
    {
        Config1 = FileConfigManager.GetConfigOrLoad<TConfig1>();
        Config2 = FileConfigManager.GetConfigOrLoad<TConfig2>();
        NewConfig1 = (TConfig1)Config1.Clone();
        NewConfig2 = (TConfig2)Config2.Clone();
    }

    public virtual async Task LoadConfigAsync(CancellationToken token = default)
    {
        Config1 = await FileConfigManager.GetConfigOrLoadAsync<TConfig1>(token);
        Config2 = await FileConfigManager.GetConfigOrLoadAsync<TConfig2>(token);
        NewConfig1 = (TConfig1)Config1.Clone();
        NewConfig2 = (TConfig2)Config2.Clone();
    }

    public virtual void ApplyConfig()
    {
        if (!NewConfig1.Equals(Config1))
            NewConfig1.CopyTo(Config1);

        if (!NewConfig2.Equals(Config2))
            NewConfig2.CopyTo(Config2);
    }

    public virtual Task ApplyConfigAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        ApplyConfig();
        return Task.CompletedTask;
    }

    public virtual void CancelConfigChange()
    {
        Config1.CopyTo(NewConfig1);
        Config2.CopyTo(NewConfig2);
    }

    public virtual Task CancelConfigChangeAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        CancelConfigChange();
        return Task.CompletedTask;
    }
}

public abstract partial class SettingsViewModelBase<TConfig1, TConfig2, TConfig3> : ViewModelBase, ISettingsViewModel
    where TConfig1 : IConfig, new()
    where TConfig2 : IConfig, new()
    where TConfig3 : IConfig, new()
{
    public TConfig1 Config1 { get; private set; }

    [ObservableProperty]
    public partial TConfig1 NewConfig1 { get; set; }

    public TConfig2 Config2 { get; private set; }

    [ObservableProperty]
    public partial TConfig2 NewConfig2 { get; set; }

    public TConfig3 Config3 { get; private set; }

    [ObservableProperty]
    public partial TConfig3 NewConfig3 { get; set; }

    protected readonly IFileConfigManager FileConfigManager;

    protected SettingsViewModelBase(IFileConfigManager configManager)
    {
        FileConfigManager = configManager;
        LoadConfig();
    }

    public bool HasConfigChanged()
    {
        return !NewConfig1.Equals(Config1) || !NewConfig2.Equals(Config2) || !NewConfig3.Equals(Config3);
    }

    [MemberNotNull(
        nameof(Config1), nameof(NewConfig1),
        nameof(Config2), nameof(NewConfig2),
        nameof(Config3), nameof(NewConfig3))]
    public virtual void LoadConfig()
    {
        Config1 = FileConfigManager.GetConfigOrLoad<TConfig1>();
        Config2 = FileConfigManager.GetConfigOrLoad<TConfig2>();
        Config3 = FileConfigManager.GetConfigOrLoad<TConfig3>();
        NewConfig1 = (TConfig1)Config1.Clone();
        NewConfig2 = (TConfig2)Config2.Clone();
        NewConfig3 = (TConfig3)Config3.Clone();
    }

    public virtual async Task LoadConfigAsync(CancellationToken token = default)
    {
        Config1 = await FileConfigManager.GetConfigOrLoadAsync<TConfig1>(token);
        Config2 = await FileConfigManager.GetConfigOrLoadAsync<TConfig2>(token);
        Config3 = await FileConfigManager.GetConfigOrLoadAsync<TConfig3>(token);
        NewConfig1 = (TConfig1)Config1.Clone();
        NewConfig2 = (TConfig2)Config2.Clone();
        NewConfig3 = (TConfig3)Config3.Clone();
    }

    public virtual void ApplyConfig()
    {
        if (!NewConfig1.Equals(Config1))
            NewConfig1.CopyTo(Config1);
        if (!NewConfig2.Equals(Config2))
            NewConfig2.CopyTo(Config2);
        if (!NewConfig3.Equals(Config3))
            NewConfig3.CopyTo(Config3);
    }

    public virtual Task ApplyConfigAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        ApplyConfig();
        return Task.CompletedTask;
    }

    public virtual void CancelConfigChange()
    {
        Config1.CopyTo(NewConfig1);
        Config2.CopyTo(NewConfig2);
        Config3.CopyTo(NewConfig3);
    }

    public virtual Task CancelConfigChangeAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled(token);

        CancelConfigChange();
        return Task.CompletedTask;
    }
}