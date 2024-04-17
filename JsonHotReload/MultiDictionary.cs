namespace JsonHotReload;

public class MultiDictionary<TKey, TValue> where TKey:  notnull
{
    private readonly Dictionary<TKey, List<TValue>> valuesByKey = new();

    public void Add(TKey key, TValue value)
    {
        if (!valuesByKey.TryGetValue(key, out var values))
        {
            values = new List<TValue>();
            valuesByKey.Add(key, values);
        }
        
        values.Add(value);
    }

    public IReadOnlyCollection<TValue> this[TKey key] =>
        valuesByKey.TryGetValue(key, out var values) ? values : [];
}