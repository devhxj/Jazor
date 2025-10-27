using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

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
	public override Acornima.Ast.Node? VisitSwitch(ISwitchOperation operation, Queue<VariableDeclaration> argument)
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
					VisitNull(tests, singleValue.Value, argument);
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
	public override Acornima.Ast.Node? VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, Queue<VariableDeclaration> argument)
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
	public override Acornima.Ast.Node? VisitSwitchCase(ISwitchCaseOperation operation, Queue<VariableDeclaration> argument)
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
	public override Acornima.Ast.Node? VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, Queue<VariableDeclaration> argument)
	{
		// 将单值case转换为条件比较
		// 返回比较表达式，需要在上级switch中组合成if-else
		return Translate<Expression>(operation.Value, argument);
	}

	/// <summary>
	/// 处理 switch 表达式操作
	/// C# 示例：
	/// var result = value switch {
	///     1 => "One",              // 常量模式
	///     string s => $"String: {s}", // 类型模式
	///     { Length: > 5 } => "Long",   // 属性模式
	///     var x when x > 0 => "Positive", // when 子句
	///     _ => "Other"             // 默认模式
	/// };
	/// 转换结果：根据模式复杂度转换为嵌套条件表达式或函数调用
	/// <summary>
	/// 将C# switch表达式转换为JavaScript switch语句或IIFE
	/// 非模式匹配switch转换为switch语句，模式匹配switch转换为IIFE
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSwitchExpression(ISwitchExpressionOperation operation, Queue<VariableDeclaration> argument)
	{
		if (operation.Arms.Length == 0)
			return null;

		var input = Translate<Expression>(operation.Value, argument);
		// 检查是否为非模式匹配switch（传统switch）
		bool isTraditionalSwitch = true;
		foreach (var arm in operation.Arms)
		{
			// 检查是否为常量模式或丢弃模式
			if (arm.Pattern.Kind != OperationKind.ConstantPattern &&
				arm.Pattern.Kind != OperationKind.DiscardPattern)
			{
				isTraditionalSwitch = false;
				break;
			}

			// 检查是否有when子句
			if (arm.Guard is not null)
			{
				isTraditionalSwitch = false;
				break;
			}
		}

		if (isTraditionalSwitch)
		{
			// 非模式匹配switch，转换为JavaScript switch语句
			var cases = new List<SwitchCase>();

			foreach (var arm in operation.Arms)
			{
				if (VisitSwitchExpressionArm(arm, argument) is SwitchCase switchCase)
				{
					cases.Add(switchCase);
				}
			}

			// 确保有默认情况
			bool hasDefault = false;
			foreach (var c in cases)
			{
				if (c.Test == null)
				{
					hasDefault = true;
					break;
				}
			}

			if (!hasDefault)
			{
				cases.Add(new SwitchCase(
					null,
					NodeList.From<Statement>(new ReturnStatement(new Identifier("undefined")))
				));
			}

			var switchStatement = new SwitchStatement(input, NodeList.From<SwitchCase>(cases));
			var functionBody = new FunctionBody(NodeList.From<Statement>(switchStatement), strict: true);
			var arrowFunction = new ArrowFunctionExpression(
				NodeList.From<Node>(),
				functionBody,
				expression: false,
				async: false
			);

			// 立即调用箭头函数 (() => { ... })()
			return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
		}
		else
		{
			// 复杂模式匹配switch，生成健全的IIFE保证副作用顺序
			// 采用分层判断：先模式匹配，后when条件，确保求值节拍与C#一致
			var statements = new List<Statement>();
			var discardArm = (ISwitchExpressionArmOperation?)null;

			// 创建临时变量存储输入值，确保仅求值一次
			var inputVar = new Identifier(GetUniqueName(operation));
			statements.Add(new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(
					new VariableDeclarator(inputVar, input)
				)
			));

			// 处理所有非丢弃模式，采用嵌套if确保副作用顺序
			foreach (var arm in operation.Arms)
			{
				if (arm.Pattern.Kind == OperationKind.DiscardPattern)
				{
					discardArm = arm; // 保存丢弃模式，最后处理
					continue;
				}

				var value = Translate<Expression>(arm.Value, argument);
				// 生成模式匹配条件
				Expression? patternCondition = null;

				if (arm.Pattern.Kind == OperationKind.ConstantPattern)
				{
					var constantValue = Translate<Expression>(arm.Pattern, argument);
					patternCondition = new LogicalExpression(Operator.StrictEquality, inputVar, constantValue);
				}
				else if (arm.Pattern.Kind == OperationKind.TypePattern)
				{
					// 对于类型模式，使用typeof或instanceof检查
					if (arm.Pattern is ITypePatternOperation typeOp && typeOp.InputType is not null)
					{
						var typeName = typeOp.InputType.Name.ToLowerInvariant();
						patternCondition = typeName switch
						{
							"string" => new LogicalExpression(Operator.StrictEquality,
								new UpdateExpression(Operator.TypeOf, inputVar, prefix: true),
								new StringLiteral("string", "\"string\"")),
							"number" or "int32" or "int64" or "double" or "float" or "decimal" =>
								new LogicalExpression(Operator.StrictEquality,
									new UpdateExpression(Operator.TypeOf, inputVar, prefix: true),
									new StringLiteral("number", "\"number\"")),
							"boolean" => new LogicalExpression(Operator.StrictEquality,
								new UpdateExpression(Operator.TypeOf, inputVar, prefix: true),
								new StringLiteral("boolean", "\"boolean\"")),
							_ => new LogicalExpression(Operator.InstanceOf, inputVar, new Identifier(typeName))
						};
					}
				}
				else
				{
					// 其他模式类型，尝试访问并处理占位符替换
					// 这里需要实现占位符替换逻辑
					// 暂时使用原始表达式，但需要注意占位符问题
					patternCondition = Translate<Expression>(arm.Pattern, argument);

				}

				if (patternCondition is not null)
				{
					Statement branchStatement;

					if (arm.Guard is not null)
					{
						// 有when子句：关键的分层判断保证副作用顺序
						// 先模式匹配，成功后才执行when条件
						var guardCondition = Translate<Expression>(arm.Guard, argument);
						// 嵌套if结构：
						// if (pattern) {
						//   if (when) return value;
						// }
						var innerIf = new IfStatement(guardCondition, new ReturnStatement(value), null);
						branchStatement = new IfStatement(patternCondition, innerIf, null);
					}
					else
					{
						// 无when子句：直接判断模式
						branchStatement = new IfStatement(patternCondition, new ReturnStatement(value), null);
					}

					statements.Add(branchStatement);
				}
			}

			// 最后处理丢弃模式（默认情况）
			if (discardArm is not null)
			{
				var discardValue = Translate<Expression>(discardArm.Value, argument);
				statements.Add(new ReturnStatement(discardValue));
			}

			// 确保有返回值
			if (statements.Count == 0 || statements[statements.Count - 1] is not ReturnStatement)
			{
				statements.Add(new ReturnStatement(new Identifier("undefined")));
			}

			var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
			var arrowFunction = new ArrowFunctionExpression(
				NodeList.From<Node>(),
				functionBody,
				expression: false,
				async: false
			);

			// 立即调用箭头函数
			return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
		}
	}

	/// <summary>
	/// 处理 switch 表达式分支操作
	/// 根据上下文返回SwitchCase（传统switch）或Statement（模式匹配）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation, Queue<VariableDeclaration> argument)
	{
		var pattern = Translate<Expression>(operation.Pattern, argument);
		var guard = VisitNull<Expression>(operation.Guard, argument);
		var value = Translate<Expression>(operation.Value, argument);

		// 检查是否为传统的常量模式（无when子句）
		bool isTraditionalPattern = (operation.Pattern.Kind == OperationKind.ConstantPattern ||
								   operation.Pattern.Kind == OperationKind.DiscardPattern) &&
								   operation.Guard == null;

		if (isTraditionalPattern)
		{
			// 生成SwitchCase用于传统switch语句
			Expression? test = null;

			if (operation.Pattern.Kind == OperationKind.ConstantPattern)
				test = pattern;

			// DiscardPattern的test为null（默认情况）
			// 修复P0：SwitchCase中不能直接使用ReturnStatement，应该使用break
			// 对于switch表达式转换为switch语句的场景，需要在外层包装函数来处理返回值
			var breakStatement = new BreakStatement(null);

			return new SwitchCase(
				test,
				NodeList.From<Statement>(new NonSpecialExpressionStatement(value), breakStatement)
			);
		}
		else
		{
			// 生成Statement用于模式匹配IIFE
			if (operation.Pattern.Kind == OperationKind.DiscardPattern)
			{
				// 默认情况，直接返回
				return new ReturnStatement(value);
			}
			else if (pattern is not null)
			{
				Expression condition;

				if (operation.Pattern.Kind == OperationKind.ConstantPattern)
				{
					// 从父operation获取switch目标名称并构建表达式
					var targetName = ExtractPatternValName(operation.Parent);
					var target = new Identifier(targetName);
					condition = new LogicalExpression(Operator.StrictEquality, target, pattern);
				}
				else
				{
					// 复杂模式，直接使用模式表达式（已经包含实际目标）
					condition = pattern;
				}

				// 处理when子句
				if (guard is not null)
				{
					condition = new LogicalExpression(Operator.LogicalAnd, condition, guard);
				}

				return new IfStatement(condition, new ReturnStatement(value), null);
			}
		}

		return HandleTransformationFailure(operation, "Switch expression arm could not be translated to JavaScript.");
	}
}
