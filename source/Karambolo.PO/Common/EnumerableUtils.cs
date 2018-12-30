using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    static partial class EnumerableUtils
    {
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
}
