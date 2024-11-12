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

        target.Should().Be(new TestObject("Another Test Name", 3));
    }
    
    [TestMethod]
    public void TestPopulateNested()
    {
        var inner = new TestObject("Test Name", 3);
        var target = new NestedTestObject(inner, true);
        
        var objectElement = JsonDocument.Parse("""{"inner":{"name":"Another Test Name","value":5},"flag":true}""").RootElement;
        objectElement.Populate(target);

        target.Should().Be(new NestedTestObject(new TestObject("Another Test Name", 5), true));
        target.Inner.Should().BeSameAs(inner);
    }
    
    private record TestObject(string Name, int Value);
    private record NestedTestObject(TestObject Inner, bool Flag);
}