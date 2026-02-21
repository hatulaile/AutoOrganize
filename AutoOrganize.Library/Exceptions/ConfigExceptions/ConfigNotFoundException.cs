namespace AutoOrganize.Library.Exceptions.ConfigExceptions;

public sealed class ConfigNotFoundException : ConfigException
{
    public ConfigNotFoundException(Type configType)
        : base(configType, $"Config '{configType.Name}' does not exist.")
    {
    }

    public ConfigNotFoundException(Type configType, string message)
        : base(configType, message)
    {
    }
}