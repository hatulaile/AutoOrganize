using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Extensions;

public static class CollectionExtensions
{
    extension<TSource>(List<TSource> list)
    {
        public void AddRangeIfNotExists(IEnumerable<TSource> addItems)
        {
            AddRangeIfNotExists(list, addItems, null);
        }

        public void AddRangeIfNotExists(IEnumerable<TSource> addItems, IEqualityComparer<TSource>? comparer)
        {
            list.AddRange(addItems.Where(x => list.Contains(x, comparer)));
        }
    }

    extension<TSource>(IEnumerable<TSource> enumerable)
    {
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<TSource>? NullIfEmpty() =>
            enumerable.Any() ? enumerable : null;
    }
}