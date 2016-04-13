using System;
using System.Collections.Generic;

namespace HyperMapper
{
    public static class AppendExtension
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, T newItem)
        {
            foreach (var item in items)
            {
                yield return item;
            }
            yield return newItem;
        }
         
    }
}