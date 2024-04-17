using System.Text.Json;

namespace JsonHotReload;

public interface IReloadable
{
    void OnReload(JsonElement jsonElement);
}