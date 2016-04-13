using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HyperMapper
{
    static class AsReadonlyDictionaryExtension
    {
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}