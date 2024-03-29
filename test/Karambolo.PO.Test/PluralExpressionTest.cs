﻿#if ENABLE_PLURALFORMS

using System.Collections.Generic;
using System;
using Xunit;
using System.Linq;
using Karambolo.PO.PluralExpression;
using System.Linq.Expressions;
using Karambolo.Po.Test.Helpers;
using Karambolo.PO.Test.Helpers;
using Hime.Redist;

namespace Karambolo.PO.Test
{
    public class PluralExpressionTest
    {
        private static readonly IReadOnlyList<(int, string, string)> s_pluralForms = new[]
        {
            (2, "(n > 1)", "ToCBool((n > 1))"),
            (2, "(n != 1)", "ToCBool((n != 1))"),
            (6, "(n==0 ? 0 : n==1 ? 1 : n==2 ? 2 : n%100>=3 && n%100<=10 ? 3 : n%100>=11 ? 4 : 5)", "IIF((n == 0), 0, IIF((n == 1), 1, IIF((n == 2), 2, IIF((((n % 100) >= 3) AndAlso ((n % 100) <= 10)), 3, IIF(((n % 100) >= 11), 4, 5)))))"),
            (1, "0", "0"),
            (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)", "IIF((((n % 10) == 1) AndAlso ((n % 100) != 11)), 0, IIF(((((n % 10) >= 2) AndAlso ((n % 10) <= 4)) AndAlso (((n % 100) < 10) OrElse ((n % 100) >= 20))), 1, 2))"),
            (3, "(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2", "IIF((n == 1), 0, IIF(((n >= 2) AndAlso (n <= 4)), 1, 2))"),
            (3, "(n==1) ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2", "IIF((n == 1), 0, IIF(((((n % 10) >= 2) AndAlso ((n % 10) <= 4)) AndAlso (((n % 100) < 10) OrElse ((n % 100) >= 20))), 1, 2))"),
            (4, "(n==1) ? 0 : (n==2) ? 1 : (n != 8 && n != 11) ? 2 : 3", "IIF((n == 1), 0, IIF((n == 2), 1, IIF(((n != 8) AndAlso (n != 11)), 2, 3)))"),
            (5, "n==1 ? 0 : n==2 ? 1 : (n>2 && n<7) ? 2 :(n>6 && n<11) ? 3 : 4", "IIF((n == 1), 0, IIF((n == 2), 1, IIF(((n > 2) AndAlso (n < 7)), 2, IIF(((n > 6) AndAlso (n < 11)), 3, 4))))"),
            (4, "(n==1 || n==11) ? 0 : (n==2 || n==12) ? 1 : (n > 2 && n < 20) ? 2 : 3", "IIF(((n == 1) OrElse (n == 11)), 0, IIF(((n == 2) OrElse (n == 12)), 1, IIF(((n > 2) AndAlso (n < 20)), 2, 3)))"),
            (2, "(n%10!=1 || n%100==11)", "ToCBool((((n % 10) != 1) OrElse ((n % 100) == 11)))"),
            (2, "(n != 0)", "ToCBool((n != 0))"),
            (4, "(n==1) ? 0 : (n==2) ? 1 : (n == 3) ? 2 : 3", "IIF((n == 1), 0, IIF((n == 2), 1, IIF((n == 3), 2, 3)))"),
            (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && (n%100<10 || n%100>=20) ? 1 : 2)", "IIF((((n % 10) == 1) AndAlso ((n % 100) != 11)), 0, IIF((((n % 10) >= 2) AndAlso (((n % 100) < 10) OrElse ((n % 100) >= 20))), 1, 2))"),
            (3, "(n%10==1 && n%100!=11 ? 0 : n != 0 ? 1 : 2)", "IIF((((n % 10) == 1) AndAlso ((n % 100) != 11)), 0, IIF((n != 0), 1, 2))"),
            (3, "n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2", "IIF((((n % 10) == 1) AndAlso ((n % 100) != 11)), 0, IIF(((((n % 10) >= 2) AndAlso ((n % 10) <= 4)) AndAlso (((n % 100) < 10) OrElse ((n % 100) >= 20))), 1, 2))"),
            (2, "n==1 || n%10==1 ? 0 : 1", "IIF(((n == 1) OrElse ((n % 10) == 1)), 0, 1)"),
            (3, "(n==0 ? 0 : n==1 ? 1 : 2)", "IIF((n == 0), 0, IIF((n == 1), 1, 2))"),
            (4, "(n==1 ? 0 : n==0 || ( n%100>1 && n%100<11) ? 1 : (n%100>10 && n%100<20 ) ? 2 : 3)", "IIF((n == 1), 0, IIF(((n == 0) OrElse (((n % 100) > 1) AndAlso ((n % 100) < 11))), 1, IIF((((n % 100) > 10) AndAlso ((n % 100) < 20)), 2, 3)))"),
            (3, "(n==1 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)", "IIF((n == 1), 0, IIF(((((n % 10) >= 2) AndAlso ((n % 10) <= 4)) AndAlso (((n % 100) < 10) OrElse ((n % 100) >= 20))), 1, 2))"),
            (3, "(n==1 ? 0 : (n==0 || (n%100 > 0 && n%100 < 20)) ? 1 : 2)", "IIF((n == 1), 0, IIF(((n == 0) OrElse (((n % 100) > 0) AndAlso ((n % 100) < 20))), 1, 2))"),
            (4, "(n%100==1 ? 0 : n%100==2 ? 1 : n%100==3 || n%100==4 ? 2 : 3)", "IIF(((n % 100) == 1), 0, IIF(((n % 100) == 2), 1, IIF((((n % 100) == 3) OrElse ((n % 100) == 4)), 2, 3)))"),
            (-1, "+1 + +2", "(1 + 2)"),
            (-1, "-1 + -2", "(-1 + -2)"),
            (-1, "1+-2", "(1 + -2)"),
            (-1, "1++2", "(1 + 2)"),
            (-1, "1--2", "(1 - -2)"),
            (-1, "1-+2", "(1 - 2)"),
            (-1, "1 + 2 * 3", "(1 + (2 * 3))"),
            (-1, "(1 - 2) / 3 % 4", "(((1 - 2) / 3) % 4)"),
            (-1, "1 + 2 == 1 * 2", "ToCBool(((1 + 2) == (1 * 2)))"),
            (-1, "1 + (2 == 1) * 2", "(1 + (ToCBool((2 == 1)) * 2))"),
            (-1, "1 > 2 != 1 < 2", "ToCBool((ToCBool((1 > 2)) != ToCBool((1 < 2))))"),
            (-1, "1 >= 2 + 1 <= 2", "ToCBool((ToCBool((1 >= (2 + 1))) <= 2))"),
            (-1, "1 != 2 && 3 == 4", "ToCBool(((1 != 2) AndAlso (3 == 4)))"),
            (-1, "1 != 2 || 3 == 4", "ToCBool(((1 != 2) OrElse (3 == 4)))"),
            (-1, "(1 == 1) == (2 == 2)", "ToCBool((ToCBool((1 == 1)) == ToCBool((2 == 2))))"),
            (-1, "n > 0 || n % 2 == 0 && 3", "ToCBool(((n > 0) OrElse (((n % 2) == 0) AndAlso FromCBool(3))))"),
            (-1, "(n > 0 || n % 2 == 0) && 3", "ToCBool((((n > 0) OrElse ((n % 2) == 0)) AndAlso FromCBool(3)))"),
            (-1, "n >= 0 && n <= 2 ? 1 : n == 3 ? 3 : 4", "IIF(((n >= 0) AndAlso (n <= 2)), 1, IIF((n == 3), 3, 4))"),
            (-1, "(n >= 0 && n <= 2 ? 1 : n == 3) ? 3 : 4", "IIF(FromCBool(IIF(((n >= 0) AndAlso (n <= 2)), 1, ToCBool((n == 3)))), 3, 4)"),
            (-1, "n >= 0 && n <= 2 ? (n == 1 ? 1 : 0) : 3", "IIF(((n >= 0) AndAlso (n <= 2)), IIF((n == 1), 1, 0), 3)"),
        };

        public static IEnumerable<object[]> PluralForms { get; } = s_pluralForms.Select(item => new object[] { item.Item1, item.Item2, item.Item3 }).ToArray();

        [Theory]
        [MemberData(nameof(PluralForms))]
        public void ParserCanParsePluralForms(int _, string input, string expectedExpression)
        {
            var lexer = new TestPluralExpressionLexer(input);
            var parser = new TestPluralExpressionParser(lexer);
            ParseResult parseResult = parser.Parse();
            Assert.True(parseResult.IsSuccess);
            Expression verificationExpression = new TestPluralExpressionCompiler(parseResult.Root).Visit();
            Assert.Equal(verificationExpression.ToString(), expectedExpression);

            Expression actualExpression = PluralExpressionParser.Parse(input, out ParameterExpression param);

            Assert.Equal(typeof(int), param.Type);
            Assert.Equal("n", param.Name);
            Assert.Equal(expectedExpression, actualExpression.ToString());
        }

        [Theory]
        [InlineData("", "Unexpected end of input")]
        [InlineData(" ", "Unexpected end of input")]
        [InlineData("x", "Unexpected token")]
        [InlineData("()", "Unexpected token")]
        [InlineData("(n", "Unexpected end of input")]
        [InlineData("n)", "Unexpected token")]
        [InlineData("(2 * (n + 1) > 1", "Unexpected end of input")]
        [InlineData("2 * (n + 1)) > 1", "Unexpected token")]
        [InlineData("1+++2", "Unexpected token")]
        [InlineData("1++-2", "Unexpected token")]
        [InlineData("1+-+2", "Unexpected token")]
        [InlineData("1+--2", "Unexpected token")]
        [InlineData("1-++2", "Unexpected token")]
        [InlineData("1-+-2", "Unexpected token")]
        [InlineData("1--+2", "Unexpected token")]
        [InlineData("1---2", "Unexpected token")]
        [InlineData("n |", "Unexpected token")]
        [InlineData("n | 1", "Unexpected token")]
        [InlineData("n & 1", "Unexpected token")]
        [InlineData("n = 1", "Unexpected token")]
        [InlineData("!n", "Unexpected token")]
        [InlineData("n + n | 1", "Unexpected token")]
        [InlineData("n + n & 1", "Unexpected token")]
        [InlineData("n + n = 1", "Unexpected token")]
        [InlineData("n + !n", "Unexpected token")]
        public void ParserCanHandleInvalidInput(string input, string expectedMessage)
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => PluralExpressionParser.Parse(input, out _));

            Assert.StartsWith(expectedMessage, ex.Message);
        }

        [Theory]
        [MemberData(nameof(PluralForms))]
        public void CompilerCanEvaluateParsedExpressions(int expectedPluralCount, string input, string _)
        {
            Expression expression = PluralExpressionParser.Parse(input, out ParameterExpression param);

            Func<int, int> compiledEvaluator = PluralExpressionEvaluator.Compile(expression, param);
            Func<int, int> interpreterEvaluator = PluralExpressionEvaluator.BuildInterpreter(expression, param);

            var actualEvaluationResults = new HashSet<int>();

            for (var n = 0; n < 1000; n++)
            {
                var compiledEvaluatorResult = compiledEvaluator(n);
                var interpreterEvaluatorResult = interpreterEvaluator(n);

                Assert.Equal(compiledEvaluatorResult, interpreterEvaluatorResult);

                actualEvaluationResults.Add(compiledEvaluatorResult);
            }

            if (expectedPluralCount >= 0)
                Assert.Equal(expectedPluralCount, actualEvaluationResults.Count);
        }
    }
}

#endif
