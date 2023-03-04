using System.Runtime.CompilerServices;

namespace Karambolo.PO.PluralExpression
{
    internal struct PluralExpressionTokenizer
    {
        public enum Symbol
        {
            Error = -1,
            Eos = 0,
            Integer = 1,
            Variable = 2,
            OpConsequent = 3,
            OpAlternate = 4,
            OpLogicalOr = 5,
            OpLogicalAnd = 6,
            OpEq = 7,
            OpNeq = 8,
            OpLt = 9,
            OpGt = 10,
            OpLte = 11,
            OpGte = 12,
            OpAdd = 13,
            OpSub = 14,
            OpMul = 15,
            OpDiv = 16,
            OpMod = 17,
            OpenParen = 18,
            CloseParen = 19,
        }

        public readonly struct Token
        {
            public readonly int Start;
            public readonly int End;
            public readonly Symbol Symbol;

            public Token(int start, int end, Symbol symbol)
            {
                Start = start;
                End = end;
                Symbol = symbol;
            }
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsInRange(char ch, char min, char max) => ch - (uint)min <= max - (uint)min;

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsDecimalDigit(char ch) => IsInRange(ch, '0', '9');

        public readonly string Input;
        private int _index;

        public PluralExpressionTokenizer(string input)
        {
            Input = input;
            _index = 0;
        }

        public Token NextToken()
        {
            return SkipWhiteSpace() ? ReadToken() : new Token(_index, _index, Symbol.Eos);
        }

        private bool SkipWhiteSpace()
        {
            for (; _index < Input.Length; _index++)
            {
                var ch = Input[_index];
                if (ch != '\x20' && !IsInRange(ch, '\t', '\r'))
                {
                    return true;
                }
            }

            return false;
        }

        private Token ReadToken()
        {
            var start = _index;
            char ch = Input[start];
            switch (ch)
            {
                case 'n':
                    _index++;
                    return new Token(start, _index, Symbol.Variable);

                case '+':
                case '-':
                    _index++;
                    if (!IsDecimalDigit((char)CharCodeAtIndex()))
                    {
                        return new Token(start, _index, ch == '+' ? Symbol.OpAdd : Symbol.OpSub);
                    }
                    goto case '0';

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    for (_index++; _index < Input.Length; _index++)
                    {
                        if (!IsDecimalDigit((char)CharCodeAtIndex()))
                        {
                            break;
                        }
                    }
                    return new Token(start, _index, Symbol.Integer);

                case '?':
                    _index++;
                    return new Token(start, _index, Symbol.OpConsequent);

                case ':':
                    _index++;
                    return new Token(start, _index, Symbol.OpAlternate);

                case '|':
                case '&':
                    _index++;
                    if (CharCodeAtIndex() != ch)
                    {
                        goto default;
                    }
                    _index++;
                    return new Token(start, _index, ch == '|' ? Symbol.OpLogicalOr : Symbol.OpLogicalAnd);

                case '=':
                case '!':
                    _index++;
                    if (CharCodeAtIndex() != '=')
                    {
                        goto default;
                    }
                    _index++;
                    return new Token(start, _index, ch == '=' ? Symbol.OpEq : Symbol.OpNeq);

                case '<':
                case '>':
                    _index++;
                    if (CharCodeAtIndex() != '=')
                    {
                        return new Token(start, _index, ch == '<' ? Symbol.OpLt : Symbol.OpGt);
                    }
                    else
                    {
                        _index++;
                        return new Token(start, _index, ch == '<' ? Symbol.OpLte : Symbol.OpGte);
                    }

                case '*':
                    _index++;
                    return new Token(start, _index, Symbol.OpMul);

                case '/':
                    _index++;
                    return new Token(start, _index, Symbol.OpDiv);

                case '%':
                    _index++;
                    return new Token(start, _index, Symbol.OpMod);

                case '(':
                    _index++;
                    return new Token(start, _index, Symbol.OpenParen);

                case ')':
                    _index++;
                    return new Token(start, _index, Symbol.CloseParen);

                default:
                    return new Token(start, _index, Symbol.Error);
            }
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int CharCodeAtIndex()
        {
            return (uint)_index < Input.Length ? Input[_index] : int.MinValue;
        }
    }
}
