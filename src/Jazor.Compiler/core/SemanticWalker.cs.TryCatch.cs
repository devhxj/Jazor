using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
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
    public override Node? VisitTry(ITryOperation operation, WalkerArgument argument)
    {
        var bodyStatements = new List<Statement>();
        foreach (var stmt in operation.Body.Operations)
        {
            Translate(bodyStatements, stmt, argument);
        }
        var block = new NestedBlockStatement(NodeList.From(bodyStatements));

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
            // 定义catch使用的param
            var tryParam = new Identifier(GetUniqueName(operation));
            var queue = new Stack<ICatchClauseOperation>();
            foreach (var @catch in operation.Catches)
                queue.Push(@catch);

            Statement? alternate = null;
            while (queue.Count > 0)
            {
                var @catch = queue.Pop();
                var right = new Identifier(@catch.ExceptionType.Name);
                // 传递 tryParam 作为 exceptionParam，用于 when 条件检查
                var statements = ExtractCatchClauseBody(@catch, argument, tryParam);
                var param = ExtractCatchClauseParam(@catch);
                if (param is not null)
                {
                    // 需要定义一个变量，将tryParam转为当前catch的param
                    var catchParamDeclarator = new VariableDeclarator(param, tryParam);
                    var catchParamDeclaration = new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(catchParamDeclarator));
                    statements.Insert(0, catchParamDeclaration);
                }
                var body = new NestedBlockStatement(NodeList.From(statements));
                var test = new NonLogicalBinaryExpression(Operator.InstanceOf, tryParam, right);
                alternate = new IfStatement(test, body, alternate: alternate);
            }

            if (alternate is null)
                return HandleTransformationFailure<Node>(operation, "Try statement catch clause could not be translated to JavaScript.");

            var catchBody = new NestedBlockStatement(NodeList.From(alternate));
            handler = new CatchClause(tryParam, catchBody);
        }

        NestedBlockStatement? finalizer = null;
        if (operation.Finally is not null)
        {
            var finallyStatements = new List<Statement>();
            foreach (var stmt in operation.Finally.Operations)
            {
                Translate(finallyStatements, stmt, argument);
            }
            finalizer = new NestedBlockStatement(NodeList.From(finallyStatements));
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

    /// <summary>
    /// 从ExceptionDeclarationOrExpression中提取异常变量名
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="argument"></param>
    /// <param name="exceptionParam">异常参数标识符，用于 when 条件和重新抛出</param>
    /// <returns></returns>
    private List<Statement> ExtractCatchClauseBody(ICatchClauseOperation operation, WalkerArgument argument, Identifier? exceptionParam)
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

        foreach (var stmt in operation.Handler.Operations)
        {
            var node = Visit(stmt, argument);
            if (node is Statement statement)
                bodyStatements.Add(statement);
            else if (node is Expression expr)
                bodyStatements.Add(new NonSpecialExpressionStatement(expr));
            else
                HandleTransformationFailure<Node>(stmt, "Try statement catch clause could not be translated to JavaScript.");
        }
        return bodyStatements;
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
    public override Node? VisitCatchClause(ICatchClauseOperation operation, WalkerArgument argument)
    {
        // 此处不用担心多个catch，多catch会在 VisitTry中处理
        var param = ExtractCatchClauseParam(operation);
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
	public override Node? VisitThrow(IThrowOperation operation, WalkerArgument argument)
	{
		Expression expr;
		if (operation.Exception is not null)
			expr = Translate<Expression>(operation.Exception, argument);
		else
			expr = new Identifier(GetUniqueName(operation));

		return new ThrowStatement(expr);
	}

}
