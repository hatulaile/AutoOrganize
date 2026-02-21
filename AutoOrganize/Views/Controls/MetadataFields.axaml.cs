using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AutoOrganize.Views.Controls;

public class MetadataFields : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<MetadataFields, string>(
            nameof(Title));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<MetadataFields, string?>(
            nameof(Value));

    public static readonly StyledProperty<string?> SuffixProperty =
        AvaloniaProperty.Register<MetadataFields, string?>(
            nameof(Suffix));

    public static readonly StyledProperty<string?> PrefixProperty =
        AvaloniaProperty.Register<MetadataFields, string?>(
            nameof(Prefix));

    public static readonly AttachedProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, Control, TextWrapping>(nameof(TextWrapping),
            inherits: true);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [Content]
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public string? Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
}