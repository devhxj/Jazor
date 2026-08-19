// File: SemanticWalker.cs.Lock.cs
// Purpose: Defines the supported lowering boundary for C# lock operations.
// JavaScript 没有 CLR monitor；本文件只实现已设计的 protocol，并对超出边界的同步语义明确拒绝。
using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// 处理 lock operation 的宿主协议转换。
/// </summary>
/// <remarks>
/// Jazor 的 supported host executes synchronous JavaScript on one event-loop turn. C# forbids
/// <c>await</c> in a lock body, so retaining a lexical block after the null check preserves the
/// non-interleaving execution contract without inventing a Monitor runtime object. Do not emit a
/// bare <c>try</c>: JavaScript requires a catch or finally clause and lock has neither here.
/// </remarks>
public partial class SemanticWalker
{
    public override Node? VisitLock(ILockOperation operation, SenseArgument argument)
    {
        var scopedArgument = EnsureScopeContext(operation, argument);
        var bodyContext = scopedArgument.EnterScope(operation.Body, ScopeSite.NestedBlock());
        var bodyStatements = operation.Body is IBlockOperation blockOperation
            ? MaterializeScopedStatements(bodyContext, TranslateOperationsToStatements(blockOperation.Operations, bodyContext))
            : MaterializeScopedStatements(bodyContext, TranslateOperationsToStatements([operation.Body], bodyContext));

        var statements = LowerLockStatements(operation, bodyStatements, scopedArgument);
        // LowerLockStatements always emits the null guard and the locked body, so a lock
        // operation cannot collapse to a single statement. Keep the result shape explicit.
        return new NestedBlockStatement(NodeList.From(statements));
    }

    private List<Statement> LowerLockStatements(
        ILockOperation operation,
        List<Statement> bodyStatements,
        SenseArgument context)
    {
        var prefixStatements = new List<Statement>();
        var lockedValue = MaterializeLockExpression(operation.LockedValue, context, prefixStatements);
        var nullGuard = new IfStatement(
            new NonLogicalBinaryExpression(Operator.Equality, lockedValue, Null),
            BuildArgumentNullTypeErrorThrowStatement("obj"),
            null);

        var statements = new List<Statement>(prefixStatements.Count + 2);
        if (prefixStatements.Count > 0)
            statements.AddRange(prefixStatements);

        statements.Add(nullGuard);
        statements.Add(new NestedBlockStatement(NodeList.From(bodyStatements)));
        return statements;
    }

    private Expression MaterializeLockExpression(
        IOperation lockedValueOperation,
        SenseArgument context,
        List<Statement> prefixStatements)
    {
        var expression = Translate<Expression>(lockedValueOperation, context);
        if (CanReuseLockExpression(expression))
            return expression;

        var tempIdentifier = new Identifier(
            AllocateUniqueName(
                lockedValueOperation,
                context,
                LoweringSite.LockValueTemp("value")));
        prefixStatements.Add(new VariableDeclaration(
            VariableDeclarationKind.Const,
            NodeList.From(new VariableDeclarator(tempIdentifier, expression))));
        return tempIdentifier;
    }

    private static bool CanReuseLockExpression(Expression expression)
        => expression is Identifier or ThisExpression or Super;
}
