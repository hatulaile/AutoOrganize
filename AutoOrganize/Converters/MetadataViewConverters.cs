using AutoOrganize.Models.MetadataViewModels;
using AutoOrganize.Models.MetadataViewModels.FileSystem;
using AutoOrganize.Utils;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoOrganize.Converters;

public static class MetadataViewConverters
{
    public static FuncValueConverter<FileMetadataBase, string?> TitleConverter => field ??=
        new FuncValueConverter<FileMetadataBase, string?>(metadata => metadata?.Title);

    public static FuncValueConverter<FileMetadataBase, string?> SubtitleConverter => field ??=
        new FuncValueConverter<FileMetadataBase, string?>(FileMetadataUtils.IfHasSubtitleGetSubtitle);

    public static FuncValueConverter<FileMetadataBase, bool> StateVisibleConverter => field ??=
        new FuncValueConverter<FileMetadataBase, bool>(item => item is TransferFileModel or FailedTransferFileModel);

    public static FuncValueConverter<FileMetadataBase, string> StateTextConverter => field ??=
        new FuncValueConverter<FileMetadataBase, string>(item => item is TransferFileModel ? "成功" : "失败");

    public static FuncValueConverter<object?, IBrush> StateBrushConverter => field ??=
        new FuncValueConverter<object?, IBrush>(item =>
            item is TransferFileModel ? Brushes.LimeGreen : Brushes.IndianRed);

    public static FuncValueConverter<FileMetadataBase, string?> MetadataBaseTypeDisplayConverter => field ??=
        new FuncValueConverter<FileMetadataBase, string?>(metadata =>
            metadata is null ? null : FileMetadataUtils.GetTypeDisplayName(metadata.Type));

    public static FuncValueConverter<FileMetadataType, string?> TypeDisplayConverter => field ??=
        new FuncValueConverter<FileMetadataType, string?>(FileMetadataUtils.GetTypeDisplayName);
}