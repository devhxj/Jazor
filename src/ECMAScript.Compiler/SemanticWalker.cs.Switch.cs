using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理 switch 语句操作
	/// C# 示例：
	/// switch (value) {
	///     case 1: 
	///         DoOne(); 
	///         break;
	///     case 2:
	///         DoTwo();
	///         break;
	///     default: 
	///         DoDefault(); 
	///         break;
	/// }
	/// 转换结果：直接转换为 JavaScript 的 switch 语句
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSwitch(ISwitchOperation operation, Context argument)
	{
		if (Visit(operation.Value, argument) is not Expression discriminant)
			return HandleTransformationFailure(operation.Value, "Switch discriminant could not be translated to JavaScript.");

		var cases = new List<SwitchCase>();
		foreach (var switchCase in operation.Cases)
		{
			var tests = new List<Expression?>();
			var consequent = new List<Statement>();

			// 处理case条件
			foreach (var clause in switchCase.Clauses)
			{
				if (clause.CaseKind == CaseKind.Default)
				{
					tests.Add(null); // null表示default case
				}
				else if (clause is ISingleValueCaseClauseOperation singleValue)
				{
					Translate(tests, singleValue.Value, argument,null);
				}
				else
					return HandleTransformationFailure(clause, "Switch case clause could not be translated to JavaScript.");
			}

			// 处理case体
			foreach (var bodyOp in switchCase.Body)
			{
				var bodyNode = Visit(bodyOp, argument);
				if (bodyNode is Statement stmt)
					consequent.Add(stmt);
				else if (bodyNode is Expression expr)
					consequent.Add(new NonSpecialExpressionStatement(expr));
				else
					HandleTransformationFailure(bodyOp, "Switch case body statement could not be translated to JavaScript.");
			}

			// 为每个test值创建一个SwitchCase
			// 避免重复添加相同的consequent，只为第一个case添加语句
			for (int i = 0; i < tests.Count; i++)
			{
				var testExpr = tests[i];
				// 只有第一个case包含语句，其余case为fallthrough
				var statements = i == 0 ? consequent : [];
				cases.Add(new SwitchCase(testExpr, NodeList.From(statements)));
			}
		}

		return new SwitchStatement(discriminant, NodeList.From(cases));
	}

	/// <summary>
	/// 处理 switch 默认 case 子句操作
	/// 转换结果：实际处理在VisitSwitch中完成，此处不处理
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, Context argument)
	{
		// 默认case子句转换为switch语句中的default case
		// 在JavaScript中，default case的test为null（表示没有条件）
		// 在VisitSwitch方法中，当遇到CaseKind.Default时会设置tests.Add(null)
		// 所以这个方法实际上不需要返回任何具体的AST节点
		// 它的调用主要用于遍历语法树，实际处理在VisitSwitch中完成
		// 返回null，因为实际处理在VisitSwitch中完成
		return null;
	}

	/// <summary>
	/// 处理 switch case 操作
	/// C# 示例：
	/// switch (value) {
	///     case 1:
	///         DoSomething();
	///         break;
	/// }
	/// 转换结果：转换为 if-else 链的一部分
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSwitchCase(ISwitchCaseOperation operation, Context argument)
	{
		// 将switch case转换为if-else链
		var clauses = operation.Clauses;
		if (clauses.Length == 0)
			return null;

		var statements = new List<Statement>();
		foreach (var clause in clauses)
		{
			Translate(statements, clause, argument);
		}

		// 如果有body操作，添加到语句中
		foreach (var bodyOp in operation.Body)
		{
			var bodyNode = Visit(bodyOp, argument);
			if (bodyNode is Statement bodyStmt)
				statements.Add(bodyStmt);
			else if (bodyNode is Expression bodyExpr)
				statements.Add(new NonSpecialExpressionStatement(bodyExpr));
			else
				return HandleTransformationFailure(bodyOp, "Switch case body could not be translated to JavaScript.");
		}

		return statements.Count > 0 ? new NestedBlockStatement(NodeList.From(statements)) : null;
	}

	/// <summary>
	/// 处理单值 case 子句操作
	/// C# 示例：
	/// switch (value) {
	///     case 42:
	///         DoSomething();
	///         break;
	/// }
	/// 转换结果：返回比较值，在上级组合成 if-else 链
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, Context argument)
	{
		// 将单值case转换为条件比较
		// 返回比较表达式，需要在上级switch中组合成if-else
		return Translate<Expression>(operation.Value, argument);
	}
}
