using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理 foreach 循环操作
	/// C# 示例：
	/// foreach (var item in collection) {
	///     Console.WriteLine(item);
	/// }
	/// 转换结果：for (let item of collection) { console.log(item); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitForEachLoop(IForEachLoopOperation operation, WalkerArgument argument)
	{
		// 获取循环变量 - 使用 LoopControlVariable 直接访问
		var left = Translate<Node>(operation.LoopControlVariable, argument);
		var right = Translate<Expression>(operation.Collection, argument);
		var body = Translate<Statement>(operation.Body, argument);

		return new ForOfStatement(left, right, body, @await: false);
	}

	/// <summary>
	/// 处理 for 循环操作
	/// C# 示例：
	/// for (int i = 0; i < 10; i++) {
	///     Console.WriteLine(i);
	/// }
	/// 转换结果：for (let i = 0; i < 10; i++) { console.log(i); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitForLoop(IForLoopOperation operation, WalkerArgument argument)
	{
		StatementOrExpression? init = null;
		Expression? test = null;
		if (operation.Before.Length > 0)
		{
			var variableDecls = new List<VariableDeclaration>();
			foreach (var before in operation.Before)
			{
				Translate(variableDecls, before, argument);
			}
			if (variableDecls.Count == 1)
				init = variableDecls[0];
			else
			{
				var declarations = new List<VariableDeclarator>();
				foreach (var decl in variableDecls)
					declarations.AddRange(decl.Declarations);

				variableDecls.Select(x => x.Declarations);
				init = new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarations));
			}
		}

		if (operation.Condition is not null)
		{
			test = TranslateExpression(operation.Condition, argument);
		}

		// 处理多个 AtLoopBottom 操作的情况
		// IForLoopOperation.AtLoopBottom 出现“多个”运算并不代表 C# 支持写多个迭代表达式，
		// 而是 Roslyn 在 lowering 阶段把源码里的一个迭代表达式拆成多条中间指令。
		// 常见场景：
		// 1. 复合赋值  i += x + y  →  先算临时变量，再执行加法赋值
		// 2. 方法调用  M(out var tmp)  →  调用 + 丢弃返回值
		// 3. 异步/迭代器状态机生成
		// 遍历列表时按顺序依次输出即可，源代码层面仍只有一段“迭代表达式”。        
		Expression? updateExpression = null;
		if (operation.AtLoopBottom.Length > 0)
		{
			// 如果只有一个操作，直接使用
			if (operation.AtLoopBottom.Length == 1)
			{
				updateExpression = TranslateExpression(operation.AtLoopBottom[0], argument);
				var updateStatement = new NonSpecialExpressionStatement(updateExpression);
			}
			else
			{
				// 如果有多个操作，将它们组合成一个逗号表达式
				var expressions = new List<Expression>();
				foreach (var atLoopBottomOp in operation.AtLoopBottom)
				{
					var expr = TranslateExpression(atLoopBottomOp, argument);
					expressions.Add(expr);
				}

				// 如果只有一个有效表达式，直接使用
				if (expressions.Count == 1)
					updateExpression = expressions[0];

				// 如果有多个有效表达式，使用逗号表达式组合
				else if (expressions.Count > 1)
				{
					updateExpression = expressions[0];
					for (int i = 1; i < expressions.Count; i++)
					{
						updateExpression = new SequenceExpression(
							NodeList.From([updateExpression, expressions[i]])
						);
					}
				}
			}
		}

		var body = Translate<Statement>(operation.Body, argument);
		return new ForStatement(init, test, updateExpression, body);
	}

	/// <summary>
	/// 处理 while 和 do-while 循环操作
	/// C# 示例：
	/// while (condition) { ... }        → while (condition) { ... }
	/// do { ... } while (condition);    → do { ... } while (condition);
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitWhileLoop(IWhileLoopOperation operation, WalkerArgument argument)
	{
		if (operation.Condition is null)
			return null;

		var test = Translate<Expression>(operation.Condition, argument);
		var body = Translate<Statement>(operation.Body, argument);

		// ConditionIsTop: true = while (条件在顶部), false = do-while (条件在底部)
		if (!operation.ConditionIsTop)
			return new DoWhileStatement(body, test);
		else
			return new WhileStatement(test, body);
	}

}
