namespace AutoOrganize.Library.Exceptions.ConfigExceptions;

public sealed class ConfigCopyNotSupportedException : ConfigException
{
    public ConfigCopyNotSupportedException(Type configType)
        : base(configType, $"Config '{configType.Name}' does not support copying.")
    {
    }

    public ConfigCopyNotSupportedException(Type configType, string message)
        : base(configType, message)
    {
    }
}