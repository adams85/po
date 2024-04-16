using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Karambolo.PO.PluralExpression
{
    using static PluralExpressionTokenizer;

    internal struct PluralExpressionParser
    {
        internal static Expression EnsureBoolean(Expression expression)
        {
            if (expression.Type == typeof(bool))
                return expression;

            if (expression is MethodCallExpression methodCallExpression && methodCallExpression.Method == PluralExpressionEvaluator.ToCBoolMethod)
                return methodCallExpression.Arguments[0];

            return Expression.Call(PluralExpressionEvaluator.FromCBoolMethod, expression);
        }

        internal static Expression EnsureInt32(Expression expression)
        {
            if (expression.Type == typeof(int))
                return expression;

            if (expression is MethodCallExpression methodCallExpression && methodCallExpression.Method == PluralExpressionEvaluator.FromCBoolMethod)
                return methodCallExpression.Arguments[0];

            return Expression.Call(PluralExpressionEvaluator.ToCBoolMethod, expression);
        }

        public static Expression Parse(string input, out ParameterExpression param)
        {
            var parser = new PluralExpressionParser(input, Expression.Parameter(typeof(int), "n"));
            Expression expression = parser.ParseExpression();
            param = parser._param;
            return expression;
        }

        private PluralExpressionTokenizer _tokenizer;
        private readonly ParameterExpression _param;
        private Token _lookahead;

        private PluralExpressionParser(string input, ParameterExpression param)
        {
            _tokenizer = new PluralExpressionTokenizer(input);
            _param = param;
            _lookahead = default;
            NextToken();
        }

        private Token NextToken()
        {
            Token token = _lookahead;
            _lookahead = _tokenizer.NextToken();
            return token;
        }

        private Token Expect(Symbol symbol)
        {
            Token token = NextToken();
            if (token.Symbol != symbol)
            {
                HandleUnexpectedToken(token);
            }
            return token;
        }

        private void HandleUnexpectedToken(Token token)
        {
            if (token.Symbol == Symbol.Eos)
            {
                throw new InvalidOperationException($"Unexpected end of input at position {token.Start}.");
            }
            else
            {
                throw new InvalidOperationException($"Unexpected token at position {token.Start}.");
            }
        }

        private Expression ParseExpression()
        {
            Expression expression = ParseConditionalExpression();

            if (_lookahead.Symbol != Symbol.Eos)
            {
                throw new InvalidOperationException($"Unexpected token at position {_lookahead.Start}.");
            }

            return expression;
        }

        // conditional_expression = logical_or_expression
        //                        | logical_or_expression OpConsequent conditional_expression OpAlternate conditional_expression
        //                        ;
        private Expression ParseConditionalExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseLogicalOrExpression();

            if (_lookahead.Symbol != Symbol.OpConsequent)
                return left;

            NextToken();
            Expression consequent = ParseConditionalExpression();

            Expect(Symbol.OpAlternate);
            Expression alternate = ParseConditionalExpression();

            return Expression.Condition(EnsureBoolean(left), consequent, alternate);
        }

        // logical_or_expression = logical_and_expression
        //                       | logical_or_expression OpLogicalOr logical_and_expression
        //                       ;
        private Expression ParseLogicalOrExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseLogicalAndExpression();

            for (; ; )
            {
                ExpressionType binaryType;
                if (_lookahead.Symbol == Symbol.OpLogicalOr)
                    binaryType = ExpressionType.OrElse;
                else
                    return left;

                NextToken();
                left = EnsureInt32(Expression.MakeBinary(binaryType, EnsureBoolean(left), EnsureBoolean(ParseLogicalAndExpression())));
            }
        }

        // logical_and_expression = equality_expression
        //                        | logical_and_expression OpLogicalAnd equality_expression
        //                        ;
        private Expression ParseLogicalAndExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseEqualityExpression();

            for (; ; )
            {
                ExpressionType binaryType;
                if (_lookahead.Symbol == Symbol.OpLogicalAnd)
                    binaryType = ExpressionType.AndAlso;
                else
                    return left;

                NextToken();
                left = EnsureInt32(Expression.MakeBinary(binaryType, EnsureBoolean(left), EnsureBoolean(ParseEqualityExpression())));
            }
        }

        // equality_expression = relational_expression
        //                     | equality_expression OpEq relational_expression
        //                     | equality_expression OpNeq relational_expression
        //                     ;
        private Expression ParseEqualityExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseRelationalExpression();

            for (; ; )
            {
                ExpressionType binaryType;
                switch (_lookahead.Symbol)
                {
                    case Symbol.OpEq:
                        binaryType = ExpressionType.Equal;
                        break;
                    case Symbol.OpNeq:
                        binaryType = ExpressionType.NotEqual;
                        break;
                    default:
                        return left;
                }

                NextToken();
                left = EnsureInt32(Expression.MakeBinary(binaryType, left, ParseRelationalExpression()));
            }
        }

        // relational_expression = additive_expression
        //                       | relational_expression OpLt additive_expression
        //                       | relational_expression OpGt additive_expression
        //                       | relational_expression OpLte additive_expression
        //                       | relational_expression OpGte additive_expression
        //                       ;
        private Expression ParseRelationalExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseAdditiveExpression();

            for (; ; )
            {
                ExpressionType binaryType;
                switch (_lookahead.Symbol)
                {
                    case Symbol.OpLt:
                        binaryType = ExpressionType.LessThan;
                        break;
                    case Symbol.OpGt:
                        binaryType = ExpressionType.GreaterThan;
                        break;
                    case Symbol.OpLte:
                        binaryType = ExpressionType.LessThanOrEqual;
                        break;
                    case Symbol.OpGte:
                        binaryType = ExpressionType.GreaterThanOrEqual;
                        break;
                    default:
                        return left;
                }

                NextToken();
                left = EnsureInt32(Expression.MakeBinary(binaryType, left, ParseAdditiveExpression()));
            }
        }

        // additive_expression = multiplicative_expression
        //                     | additive_expression OpPlus multiplicative_expression
        //                     | additive_expression OpMinus multiplicative_expression
        //                     ;
        private Expression ParseAdditiveExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseMultiplicativeExpression();

            for (; ; )
            {
                ExpressionType binaryType;
                switch (_lookahead.Symbol)
                {
                    case Symbol.OpPlus:
                        binaryType = ExpressionType.Add;
                        break;
                    case Symbol.OpMinus:
                        binaryType = ExpressionType.Subtract;
                        break;
                    default:
                        return left;
                }

                NextToken();
                left = Expression.MakeBinary(binaryType, left, ParseMultiplicativeExpression());
            }
        }

        // multiplicative_expression = unary_expression
        //                           | multiplicative_expression OpMul unary_expression
        //                           | multiplicative_expression OpDiv unary_expression
        //                           | multiplicative_expression OpRem unary_expression
        //                           ;
        private Expression ParseMultiplicativeExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression left = ParseUnaryExpression();

            for (; ; )
            {
                ExpressionType binaryType;
                switch (_lookahead.Symbol)
                {
                    case Symbol.OpMul:
                        binaryType = ExpressionType.Multiply;
                        break;
                    case Symbol.OpDiv:
                        binaryType = ExpressionType.Divide;
                        break;
                    case Symbol.OpRem:
                        binaryType = ExpressionType.Modulo;
                        break;
                    default:
                        return left;
                }

                NextToken();
                left = Expression.MakeBinary(binaryType, left, ParseUnaryExpression());
            }
        }

        // unary_expression = factor
        //                  | OpLogicalNot unary_expression
        //                  | OpPlus unary_expression
        //                  | OpMinus unary_expression
        //                  ;
        private Expression ParseUnaryExpression()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Expression operand;

            for (; ; )
            {
                switch (_lookahead.Symbol)
                {
                    case Symbol.OpLogicalNot:
                        NextToken();
                        operand = ParseUnaryExpression();
                        return EnsureInt32(Expression.MakeUnary(ExpressionType.Not, EnsureBoolean(operand), type: null));
                    case Symbol.OpPlus:
                        NextToken();
                        continue;
                    case Symbol.OpMinus:
                        NextToken();
                        operand = ParseUnaryExpression();
                        if (operand.NodeType == ExpressionType.Constant)
                        {
                            var value = (int)((ConstantExpression)operand).Value;
                            return Expression.Constant(-value, typeof(int));
                        }
                        else if (operand.NodeType == ExpressionType.Negate)
                        {
                            return ((UnaryExpression)operand).Operand;
                        }
                        else
                        {
                            return Expression.MakeUnary(ExpressionType.Negate, operand, type: null);
                        }
                    default:
                        return ParseFactor();
                }
            }
        }

        // factor = integer
        //        | variable 
        //        | open_paren conditional_expression close_paren
        //        ;
        private Expression ParseFactor()
        {
#if !NETSTANDARD1_0
            RuntimeHelpers.EnsureSufficientExecutionStack();
#endif

            Token token;
            switch (_lookahead.Symbol)
            {
                case Symbol.Integer:
                    token = NextToken();
                    var value =
#if NETSTANDARD2_1_OR_GREATER
                        int.Parse(_tokenizer.Input.AsSpan(token.Start, token.End - token.Start), NumberStyles.None, CultureInfo.InvariantCulture);
#else
                        int.Parse(_tokenizer.Input.Substring(token.Start, token.End - token.Start), NumberStyles.None, CultureInfo.InvariantCulture);
#endif
                    return Expression.Constant(value, typeof(int));

                case Symbol.Variable:
                    token = NextToken();
                    return _param;

                case Symbol.OpenParen:
                    token = NextToken();
                    Expression expression = ParseConditionalExpression();
                    Expect(Symbol.CloseParen);
                    return expression;

                default:
                    HandleUnexpectedToken(_lookahead);
                    throw new InvalidOperationException();
            }
        }
    }
}
