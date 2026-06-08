using System;
using AutoOrganize.Library.Models.FileTransfers;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.ViewModels.SettingsViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoOrganize.Views.SettingsViews;

public partial class FileTransferSettingsView : UserControl
{
    public FileTransferSettingsView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ((FileTransferSettingsViewModel?)DataContext)?.NewConfig.ModeChanged += NewConfigOnModeChanged;
        ((FileTransferSettingsViewModel?)DataContext)?.NewConfig.OutputDirectoryChanged +=
            NewConfigOnOutputDirectoryChanged;
        SelectRadio(((FileTransferSettingsViewModel?)DataContext)?.NewConfig.Mode ?? FileTransferMode.Copy);
    }

    private void NewConfigOnOutputDirectoryChanged(ConfigPropertyChangedEventArgs<string> ev)
    {
        if (((FileTransferSettingsViewModel?)DataContext)?.IsOutputPathValid is not true)
        {
            DataValidationErrors.SetError(OutputDirectoryTextBox, new InvalidOperationException("无效的输出路径"));
            return;
        }

        DataValidationErrors.ClearErrors(OutputDirectoryTextBox);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        ((FileTransferSettingsViewModel?)DataContext)?.NewConfig.ModeChanged -= NewConfigOnModeChanged;
    }

    private void NewConfigOnModeChanged(ConfigPropertyChangedEventArgs<FileTransferMode> ev)
    {
        SelectRadio(ev.NewValue);
    }

    public void SelectRadio(FileTransferMode mode)
    {
        switch (mode)
        {
            case FileTransferMode.None:
                CopyRadio.IsChecked = false;
                ClippingRadio.IsChecked = false;
                SymbolicRadio.IsChecked = false;
                HardRadio.IsChecked = false;
                break;
            case FileTransferMode.HardLink:
                HardRadio.IsChecked = true;
                break;
            case FileTransferMode.SymbolicLink:
                SymbolicRadio.IsChecked = true;
                break;
            case FileTransferMode.Copy:
                CopyRadio.IsChecked = true;
                break;
            case FileTransferMode.Clipping:
                ClippingRadio.IsChecked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var radioButton = (RadioButton?)sender;
        if (radioButton is not { IsChecked: true, Tag: not null } ||
            DataContext is not FileTransferSettingsViewModel vm)
            return;

        vm.NewConfig.Mode = (FileTransferMode)radioButton.Tag;
    }
}