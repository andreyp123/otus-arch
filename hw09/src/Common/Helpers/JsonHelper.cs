using System.Text.Json;

namespace Common.Helpers;

public static class JsonHelper
{
    public static JsonSerializerOptions? Options { get; } =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    public static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize<TValue>(value, Options);
    }

    public static TValue? Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, Options);
    }
}