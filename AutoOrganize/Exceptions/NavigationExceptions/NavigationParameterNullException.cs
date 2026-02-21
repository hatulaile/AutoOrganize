namespace AutoOrganize.Exceptions.NavigationExceptions;

public class NavigationParameterNullException : NavigationException
{
    public string ParameterName { get; }

    public NavigationParameterNullException(string navigationName, string parameterName)
        : base(navigationName, $"Parameter '{parameterName}' is null for navigation '{navigationName}'.")
    {
        ParameterName = parameterName;
    }

    public NavigationParameterNullException(string navigationName, string parameterName, string message)
        : base(navigationName, message)
    {
        ParameterName = parameterName;
    }
}