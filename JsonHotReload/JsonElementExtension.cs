using System.Reflection;
using System.Text.Json;

namespace JsonHotReload;

public static class JsonElementExtension
{
    private const BindingFlags PropertyBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

    public static void Populate(this JsonElement jsonObjectElement, object target)
    {
        foreach (var property in jsonObjectElement.EnumerateObject())
            SetProperty(target, property);
    }

    private static void SetProperty(object target, JsonProperty property)
    {
        var propertyInfo = target.GetType().GetProperty(property.Name, PropertyBindingFlags);
        if (propertyInfo != null)
        {
            var deserializedValue = JsonSerializer.Deserialize(
                property.Value.GetRawText(),
                propertyInfo.PropertyType,
                CommonJsonSerializerOptions.CaseInsensitive
            );

            propertyInfo.SetValue(target, deserializedValue);
        }
    }
}