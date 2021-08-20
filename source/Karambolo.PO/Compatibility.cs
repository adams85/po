using System;
using System.Text;

namespace Karambolo.PO
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder Append(this StringBuilder builder, int value, IFormatProvider provider)
        {
            // TODO: .NET 6 will provide a more efficient solution:
            // https://github.com/dotnet/runtime/issues/50674#issuecomment-812782309

            return builder.Append(value.ToString(provider));
        }
    }
}
