using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Karambolo.PO.PluralExpressions
{
    internal readonly struct PluralExpressionEvaluator
    {
        internal static readonly MethodInfo FromCBoolMethod = new Func<int, bool>(FromCBool).GetMethodInfo();
        internal static readonly MethodInfo ToCBoolMethod = new Func<bool, int>(ToCBool).GetMethodInfo();

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool FromCBool(int value)
        {
            return value != 0;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static int ToCBool(bool value)
        {
            return value ? 1 : 0;
        }

        internal static Func<int, int> BuildInterpreter(Expression expression, ParameterExpression _)
        {
            return n => new PluralExpressionEvaluator(n).Evaluate(expression);
        }

        internal static Func<int, int> Compile(Expression expression, ParameterExpression param)
        {
            var lambdaExpression = Expression.Lambda<Func<int, int>>(expression, param);
            return lambdaExpression.Compile();
        }

        public static readonly Func<Expression, ParameterExpression, Func<int, int>> From =
#if NETSTANDARD2_1_OR_GREATER
            RuntimeFeature.IsDynamicCodeSupported && RuntimeFeature.IsDynamicCodeCompiled ?
            Compile :
            new Func<Expression, ParameterExpression, Func<int, int>>(BuildInterpreter);
#else
            (expression, param) =>
            {
                try { return Compile(expression, param); }
                catch { return BuildInterpreter(expression, param); }
            };
#endif

        private readonly int _n;

        private PluralExpressionEvaluator(int n)
        {
            _n = n;
        }

        public int Evaluate(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Parameter:
                    return _n;
                case ExpressionType.Constant:
                    return (int)((ConstantExpression)node).Value;
                case ExpressionType.Negate:
                    return -Evaluate(((UnaryExpression)node).Operand);
                case ExpressionType.Multiply:
                    return Evaluate(((BinaryExpression)node).Left) * Evaluate(((BinaryExpression)node).Right);
                case ExpressionType.Divide:
                    return Evaluate(((BinaryExpression)node).Left) / Evaluate(((BinaryExpression)node).Right);
                case ExpressionType.Modulo:
                    return Evaluate(((BinaryExpression)node).Left) % Evaluate(((BinaryExpression)node).Right);
                case ExpressionType.Add:
                    return Evaluate(((BinaryExpression)node).Left) + Evaluate(((BinaryExpression)node).Right);
                case ExpressionType.Subtract:
                    return Evaluate(((BinaryExpression)node).Left) - Evaluate(((BinaryExpression)node).Right);
                case ExpressionType.LessThan:
                    return ToCBool(Evaluate(((BinaryExpression)node).Left) < Evaluate(((BinaryExpression)node).Right));
                case ExpressionType.GreaterThan:
                    return ToCBool(Evaluate(((BinaryExpression)node).Left) > Evaluate(((BinaryExpression)node).Right));
                case ExpressionType.LessThanOrEqual:
                    return ToCBool(Evaluate(((BinaryExpression)node).Left) <= Evaluate(((BinaryExpression)node).Right));
                case ExpressionType.GreaterThanOrEqual:
                    return ToCBool(Evaluate(((BinaryExpression)node).Left) >= Evaluate(((BinaryExpression)node).Right));
                case ExpressionType.Equal:
                    return ToCBool(Evaluate(((BinaryExpression)node).Left) == Evaluate(((BinaryExpression)node).Right));
                case ExpressionType.NotEqual:
                    return ToCBool(Evaluate(((BinaryExpression)node).Left) != Evaluate(((BinaryExpression)node).Right));
                case ExpressionType.Not:
                    return ToCBool(!FromCBool(Evaluate(((UnaryExpression)node).Operand)));
                case ExpressionType.AndAlso:
                    return ToCBool(FromCBool(Evaluate(((BinaryExpression)node).Left)) && FromCBool(Evaluate(((BinaryExpression)node).Right)));
                case ExpressionType.OrElse:
                    return ToCBool(FromCBool(Evaluate(((BinaryExpression)node).Left)) || FromCBool(Evaluate(((BinaryExpression)node).Right)));
                case ExpressionType.Conditional:
                    return
                        FromCBool(Evaluate(((ConditionalExpression)node).Test)) ?
                        Evaluate(((ConditionalExpression)node).IfTrue) :
                        Evaluate(((ConditionalExpression)node).IfFalse);
                case ExpressionType.Call:
                    return Evaluate(((MethodCallExpression)node).Arguments[0]);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
