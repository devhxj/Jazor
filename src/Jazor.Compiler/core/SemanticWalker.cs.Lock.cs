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
/// JavaScript 没有 CLR monitor 的直接对应物，因此这里实现的是项目定义的 lock/runtime seam。
/// 不能把 lock 直接擦除为普通代码块，否则会静默丢失并发或互斥协议的可观察约束。
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
        return statements.Count == 1
            ? statements[0]
            : new NestedBlockStatement(NodeList.From(statements));
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
        statements.Add(new TryStatement(
            new NestedBlockStatement(NodeList.From(bodyStatements)),
            handler: null,
            finalizer: null));
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
