namespace AutoOrganize.Library.Services.Config;

public readonly struct ConfigPropertyChangedEventArgs<TType>
{
    public TType? OldValue { get; }

    public TType? NewValue { get; }

    public ConfigPropertyChangedEventArgs(TType? oldValue, TType? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public delegate void ConfigPropertyChangedEventHandler<TType>(ConfigPropertyChangedEventArgs<TType> ev);