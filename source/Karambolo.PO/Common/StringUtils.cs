using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    static class StringUtils
    {
        public static int FindIndex(this string @this, Func<char, bool> match)
        {
            return @this.FindIndex(match, 0, @this.Length);
        }

        public static int FindIndex(this string @this, Func<char, bool> match, int startIndex)
        {
            return @this.FindIndex(match, startIndex, @this.Length - startIndex);
        }

        public static int FindIndex(this string @this, Func<char, bool> match, int startIndex, int count)
        {
            var length = @this.Length;
            var endIndex = startIndex + count;

            if (startIndex < 0 || length < startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || length < endIndex)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (; startIndex < endIndex; startIndex++)
                if (match(@this[startIndex]))
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
