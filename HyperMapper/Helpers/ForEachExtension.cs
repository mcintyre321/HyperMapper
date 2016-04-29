using System;
using System.Collections.Generic;

namespace HyperMapper.Helpers
{
    static class ForEachExtension
    {
        public static void ForEach<T>(this T[] items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}