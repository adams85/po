using System;
using System.Reflection;

namespace Karambolo.PO
{
    internal readonly struct TypeInfo
    {
        public TypeInfo(Assembly assembly)
        {
            Assembly = assembly;
        }

        public Assembly Assembly { get; }
    }

    internal static partial class ReflectionExtensions
    {
        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type.Assembly);
        }

#if ENABLE_PLURALFORMS
        public static MethodInfo GetMethodInfo(this Delegate del)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));

            return del.Method;
        }
#endif
    }
}

#if !USE_COMMON
namespace Karambolo.Common.Collections
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }
    }

    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        T this[int index] { get; }
    }

    public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
        TValue this[TKey key] { get; }
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }
}
#endif
