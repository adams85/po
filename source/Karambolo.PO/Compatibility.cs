using System;
using System.Text;

namespace Karambolo.PO
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder Append(this StringBuilder builder, int value, IFormatProvider provider)
        {
#if NET6_0_OR_GREATER
            return builder.Append(provider, $"{value}");
#else
            return builder.Append(value.ToString(provider));
#endif
        }
    }
}
