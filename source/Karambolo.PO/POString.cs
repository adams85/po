using System;
using System.Text;

namespace Karambolo.PO
{
    internal static class POString
    {
        public static string NewLine(bool isPlatformIndependent)
        {
            return isPlatformIndependent ? "\n" : Environment.NewLine;
        }

        public static int Decode(StringBuilder builder, string source, int startIndex, int count, string newLine)
        {
            for (var endIndex = startIndex + count; startIndex < endIndex; startIndex++)
            {
                var c = source[startIndex];
                if (c != '\\')
                {
                    builder.Append(c);
                    continue;
                }
                
                if (++startIndex < endIndex)
                {
                    c = source[startIndex];
                    switch (c)
                    {
                        case '\\':
                        case '"':
                            builder.Append(c);
                            continue;
                        case 't':
                            builder.Append('\t');
                            continue;
                        case 'r':
                            var index = startIndex;
                            if (++index + 1 < endIndex && source[index] == '\\' && source[++index] == 'n')
                                startIndex = index;
                            // "\r" and "\r\n" are both accepted as new line
                            goto case 'n';
                        case 'n':
                            builder.Append(newLine);
                            continue;
                    }
                }

                // invalid escape sequence
                return startIndex - 1;
            }

            return -1;
        }

        public static void Encode(StringBuilder builder, string source, int startIndex, int count)
        {
            for (var endIndex = startIndex + count; startIndex < endIndex; startIndex++)
            {
                var c = source[startIndex];
                switch (c)
                {
                    case '\\':
                    case '"':
                        builder.Append('\\').Append(c);
                        continue;
                    case '\t':
                        builder.Append('\\').Append('t');
                        continue;
                    case '\r':
                        var index = startIndex;
                        if (++index < endIndex && source[index] == '\n')
                            startIndex = index;
                        // "\r" and "\r\n" are encoded the same as "\n" to keep PO content platform-independent
                        goto case '\n';
                    case '\n':
                        builder.Append('\\').Append('n');
                        continue;
                }

                builder.Append(c);
            }
        }
    }
}
