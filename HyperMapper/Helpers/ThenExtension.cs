using System;
using System.Collections.Generic;

namespace HyperMapper.Helpers
{
    public static class ThenExtension
    {
        public static T Then<T>(this T t, Action<T> action)
        {
            action(t);
            return t;
        }

    }
}