using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 构建对象创建表达式
	/// </summary>
	/// <param name="assignObj"></param>
	/// <param name="operation"></param>
	/// <param name="argument"></param>
	/// <returns></returns>
	private Expression BuildObjectCreation(Expression? assignObj, IObjectCreationOperation operation, WalkerArgument argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure<Expression>(operation, "Object creation type could not be translated to JavaScript.");

		// 普通对象创建
		var mapper = GetMapperType(operation.Type, out var typeName);
		var callee = new Identifier(typeName);
		var arguments = new List<Expression>();

		foreach (var arg in operation.Arguments)
			Translate(arguments, arg.Value, argument);

		Expression expr = new NewExpression(callee, NodeList.From(arguments));
		if (mapper == TypeMapper.BigInt)
			expr = new CallExpression(callee, NodeList.From(arguments), false);

		else if (mapper == TypeMapper.Array)
			expr = new ArrayExpression(NodeList.From<Expression?>(arguments));

		if (assignObj is not null)
			expr = new AssignmentExpression(Operator.Assignment, assignObj, expr);

		// 如果有初始化器，则需要用IIFE处理
		if (operation.Initializer?.Initializers.Length > 0)
		{
			if (assignObj is null)
				return BuildObjectOrCollectionInitializer(expr, operation.Initializer, argument)!;
			else
			{
				var exprs = new List<Expression> { expr };
				var initExprs = BuildObjectCreationInitializer(assignObj, operation.Initializer, argument);
				exprs.AddRange(initExprs);
				return new SequenceExpression(NodeList.From(exprs));
			}
		}

		return expr;
	}

	/// <summary>
	/// 处理对象创建初始化器
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="initializers"></param>
	/// <param name="argument"></param>
	/// <returns></returns>
	private List<Expression> BuildObjectCreationInitializer(Expression? obj, IObjectOrCollectionInitializerOperation initializers, WalkerArgument argument)
	{
		var exprs = new List<Expression>();
		// 处理对象初始化器，只处理第一层，内部嵌套转换为对象字面量
		foreach (var initializer in initializers.Initializers)
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
					var sequenceExpr = BuildObjectCreation(left, subObjectCreationOp, argument);
					if (sequenceExpr is SequenceExpression seqExpr)
						exprs.AddRange(seqExpr.Expressions);
					else
						exprs.Add(sequenceExpr);
				}
				else
				{
					var right = Translate<Expression>(simpleAssignmentOp.Value, argument);
					var expr = new AssignmentExpression(Operator.Assignment, left, right);
					exprs.Add(expr);
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
					return HandleTransformationFailure<List<Expression>>(initializer, "Member initializer target could not be translated to JavaScript.");

				Expression left = obj is null
					 ? target
					 : new MemberExpression(obj, target, computed: false, optional: false);
				var right = RecursiveObjectOrCollectionInitializer(memberInitializerOp.Initializer);
				var expr = new AssignmentExpression(Operator.Assignment, left, right);
				exprs.Add(expr);
			}
			else if (initializer is IInvocationOperation invocationOp)
			{
				if (obj is null)
					return HandleTransformationFailure<List<Expression>>(initializer, "Member initializer target could not be translated to JavaScript.");

				var methodName = invocationOp.TargetMethod.Name;
				var arguments = new List<Expression>();

				foreach (var arg in invocationOp.Arguments)
				{
					var argExpr = Translate<Expression>(arg.Value, argument);
					arguments.Add(argExpr);
				}

				var callee = new MemberExpression(
					obj,
					new Identifier(methodName),
					computed: false,
					optional: false
				);

				var expr = new CallExpression(callee, NodeList.From(arguments), optional: false);
				exprs.Add(expr);
			}
			else
				HandleTransformationFailure<Expression>(initializer, "Member initializer could not be translated to JavaScript.");
		}

		return exprs;
	}

	/// <summary>
	/// 此处主要处理IMemberInitializerOperation.Initializer中或可能嵌套的对象或集合初始化器操作
	/// </summary>
	/// <param name="operation"></param>
	/// <returns>转换为字面量对象</returns>
	private ObjectExpression RecursiveObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation)
	{
		var nodes = new List<Node>();
		foreach (var initializer in operation.Initializers)
		{
			Expression? target = null, value = null;
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				target = Translate<Expression>(simpleAssignmentOp.Target, new());
				value = Translate<Expression>(simpleAssignmentOp.Value, new());
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
					value = RecursiveObjectOrCollectionInitializer(memberInitializerOp.Initializer);
			}

			if (target is null || value is null)
				return HandleTransformationFailure<ObjectExpression>(operation, "Member initializer could not be translated to JavaScript.");

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

	/// <summary>
	///  
	/// </summary>
	/// <param name="initExpr"></param>
	/// <param name="operation"></param>
	/// <param name="argument"></param>
	/// <returns>IIFE箭头函数</returns>
	private Expression? BuildObjectOrCollectionInitializer(Expression? initExpr, IObjectOrCollectionInitializerOperation operation, WalkerArgument argument)
	{
		if (operation.Initializers.Length == 0)
			return null;

		var name = GetUniqueName(operation);
		var obj = new Identifier(name);
		var initExprs = BuildObjectCreationInitializer(obj, operation, argument);
		if (initExpr is null)
			return new SequenceExpression(NodeList.From(initExprs));

		var statements = new List<Statement>();
		// 定义临时变量
		var declarator = new VariableDeclarator(obj, initExpr);
		var declaration = new VariableDeclaration(
			VariableDeclarationKind.Let,
			NodeList.From(declarator)
		);
		statements.Add(declaration);

		// 处理初始化器
		statements.AddRange(initExprs.Select(x => new NonSpecialExpressionStatement(x)));

		// 返回临时变量
		var returnStatement = new ReturnStatement(obj);
		statements.Add(returnStatement);

		// 使用立即调用的箭头函数包装
		var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
		var arrowFunction = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			functionBody,
			expression: false,
			async: false
		);
		return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
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
	public override Node? VisitObjectCreation(IObjectCreationOperation operation, WalkerArgument argument)
		=> BuildObjectCreation(null, operation, argument);

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
	public override Node? VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, WalkerArgument argument)
		=> BuildObjectOrCollectionInitializer(null, operation, argument);
	
	/// <summary>
	/// 处理匿名对象创建操作
	/// C# 示例：
	/// new { Name = "John", Age = 25 }  // 匿名对象
	/// 转换结果：{ name: "John", age: 25 }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, WalkerArgument argument)
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
			else HandleTransformationFailure<Node>(initializer, "Anonymous object initializer could not be translated to JavaScript.");
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
	public override Node? VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, WalkerArgument argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure<Node>(operation, "Type parameter object creation type could not be translated to JavaScript.");

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
	public override Node? VisitArrayCreation(IArrayCreationOperation operation, WalkerArgument argument)
	{
		// 检查是否为多维数组
		if (operation.Type is IArrayTypeSymbol arrayType)
		{
			if (arrayType.Rank > 1)
			{
				// 多维数组在 C# 中可以创建但无法访问，导致“能创建却无法访问”的悄论
				// 为保证语义一致性，禁止创建多维数组
				return HandleTransformationFailure<Node>(operation, "Array creation with unsupported initializer or dimension.");
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

	/// <summary>
	/// 处理成员初始化器操作
	/// C# 示例：
	/// new MyClass { Property = value } 中的 Property = value 部分
	/// 转换结果：property = value
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitMemberInitializer(IMemberInitializerOperation operation, WalkerArgument argument)
	{
		var target = operation.InitializedMember switch
		{
			IPropertyReferenceOperation propertyReferenceOp => new Identifier(propertyReferenceOp.Property.Name),
			IFieldReferenceOperation fieldReferenceOp => new Identifier(fieldReferenceOp.Field.Name),
			_ => null
		};
		if (target is null)
			return HandleTransformationFailure<Node>(operation.InitializedMember, "Member initializer target could not be translated to JavaScript.");

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
	public override Node? VisitDelegateCreation(IDelegateCreationOperation operation, WalkerArgument argument)
	{
		// 委托创建转换为函数引用或箭头函数
		return Visit(operation.Target, argument);
	}	
}
