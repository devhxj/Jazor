using Acornima.Ast;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理数组初始化器操作
	/// C# 示例：
	/// new int[] {1, 2, 3, 4, 5}           // 数组初始化器
	/// {"apple", "banana", "cherry"}      // 集合初始化器
	/// 转换结果：[1, 2, 3, 4, 5] / ["apple", "banana", "cherry"]
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitArrayInitializer(IArrayInitializerOperation operation, Context argument)
	{
		var elements = new List<Expression?>();
		foreach (var element in operation.ElementValues)
		{
			Translate(elements, element, argument,null);
		}
		return new ArrayExpression(NodeList.From(elements));
	}

	/// <summary>
	/// 处理字段初始化器操作
	/// C# 示例：
	/// public int Field = 42;              // 字段初始化器
	/// private string _name = "default";   // 私有字段初始化
	/// 转换结果：直接返回初始化值表达式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitFieldInitializer(IFieldInitializerOperation operation, Context argument)
	{
		return Visit(operation.Value, argument);
	}

	/// <summary>
	/// 处理变量初始化器操作
	/// C# 示例：
	/// int x = 10;                         // 变量初始化器
	/// string name = "Hello";              // 字符串初始化
	/// 转换结果：直接返回初始化值表达式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitVariableInitializer(IVariableInitializerOperation operation, Context argument)
	{
		return Visit(operation.Value, argument);
	}

	/// <summary>
	/// 处理变量声明符操作
	/// C# 示例：
	/// int x = 5 中的 "x = 5" 部分      // 变量声明符
	/// string name 中的 "name" 部分     // 无初始化的声明符
	/// 转换结果：x = 5 / name
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitVariableDeclarator(IVariableDeclaratorOperation operation, Context argument)
	{
		var identifier = new Identifier(operation.Symbol.Name);
		var init = Translate<Expression>(operation.Initializer, argument, null);

		return new VariableDeclarator(identifier, init);
	}

	/// <summary>
	/// 处理变量声明操作
	/// C# 示例：
	/// int x = 5, y = 10;                  // 多变量声明
	/// string name = "test", message;      // 混合声明
	/// 转换结果：let x = 5, y = 10; / let name = "test", message;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitVariableDeclaration(IVariableDeclarationOperation operation, Context argument)
	{
		var declarators = new List<VariableDeclarator>();
		foreach (var declarator in operation.Declarators)
			Translate(declarators, declarator, argument);
			
		return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
	}

	/// <summary>
	/// 处理变量声明组操作
	/// C# 示例：int a = 1, b = 2, c;
	/// 转换结果：let a = 1, b = 2, c;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, Context argument)
	{
		// 可以假设 IVariableDeclarationGroupOperation.Declarations 只包含一个元素
		// 除非你正在处理一些非常特殊的、涉及类型推断的复合声明（如 using 语句）。
		// 这里有一个关键点：IVariableDeclarationGroupOperation 主要用于表示局部变量。
		// 对于 using 语句，Roslyn 更倾向于使用 IUsingDeclarationOperation 或 IUsingOperation 来封装其语义。
		// 在这些操作内部，你可能会找到多个 IVariableDeclarationOperation，但它们不一定被包装在一个公开的 IVariableDeclarationGroupOperation 中。
		var declarators = new List<VariableDeclarator>();
		foreach (var declaration in operation.Declarations)
			foreach (var declarator in declaration.Declarators)
				Translate(declarators, declarator, argument);

		return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
	}

	/// <summary>
	/// 处理声明表达式操作
	/// C# 示例：
	/// if (int.TryParse(input, out var result)) // out var 声明
	/// if (dict.TryGetValue(key, out string value)) // out 声明
	/// 转换结果：转换为 let 变量声明
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitDeclarationExpression(IDeclarationExpressionOperation operation, Context argument)
	{
		var expr = Translate<Expression>(operation.Expression, argument);
		if (operation.Parent is IArgumentOperation)
		{
			var declarator = new VariableDeclarator(expr, null);
			argument.Enqueue(declarator);
		}

		return expr;
	}
}
