using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

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
	/// 转换结果：
	/// - 常量 case: 转换为 JavaScript 的 switch 语句
	/// - 模式 case: 转换为 IIFE + if-else 链
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitSwitch(ISwitchOperation operation, SenseArgument argument)
	{
		// 检测是否包含模式匹配 case
		var hasPatternCase = operation.Cases
			.Any(x=>x.Clauses.Any(y=>y.CaseKind == CaseKind.Pattern));

		// 如果包含模式匹配，转换为 IIFE + if-else 链
		if (hasPatternCase)
			return VisitSwitchPatternMatching(operation, argument);

		// 否则转换为传统的 switch 语句
		return VisitSwitchTraditional(operation, argument);
	}

	/// <summary>
	/// 处理传统的 switch 语句（常量 case）
	/// </summary>
	private SwitchStatement VisitSwitchTraditional(ISwitchOperation operation, SenseArgument argument)
	{
		if (Visit(operation.Value, argument) is not Expression discriminant)
			return HandleTransformationFailure<SwitchStatement>(operation.Value, "Switch discriminant could not be translated to JavaScript.");

		var cases = new List<SwitchCase>();
		foreach (var switchCase in operation.Cases)
		{
			var tests = new List<Expression?>();
			var consequent = new List<Statement>();

			// 处理case条件
			foreach (var clause in switchCase.Clauses)
			{
				// 特殊处理 default case clause：需要添加 null 到 tests 列表
				if (clause.CaseKind == CaseKind.Default)
					tests.Add(null);  // null 表示 default case
				else
					Translate(tests, clause, argument, null);
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
					return HandleTransformationFailure<SwitchStatement>(bodyOp, "Switch case body statement could not be translated to JavaScript.");
			}

			// 为每个 test 值创建一个 SwitchCase。
			// C# 的 case label 共享同一个 body 时，语句应挂在最后一个 label 上，
			// 这样 case 2/case 3 直接命中时也能落到真实 body，而不是跳空后继续穿透。
			for (int i = 0; i < tests.Count; i++)
			{
				var testExpr = tests[i];
				var statements = i == tests.Count - 1 ? consequent : [];
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
	public override Node? VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, SenseArgument argument)
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
	public override Node? VisitSwitchCase(ISwitchCaseOperation operation, SenseArgument argument)
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
				return HandleTransformationFailure<Node>(bodyOp, "Switch case body could not be translated to JavaScript.");
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
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, SenseArgument argument)
	{
		return Translate<Expression>(operation.Value, argument);
	}
}
