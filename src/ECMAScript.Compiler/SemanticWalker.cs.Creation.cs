using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理对象创建操作
	/// C# 示例：
	/// new MyClass()               // 无参数构造
	/// new MyClass(arg1, arg2)     // 有参数构造
	/// new { Name = "John", Age = 30 }  // 匿名类型
	/// 转换结果：new MyClass() / new MyClass(arg1, arg2) / { Name: "John", Age: 30 }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitObjectCreation(IObjectCreationOperation operation, Queue<VariableDeclaration> argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure(operation, "Object creation type could not be translated to JavaScript.");

		// 普通对象创建
		var callee = new Identifier(operation.Type.Name);
		var arguments = new List<Expression>();

		foreach (var arg in operation.Arguments)
		{
			Translate(arguments, arg.Value, argument);
		}

		return new NewExpression(callee, NodeList.From(arguments));
	}

	/// <summary>
	/// 处理匿名对象创建操作
	/// C# 示例：
	/// new { Name = "John", Age = 25 }  // 匿名对象
	/// 转换结果：{ name: "John", age: 25 }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, Queue<VariableDeclaration> argument)
	{
		var properties = new List<Node>();

		foreach (var initializer in operation.Initializers)
		{
			Translate(properties, initializer, argument);
		}

		return new ObjectExpression(NodeList.From(properties));
	}

	/// <summary>
	/// 处理泛型对象创建操作
	/// C# 示例：
	/// new T()                            // 泛型类型参数构造
	/// new List<T>()                      // 泛型集合构造
	/// 转换结果：忽略泛型参数，转换为普通对象创建 new T() / new List()
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, Queue<VariableDeclaration> argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure(operation, "Type parameter object creation type could not be translated to JavaScript.");

		// 泛型类型参数对象创建，忽略泛型参数，当作普通对象创建
		var typeName = operation.Type.Name;
		var callee = new Identifier(typeName);

		// 泛型类型参数对象通常使用无参数构造函数
		return new NewExpression(callee, NodeList.From<Expression>());
	}

	/// <summary>
	/// 处理数组创建操作
	/// C# 示例：
	/// new int[] {1, 2, 3}         // 带初始化器的数组
	/// new int[5]                  // 指定大小的数组
	/// new int[,] {{1,2}, {3,4}}   // 多维数组（转为 new Array(size)）
	/// 转换结果：[1, 2, 3] / new Array(5)
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitArrayCreation(IArrayCreationOperation operation, Queue<VariableDeclaration> argument)
	{
		// 检查是否为多维数组
		if (operation.Type is IArrayTypeSymbol arrayType)
		{
			if (arrayType.Rank > 1)
			{
				// 多维数组在 C# 中可以创建但无法访问，导致“能创建却无法访问”的悄论
				// 为保证语义一致性，禁止创建多维数组
				return HandleTransformationFailure(operation, "Array creation with unsupported initializer or dimension.");
			}
		}

		var elements = new List<Expression?>();
		if (operation.Initializer is not null)
		{
			foreach (var element in operation.Initializer.ElementValues)
			{
				Translate(elements, element, argument,null);
			}
		}
		else
		{
			// 处理空数组或基于大小的数组
			foreach (var dimension in operation.DimensionSizes)
			{
				// 为简化，创建一个空数组，实际可能需要 new Array(size)
				var sizeNode = Translate<Expression>(dimension, argument);
				return new NewExpression(new Identifier("Array"), NodeList.From(sizeNode));
			}
		}

		return new ArrayExpression(NodeList.From(elements));
	}

	/// <summary>
	/// 处理对象或集合初始化器操作
	/// C# 示例：
	/// new List<int> { 1, 2, 3 }      // 集合初始化器
	/// new MyClass { Prop1 = val1 }   // 对象初始化器
	/// 转换结果：{ prop1: val1 } / [1, 2, 3]
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, Queue<VariableDeclaration> argument)
	{
		var initializers = new List<Node>();

		foreach (var initializer in operation.Initializers)
		{
			Translate(initializers, initializer, argument);
		}

		// 默认返回对象表达式
		return new ObjectExpression(NodeList.From(initializers));
	}

	/// <summary>
	/// 处理成员初始化器操作
	/// C# 示例：
	/// new MyClass { Property = value } 中的 Property = value 部分
	/// 转换结果：property = value
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitMemberInitializer(IMemberInitializerOperation operation, Queue<VariableDeclaration> argument)
	{
		string memberName;
		if (operation.InitializedMember is IFieldSymbol field)
		{
			memberName = field.Name;
		}
		else if (operation.InitializedMember is IPropertySymbol property)
		{
			memberName = property.Name;
		}
		else
			return HandleTransformationFailure(operation.InitializedMember, "Member initializer could not be translated to JavaScript.");

		var key = new Identifier(memberName);
		var value = Translate<Expression>(operation.Initializer, argument);

		return new AssignmentExpression(Operator.Assignment, key, value);
	}

	/// <summary>
	/// 处理插值字符串操作
	/// C# 示例：
	/// $"Hello, {name}!"           // 插值字符串
	/// $"Value: {x + y}"           // 包含表达式的插值字符串
	/// 转换结果：`Hello${name}!` / `Value: ${(x + y)}`
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolatedString(IInterpolatedStringOperation operation, Queue<VariableDeclaration> argument)
	{
		var quasis = new List<TemplateElement>();
		var expressions = new List<Expression>();

		foreach (var part in operation.Parts)
		{
			switch (part)
			{
				case IInterpolatedStringTextOperation textOp:
					// 遇到文本，直接添加为 quasi
					var literal = textOp.Text as ILiteralOperation;
					var cooked = literal?.ConstantValue.Value as string ?? "";
					quasis.Add(new TemplateElement(
						TemplateValue.From(cooked, cooked),
						tail: false // tail 将在最后统一设置
					));
					break;

				case IInterpolationOperation interpOp:
					// 核心逻辑：在处理表达式前，确保它前面有一个 quasi。
					// 如果当前 quasi 数量不比 expression 多一个，说明前面是表达式或这是开头，需要补一个空的 quasi。
					if (quasis.Count == expressions.Count)
					{
						quasis.Add(new TemplateElement(
							TemplateValue.From("", ""),
							tail: false
						));
					}

					// 转换并添加表达式
					var expr = Visit(interpOp.Expression, argument) as Expression;
					if (expr is not null)
					{
						expressions.Add(expr);
					}
					break;
			}
		}

		// 循环结束后，处理尾部 quasi
		if (quasis.Count == expressions.Count)
		{
			// 如果数量相等，说明字符串以表达式结尾，需要补一个空的尾部 quasi。
			quasis.Add(new TemplateElement(TemplateValue.From("", ""), tail: true));
		}
		else if (quasis.Count > 0)
		{
			// 否则，字符串以文本结尾，将最后一个 quasi 标记为 tail。
			var lastQuasi = quasis[quasis.Count - 1];
			quasis[quasis.Count - 1] = new TemplateElement(lastQuasi.Value, tail: true);
		}

		// 优化：如果没有任何表达式，只有一个文本部分，返回更简单的 StringLiteral。
		if (expressions.Count == 0 && quasis.Count == 1)
		{
			return new StringLiteral(quasis[0].Value.Cooked ?? "", quasis[0].Value.Raw);
		}

		// 返回结构完整的 TemplateLiteral
		return new TemplateLiteral(NodeList.From(quasis), NodeList.From(expressions));
	}
}
