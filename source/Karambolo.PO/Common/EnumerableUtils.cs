using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
#if NET40 || NET45 || NETSTANDARD1_0 || NETSTANDARD2_0
    internal static partial class EnumerableUtils
    {
#if NET40 || NET45 || NETSTANDARD1_0
        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            foreach (TSource e in source)
                yield return e;

            yield return element;
        }
#endif

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new HashSet<T>(source, comparer);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return ToHashSet(source, null);
        }
    }
#endif
}
