using System.Text.Json;
using FluentAssertions;

namespace JsonHotReload.Tests;

[TestClass]
public class HotReloadingJsonParserTest
{
    private const string TestDirectory = "tmp";

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        Directory.CreateDirectory(TestDirectory);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Directory.Delete(TestDirectory, true);
    }

    [TestMethod]
    public async Task TestParseAsyncWithMultipleTargets()
    {
        using var jsonDeserializer = new HotReloadingJsonParser(TestDirectory);

        var filePath = Path.Combine(TestDirectory, "test-multiple.json");

        await WriteAndWaitAsync(filePath, """{"name":"Test Name","value":3,"flag":true}""");

        var result = await jsonDeserializer.ParseAsync<DeserializationTarget>(filePath);
        var anotherResult = await jsonDeserializer.ParseAsync<AnotherDeserializationTarget>(filePath);

        result.Should().Be(new DeserializationTarget("Test Name", 3));
        anotherResult.Should().Be(new AnotherDeserializationTarget("Test Name", 3, true));

        await WriteAndWaitAsync(filePath, """{"name":"Another Test Name","value":42}""");

        result.Should().Be(new DeserializationTarget("Another Test Name", 42));
        anotherResult.Should().Be(new AnotherDeserializationTarget("Another Test Name", 42, true));
    }

    [TestMethod]
    public async Task TestParseAsyncWithCapitalizedExtension()
    {
        using var jsonDeserializer = new HotReloadingJsonParser(TestDirectory);

        var filePath = Path.Combine(TestDirectory, "test-extension-capitalization.JSoN");

        await WriteAndWaitAsync(filePath, """{"name":"Test Name","value":3}""");

        var result = await jsonDeserializer.ParseAsync<DeserializationTarget>(filePath);
        result.Should().Be(new DeserializationTarget("Test Name", 3));
    }

    [TestMethod]
    public async Task TestParseAsyncWithNonJsonExtension()
    {
        var filePath = Path.Combine(TestDirectory, "test-extension-unsupported.xml");

        var action = async () =>
        {
            using var jsonDeserializer = new HotReloadingJsonParser(TestDirectory);
            await jsonDeserializer.ParseAsync<DeserializationTarget>(filePath);
        };

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("'tmp/test-extension-unsupported.xml' is not a .json file.*");
    }

    [TestMethod]
    public async Task TestParseAsyncWithUntrackedFilePath()
    {
        var filePath = "untracked/test-untracked.json";

        var action = async () =>
        {
            using var jsonDeserializer = new HotReloadingJsonParser(TestDirectory);
            await jsonDeserializer.ParseAsync<DeserializationTarget>(filePath);
        };

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("'untracked/test-untracked.json' is not part of tracked directory 'tmp'.*");
    }

    [TestMethod]
    public async Task TestParseAsyncWithReloadableTarget()
    {
        using var jsonDeserializer = new HotReloadingJsonParser(TestDirectory);

        var filePath = Path.Combine(TestDirectory, "test-reloadable.json");

        await WriteAndWaitAsync(filePath, """{"name":"Test Name"}""");

        var result = await jsonDeserializer.ParseAsync<ReloadableDeserializationTarget>(filePath);

        result.Name.Should().Be("Test Name");
        result.Reloaded.Should().BeFalse();

        await WriteAndWaitAsync(filePath, """{"name":"Another Test Name"}""");

        result.Name.Should().Be("Test Name");
        result.Reloaded.Should().BeTrue();
    }

    [TestMethod]
    public async Task TestParseAsyncWithArray()
    {
        using var jsonDeserializer = new HotReloadingJsonParser(TestDirectory);

        var filePath = Path.Combine(TestDirectory, "test-array.json");

        await WriteAndWaitAsync(filePath, """[{"name":"Test Name","value":3}]""");

        var result = await jsonDeserializer.ParseAsync<List<DeserializationTarget>>(filePath);

        result.Should().BeEquivalentTo([new DeserializationTarget("Test Name", 3)]);
        
        await WriteAndWaitAsync(filePath, """[{"name":"Another Test Name","value":42}]""");
        
        result.Should().BeEquivalentTo([new DeserializationTarget("Another Test Name", 42)]);
    }

    private static async Task WriteAndWaitAsync(string filePath, string contents)
    {
        await File.WriteAllTextAsync(filePath, contents);
        await Task.Delay(50);
    }

    private record DeserializationTarget(string Name, int Value);

    private record AnotherDeserializationTarget(string Name, int Value, bool Flag);


    private class ReloadableDeserializationTarget(string name) : IReloadable
    {
        public string Name { get; } = name;
        public bool Reloaded { get; private set; }

        public void OnReload(JsonElement jsonElement)
        {
            Reloaded = true;
        }
    }
}