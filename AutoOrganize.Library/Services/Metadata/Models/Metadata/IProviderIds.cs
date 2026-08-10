using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Services.Metadata.Models.Metadata;

public interface IProviderIds : IEnumerable<KeyValuePair<string, string>>
{
    int Count { get; }

    string this[string key] { get; set; }

    bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    void Add(string key, string value);

    bool TryAdd(string key, string value);

    bool ContainsKey(string key);

    bool Remove(string key);

    void Clear();
}
