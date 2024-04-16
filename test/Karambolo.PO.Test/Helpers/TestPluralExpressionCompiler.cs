using System;
using System.Globalization;
using System.Linq.Expressions;
using Hime.Redist;
using Karambolo.PO.PluralExpressions;

namespace Karambolo.PO.Test.Helpers
{
    internal sealed class TestPluralExpressionCompiler
    {
        private readonly ASTNode _syntaxTree;
        private ParameterExpression _param;

        public TestPluralExpressionCompiler(ASTNode syntaxTree)
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

        private Expression VisitUnary(ASTNode node)
        {
            Expression operand = Visit(node.Children[1]);

            switch (node.Children[0].Symbol.Name)
            {
                case "!":
                    return PluralExpressionParser.EnsureInt32(Expression.Not(PluralExpressionParser.EnsureBoolean(operand)));
                case "+":
                    return operand;
                case "-":
                    if (operand.NodeType == ExpressionType.Constant)
                    {
                        var value = (int)((ConstantExpression)operand).Value;
                        return Expression.Constant(-value);
                    }
                    else
                    {
                        return Expression.Negate(operand);
                    }
                default:
                    throw new InvalidOperationException();
            }
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
                    return PluralExpressionParser.EnsureInt32(Expression.LessThan(left, right));
                case ">":
                    return PluralExpressionParser.EnsureInt32(Expression.GreaterThan(left, right));
                case "<=":
                    return PluralExpressionParser.EnsureInt32(Expression.LessThanOrEqual(left, right));
                case ">=":
                    return PluralExpressionParser.EnsureInt32(Expression.GreaterThanOrEqual(left, right));
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
                    return PluralExpressionParser.EnsureInt32(Expression.Equal(left, right));
                case "!=":
                    return PluralExpressionParser.EnsureInt32(Expression.NotEqual(left, right));
                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression VisitLogicalAnd(ASTNode node)
        {
            Expression left = PluralExpressionParser.EnsureBoolean(Visit(node.Children[0]));
            Expression right = PluralExpressionParser.EnsureBoolean(Visit(node.Children[1]));

            return PluralExpressionParser.EnsureInt32(Expression.AndAlso(left, right));
        }

        private Expression VisitLogicalOr(ASTNode node)
        {
            Expression left = PluralExpressionParser.EnsureBoolean(Visit(node.Children[0]));
            Expression right = PluralExpressionParser.EnsureBoolean(Visit(node.Children[1]));

            return PluralExpressionParser.EnsureInt32(Expression.OrElse(left, right));
        }

        private Expression VisitCondition(ASTNode node)
        {
            Expression test = PluralExpressionParser.EnsureBoolean(Visit(node.Children[0]));
            Expression ifTrue = Visit(node.Children[1]);
            Expression ifFalse = Visit(node.Children[2]);

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        private Expression Visit(ASTNode node)
        {
            switch (node.Symbol.ID)
            {
                case TestPluralExpressionLexer.ID.TerminalVariable:
                    return VisitVariable(node);
                case TestPluralExpressionLexer.ID.TerminalInteger:
                    return VisitInteger(node);
                case TestPluralExpressionParser.ID.VariableUnaryExpression:
                    return VisitUnary(node);
                case TestPluralExpressionParser.ID.VariableMultiplicativeExpression:
                    return VisitMultiplication(node);
                case TestPluralExpressionParser.ID.VariableAdditiveExpression:
                    return VisitAddition(node);
                case TestPluralExpressionParser.ID.VariableRelationalExpression:
                    return VisitRelation(node);
                case TestPluralExpressionParser.ID.VariableEqualityExpression:
                    return VisitEquality(node);
                case TestPluralExpressionParser.ID.VariableLogicalAndExpression:
                    return VisitLogicalAnd(node);
                case TestPluralExpressionParser.ID.VariableLogicalOrExpression:
                    return VisitLogicalOr(node);
                case TestPluralExpressionParser.ID.VariableExpression:
                    return VisitCondition(node);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Expression Visit()
        {
            _param = Expression.Parameter(typeof(int), "n");
            return Visit(_syntaxTree);
        }

        public Func<int, int> Compile()
        {
            Expression expression = Visit();
            var lambda = Expression.Lambda<Func<int, int>>(expression, _param);
            return lambda.Compile();
        }
    }
}
