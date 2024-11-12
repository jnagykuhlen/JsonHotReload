using System.Collections;

namespace JsonHotReload;

public static class ListExtension
{
    public static void RemoveAllStartingAt(this IList list, int startIndex)
    {
        for (var index = list.Count - 1; index >= startIndex; index--)
            list.RemoveAt(index);
    }
    
    public static Type? GetItemType(this IList list) =>
        list.GetType().GetInterfaces()
            .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(interfaceType => interfaceType.GetGenericArguments()[0])
            .FirstOrDefault();
}