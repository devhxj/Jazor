using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

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
        // try 体：隔离 scope，变量声明不泄漏到 try 外
        var tryCtx = argument.WithNewScope();
        var tryPending = TranslateOperationsToStatements(operation.Body.Operations, tryCtx);

        var tryBodyStatements = new List<Statement>();
        if (tryCtx.HasVarDeclarator)
        {
            var declarators = tryCtx.FlushVarDeclarator();
            tryBodyStatements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
        }
        tryBodyStatements.AddRange(tryPending);
        var block = new NestedBlockStatement(NodeList.From(tryBodyStatements));

        // js只有单catch，多个catch需要合并成一个catch，在内部使用if分支
        CatchClause? handler = null;
        if (operation.Catches.Length == 1)
        {
            var @catch = operation.Catches[0];
            if (Visit(@catch, argument) is not CatchClause node)
                return HandleTransformationFailure<Node>(@catch, "Try statement catch clause could not be translated to JavaScript.");

            handler = node;
        }
        else if (operation.Catches.Length > 1)
        {
            var tryParam = new Identifier(GetUniqueName(operation));
            var groups = new List<(string TypeName, List<ICatchClauseOperation> Clauses)>();
            foreach (var @catch in operation.Catches)
            {
                var (_, typeName) = GetMapperType(@catch.ExceptionType);

                if (groups.Count > 0 && groups[groups.Count - 1].TypeName == typeName)
                {
                    var last = groups[groups.Count - 1];
                    last.Clauses.Add(@catch);
                    groups[groups.Count - 1] = last;
                }
                else
                {
                    groups.Add((typeName, new List<ICatchClauseOperation> { @catch }));
                }
            }

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

                var handlerStatements = ExtractCatchHandlerStatements(@catch, argument, tryParam);
                if (@catch.Filter is not null)
                {
                    var filterExpr = TranslateExpression(@catch.Filter, argument);
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

            NestedBlockStatement BuildGroupBody(List<ICatchClauseOperation> clauses, Statement fallback)
            {
                var bodyStatements = new List<Statement>();
                var parameters = new List<string>();
                foreach (var @catch in clauses)
                {
                    var param = ExtractCatchClauseParam(@catch);
                    if (param is null || parameters.Contains(param.Name))
                        continue;

                    parameters.Add(param.Name);
                }

                Identifier? sharedParam = parameters.Count == 1
                    ? new Identifier(parameters[0])
                    : null;

                if (sharedParam is not null)
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
                var body = BuildGroupBody(group.Clauses, fallback);
                var test = new NonLogicalBinaryExpression(Operator.InstanceOf, tryParam, new Identifier(group.TypeName));
                // 同一 JS 运行时类型的多个 catch 需要先聚合到一个分支里，
                // 这样 when 过滤失败时才能继续尝试同组后续 catch，而不是提前 rethrow。
                chain = new IfStatement(test, body, fallback);
            }

            var catchBody = new NestedBlockStatement(NodeList.From<Statement>(chain));
            handler = new CatchClause(tryParam, catchBody);
        }

        NestedBlockStatement? finalizer = null;
        if (operation.Finally is not null)
        {
            // finally 体：隔离 scope，变量声明不泄漏到 try 外
            var finallyCtx = argument.WithNewScope();
            var finallyPending = TranslateOperationsToStatements(operation.Finally.Operations, finallyCtx);

            var finallyBodyStatements = new List<Statement>();
            if (finallyCtx.HasVarDeclarator)
            {
                var declarators = finallyCtx.FlushVarDeclarator();
                finallyBodyStatements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
            }
            finallyBodyStatements.AddRange(finallyPending);
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

        return param;
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
        => operation.Filter is not null
        || operation.ExceptionDeclarationOrExpression is not null
        || ContainsBareRethrow(operation.Handler);

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
                : new Identifier(GetUniqueName(operation));

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
        var catchContext = (exceptionParam is not null
            ? argument.WithCatchVar(exceptionParam.Name)
            : argument).WithNewScope();
        var catchPending = TranslateOperationsToStatements(operation.Handler.Operations, catchContext);

        var statements = new List<Statement>();
        if (catchContext.HasVarDeclarator)
        {
            var declarators = catchContext.FlushVarDeclarator();
            statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
        }
        statements.AddRange(catchPending);
        return statements;
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
        var param = RequiresCatchBinding(operation)
            ? ExtractCatchClauseParam(operation) ?? new Identifier(GetUniqueName(operation))
            : null;
        var statements = ExtractCatchClauseBody(operation, argument, param);
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
