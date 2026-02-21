using System;

namespace AutoOrganize.Exceptions.NavigationExceptions;

public class NavigationException : Exception
{
    public string NavigationName { get; }

    public NavigationException(string navigationName) :
        base($"Navigation error occurred in '{navigationName}'.")
    {
        NavigationName = navigationName;
    }

    public NavigationException(string navigationName, string message) : base(message)
    {
        NavigationName = navigationName;
    }
}