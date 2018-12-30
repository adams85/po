using System;
using System.Linq.Expressions;
using Hime.Redist;

namespace Karambolo.PO.PluralExpression
{
    class PluralExpressionCompiler
    {
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

        Expression VisitVariable(ASTNode node)
        {
            return _param;
        }

        Expression VisitInteger(ASTNode node)
        {
            return Expression.Constant(int.Parse(node.Value));
        }

        Expression VisitMultiplication(ASTNode node)
        {
            var left = Visit(node.Children[0]);
            var right = Visit(node.Children[2]);

            switch (node.Children[1].Symbol.Name)
            {
                case "*":
                    return Expression.Multiply(left, right);
                case "/":
                    return Expression.Divide(left, right);
                case "%":
                    return Expression.Modulo(left, right);
                default:
                    throw new InvalidOperationException();
            }
        }

        Expression VisitAddition(ASTNode node)
        {
            var left = Visit(node.Children[0]);
            var right = Visit(node.Children[2]);

            switch (node.Children[1].Symbol.Name)
            {
                case "+":
                    return Expression.Add(left, right);
                case "-":
                    return Expression.Subtract(left, right);
                default:
                    throw new InvalidOperationException();
            }
        }

        Expression VisitRelation(ASTNode node)
        {
            var left = Visit(node.Children[0]);
            var right = Visit(node.Children[2]);

            switch (node.Children[1].Symbol.Name)
            {
                case "<":
                    return ToCBool(Expression.LessThan(left, right));
                case ">":
                    return ToCBool(Expression.GreaterThan(left, right));
                case "<=":
                    return ToCBool(Expression.LessThanOrEqual(left, right));
                case ">=":
                    return ToCBool(Expression.GreaterThanOrEqual(left, right));
                default:
                    throw new InvalidOperationException();
            }
        }

        Expression VisitEquality(ASTNode node)
        {
            var left = Visit(node.Children[0]);
            var right = Visit(node.Children[2]);

            switch (node.Children[1].Symbol.Name)
            {
                case "==":
                    return ToCBool(Expression.Equal(left, right));
                case "!=":
                    return ToCBool(Expression.NotEqual(left, right));
                default:
                    throw new InvalidOperationException();
            }
        }

        Expression VisitLogicalAnd(ASTNode node)
        {
            var left = FromCBool(Visit(node.Children[0]));
            var right = FromCBool(Visit(node.Children[1]));

            return ToCBool(Expression.AndAlso(left, right));
        }

        Expression VisitLogicalOr(ASTNode node)
        {
            var left = FromCBool(Visit(node.Children[0]));
            var right = FromCBool(Visit(node.Children[1]));

            return ToCBool(Expression.OrElse(left, right));
        }

        Expression VisitCondition(ASTNode node)
        {
            var test = FromCBool(Visit(node.Children[0]));
            var ifTrue = Visit(node.Children[1]);
            var ifFalse = Visit(node.Children[2]);

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        Expression Visit(ASTNode node)
        {
            switch (node.Symbol.ID)
            {
                case PluralExpressionLexer.ID.VARIABLE:
                    return VisitVariable(node);
                case PluralExpressionLexer.ID.INTEGER:
                    return VisitInteger(node);
                case PluralExpressionParser.ID.multiplicative_expression:
                    return VisitMultiplication(node);
                case PluralExpressionParser.ID.additive_expression:
                    return VisitAddition(node);
                case PluralExpressionParser.ID.relational_expression:
                    return VisitRelation(node);
                case PluralExpressionParser.ID.equality_expression:
                    return VisitEquality(node);
                case PluralExpressionParser.ID.logical_and_expression:
                    return VisitLogicalAnd(node);
                case PluralExpressionParser.ID.logical_or_expression:
                    return VisitLogicalOr(node);
                case PluralExpressionParser.ID.expression:
                    return VisitCondition(node);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Func<int, int> Compile()
        {
            _param = Expression.Parameter(typeof(int), "n");

            var expression = Visit(_syntaxTree);

            var lambda = Expression.Lambda<Func<int, int>>(expression, _param);
            return lambda.Compile();
        }
    }
}
