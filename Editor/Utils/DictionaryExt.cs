using System.Collections.Generic;

namespace io.github.azukimochi;

internal static class DictionaryExt
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
    {
        if (dictionary.TryGetValue(key, out TValue value)) 
            return value;

        value = factory(key);
        dictionary.Add(key, value);
        return value;
    }
}