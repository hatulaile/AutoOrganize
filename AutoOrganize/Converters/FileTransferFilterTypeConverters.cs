using System;
using AutoOrganize.Models;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoOrganize.Converters;

public static class FileTransferFilterTypeConverters
{
    public static FuncValueConverter<FileTransferFilterType, string?> FileTransferFilterTypeToStringConverter =>
        field ??= new FuncValueConverter<FileTransferFilterType, string?>(type => type switch
        {
            FileTransferFilterType.None => "保留全部",
            FileTransferFilterType.SuccessOnly => "仅成功",
            FileTransferFilterType.FailedOnly => "仅失败",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        });

    public static FuncValueConverter<FileTransferFilterType, IBrush> FileTransferFilterTypeToBrushConverter =>
        field ??= new FuncValueConverter<FileTransferFilterType, IBrush>(type => type switch
        {
            FileTransferFilterType.None => Brushes.SlateGray,
            FileTransferFilterType.SuccessOnly => Brushes.LimeGreen,
            FileTransferFilterType.FailedOnly => Brushes.IndianRed,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        });
}