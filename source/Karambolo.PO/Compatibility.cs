#if !NETSTANDARD2_0
namespace System.Linq
{
    using System.Collections.Generic;

    static class EnumerableExtensions
    {
#if USE_COMMON

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            return Karambolo.Common.EnumerableUtils.WithHead(source, element);
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            return Karambolo.Common.EnumerableUtils.WithTail(source, element);
        }
#else
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            yield return element;

            foreach (var e in source)
                yield return e;
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            foreach (var e in source)
                yield return e;

            yield return element;
        }
#endif
    }
}
#endif