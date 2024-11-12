# JSON Hot Reload

C# library for JSON parsing that allows hot-reloading on file changes in real-time.

## Usage

```csharp
IJsonParser jsonParser = new HotReloadingJsonParser("My/Resource/Directory");
MyObject deserializedObject = jsonParser.Parse<MyObject>("My/Resource/Directory/MyObject.json");
```

`deserializedObject` is tracked and its properties are updated in-place whenever _MyObject.json_ is changed on disk.

`jsonParser` can be exchanged by `JsonParser.Instance` for production builds when hot-reloading is not required.
