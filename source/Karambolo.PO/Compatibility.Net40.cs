using System;
using System.Linq;
using System.Reflection;

namespace Karambolo.PO
{
    class TypeInfo
    {
        public Assembly Assembly { get; set; }
    }

    static partial class ReflectionExtensions
    {
        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo { Assembly = type.Assembly };
        }
    }
}