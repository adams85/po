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

        public static int Decode(StringBuilder builder, string source, int startIndex, int endIndex, string newLine)
        {
            var chunkStart = startIndex;
            for (; startIndex < endIndex; startIndex++)
            {
                var c = source[startIndex];
                if (c == '\\')
                {
                    builder.Append(source, chunkStart, startIndex - chunkStart);

                    if (++startIndex >= endIndex)
                    {
                        // unterminated escape sequence
                        return startIndex - 1;
                    }

                    c = source[startIndex];
                    switch (c)
                    {
                        case '\\':
                        case '\'':
                        case '"':
                        case '?':
                            builder.Append(c);
                            break;
                        case 'r':
                            var index = startIndex;
                            if (++index + 1 < endIndex && source[index] == '\\' && source[++index] == 'n')
                                startIndex = index;
                            // "\r" and "\r\n" are both accepted as new line
                            goto case 'n';
                        case 'n':
                            builder.Append(newLine);
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'v':
                            builder.Append('\v');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'a':
                            builder.Append('\a');
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case '0':
                            builder.Append('\0');
                            break;
                        default:
                            // invalid escape sequence
                            return startIndex - 1;
                    }

                    chunkStart = startIndex + 1;
                }
            }

            builder.Append(source, chunkStart, startIndex - chunkStart);
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
