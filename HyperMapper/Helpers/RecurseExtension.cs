using System;
using System.Collections.Generic;

namespace HyperMapper
{
    public static class RecurseExtension
    {
        public static IEnumerable<T> Recurse<T>(this T t, Func<T, T> getNext)
        {
            yield return t;
            while (t != null)
            {
                t = getNext(t);
                yield return t;
            }
        }

        public static IEnumerable<T> Recurse<T>(this T t, Func<T, IEnumerable<T>> getNext)
        {
            yield return t;
            foreach (var child in getNext(t))
            {
                yield return child;
            }
        }
    }
}