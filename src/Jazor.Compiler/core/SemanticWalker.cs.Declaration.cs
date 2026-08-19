// File: SemanticWalker.cs.Declaration.cs
// Purpose: Lowers local declarations, fields, functions, and declaration-oriented operations.
// 负责把 C# 声明形状落到合法 JavaScript binding，并与发射作用域和稳定命名设施协作。
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// 负责处理变量、参数、字段、属性和其他声明相关 operation 的发射。
/// </summary>
/// <remarks>
/// 声明 lowering 需要同时维护 JavaScript 绑定、C# 符号名称和发射作用域。
/// 这里的变量声明集合由 <see cref="SenseArgument"/> 收集后统一落地，避免嵌套表达式在错误位置
/// 生成 <c>var</c>/<c>let</c>，也避免 synthetic temp 泄漏到外层作用域。
/// </remarks>
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
	public override Node? VisitArrayInitializer(IArrayInitializerOperation operation, SenseArgument argument)
	{
		var elements = new List<Expression?>();
		// Roslyn only exposes IArrayInitializerOperation as the child of an array creation.
		// The parent contract supplies the target element type for collection-expression lowering.
		var elementTargetType = GetCollectionElementTargetType(((IArrayCreationOperation)operation.Parent!).Type);
		foreach (var element in operation.ElementValues)
		{
			elements.Add(TranslateTupleForTarget(element, elementTargetType, argument));
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
	public override Node? VisitFieldInitializer(IFieldInitializerOperation operation, SenseArgument argument)
	{
		// An initializer operation is created for at least one field by Roslyn.
		var targetType = operation.InitializedFields[0].Type;
		return TranslateTupleForTarget(operation.Value, targetType, argument);
	}

	/// <summary>
	/// 处理属性初始化器操作
	/// </summary>
	public override Node? VisitPropertyInitializer(IPropertyInitializerOperation operation, SenseArgument argument)
	{
		// An initializer operation is created for at least one property by Roslyn.
		var targetType = operation.InitializedProperties[0].Type;
		return TranslateTupleForTarget(operation.Value, targetType, argument);
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
	public override Node? VisitVariableInitializer(IVariableInitializerOperation operation, SenseArgument argument)
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
	public override Node? VisitVariableDeclarator(IVariableDeclaratorOperation operation, SenseArgument argument)
	{
		if (Host?.ShouldSkipVariableDeclarator(operation, argument) == true)
			return null;

		if (Host?.RewriteVariableDeclaratorPreorder(operation, argument) is VariableDeclarator preorderHostDeclarator)
			return WithOriginIfMissing(preorderHostDeclarator, operation);

		var identifier = Host?.RewriteLocalDeclarationIdentifier(operation.Symbol, operation, argument) ??
			new Identifier(GetJavaScriptBindingName(operation.Symbol));
		var init = operation.Initializer?.Value is IOperation value
			? TranslateTupleForTarget(value, operation.Symbol.Type, argument)
			: null;

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
	public override Node? VisitVariableDeclaration(IVariableDeclarationOperation operation, SenseArgument argument)
	{
		var declarators = new List<VariableDeclarator>();
		foreach (var declarator in operation.Declarators)
			Translate(declarators, declarator, argument);

		if (declarators.Count == 0)
			return null;

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
	public override Node? VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, SenseArgument argument)
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

		if (declarators.Count == 0)
			return null;

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
	public override Node? VisitDeclarationExpression(IDeclarationExpressionOperation operation, SenseArgument argument)
	{
		if (argument.Sense == Sense.OutParameter)
		{
			if (operation.Expression is ILocalReferenceOperation localReference &&
				Host?.RewriteLocalDeclarationIdentifier(localReference.Local, operation, argument) is Identifier declarationIdentifier)
			{
				argument.AddVarDeclarator(new VariableDeclarator(declarationIdentifier, null), _recursionDepth);
				return declarationIdentifier;
			}

			var expr = Translate<Expression>(operation.Expression, argument);
			var declarator = new VariableDeclarator(expr, null);
			argument.AddVarDeclarator(declarator, _recursionDepth);
			return expr;
		}

		return Translate<Expression>(operation.Expression, argument);
	}
}
