using System.Text.Json;
using FluentAssertions;

namespace JsonHotReload.Tests;

[TestClass]
public class JsonElementExtensionTest
{
    [TestMethod]
    public void TestPopulate()
    {
        var target = new TestObject("Test Name", 3);
        
        var objectElement = JsonDocument.Parse("""{"name":"Another Test Name","unused":42}""").RootElement;
        objectElement.Populate(target);

        target.Should().BeEquivalentTo(new TestObject("Another Test Name", 3));
    }
    
    private record TestObject(string Name, int Value);
}