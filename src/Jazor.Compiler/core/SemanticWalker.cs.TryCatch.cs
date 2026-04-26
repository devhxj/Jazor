using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
    private List<Statement> TranslateOperationsToStatements(IEnumerable<IOperation> operations, SenseArgument context)
    {
        var pendingStatements = new List<Statement>();
        foreach (var operation in operations)
        {
            var node = Visit(operation, context);

            if (node is Statement statement)
                pendingStatements.Add(statement);
            else if (node is Expression expr)
            {
                if (expr is SequenceExpression seqExpr)
                {
                    if (seqExpr.Expressions.Count == 1)
                        pendingStatements.Add(new NonSpecialExpressionStatement(seqExpr.Expressions[0]));
                    else if (seqExpr.Expressions.Count > 1)
                        pendingStatements.Add(new NonSpecialExpressionStatement(expr));
                }
                else
                    pendingStatements.Add(new NonSpecialExpressionStatement(expr));
            }
            else
                HandleTransformationFailure<Node>(operation, $"{operation.Kind} could not be translated to JavaScript.");
        }

        return pendingStatements;
    }

    /// <summary>
    /// 处理 try-catch-finally 语句操作
    /// C# 示例：
    /// try {
    ///     RiskyOperation();
    /// } catch (Exception ex) {
    ///     HandleError(ex);
    /// } finally {
    ///     Cleanup();
    /// }
    /// 转换结果：try { riskyOperation(); } catch (ex) { handleError(ex); } finally { cleanup(); }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Node? VisitTry(ITryOperation operation, SenseArgument argument)
    {
        var scopedArgument = EnsureScopeContext(operation, argument);

        // try 体：隔离 scope，变量声明不泄漏到 try 外
        var tryCtx = scopedArgument.EnterScope(operation.Body, ScopeSite.TryBody());
        var tryPending = TranslateOperationsToStatements(operation.Body.Operations, tryCtx);

        var tryBodyStatements = MaterializeScopedStatements(tryCtx, tryPending);
        var block = new NestedBlockStatement(NodeList.From(tryBodyStatements));

        // js只有单catch，多个catch需要合并成一个catch，在内部使用if分支
        CatchClause? handler = null;
        if (operation.Catches.Length == 1)
        {
            var @catch = operation.Catches[0];
            RejectUnsupportedSingleCatchTypeIfNeeded(@catch);
            if (Visit(@catch, argument) is not CatchClause node)
                return HandleTransformationFailure<Node>(@catch, "Try statement catch clause could not be translated to JavaScript.");

            handler = node;
        }
        else if (operation.Catches.Length > 1)
        {
            var mergedCatchArg = scopedArgument.EnterScope(operation, ScopeSite.CatchBody());
            var tryParam = new Identifier(AllocateUniqueName(operation, mergedCatchArg, LoweringSite.MultiCatchParameter()));
            var sharedCatchParam = TryExtractSharedCatchParam(operation.Catches);
            var groups = new List<(string TypeKey, ITypeSymbol ExceptionType, List<ICatchClauseOperation> Clauses)>();
            foreach (var @catch in operation.Catches)
            {
                RejectUnsupportedTypeFallback(@catch, @catch.ExceptionType, "catch type filtering");
                var (_, typeName) = GetMapperType(@catch.ExceptionType);
                var typeKey = $"{@catch.ExceptionType.OriginalDefinition.ToDisplayString(Jazor.Name.Format.NameFormat)}|{typeName}";

                if (groups.Count > 0 && groups[groups.Count - 1].TypeKey == typeKey)
                {
                    var last = groups[groups.Count - 1];
                    last.Clauses.Add(@catch);
                    groups[groups.Count - 1] = last;
                }
                else
                {
                    groups.Add((typeKey, @catch.ExceptionType, new List<ICatchClauseOperation> { @catch }));
                }
            }

            var hoistSharedCatchParamOutsideGroups = sharedCatchParam is not null;

            Statement BuildGroupChain(List<ICatchClauseOperation> clauses, int index, Identifier? sharedParam, Statement fallback)
            {
                if (index >= clauses.Count)
                    return fallback;

                var @catch = clauses[index];
                var currentParam = ExtractCatchClauseParam(@catch);
                var branchStatements = new List<Statement>();

                if (sharedParam is null && currentParam is not null)
                {
                    branchStatements.Add(new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(new VariableDeclarator(currentParam, tryParam))));
                }

                var filterArgument = currentParam is not null
                    ? mergedCatchArg.WithCatchVar(currentParam.Name)
                    : mergedCatchArg;
                var handlerStatements = ExtractCatchHandlerStatements(@catch, mergedCatchArg, tryParam);
                if (@catch.Filter is not null)
                {
                    var filterExpr = TranslateExpression(@catch.Filter, filterArgument);
                    var consequent = new NestedBlockStatement(NodeList.From(handlerStatements));
                    var alternate = BuildGroupChain(clauses, index + 1, sharedParam, fallback);
                    branchStatements.Add(new IfStatement(filterExpr, consequent, alternate));
                }
                else
                {
                    if (index > 0)
                        branchStatements.Add(new NestedBlockStatement(NodeList.From(handlerStatements)));
                    else
                        branchStatements.AddRange(handlerStatements);
                }

                return branchStatements.Count == 1
                    ? branchStatements[0]
                    : new NestedBlockStatement(NodeList.From(branchStatements));
            }

            NestedBlockStatement BuildGroupBody(List<ICatchClauseOperation> clauses, Statement fallback, Identifier? sharedParamFromCatch, bool declareSharedParam)
            {
                var bodyStatements = new List<Statement>();
                Identifier? sharedParam = sharedParamFromCatch;
                if (sharedParam is null)
                {
                    var parameters = new List<string>();
                    foreach (var @catch in clauses)
                    {
                        var param = ExtractCatchClauseParam(@catch);
                        if (param is null || parameters.Contains(param.Name))
                            continue;

                        parameters.Add(param.Name);
                    }

                    sharedParam = parameters.Count == 1
                        ? new Identifier(parameters[0])
                        : null;
                }

                if (sharedParam is not null && declareSharedParam)
                {
                    bodyStatements.Add(new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(new VariableDeclarator(sharedParam, tryParam))));
                }

                bodyStatements.Add(BuildGroupChain(clauses, 0, sharedParam, fallback));
                return new NestedBlockStatement(NodeList.From(bodyStatements));
            }

            Statement chain = new ThrowStatement(tryParam);
            for (var index = groups.Count - 1; index >= 0; index--)
            {
                var group = groups[index];
                var fallback = chain;
                var body = BuildGroupBody(group.Clauses, fallback, sharedCatchParam, !hoistSharedCatchParamOutsideGroups);
                var test = CreateTypeMatchExpr(operation, group.ExceptionType, tryParam, nullable: false, context: mergedCatchArg);
                // 同一 JS 运行时类型的多个 catch 需要先聚合到一个分支里，
                // 这样 when 过滤失败时才能继续尝试同组后续 catch，而不是提前 rethrow。
                chain = new IfStatement(test, body, fallback);
            }

            var catchBodyStatements = new List<Statement>();
            if (hoistSharedCatchParamOutsideGroups)
            {
                // sharedCatchParam 非空时才会进入这条分支；
                // 这里显式收窄，避免可空分析把共享绑定误判成潜在空值。
                catchBodyStatements.Add(new VariableDeclaration(
                    VariableDeclarationKind.Const,
                    NodeList.From(new VariableDeclarator(sharedCatchParam!, tryParam))));
            }

            catchBodyStatements.Add(chain);
            var catchBody = new NestedBlockStatement(NodeList.From(catchBodyStatements));
            handler = new CatchClause(tryParam, catchBody);
        }

        NestedBlockStatement? finalizer = null;
        if (operation.Finally is not null)
        {
            // finally 体：隔离 scope，变量声明不泄漏到 try 外
            var finallyCtx = scopedArgument.EnterScope(operation.Finally, ScopeSite.FinallyBody());
            var finallyPending = TranslateOperationsToStatements(operation.Finally.Operations, finallyCtx);

            var finallyBodyStatements = MaterializeScopedStatements(finallyCtx, finallyPending);
            finalizer = new NestedBlockStatement(NodeList.From(finallyBodyStatements));
        }

        return new TryStatement(block, handler, finalizer);
    }

    /// <summary>
    /// 从ExceptionDeclarationOrExpression中提取异常变量名
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="argument"></param>
    /// <returns></returns>
    private Identifier? ExtractCatchClauseParam(ICatchClauseOperation operation)
    {
        // 从ExceptionDeclarationOrExpression中提取异常变量名
        Identifier? param = null;

        if (operation.ExceptionDeclarationOrExpression is not null)
        {
            // 尝试从异常声明中提取变量名
            switch (operation.ExceptionDeclarationOrExpression)
            {
                case ILocalReferenceOperation localRef when localRef.Local is not null:
                    param = new Identifier(localRef.Local.Name);
                    break;
                case IParameterReferenceOperation paramRef when paramRef.Parameter is not null:
                    param = new Identifier(paramRef.Parameter.Name);
                    break;
                case IVariableDeclaratorOperation varDeclarator when varDeclarator.Symbol is not null:
                    param = new Identifier(varDeclarator.Symbol.Name);
                    break;
                default:
                    HandleTransformationFailure<Node>(operation.ExceptionDeclarationOrExpression, "Try statement catch clause could not be translated to JavaScript.");
                    break;
            }
        }

        if (param is null &&
            operation.Syntax is CatchClauseSyntax catchClause &&
            catchClause.Declaration is not null &&
            catchClause.Declaration.Identifier.ValueText.Length > 0)
        {
            // Roslyn 在部分 catch lowering 场景里不会稳定暴露 ExceptionDeclarationOrExpression，
            // 回退到语法声明可以保证多 catch 合并时仍能识别共享异常变量。
            param = new Identifier(catchClause.Declaration.Identifier.ValueText);
        }

        return param;
    }

    private Identifier? TryExtractSharedCatchParam(ImmutableArray<ICatchClauseOperation> catches)
    {
        string? sharedName = null;
        foreach (var @catch in catches)
        {
            var param = ExtractCatchClauseParam(@catch);
            if (param is null)
                return null;

            if (sharedName is null)
            {
                sharedName = param.Name;
                continue;
            }

            if (sharedName != param.Name)
                return null;
        }

        return sharedName is null ? null : new Identifier(sharedName);
    }

    private static bool ContainsBareRethrow(IOperation operation)
    {
        if (operation is IThrowOperation { Exception: null })
            return true;

        foreach (var child in operation.ChildOperations)
        {
            if (ContainsBareRethrow(child))
                return true;
        }

        return false;
    }

    private static bool RequiresCatchBinding(ICatchClauseOperation operation)
        => RequiresCatchTypeFilter(operation)
        || operation.Filter is not null
        || operation.ExceptionDeclarationOrExpression is not null
        || ContainsBareRethrow(operation.Handler);

    private static bool RequiresCatchTypeFilter(ICatchClauseOperation operation)
        => HasDeclaredCatchType(operation)
        && operation.ExceptionType is not null
        && !IsCatchAllExceptionType(operation.ExceptionType);

    private static bool HasDeclaredCatchType(ICatchClauseOperation operation)
        => operation.Syntax is CatchClauseSyntax { Declaration: not null };

    private static bool IsCatchAllExceptionType(ITypeSymbol? typeSymbol)
        => typeSymbol?.OriginalDefinition is INamedTypeSymbol namedType
        && namedType.Name == "Exception"
        && namedType.ContainingNamespace?.ToDisplayString() == "System";

    private void RejectUnsupportedSingleCatchTypeIfNeeded(ICatchClauseOperation operation)
    {
        if (!RequiresCatchTypeFilter(operation) || operation.ExceptionType is null)
            return;

        RejectUnsupportedTypeFallback(operation, operation.ExceptionType, "catch type filtering");
        RejectAmbiguousRuntimeTypeFilter(operation, operation.ExceptionType, "catch type filtering");
    }

    private IfStatement? BuildCatchTypeGuard(ICatchClauseOperation operation, SenseArgument argument, Identifier? exceptionParam)
    {
        if (!RequiresCatchTypeFilter(operation) || operation.ExceptionType is null)
            return null;

        if (exceptionParam is null)
            throw new InvalidOperationException("Typed catch filtering requires an exception binding.");

        var test = CreateTypeMatchExpr(
            operation,
            operation.ExceptionType,
            exceptionParam,
            nullable: false,
            context: argument);
        var notMatch = new NonUpdateUnaryExpression(Operator.LogicalNot, test);
        return new IfStatement(notMatch, new ThrowStatement(exceptionParam), null);
    }

    /// <summary>
    /// 从ExceptionDeclarationOrExpression中提取异常变量名
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="argument"></param>
    /// <param name="exceptionParam">异常参数标识符，用于 when 条件和重新抛出</param>
    /// <returns></returns>
    private List<Statement> ExtractCatchClauseBody(ICatchClauseOperation operation, SenseArgument argument, Identifier? exceptionParam)
    {
        var bodyStatements = new List<Statement>();

        var typeGuard = BuildCatchTypeGuard(operation, argument, exceptionParam);
        if (typeGuard is not null)
            bodyStatements.Add(typeGuard);

        // 处理 when 条件过滤器
        // C# 示例：
        // catch (Exception ex) when (condition) { handler }
        // 转换结果：
        // catch (ex) {
        //     if (!(condition)) throw ex;
        //     handler
        // }
        if (operation.Filter is not null)
        {
            var filterExpr = TranslateExpression(operation.Filter, argument);

            // 获取用于重新抛出的异常标识符
            // 如果 catch 有参数名则使用参数名，否则使用 tryParam
            var throwExpr = exceptionParam is not null
                ? (Expression)new Identifier(exceptionParam.Name)
                : new Identifier(AllocateUniqueName(operation, argument, LoweringSite.SyntheticCatchParameter()));

            // 构造 if (!(condition)) throw ex; 语句
            var notFilter = new NonUpdateUnaryExpression(Operator.LogicalNot, filterExpr);
            var throwStmt = new ThrowStatement(throwExpr);
            var filterCheck = new IfStatement(notFilter, throwStmt, null);

            bodyStatements.Add(filterCheck);
        }

        bodyStatements.AddRange(ExtractCatchHandlerStatements(operation, argument, exceptionParam));
        return bodyStatements;
    }

    private List<Statement> ExtractCatchHandlerStatements(ICatchClauseOperation operation, SenseArgument argument, Identifier? exceptionParam)
    {
        // catch 体：隔离 scope，变量声明不泄漏到 catch 外
        // 同时传递异常参数名（用于 re-throw）
        var catchContextBase = exceptionParam is not null
            ? argument.WithCatchVar(exceptionParam.Name)
            : argument;
        var catchContext = catchContextBase.ScopeContext is not null && ReferenceEquals(catchContextBase.ScopeContext.Anchor, operation)
            ? catchContextBase
            : catchContextBase.EnterScope(operation, ScopeSite.CatchBody());
        var catchPending = TranslateOperationsToStatements(operation.Handler.Operations, catchContext);

        return MaterializeScopedStatements(catchContext, catchPending);
    }

    /// <summary>
    /// 处理 catch 子句操作
    /// C# 示例：
    /// try { ... }
    /// catch (Exception ex) { ... }     // catch 子句
    /// catch (InvalidOperationException) { ... } // 不带变量的 catch
    /// catch (Exception ex) when (condition) { ... } // 带 when 条件的 catch
    /// 转换结果：catch (ex) { if (!(condition)) throw ex; ... } / catch (error) { ... }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Node? VisitCatchClause(ICatchClauseOperation operation, SenseArgument argument)
    {
        // 此处不用担心多个catch，多catch会在 VisitTry中处理
        RejectUnsupportedSingleCatchTypeIfNeeded(operation);
        var catchScope = EnsureScopeContext(operation, argument).EnterScope(operation, ScopeSite.CatchBody());
        var param = RequiresCatchBinding(operation)
            ? ExtractCatchClauseParam(operation) ?? new Identifier(AllocateUniqueName(operation, catchScope, LoweringSite.SyntheticCatchParameter()))
            : null;
        var catchArgument = param is not null
            ? catchScope.WithCatchVar(param.Name)
            : catchScope;
        var statements = ExtractCatchClauseBody(operation, catchArgument, param);
        var body = new NestedBlockStatement(NodeList.From(statements));

        return new CatchClause(param, body);
    }

	/// <summary>
	/// 处理 throw 语句操作
	/// C# 示例：
	/// throw new Exception("Error message"); // 抛出异常
	/// throw;                              // 重新抛出当前异常
	/// 转换结果：throw new Error("Error message") / throw Error
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitThrow(IThrowOperation operation, SenseArgument argument)
	{
		Expression expr;
        if (operation.Exception is not null)
            expr = Translate<Expression>(operation.Exception, argument);
        else
        {
            // 从上下文获取异常参数名（用于 re-throw）
            if (argument.CatchExceptionVar is not null)
                expr = new Identifier(argument.CatchExceptionVar);
            else
                return HandleTransformationFailure<Node>(operation, "Throw statement could not be translated to JavaScript because it is not within a try block.");
        }

		return new ThrowStatement(expr);
	}

}
