using System;

namespace UnityEventBus.Utils
{
    internal static class Utils
    {
        internal static bool Implements<T>(this Type source) where T : class
        {
            return typeof(T).IsAssignableFrom(source);
        }

        internal static bool IsNull<T>(this T o) where T : class
        {
            return ReferenceEquals(o, null);
        }
    }
}