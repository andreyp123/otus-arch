using System.Text.Json;
using System.Text.Json.Nodes;

namespace Common.Helpers;

public static class JsonHelper
{
    private static JsonSerializerOptions? Options { get; } =
        new()
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
    
    public static TValue? Deserialize<TValue>(JsonObject jsonObj)
    {
        return jsonObj.Deserialize<TValue>(Options);
    }
}