// File: SemanticWalkerHost.cs
// Purpose: Defines optional product extension points around core SemanticWalker lowering.
// host 仅处理明确拥有的 operation rewrite；未声明的场景仍回到标准编译器路径或显式失败。
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Optional host seam for consumers that need to project selected Roslyn operations
/// into a different runtime surface while still reusing SemanticWalker for the rest
/// of the expression tree.
/// </summary>
/// <remarks>
/// 这是宿主语义的窄扩展点，不是第二个 compiler。Host 只能重写明确属于宿主的 operation，
/// 普通 C# 表达式仍由 SemanticWalker 负责；若返回 null，应继续走标准 compiler 路径。
/// </remarks>
public abstract class SemanticWalkerHost
{
    public virtual Expression? RewriteConversionPreorder(IConversionOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        => null;

    public virtual bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
        => false;

    public virtual Expression? RewriteObjectCreation(
        IObjectCreationOperation operation,
        SenseArgument argument,
        IReadOnlyList<Expression> arguments)
        => null;

    public virtual void ObserveTypeReference(ITypeSymbol type, SenseArgument argument)
    {
    }

    public virtual VariableDeclarator? RewriteVariableDeclaratorPreorder(IVariableDeclaratorOperation operation, SenseArgument argument)
        => null;

    public virtual bool ShouldSkipVariableDeclarator(IVariableDeclaratorOperation operation, SenseArgument argument)
        => false;

    public virtual Identifier? RewriteLocalDeclarationIdentifier(ILocalSymbol local, IOperation operation, SenseArgument argument)
        => null;

    public virtual Identifier? RewriteCatchClauseParameterIdentifier(ICatchClauseOperation operation, ILocalSymbol local, SenseArgument argument)
        => null;

    public virtual Expression? RewriteSimpleAssignmentPreorder(ISimpleAssignmentOperation operation, SenseArgument argument)
        => null;

    /// <summary>
    /// Rewrites a host-owned simple assignment after the right-hand value has been lowered once,
    /// but before the compiler selects the ordinary assignment target protocol.
    /// </summary>
    /// <remarks>
    /// This is intentionally post-value-lowering so a host can project storage without bypassing
    /// normal expression semantics or evaluating the value more than once.
    /// </remarks>
    public virtual Expression? RewriteSimpleAssignmentPostorder(
        ISimpleAssignmentOperation operation,
        SenseArgument argument,
        Expression value)
        => null;

    public virtual Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocationArgumentPreorder(
        IInvocationOperation operation,
        IArgumentOperation argumentOperation,
        int argumentIndex,
        SenseArgument argument)
        => null;

    public virtual bool ShouldSkipLocalFunctionDeclaration(ILocalFunctionOperation operation, SenseArgument argument)
        => false;

    public virtual Expression? RewriteFieldReference(
        IFieldReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => null;

    public virtual Expression? RewritePropertyReference(
        IPropertyReferenceOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => null;

    public virtual Expression? RewriteMethodReference(
        IMethodReferenceOperation operation,
        SenseArgument argument,
        Expression? instance)
        => null;

    public virtual Expression? RewriteMethodReferencePreorder(IMethodReferenceOperation operation, SenseArgument argument)
        => null;

    public virtual Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
        => null;

    /// <summary>
    /// Rewrites a host-owned invocation intrinsic after ordinary invocation operands have been
    /// lowered and before compiler-owned intrinsics and whitelist dispatch are attempted.
    /// </summary>
    public virtual Expression? RewriteInvocationIntrinsic(
        IInvocationOperation operation,
        Expression? instance,
        IReadOnlyList<Expression> arguments,
        SemanticInvocationLoweringContext context)
        => null;

    public virtual Expression? RewriteInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
        => null;

    /// <summary>
    /// Rewrites a host-owned event subscription before the CLR field-like event protocol is
    /// validated. This is intentionally narrow: external events still fail through the normal
    /// compiler boundary unless a host explicitly owns their runtime contract.
    /// </summary>
    public virtual Expression? RewriteEventAssignment(
        IEventAssignmentOperation operation,
        SenseArgument argument)
        => null;

    public virtual Expression? RewriteEventReference(
        IEventReferenceOperation operation,
        SenseArgument argument)
        => null;
}
