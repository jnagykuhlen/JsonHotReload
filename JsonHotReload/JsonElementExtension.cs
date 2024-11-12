using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace JsonHotReload;

public static class JsonElementExtension
{
    private const BindingFlags PropertyBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

    public static void Populate(this JsonElement jsonElement, object target)
    {
        if (target is IPopulatable populatableTarget)
        {
            populatableTarget.PopulateFrom(jsonElement);
        }
        else if (target is IEnumerable<object> enumerableTarget && jsonElement.ValueKind == JsonValueKind.Array)
        {
            jsonElement.PopulateArray(enumerableTarget);
        }
        else if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            jsonElement.PopulateObject(target);
        }
    }

    public static void PopulateArray(this JsonElement jsonArrayElement, IEnumerable<object> enumerableTarget)
    {
        using var targetEnumerator = enumerableTarget.GetEnumerator();
        using var jsonArrayEnumerator = jsonArrayElement.EnumerateArray().GetEnumerator();

        var count = 0;
        while (targetEnumerator.MoveNext() && jsonArrayEnumerator.MoveNext())
        {
            jsonArrayEnumerator.Current.Populate(targetEnumerator.Current);
            count++;
        }

        if (enumerableTarget is IList listTarget)
        {
            listTarget.RemoveAllStartingAt(count);

            var itemType = listTarget.GetItemType();
            if (itemType != null)
            {
                while (jsonArrayEnumerator.MoveNext())
                    listTarget.Add(jsonArrayEnumerator.Current.Deserialize(itemType, CommonJsonSerializerOptions.CaseInsensitive));
            }
        }
    }

    private static void PopulateObject(this JsonElement jsonObjectElement, object target)
    {
        foreach (var property in jsonObjectElement.EnumerateObject())
            SetProperty(target, property);
    }

    private static void SetProperty(object target, JsonProperty property)
    {
        var propertyInfo = target.GetType().GetProperty(property.Name, PropertyBindingFlags);
        if (propertyInfo != null)
        {
            if (property.Value.ValueKind == JsonValueKind.Object && propertyInfo.CanRead)
            {
                var propertyValue = propertyInfo.GetValue(target);
                if (propertyValue != null)
                    property.Value.Populate(propertyValue);
            }
            else if (propertyInfo.CanWrite)
            {
                var deserializedValue =
                    property.Value.Deserialize(propertyInfo.PropertyType, CommonJsonSerializerOptions.CaseInsensitive);

                propertyInfo.SetValue(target, deserializedValue);
            }
        }
    }
}