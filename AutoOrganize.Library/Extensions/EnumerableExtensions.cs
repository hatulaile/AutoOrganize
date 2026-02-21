using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Extensions;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T>? enumerable)
    {
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<T>? NullIfEmpty()
        {
            if (enumerable is null)
                return null;

            return enumerable.Any() ? enumerable : null;
        }
    }
}