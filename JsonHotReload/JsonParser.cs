using System.Text.Json;

namespace JsonHotReload;

public class JsonParser : IJsonParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public static readonly JsonParser Instance = new();

    public async Task<T> ParseAsync<T>(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);
        var result = await JsonSerializer.DeserializeAsync<T>(fileStream, SerializerOptions);
        return result ?? throw new ArgumentException("JSON file deserializes to null.", nameof(filePath));
    }

    public async Task<object> ParseAsync(string filePath, Type type)
    {
        await using var fileStream = File.OpenRead(filePath);
        var result = await JsonSerializer.DeserializeAsync(fileStream, type, SerializerOptions);
        return result ?? throw new ArgumentException("JSON file deserializes to null.", nameof(filePath));
    }
}