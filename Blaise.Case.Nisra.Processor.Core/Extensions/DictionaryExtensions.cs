using System;
using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Core.Extensions
{
    public static class DictionaryExtensions
    {
        //this will throw an error if an existing key from the dictionary to add exists in the base dictionary
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> baseDictionary,
            IDictionary<TKey, TValue> dictionaryToAdd)
        {
            dictionaryToAdd.ForEach(x => baseDictionary.Add(x.Key, x.Value));
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}
