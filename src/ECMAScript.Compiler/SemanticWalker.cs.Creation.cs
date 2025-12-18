using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	private Acornima.Ast.StatementOrExpression VisitObjectCreation(Expression? simpleAssignmentExpr,
		IObjectCreationOperation operation, Context argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure<StatementOrExpression>(operation, "Object creation type could not be translated to JavaScript.");

		// 普通对象创建
		var callee = new Identifier(operation.Type.Name);
		var arguments = new List<Expression>();

		foreach (var arg in operation.Arguments)
		{
			Translate(arguments, arg.Value, argument);
		}

		Expression expr = new NewExpression(callee, NodeList.From(arguments));

		// 如果祖先是参数类型，中间有个转换
		// IObjectCreationOperation->IConversionOperation->IArgumentOperation
		if (operation.Parent?.Parent is IArgumentOperation argumentOp)
		{
			var definitions = new List<Statement>();
			var name = GetUniqueName(operation.Syntax);
			var obj = new Identifier(name);
			var assignmentExpr = new AssignmentExpression(Operator.Assignment, obj, expr);
			definitions.Add(new NonSpecialExpressionStatement(assignmentExpr));

			if (operation.Initializer is not null)
			{
				var node = VisitObjectOrCollectionInitializer(obj, operation.Initializer, argument);
				if (node is StatementGroup group)
					definitions.AddRange(group.Elements);
				else if (node is Statement statement)
					definitions.Add(statement);
				else
					definitions.Add(new NonSpecialExpressionStatement((Expression)node));
			}

			return new DefinitionExpression(NodeList.From(definitions), obj);
		}

		if (simpleAssignmentExpr is not null)
			expr = new AssignmentExpression(Operator.Assignment, simpleAssignmentExpr, expr);

		// 检查是否应该返回简单的对象创建（当没有复杂的初始化器时）
		if (operation.Initializer is not null)
		{
			var node = VisitObjectOrCollectionInitializer(simpleAssignmentExpr, operation.Initializer, argument);
			if (node is StatementGroup groups)
				return groups.With(expr, false);

			return node;
		}

		return expr;
	}

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
	public override Acornima.Ast.Node? VisitObjectCreation(IObjectCreationOperation operation, Context argument)
		=> VisitObjectCreation(null, operation, argument);
	
	/// <summary>
	/// 处理匿名对象创建操作
	/// C# 示例：
	/// new { Name = "John", Age = 25 }  // 匿名对象
	/// 转换结果：{ name: "John", age: 25 }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, Context argument)
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
	public override Acornima.Ast.Node? VisitArrayCreation(IArrayCreationOperation operation, Context argument)
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

	private Acornima.Ast.StatementOrExpression VisitObjectOrCollectionInitializer(Expression? simpleAssignmentExpr,
		IObjectOrCollectionInitializerOperation operation, Context argument)
	{
		Expression? obj = simpleAssignmentExpr;
		if (obj is null && operation.Parent?.Parent?.Parent is IVariableDeclaratorOperation variableDeclaratorOp)
			obj = new Identifier(variableDeclaratorOp.Symbol.Name);

		// 如果是创建对象
		if (operation.Parent is IObjectCreationOperation objectCreationOp && objectCreationOp.Initializer == operation)
		{
			var initializers = new List<Statement>();
			// 处理对象初始化器，只处理第一层，内部嵌套转换为对象字面量
			foreach (var initializer in objectCreationOp.Initializer.Initializers)
			{
				if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
				{
					var prop = Translate<Expression>(simpleAssignmentOp.Target, argument);
					var left = obj is null
					 	? prop
					 	: new MemberExpression(obj, prop, computed: false, optional: false);
					if (simpleAssignmentOp.Value is IObjectCreationOperation subObjectCreationOp &&
						subObjectCreationOp.Initializer is not null)
					{
						var group = (StatementGroup)VisitObjectCreation(left, subObjectCreationOp, argument);
						initializers.AddRange(group.Elements);
					}
					else
					{
						var right = Translate<Expression>(simpleAssignmentOp.Value, argument);
						var expr = new AssignmentExpression(Operator.Assignment, left, right);
						initializers.Add(new NonSpecialExpressionStatement(expr));
					}
				}
				else if (initializer is IMemberInitializerOperation memberInitializerOp)
				{
					var target = memberInitializerOp.InitializedMember switch
					{
						IPropertyReferenceOperation propertyReferenceOp => new Identifier(propertyReferenceOp.Property.Name),
						IFieldReferenceOperation fieldReferenceOp => new Identifier(fieldReferenceOp.Field.Name),
						_ => null
					};

					if (target is null)
						return HandleTransformationFailure<StatementOrExpression>(initializer, "");

					Expression left = obj is null
					 	? target
					 	: new MemberExpression(obj, target, computed: false, optional: false);
					var right = RecursiveObjectOrCollectionInitializer(left, memberInitializerOp.Initializer);
					var expr = new AssignmentExpression(Operator.Assignment, left, right);
					initializers.Add(new NonSpecialExpressionStatement(expr));
				}
				else if (initializer is IInvocationOperation invocationOp)
				{
					if (obj is null)
						return HandleTransformationFailure<StatementOrExpression>(initializer, "");

					var methodName = invocationOp.TargetMethod.Name;
					var arguments = new List<Expression>();

					foreach (var arg in invocationOp.Arguments)
					{
						var exp = Translate<Expression>(arg.Value, argument);
						if (exp is DefinitionExpression dexp)
						{
							initializers.AddRange(dexp.Definitions);
							arguments.Add(dexp.Expression);
						}
						else
							arguments.Add(exp);
					}

					var callee = new MemberExpression(
						obj,
						new Identifier(methodName),
						computed: false,
						optional: false
					);

					var expr = new CallExpression(callee, NodeList.From(arguments), optional: false);
					initializers.Add(new NonSpecialExpressionStatement(expr));
				}
				else
					HandleTransformationFailure(initializer, "");
			}
			return new StatementGroup(NodeList.From(initializers));
		}

		return HandleTransformationFailure<StatementOrExpression>(operation, "");

		// IObjectCreationOperation.Initializer 在VisitObjectCreation中处理，
		// 此处主要处理IMemberInitializerOperation.Initializer中或可能嵌套的对象或集合初始化器操作
		// 转换为字面量对象
		ObjectExpression RecursiveObjectOrCollectionInitializer(Expression left,IObjectOrCollectionInitializerOperation op)
		{
			var nodes = new List<Node>();
			foreach (var initializer in op.Initializers)
			{
				Expression? target = null, value = null;
				if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
				{
					target = Translate<Expression>(simpleAssignmentOp.Target, []);
					value = Translate<Expression>(simpleAssignmentOp.Value, []);
				}
				else if (initializer is IMemberInitializerOperation memberInitializerOp)
				{
					target = memberInitializerOp.InitializedMember switch
					{
						IPropertyReferenceOperation propertyReferenceOp => new Identifier(propertyReferenceOp.Property.Name),
						IFieldReferenceOperation fieldReferenceOp => new Identifier(fieldReferenceOp.Field.Name),
						_ => null
					};
					if (target is not null)
						value = RecursiveObjectOrCollectionInitializer(target, memberInitializerOp.Initializer);
				}

				if (target is null || value is null)
					return HandleTransformationFailure<ObjectExpression>(op, "");

				var prop = new ObjectProperty(
					PropertyKind.Init,
					key: target,
					value: value,
					computed: false,
					shorthand: false,
					method: false
				);
				nodes.Add(prop);
			}
			return new ObjectExpression(NodeList.From(nodes));
		}
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
	public override Acornima.Ast.Node? VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, Context argument)
		=> VisitObjectOrCollectionInitializer(null, operation, argument);

	/// <summary>
	/// 处理成员初始化器操作
	/// C# 示例：
	/// new MyClass { Property = value } 中的 Property = value 部分
	/// 转换结果：property = value
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitMemberInitializer(IMemberInitializerOperation operation, Context argument)
	{
		var target = operation.InitializedMember switch
		{
			IPropertyReferenceOperation propertyReferenceOp => new Identifier(propertyReferenceOp.Property.Name),
			IFieldReferenceOperation fieldReferenceOp => new Identifier(fieldReferenceOp.Field.Name),
			_ => null
		};
		if (target is null)
			return HandleTransformationFailure(operation.InitializedMember, "");

		var value = Translate<Expression>(operation.Initializer, argument);
		return new ObjectProperty(
			PropertyKind.Init,
			key: target,
			value: value,
			computed: false,
			shorthand: false,
			method: false
		);
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
	public override Acornima.Ast.Node? VisitDelegateCreation(IDelegateCreationOperation operation, Context argument)
	{
		// 委托创建转换为函数引用或箭头函数
		return Visit(operation.Target, argument);
	}	
}
