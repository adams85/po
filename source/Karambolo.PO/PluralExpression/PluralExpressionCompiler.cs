using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hime.Redist;

namespace Karambolo.PO.PluralExpression
{
    class PluralExpressionCompiler
    {
        class VisitData
        {
            public static VisitData From(ASTNode node)
            {
                return new VisitData { Node = node };
            }

            public ASTNode Node;
            public Expression Expression;
            public VisitData[] Children;
        }

        static Expression FromCBool(Expression expression)
        {
            return
                expression.Type == typeof(int) ?
                expression = Expression.NotEqual(expression, Expression.Constant(0)) :
                expression;
        }

        static Expression ToCBool(Expression expression)
        {
            return
                expression.Type == typeof(bool) ?
                expression = Expression.Condition(expression, Expression.Constant(1), Expression.Constant(0)) :
                expression;
        }

        readonly ASTNode _syntaxTree;

        ParameterExpression _param;

        public PluralExpressionCompiler(ASTNode syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        void VisitVariable(VisitData data)
        {
            data.Expression = _param;
        }

        void VisitInteger(VisitData data)
        {
            data.Expression = Expression.Constant(int.Parse(data.Node.Value));
        }

        void VisitMultiplication(VisitData data)
        {
            var left = data.Children[0].Expression;
            var right = data.Children[2].Expression;

            switch (data.Children[1].Node.Symbol.Name)
            {
                case "*":
                    data.Expression = Expression.Multiply(left, right);
                    return;
                case "/":
                    data.Expression = Expression.Divide(left, right);
                    return;
                case "%":
                    data.Expression = Expression.Modulo(left, right);
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        void VisitAddition(VisitData data)
        {
            var left = data.Children[0].Expression;
            var right = data.Children[2].Expression;

            switch (data.Children[1].Node.Symbol.Name)
            {
                case "+":
                    data.Expression = Expression.Add(left, right);
                    return;
                case "-":
                    data.Expression = Expression.Subtract(left, right);
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        void VisitRelation(VisitData data)
        {
            var left = data.Children[0].Expression;
            var right = data.Children[2].Expression;

            switch (data.Children[1].Node.Symbol.Name)
            {
                case "<":
                    data.Expression = ToCBool(Expression.LessThan(left, right));
                    return;
                case ">":
                    data.Expression = ToCBool(Expression.GreaterThan(left, right));
                    return;
                case "<=":
                    data.Expression = ToCBool(Expression.LessThanOrEqual(left, right));
                    return;
                case ">=":
                    data.Expression = ToCBool(Expression.GreaterThanOrEqual(left, right));
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        void VisitEquality(VisitData data)
        {
            var left = data.Children[0].Expression;
            var right = data.Children[2].Expression;

            switch (data.Children[1].Node.Symbol.Name)
            {
                case "==":
                    data.Expression = ToCBool(Expression.Equal(left, right));
                    return;
                case "!=":
                    data.Expression = ToCBool(Expression.NotEqual(left, right));
                    return;
                default:
                    throw new InvalidOperationException();
            }
        }

        void VisitLogicalAnd(VisitData data)
        {
            var left = FromCBool(data.Children[0].Expression);
            var right = FromCBool(data.Children[1].Expression);

            data.Expression = ToCBool(Expression.AndAlso(left, right));
        }

        void VisitLogicalOr(VisitData data)
        {
            var left = FromCBool(data.Children[0].Expression);
            var right = FromCBool(data.Children[2].Expression);

            data.Expression = ToCBool(Expression.OrElse(left, right));
        }

        void VisitCondition(VisitData data)
        {
            var test = FromCBool(data.Children[0].Expression);
            var ifTrue = data.Children[1].Expression;
            var ifFalse = data.Children[2].Expression;

            data.Expression = Expression.Condition(test, ifTrue, ifFalse);
        }

        void Visit(VisitData data)
        {
            switch (data.Node.Symbol.ID)
            {
                case PluralExpressionLexer.ID.VARIABLE:
                    VisitVariable(data);
                    return;
                case PluralExpressionLexer.ID.INTEGER:
                    VisitInteger(data);
                    return;
                case PluralExpressionParser.ID.multiplicative_expression:
                    VisitMultiplication(data);
                    return;
                case PluralExpressionParser.ID.additive_expression:
                    VisitAddition(data);
                    return;
                case PluralExpressionParser.ID.relational_expression:
                    VisitRelation(data);
                    return;
                case PluralExpressionParser.ID.equality_expression:
                    VisitEquality(data);
                    return;
                case PluralExpressionParser.ID.logical_and_expression:
                    VisitLogicalAnd(data);
                    return;
                case PluralExpressionParser.ID.logical_or_expression:
                    VisitLogicalOr(data);
                    return;
                case PluralExpressionParser.ID.expression:
                    VisitCondition(data);
                    return;
            }
        }

        public Func<int, int> Compile()
        {
            _param = Expression.Parameter(typeof(int), "n");

            var stack = new Stack<VisitData>();
            var visited = new Stack<VisitData>();

            var root = VisitData.From(_syntaxTree);
            stack.Push(root);

            while (stack.Count > 0)
            {
                var data = stack.Peek();

                if (visited.Count == 0 || !ReferenceEquals(visited.Peek(), data))
                {
                    var childNodes = data.Node.Children;
                    var n = childNodes.Count;
                    if (n > 0)
                    {
                        data.Children = new VisitData[n];

                        visited.Push(data);
                        stack.Push(data.Children[--n] = VisitData.From(childNodes[n]));

                        while (n > 0)
                            stack.Push(data.Children[--n] = VisitData.From(childNodes[n]));

                        continue;
                    }
                }
                else
                    visited.Pop();

                stack.Pop();
                Visit(data);
            }

            if (root.Expression == null)
                return null;

            var lambda = Expression.Lambda<Func<int, int>>(root.Expression, _param);
            return lambda.Compile();
        }
    }
}
