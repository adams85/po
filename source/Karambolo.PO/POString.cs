using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Karambolo.Common;

namespace Karambolo.PO
{
    internal static class POString
    {
        public static string NewLine(bool isPlatformIndependent)
        {
            return isPlatformIndependent ? "\n" : Environment.NewLine;
        }

        public static string StringBreak(string newLine)
        {
            return "\"" + newLine + "\"";
        }

        public static int Decode(StringBuilder builder, string source, int startIndex, int endIndex, string newLine)
        {
            Debug.Assert(startIndex <= endIndex);

            var chunkStartIndex = startIndex;
            int tmp;

            for (; startIndex < endIndex; startIndex++)
            {
                var c = source[startIndex];
                if (c == '\\')
                {
                    builder.Append(source, chunkStartIndex, startIndex - chunkStartIndex);

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
                            tmp = startIndex + 2;
                            if (tmp < endIndex && source[tmp - 1] == '\\' && source[tmp] == 'n')
                                startIndex = tmp;
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

                    chunkStartIndex = startIndex + 1;
                }
            }

            builder.Append(source, chunkStartIndex, startIndex - chunkStartIndex);
            return -1;
        }

        public static void Encode(StringBuilder builder, string source)
        {
            Encode(builder, source, maxLineLength: -1, breakAfterNewLine: false, stringBreak: null);
        }

        public static void Encode(StringBuilder builder, string source, int maxLineLength, bool breakAfterNewLine, string stringBreak)
        {
            Debug.Assert(builder.ToString().FindIndex(char.IsSurrogate) < 0);

            var builderStartIndex = builder.Length;
            var builderLineStartIndex = 0;
            int lineLength, unicodeLineLength;
            int charCount, codePointCount;

            builder.Append('\"');
            lineLength = unicodeLineLength = builderStartIndex + 1;
            int potentialBreakOffset = -1, unicodePotentialBreakOffset = -1; // interpreted within the current line

            int index, originalIndex;
            var chunkStartIndex = 0;
            int tmp;

            for (index = 0; index < source.Length; ++index)
            {
                originalIndex = index;

                var c = source[index];
                char c2;

                switch (c)
                {
                    case '\\':
                    case '"':
                        c2 = c;
                        break;
                    case '\r':
                        tmp = index + 1;
                        if (tmp < source.Length && source[tmp] == '\n')
                        {
                            index = tmp;
                        }
                        // "\r" and "\r\n" are encoded the same as "\n" to keep PO content platform-independent
                        goto case '\n';
                    case '\n':
                        c2 = 'n';
                        break;
                    case '\t':
                        c2 = 't';
                        break;
                    case '\v':
                        c2 = 'v';
                        break;
                    case '\f':
                        c2 = 'f';
                        break;
                    case '\a':
                        c2 = 'a';
                        break;
                    case '\b':
                        c2 = 'b';
                        break;
                    case '\0':
                        c2 = '0';
                        break;
                    default:
                        charCount = codePointCount = 1;

                        if (char.IsHighSurrogate(c))
                        {
                            tmp = index + 1;
                            if (tmp < source.Length && char.IsLowSurrogate(c2 = source[tmp]))
                            {
                                charCount++;
                                index = tmp;
                                goto CheckLineLength;
                            }
                        }

                        c2 = '\0';
                        goto CheckLineLength;
                }

                c = '\\';
                charCount = codePointCount = 2;

            CheckLineLength:
                if ((uint)(unicodeLineLength + codePointCount) >= (uint)maxLineLength)
                {
                    builder.Append(source, chunkStartIndex, originalIndex - chunkStartIndex);
                    chunkStartIndex = originalIndex;

                    if (builderStartIndex >= 0)
                    {
                        builder.Insert(builderStartIndex + 1, stringBreak);
                        builderLineStartIndex = builderStartIndex + stringBreak.Length;
                        lineLength -= builderStartIndex;
                        unicodeLineLength -= builderStartIndex;

                        if (potentialBreakOffset >= 0)
                        {
                            potentialBreakOffset -= builderStartIndex;
                            unicodePotentialBreakOffset -= builderStartIndex;
                        }

                        builderStartIndex = -1;

                        if ((uint)(unicodeLineLength + codePointCount) < (uint)maxLineLength)
                        {
                            goto AddCharSequence;
                        }
                    }

                    if (potentialBreakOffset >= 0)
                    {
                        builder.Insert(builderLineStartIndex + potentialBreakOffset, stringBreak);
                        potentialBreakOffset--;
                        builderLineStartIndex += potentialBreakOffset + stringBreak.Length;
                        lineLength -= potentialBreakOffset;
                        unicodeLineLength -= unicodePotentialBreakOffset - 1;

                        potentialBreakOffset = unicodePotentialBreakOffset = -1;
                    }
                    else if (lineLength > 1)
                    {
                        builder.Append(stringBreak);
                        builderLineStartIndex += lineLength + stringBreak.Length - 1;
                        lineLength = unicodeLineLength = 1;
                    }
                }

            AddCharSequence:
                lineLength += charCount;
                unicodeLineLength += codePointCount;

                if (codePointCount == 1) // single code point?
                {
                    if (c.IsBreakingHyphen() || c.IsBreakingWhiteSpace())
                    {
                        potentialBreakOffset = lineLength;
                        unicodePotentialBreakOffset = unicodeLineLength;
                    }
                }
                else // escape sequence?
                {
                    builder.Append(source, chunkStartIndex, originalIndex - chunkStartIndex);
                    chunkStartIndex = index + 1;

                    if (!breakAfterNewLine || c2 != 'n')
                    {
                        builder.Append(c).Append(c2);

                        if (c2 != '"' && c2 != '\\')
                        {
                            potentialBreakOffset = lineLength;
                            unicodePotentialBreakOffset = unicodeLineLength;
                        }
                    }
                    else
                    {
                        if (builderStartIndex >= 0)
                        {
                            builder.Insert(builderStartIndex + 1, stringBreak);
                            builderLineStartIndex = builderStartIndex + stringBreak.Length;
                            lineLength -= builderStartIndex;
                            unicodeLineLength -= builderStartIndex;
                            builderStartIndex = -1;
                        }

                        builder.Append(c).Append(c2);

                        if (index < source.Length - 1)
                        {
                            builder.Append(stringBreak);
                            builderLineStartIndex += lineLength + stringBreak.Length - 1;
                            lineLength = unicodeLineLength = 1;
                            potentialBreakOffset = unicodePotentialBreakOffset = -1;
                        }
                    }
                }
            }

            builder.Append(source, chunkStartIndex, index - chunkStartIndex);
            builder.Append('\"');
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsBreakingHyphen(this char c)
        {
            return
                c == '\u002D' /* HYPHEN-MINUS */ ||
                c == '\u00AD' /* SOFT HYPHEN */ ||
                c == '\u2010' /* HYPHEN */;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsBreakingWhiteSpace(this char c)
        {
            return char.IsWhiteSpace(c) && !(
                c == '\u00A0' /* NO-BREAK SPACE */ ||
                c == '\u202F' /* ­NARROW NO-BREAK SPACE */ ||
                c == '\u2007' /* FIGURE SPACE */);
        }
    }
}
