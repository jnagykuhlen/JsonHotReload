using System.Text.Json;

namespace JsonHotReload;

public static class CommonJsonSerializerOptions
{
    public static readonly JsonSerializerOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };
}