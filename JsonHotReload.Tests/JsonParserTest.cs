using FluentAssertions;

namespace JsonHotReload.Tests;

[TestClass]
public class JsonParserTest
{
    private const string TestFilePath = "test.json";
    private const string TestFileContent = """{"name":"Test Name","value":3}""";
    
    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        File.WriteAllText(TestFilePath, TestFileContent);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        File.Delete(TestFilePath);
    }
    
    [TestMethod]
    public async Task TestGenericParseAsync()
    {
        var result = await JsonParser.Instance.ParseAsync<DeserializationTarget>(TestFilePath);
        result.Should().Be(new DeserializationTarget("Test Name", 3));
    }
    
    [TestMethod]
    public async Task TestParseAsyncByType()
    {
        var result = await JsonParser.Instance.ParseAsync(TestFilePath, typeof(DeserializationTarget));
        result.Should().Be(new DeserializationTarget("Test Name", 3));
    }
    
    private record DeserializationTarget(string Name, int Value);
}