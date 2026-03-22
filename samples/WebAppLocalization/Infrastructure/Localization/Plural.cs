using System;

namespace Karambolo.Common.Localization
{
    public readonly struct Plural : IFormattable
    {
        public static Plural From(string id, int count)
        {
            return new Plural(id, count);
        }

        private Plural(string id, int count)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            Count = count;
        }

        public string Id { get; }
        public int Count { get; }

        public override string ToString()
        {
            return Count.ToString();
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return Count.ToString(format, formatProvider);
        }
    }
}
