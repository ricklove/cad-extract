using System.Collections.Generic;

namespace CadExtract.Library
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> items, TKey key) where TValue : class => !items.TryGetValue(key, out var value) ? null : value;
        public static TValue? GetOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue?> items, TKey key) where TValue : struct => !items.TryGetValue(key, out var value) ? null : value;
        public static TValue? GetOrNullable<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> items, TKey key) where TValue : struct => !items.TryGetValue(key, out var value) ? null as TValue? : value as TValue?;
        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> items, TKey key) => !items.TryGetValue(key, out var value) ? default : value;
    }
}
