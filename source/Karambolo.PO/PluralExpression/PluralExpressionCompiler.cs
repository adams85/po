using System;
using System.Globalization;
using System.Linq.Expressions;
using Hime.Redist;

namespace Karambolo.PO.PluralExpression
{
    internal sealed class PluralExpressionCompiler
    {
        private static Expression FromCBool(Expression expression)
        {
            return
                expression.Type == typeof(int) ?
                Expression.NotEqual(expression, Expression.Constant(0)) :
                expression;
        }

        private static Expression ToCBool(Expression expression)
        {
            return
                expression.Type == typeof(bool) ?
                Expression.Condition(expression, Expression.Constant(1), Expression.Constant(0)) :
                expression;
        }

        private readonly ASTNode _syntaxTree;
        private ParameterExpression _param;

        public PluralExpressionCompiler(ASTNode syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        private Expression VisitVariable(ASTNode node)
        {
            return _param;
        }

        private Expression VisitInteger(ASTNode node)
        {
            return Expression.Constant(int.Parse(node.Value, CultureInfo.InvariantCulture));
        }

        private Expression VisitMultiplication(ASTNode node)
        {
            Expression left = Visit(node.Children[0]);
            Expression right = Visit(node.Children[2]);

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

        private Expression VisitAddition(ASTNode node)
        {
            Expression left = Visit(node.Children[0]);
            Expression right = Visit(node.Children[2]);

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

        private Expression VisitRelation(ASTNode node)
        {
            Expression left = Visit(node.Children[0]);
            Expression right = Visit(node.Children[2]);

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

        private Expression VisitEquality(ASTNode node)
        {
            Expression left = Visit(node.Children[0]);
            Expression right = Visit(node.Children[2]);

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

        private Expression VisitLogicalAnd(ASTNode node)
        {
            Expression left = FromCBool(Visit(node.Children[0]));
            Expression right = FromCBool(Visit(node.Children[1]));

            return ToCBool(Expression.AndAlso(left, right));
        }

        private Expression VisitLogicalOr(ASTNode node)
        {
            Expression left = FromCBool(Visit(node.Children[0]));
            Expression right = FromCBool(Visit(node.Children[1]));

            return ToCBool(Expression.OrElse(left, right));
        }

        private Expression VisitCondition(ASTNode node)
        {
            Expression test = FromCBool(Visit(node.Children[0]));
            Expression ifTrue = Visit(node.Children[1]);
            Expression ifFalse = Visit(node.Children[2]);

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        private Expression Visit(ASTNode node)
        {
            switch (node.Symbol.ID)
            {
                case PluralExpressionLexer.ID.TerminalVariable:
                    return VisitVariable(node);
                case PluralExpressionLexer.ID.TerminalInteger:
                    return VisitInteger(node);
                case PluralExpressionParser.ID.VariableMultiplicativeExpression:
                    return VisitMultiplication(node);
                case PluralExpressionParser.ID.VariableAdditiveExpression:
                    return VisitAddition(node);
                case PluralExpressionParser.ID.VariableRelationalExpression:
                    return VisitRelation(node);
                case PluralExpressionParser.ID.VariableEqualityExpression:
                    return VisitEquality(node);
                case PluralExpressionParser.ID.VariableLogicalAndExpression:
                    return VisitLogicalAnd(node);
                case PluralExpressionParser.ID.VariableLogicalOrExpression:
                    return VisitLogicalOr(node);
                case PluralExpressionParser.ID.VariableExpression:
                    return VisitCondition(node);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Func<int, int> Compile()
        {
            _param = Expression.Parameter(typeof(int), "n");

            Expression expression = Visit(_syntaxTree);

            var lambda = Expression.Lambda<Func<int, int>>(expression, _param);
            return lambda.Compile();
        }
    }
}
