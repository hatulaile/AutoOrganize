using System;
using System.Linq;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using Avalonia.Collections;
using Avalonia.Controls;

namespace AutoOrganize.Views.HomeViews;

public partial class SelectFilesView : UserControl
{
    public AvaloniaList<MetadataType> MetadataTypes { get; } =
        [
            MetadataType.TvSeries,
            MetadataType.Movie
        ];

    public SelectFilesView()
    {
        InitializeComponent();
    }
}