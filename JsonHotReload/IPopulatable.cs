using System.Text.Json;

namespace JsonHotReload;

public interface IPopulatable
{
    void PopulateFrom(JsonElement jsonElement);
}