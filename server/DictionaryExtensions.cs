namespace advisor;
using System.Collections.Generic;

public static class DictionaryExtensions {
    public static T Get<T, K>(this IDictionary<K, T> dic, K key, T defaultVaue = default(T))
        => dic.TryGetValue(key, out var value)
            ? value
            : defaultVaue;
}
