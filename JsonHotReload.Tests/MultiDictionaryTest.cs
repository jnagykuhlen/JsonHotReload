using FluentAssertions;

namespace JsonHotReload.Tests;

[TestClass]
public class MultiDictionaryTest
{
    [TestMethod]
    public void TestAddMultiple()
    {
        MultiDictionary<string, int> dictionary = new();
        
        dictionary.Add("key", 3);
        dictionary.Add("key", 42);
        
        dictionary["key"].Should().BeEquivalentTo([3, 42]);
        dictionary["does-not-exist"].Should().BeEmpty();
    }
}