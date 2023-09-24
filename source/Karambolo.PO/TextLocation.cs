using System;

namespace Karambolo.PO
{
    public readonly struct TextLocation
    {
        public TextLocation(int line, int column)
        {
            if (line < 0)
                throw new ArgumentOutOfRangeException(nameof(line), line, null);

            if (column < 0)
                throw new ArgumentOutOfRangeException(nameof(column), column, null);

            Line = line;
            Column = column;
        }

        public int Line { get; }
        public int Column { get; }

        public override string ToString()
        {
            return $"{Line + 1},{Column + 1}";
        }
    }
}
