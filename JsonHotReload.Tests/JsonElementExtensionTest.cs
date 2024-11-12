using System.Text.Json;
using FluentAssertions;

namespace JsonHotReload.Tests;

[TestClass]
public class JsonElementExtensionTest
{
    [TestMethod]
    public void TestPopulateObject()
    {
        var target = new TestObject("Test Name", 3);
        
        var objectElement = JsonDocument.Parse("""{"name":"Another Test Name","unused":42}""").RootElement;
        objectElement.Populate(target);

        target.Should().Be(new TestObject("Another Test Name", 3));
    }
    
    [TestMethod]
    public void TestPopulateNestedObject()
    {
        var inner = new TestObject("Test Name", 3);
        var target = new NestedTestObject(inner, true);
        
        var objectElement = JsonDocument.Parse("""{"inner":{"name":"Another Test Name","value":5},"flag":true}""").RootElement;
        objectElement.Populate(target);

        target.Should().Be(new NestedTestObject(new TestObject("Another Test Name", 5), true));
        target.Inner.Should().BeSameAs(inner);
    }
    
    [TestMethod]
    public void TestPopulatePopulatableObject()
    {
        var target = new PopulatableTestObject("Test Name");
        target.Populated.Should().BeFalse();
        
        var objectElement = JsonDocument.Parse("""{"name":"Another Test Name","value":5}""").RootElement;
        objectElement.Populate(target);

        target.Name.Should().Be("Test Name");
        target.Populated.Should().BeTrue();
    }
    
    [TestMethod]
    public void TestPopulateArray()
    {
        var target = new List<TestObject> { new("Test Name", 3) };
        
        var objectElement = JsonDocument.Parse("""[{"name":"Another Test Name","value":5}, {"name":"Ignored","value":1}]""").RootElement;
        objectElement.Populate(target);

        target.Should().Equal(new TestObject("Another Test Name", 5));
    }
    
    private record TestObject(string Name, int Value);
    private record NestedTestObject(TestObject Inner, bool Flag);
    
    private class PopulatableTestObject(string name) : IPopulatable
    {
        public string Name { get; } = name;
        public bool Populated { get; private set; }

        public void PopulateFrom(JsonElement jsonElement)
        {
            Populated = true;
        }
    }
}