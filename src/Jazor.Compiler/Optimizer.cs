using System.Collections.Generic;
using System.Linq;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

public static class Optimizer
{
    /// <summary>
    /// 优化同一层级逻辑运算中的子表达式冗余问题
    /// 注意：此优化器主要用于纯表达式，对于包含函数调用、赋值等副作用的表达式，不会进行优化
    /// </summary>
    /// <param name="expression">要优化的表达式</param>
    /// <returns>优化后的表达式</returns>
    public static Expression OptimizeLogical(Expression expression)
    {
        // 如果不是逻辑表达式，直接返回（叶子节点或非目标类型）
        if (expression is not LogicalExpression logical)
            return expression;

        // 递归优化子节点
        var leftOptimized = OptimizeLogical(logical.Left);
        var rightOptimized = OptimizeLogical(logical.Right);

        var op = logical.Operator;

        // 检查是否有副作用，如果有则不进行去重优化
        // 但仍然需要用优化后的子节点重建表达式
        if (IsEffect(leftOptimized) || IsEffect(rightOptimized))
        {
            // 如果子节点有变化，需要重建表达式
            if (leftOptimized != logical.Left || rightOptimized != logical.Right)
                return new LogicalExpression(op, leftOptimized, rightOptimized);
            return expression;
        }

        // 收集所有同级操作数
        var operands = new List<Expression>();
        Flatten(leftOptimized, op, operands);
        Flatten(rightOptimized, op, operands);

        // 使用 AST 结构比较去重，List 保持首次出现顺序。
        var seen = new HashSet<Expression>(PureExpressionComparer.Instance);
        var uniques = new List<Expression>();

        foreach (var operand in operands)
        {
            if (seen.Add(operand))
                uniques.Add(operand);
        }

        // 如果所有操作数都唯一，重建
        if (uniques.Count == operands.Count)
            return new LogicalExpression(op, leftOptimized, rightOptimized);
        
        // 如果只有一个唯一操作数，直接返回该节点 (A && A -> A)
        if (uniques.Count == 1)
            return uniques[0];

        // 重建树结构（左结合）
        Expression result = uniques[0];
        for (int i = 1; i < uniques.Count; i++)
            result = new LogicalExpression(op, result, uniques[i]);
        return result;

        // 扁平化收集操作数，如果遇到相同运算符的 LogicalExpression，则递归展开
        static void Flatten(Expression e, Operator op, List<Expression> operands)
        {
            if (e is LogicalExpression le && le.Operator == op)
            {
                Flatten(le.Left, op, operands);
                Flatten(le.Right, op, operands);
            }
            else
                operands.Add(e);
        }

        // 检测表达式是否包含副作用
        // 只判断明确无副作用的类型，其他默认有副作用
        static bool IsEffect(Expression e)
        {
            return e switch
            {
                // 无副作用 - 纯值
                Identifier => false,
                Literal => false,
                ThisExpression => false,
                Super => false,

                // 需要递归检查子节点
                LogicalExpression le => IsEffect(le.Left) || IsEffect(le.Right),
                NonLogicalBinaryExpression be => !IsPureBinaryOperator(be) || IsEffect(be.Left) || IsEffect(be.Right),
                NonUpdateUnaryExpression ue => !IsPureUnaryOperator(ue.Operator) || IsEffect(ue.Argument),
                ConditionalExpression ce => IsEffect(ce.Test) || IsEffect(ce.Consequent) || IsEffect(ce.Alternate),
                SequenceExpression se => se.Expressions.Any(IsEffect),

                // 其他类型默认有副作用
                _ => true
            };
        }

        static bool IsPureBinaryOperator(NonLogicalBinaryExpression expression)
        {
            if (expression.Operator is Operator.StrictEquality or Operator.StrictInequality)
                return true;

            if (expression.Operator is not (Operator.Equality or Operator.Inequality))
                return false;

            return expression.Left is NullLiteral || expression.Right is NullLiteral;
        }

        static bool IsPureUnaryOperator(Operator op)
            => op is Operator.LogicalNot or Operator.TypeOf or Operator.Void;
    }

    private sealed class PureExpressionComparer : IEqualityComparer<Expression>
    {
        public static PureExpressionComparer Instance { get; } = new();

        public bool Equals(Expression? left, Expression? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null || left.GetType() != right.GetType())
                return false;

            return (left, right) switch
            {
                (Identifier x, Identifier y) => string.Equals(x.Name, y.Name, System.StringComparison.Ordinal),
                (NullLiteral, NullLiteral) => true,
                (BooleanLiteral x, BooleanLiteral y) => x.Value == y.Value,
                (NumericLiteral x, NumericLiteral y) => x.Value.Equals(y.Value),
                (BigIntLiteral x, BigIntLiteral y) => x.Value.Equals(y.Value),
                (StringLiteral x, StringLiteral y) => string.Equals(x.Value, y.Value, System.StringComparison.Ordinal),
                (ThisExpression, ThisExpression) => true,
                (Super, Super) => true,
                (LogicalExpression x, LogicalExpression y) =>
                    x.Operator == y.Operator && Equals(x.Left, y.Left) && Equals(x.Right, y.Right),
                (NonLogicalBinaryExpression x, NonLogicalBinaryExpression y) =>
                    x.Operator == y.Operator && Equals(x.Left, y.Left) && Equals(x.Right, y.Right),
                (NonUpdateUnaryExpression x, NonUpdateUnaryExpression y) =>
                    x.Operator == y.Operator && Equals(x.Argument, y.Argument),
                (ConditionalExpression x, ConditionalExpression y) =>
                    Equals(x.Test, y.Test) && Equals(x.Consequent, y.Consequent) && Equals(x.Alternate, y.Alternate),
                (SequenceExpression x, SequenceExpression y) => SequenceEquals(x.Expressions, y.Expressions),
                _ => false
            };
        }

        public int GetHashCode(Expression expression)
        {
            var hash = expression.GetType().GetHashCode();
            return expression switch
            {
                Identifier identifier => Combine(hash, System.StringComparer.Ordinal.GetHashCode(identifier.Name)),
                NullLiteral => hash,
                BooleanLiteral literal => Combine(hash, literal.Value.GetHashCode()),
                NumericLiteral literal => Combine(hash, literal.Value.GetHashCode()),
                BigIntLiteral literal => Combine(hash, literal.Value.GetHashCode()),
                StringLiteral literal => Combine(hash, System.StringComparer.Ordinal.GetHashCode(literal.Value)),
                ThisExpression => hash,
                Super => hash,
                LogicalExpression logical => Combine(hash, (int)logical.Operator, GetHashCode(logical.Left), GetHashCode(logical.Right)),
                NonLogicalBinaryExpression binary => Combine(hash, (int)binary.Operator, GetHashCode(binary.Left), GetHashCode(binary.Right)),
                NonUpdateUnaryExpression unary => Combine(hash, (int)unary.Operator, GetHashCode(unary.Argument)),
                ConditionalExpression conditional => Combine(hash, GetHashCode(conditional.Test), GetHashCode(conditional.Consequent), GetHashCode(conditional.Alternate)),
                SequenceExpression sequence => CombineSequence(hash, sequence.Expressions),
                _ => hash
            };
        }

        private bool SequenceEquals(NodeList<Expression> left, NodeList<Expression> right)
        {
            if (left.Count != right.Count)
                return false;

            for (var index = 0; index < left.Count; index++)
            {
                if (!Equals(left[index], right[index]))
                    return false;
            }

            return true;
        }

        private int CombineSequence(int hash, NodeList<Expression> expressions)
        {
            foreach (var expression in expressions)
                hash = Combine(hash, GetHashCode(expression));
            return hash;
        }

        private static int Combine(int first, int second)
            => unchecked((first * 397) ^ second);

        private static int Combine(int first, int second, int third)
            => Combine(Combine(first, second), third);

        private static int Combine(int first, int second, int third, int fourth)
            => Combine(Combine(Combine(first, second), third), fourth);
    }
}
