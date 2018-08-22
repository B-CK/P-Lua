using System.Collections;
using System.Collections.Generic;

public static class DictionaryExtern
{
    public static Key GetKey<Key, Value>(this Dictionary<Key, Value> dict, Value value)
    {
        foreach (var item in dict)
            if (item.Value.Equals(value))
                return item.Key;
        return default(Key);
    }

}
