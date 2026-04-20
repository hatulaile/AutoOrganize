using System;
using AutoOrganize.Models;
using Avalonia.Controls;

namespace AutoOrganize.Views;

public partial class FileTransferResultView : UserControl
{
    public FileTransferFilterType[] FileTransferFilterTypes { get; }

    public FileTransferResultView()
    {
        FileTransferFilterTypes = Enum.GetValues<FileTransferFilterType>();
        InitializeComponent();
    }
}