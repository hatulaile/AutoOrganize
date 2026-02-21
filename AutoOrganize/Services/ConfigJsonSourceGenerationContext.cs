using System.Text.Json.Serialization;

namespace AutoOrganize.Services;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(int[]))]
public partial class ConfigJsonSourceGenerationContext : JsonSerializerContext;