using System;
using System.Text;

namespace Karambolo.PO
{
    internal static class POString
    {
        public static int Decode(StringBuilder builder, string source, int startIndex, int count, bool environmentIndependentNewLine)
        {
            var endIndex = startIndex + count;
            for (; startIndex < endIndex; startIndex++)
            {
                var c = source[startIndex];
                if (c == '\\')
                    if (++startIndex < endIndex && TryDecodeEscapeSequence(builder, source[startIndex]))
                        continue;
                    else
                        return startIndex - 1;

                builder.Append(c);
            }

            return -1;

            bool TryDecodeEscapeSequence(StringBuilder b, char c)
            {
                switch (c)
                {
                    case '\\': b.Append('\\'); return true;
                    case '"': b.Append('"'); return true;
                    case 't': b.Append('\t'); return true;
                    case 'n':
                        if (environmentIndependentNewLine)
                            b.Append('\n');
                        else
                            b.Append(Environment.NewLine);
                        return true;
                    default: return false;
                }
            }
        }

        public static void Encode(StringBuilder builder, string source, int startIndex, int count)
        {
            var endIndex = startIndex + count;
            for (; startIndex < endIndex; startIndex++)
            {
                var c = source[startIndex];
                switch (c)
                {
                    case '\\': builder.Append('\\'); break;
                    case '"': builder.Append('\\'); break;
                    case '\t': builder.Append('\\'); c = 't'; break;
                    case '\r':
                    case '\n':
                        var index = startIndex;
                        if (c == '\r' && ++index < endIndex && source[index] == '\n')
                            startIndex = index;

                        builder.Append('\\'); c = 'n'; break;
                }

                builder.Append(c);
            }
        }
    }
}
