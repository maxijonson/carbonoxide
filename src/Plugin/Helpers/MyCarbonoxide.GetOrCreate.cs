using System.Collections.Generic;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public static TValue GetOrCreate<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key)
        where TValue : new()
    {
        TValue value;
        if (!dict.TryGetValue(key, out value))
        {
            value = new TValue();
            dict[key] = value;
        }

        return value;
    }
}
