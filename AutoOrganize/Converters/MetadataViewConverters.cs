using AutoOrganize.Models.MetadataNodes.Abstractions;
using AutoOrganize.Models.MetadataNodes.FileSystem;
using AutoOrganize.Utils;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoOrganize.Converters;

public static class MetadataViewConverters
{
    public static FuncValueConverter<MetadataTreeNodeBase, string?> TitleConverter => field ??=
        new FuncValueConverter<MetadataTreeNodeBase, string?>(metadata => metadata?.Title);

    public static FuncValueConverter<MetadataTreeNodeBase, string?> SubtitleConverter => field ??=
        new FuncValueConverter<MetadataTreeNodeBase, string?>(FileMetadataUtils.IfHasSubtitleGetSubtitle);

    public static FuncValueConverter<MetadataTreeNodeBase, bool> StateVisibleConverter => field ??=
        new FuncValueConverter<MetadataTreeNodeBase, bool>(item => item is TransferredFileNode or FailedTransferFileNode);

    public static FuncValueConverter<MetadataTreeNodeBase, string> StateTextConverter => field ??=
        new FuncValueConverter<MetadataTreeNodeBase, string>(item => item is TransferredFileNode ? "成功" : "失败");

    public static FuncValueConverter<object?, IBrush> StateBrushConverter => field ??=
        new FuncValueConverter<object?, IBrush>(item =>
            item is TransferredFileNode ? Brushes.LimeGreen : Brushes.IndianRed);

    public static FuncValueConverter<MetadataTreeNodeBase, string?> MetadataBaseTypeDisplayConverter => field ??=
        new FuncValueConverter<MetadataTreeNodeBase, string?>(metadata =>
            metadata is null ? null : FileMetadataUtils.GetTypeDisplayName(metadata.NodeType));

    public static FuncValueConverter<MetadataNodeType, string?> TypeDisplayConverter => field ??=
        new FuncValueConverter<MetadataNodeType, string?>(FileMetadataUtils.GetTypeDisplayName);
}