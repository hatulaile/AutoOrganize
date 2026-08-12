using System;
using Avalonia.Controls;

namespace AutoOrganize.Views.HomeViews.EditorViews;

public partial class MetadataEditorWindow : Window
{
    public MetadataEditorWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        SizeToContent = SizeToContent.Manual;
    }
}
