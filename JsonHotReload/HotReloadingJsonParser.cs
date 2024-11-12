using System.Diagnostics;
using System.Text.Json;

namespace JsonHotReload;

public class HotReloadingJsonParser : IJsonParser, IDisposable
{
    private const string JsonFileExtension = ".json";

    private readonly MultiDictionary<string, object> trackedInstancesByFilePath = new();
    private readonly FileSystemWatcher fileSystemWatcher;

    public HotReloadingJsonParser(string trackedPath)
    {
        fileSystemWatcher = new FileSystemWatcher(Normalize(trackedPath), $"*{JsonFileExtension}");
        fileSystemWatcher.Changed += OnFileChanged;
        fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs args)
    {
        var filePath = Normalize(args.FullPath);

        var trackedInstances = trackedInstancesByFilePath[filePath];
        if (trackedInstances.Count > 0)
        {
            Debug.WriteLine($"Tracked file '{filePath}' changed, reloading...");
            
            try
            {
                await using var fileStream = File.OpenRead(filePath);
                var jsonElement = (await JsonDocument.ParseAsync(fileStream)).RootElement;

                foreach (var trackedInstance in trackedInstances)
                    jsonElement.Populate(trackedInstance);
            }
            catch (IOException exception)
            {
                Debug.WriteLine($"Failed to reload file '{filePath}': {exception.Message}");
            }
        }
    }

    public async Task<T> ParseAsync<T>(string filePath) =>
        (T)await ParseAsync(filePath, typeof(T));

    public async Task<object> ParseAsync(string filePath, Type type)
    {
        var normalizedFilePath = Normalize(filePath);

        if (!Path.GetExtension(filePath).Equals(JsonFileExtension, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"'{normalizedFilePath}' is not a {JsonFileExtension} file.", nameof(filePath));

        if (!normalizedFilePath.StartsWith(fileSystemWatcher.Path))
            throw new ArgumentException($"'{normalizedFilePath}' is not part of tracked directory '{fileSystemWatcher.Path}'.", nameof(filePath));

        var instance = await JsonParser.Instance.ParseAsync(filePath, type);
        trackedInstancesByFilePath.Add(normalizedFilePath, instance);
        return instance;
    }

    public void Dispose() => fileSystemWatcher.Dispose();

    private static string Normalize(string filePath) => filePath.Replace('\\', '/');
}