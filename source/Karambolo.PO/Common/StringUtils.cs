using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    internal static class StringUtils
    {
        public static int FindIndex(this string @string, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.FindIndex(0, @string.Length, match);
        }

        public static int FindIndex(this string @string, int startIndex, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.FindIndex(startIndex, @string.Length - startIndex, match);
        }

        public static int FindIndex(this string @string, int startIndex, int count, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            if (startIndex < 0 || length < startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            var endIndex = startIndex + count;
            if (count < 0 || length < endIndex)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (; startIndex < endIndex; startIndex++)
                if (match(@string[startIndex]))
                    return startIndex;

            return -1;
        }

        public static IEnumerable<string> Split(this string @string, Predicate<char> match, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            string section;
            var startIndex = 0;
            for (var index = 0; index < @string.Length; index++)
            {
                if (match(@string[index]))
                {
                    section = @string.Substring(startIndex, index - startIndex);
                    if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                        yield return section;

                    startIndex = index + 1;
                }
            }

            section = @string.Substring(startIndex);
            if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                yield return section;
        }
    }
}
