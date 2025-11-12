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

		var newExpr = new NewExpression(callee, NodeList.From(arguments));

		if (operation.Initializer is null)
			return newExpr;

		Expression? obj = null;
		if (operation.Parent?.Parent is IVariableDeclaratorOperation variableDeclaratorOp)
		{
			obj = new Identifier(variableDeclaratorOp.Symbol.Name);
		}
		else if (operation.Parent?.Parent is ISimpleAssignmentOperation simpleAssignmentOp)
		{
			obj = Translate<Expression>(simpleAssignmentOp.Target, argument);
		}

		var initializers = new List<Statement>() { new NonSpecialExpressionStatement(newExpr) };
		foreach (var initializer in operation.Initializer.Initializers)
		{
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				var prop = Translate<Expression>(simpleAssignmentOp.Target, argument);
				var value = Translate<Expression>(simpleAssignmentOp.Value, argument);
				var left = new MemberExpression(
					obj,
					prop,
					computed: false,
					optional: false
				);
				var expr = new AssignmentExpression(Operator.Assignment, left, value);
				initializers.Add(new NonSpecialExpressionStatement(expr));
			}
			else if (initializer is IMemberInitializerOperation memberInitializerOp)
			{
				if (memberInitializerOp.InitializedMember is IPropertyReferenceOperation propertyReferenceOp)
				{

				}
				if (memberInitializerOp.InitializedMember is IFieldReferenceOperation fieldReferenceOperation)
                {
                    
                }				
			}

			//var expr = Translate<Expression>(initializer, argument);
			//initializers.Add(new NonSpecialExpressionStatement(expr));
		}		
		return new StatementGroup(NodeList.From(initializers));
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
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				var value = Translate<Expression>(simpleAssignmentOp.Value, argument);
				var key = Translate<Expression>(simpleAssignmentOp.Target, argument);
				var prop = new ObjectProperty(
					PropertyKind.Init,
					key: key,
					value: value,
					computed: false,
					shorthand: false,
					method: false
				);
				properties.Add(prop);
			}
			else HandleTransformationFailure(initializer, "");
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
				Translate(elements, element, argument, null);
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

	private List<Statement> TranslateObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, Queue<VariableDeclaration> argument)
	{
		var initializers = new List<Statement>();

		foreach (var initializer in operation.Initializers)
		{
			var expr = Translate<Expression>(initializer, argument);
			initializers.Add(new NonSpecialExpressionStatement(expr));
		}

		return initializers;
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
		var initializers = TranslateObjectOrCollectionInitializer(operation, argument);
		return new StatementGroup(NodeList.From(initializers));
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
	/// 处理委托创建操作
	/// C# 示例：
	/// Action action = Method;              // 方法组转委托
	/// Func<int, string> func = x => x.ToString(); // Lambda 转委托
	/// EventHandler handler = new EventHandler(OnEvent); // 显式委托创建
	/// 转换结果：转换为函数引用或箭头函数
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDelegateCreation(IDelegateCreationOperation operation, Queue<VariableDeclaration> argument)
	{
		// 委托创建转换为函数引用或箭头函数
		return Visit(operation.Target, argument);
	}	
}
