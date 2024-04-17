namespace JsonHotReload;

public interface IJsonParser
{
    Task<T> ParseAsync<T>(string filePath);
    Task<object> ParseAsync(string filePath, Type type);
}