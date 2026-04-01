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

        // 使用 HashSet 进行去重判断，List 保持顺序
        // 这样可以明确保证操作数的原始顺序
        var seen = new HashSet<string>();
        var uniques = new List<Expression>();

        foreach (var operand in operands)
        {
            var code = operand.ToKnRECMAScript();
            if (seen.Add(code)) // Add 返回 true 表示是新元素
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

                // 无副作用 - 函数定义（不执行）
                FunctionExpression => false,
                ArrowFunctionExpression => false,
                ClassExpression => false,

                // 需要递归检查子节点
                LogicalExpression le => IsEffect(le.Left) || IsEffect(le.Right),
                NonLogicalBinaryExpression be => IsEffect(be.Left) || IsEffect(be.Right),
                NonUpdateUnaryExpression ue => IsEffect(ue.Argument),
                ConditionalExpression ce => IsEffect(ce.Test) || IsEffect(ce.Consequent) || IsEffect(ce.Alternate),
                MemberExpression me => me.Computed || IsEffect(me.Object) || (me.Property is Expression property && IsEffect(property)),
                SequenceExpression se => se.Expressions.Any(IsEffect),
                ArrayExpression ae => ae.Elements.Any(el => el is Expression expr && IsEffect(expr)),
                ObjectExpression oe => oe.Properties.Any(p => p is Property prop && (IsEffect(prop.Key) || (prop.Value is Expression v && IsEffect(v)))),
                TemplateLiteral tl => tl.Expressions.Any(IsEffect),

                // 其他类型默认有副作用
                _ => true
            };
        }
    }
}
