using System;
using System.Linq;
using AutoOrganize.Library.Models.Metadata;
using Avalonia.Collections;
using Avalonia.Controls;

namespace AutoOrganize.Views;

public partial class SelectFilesView : UserControl
{
    public AvaloniaList<MetadataType> MetadataTypes { get; } = [];

    public SelectFilesView()
    {
        MetadataTypes.AddRange(Enum.GetValues<MetadataType>().Skip(1));
        InitializeComponent();
    }
}