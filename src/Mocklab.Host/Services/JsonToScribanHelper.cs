using System.Text.Json;

namespace Mocklab.Host.Services;

/// <summary>
/// Converts JSON (string or JsonElement) to .NET objects suitable for Scriban (Dictionary, List, primitives).
/// </summary>
public static class JsonToScribanHelper
{
    public static object? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return FromElement(doc.RootElement);
        }
        catch
        {
            return null;
        }
    }

    public static object? FromElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Object => el.EnumerateObject()
                .ToDictionary(p => p.Name, p => FromElement(p.Value), StringComparer.OrdinalIgnoreCase),
            JsonValueKind.Array => el.EnumerateArray().Select(FromElement).ToList(),
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => null
        };
    }
}
