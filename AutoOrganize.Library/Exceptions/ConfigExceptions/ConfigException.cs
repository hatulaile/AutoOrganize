namespace AutoOrganize.Library.Exceptions.ConfigExceptions;

public abstract class ConfigException : Exception
{
    public Type ConfigType { get; }

    protected ConfigException(Type configType, string message)
        : base(message)
    {
        ConfigType = configType;
    }

    protected ConfigException(Type configType, string message, Exception innerException)
        : base(message, innerException)
    {
        ConfigType = configType;
    }
}