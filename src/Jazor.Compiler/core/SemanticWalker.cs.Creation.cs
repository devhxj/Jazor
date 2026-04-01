using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
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
	private Expression BuildObjectCreation(Expression? assignObj, IObjectCreationOperation operation, SenseArgument argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure<Expression>(operation, "Object creation type could not be translated to JavaScript.");

		var arguments = new List<Expression>();
		for (var index = 0; index < operation.Arguments.Length; index++)
		{
			var arg = operation.Arguments[index];
			var targetType = operation.Constructor?.Parameters.Length > index
				? operation.Constructor.Parameters[index].Type
				: arg.Parameter?.Type;
			// 构造器参数同样属于 tuple 边界。
			// 如果实参 tuple 的当前视图和形参 tuple 的目标视图不同，这里直接 remap。
			arguments.Add(TranslateTupleForTarget(arg.Value, targetType, argument));
		}



		// 普通对象创建
		var (mapper, typeName) = GetMapperType(operation.Type);
		var callee = new Identifier(typeName);

		Expression expr = new NewExpression(callee, NodeList.From(arguments));
		if (mapper == TypeMapper.BigInt)
			expr = new CallExpression(callee, NodeList.From(arguments), false);

		else if (mapper == TypeMapper.Array)
			expr = new ArrayExpression(NodeList.From<Expression?>(arguments));

		// 如果构造函数在白名单中，需要特殊处理
		if (operation.Constructor is not null)
		{
			var mapperExpr = GetWhiteListExpression(operation.Constructor, argument, arguments, null, out _);
			if (mapperExpr is not null)
				expr = mapperExpr;
		}			
		
		if (assignObj is not null)
			expr = new AssignmentExpression(Operator.Assignment, assignObj, expr);

		// 如果有初始化器，则需要用IIFE处理
		if (operation.Initializer?.Initializers.Length > 0)
		{
			if (TryBuildCollectionLiteral(operation.Initializer, mapper, argument, out var collectionLiteral))
			{
				expr = collectionLiteral;
				if (assignObj is not null)
					expr = new AssignmentExpression(Operator.Assignment, assignObj, expr);
				return expr;
			}

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

	private bool TryBuildCollectionLiteral(IObjectOrCollectionInitializerOperation initializer, TypeMapper mapper, SenseArgument argument, out Expression expression)
	{
		expression = null!;
		if (mapper is not (TypeMapper.Array or TypeMapper.Set or TypeMapper.Map))
			return false;

		var items = new List<Expression>();
		foreach (var init in initializer.Initializers)
		{
			if (init is not IInvocationOperation invocation)
				return false;

			if (mapper is TypeMapper.Array or TypeMapper.Set)
			{
				if (invocation.Arguments.Length != 1)
					return false;
				items.Add(Translate<Expression>(invocation.Arguments[0].Value, argument));
			}
			else
			{
				if (invocation.Arguments.Length != 2)
					return false;

				var key = Translate<Expression>(invocation.Arguments[0].Value, argument);
				var value = Translate<Expression>(invocation.Arguments[1].Value, argument);
				items.Add(new ArrayExpression(NodeList.From<Expression?>(key, value)));
			}
		}

		if (mapper == TypeMapper.Array)
		{
			expression = new ArrayExpression(NodeList.From<Expression?>(items));
			return true;
		}

		var literalArray = new ArrayExpression(NodeList.From<Expression?>(items));
		var typeName = mapper == TypeMapper.Set ? "Set" : "Map";
		expression = new NewExpression(new Identifier(typeName), NodeList.From<Expression>(literalArray));
		return true;
	}

	/// <summary>
	/// 处理对象创建初始化器
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="initializers"></param>
	/// <param name="argument"></param>
	/// <returns></returns>
	private List<Expression> BuildObjectCreationInitializer(Expression? obj, IObjectOrCollectionInitializerOperation initializers, SenseArgument argument)
	{
		var exprs = new List<Expression>();
		// 处理对象初始化器，只处理第一层，内部嵌套转换为对象字面量
		foreach (var initializer in initializers.Initializers)
		{
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				Expression? left = null;
				IPropertyReferenceOperation? propertyReference = null;
				Expression? propertyInstance = null;
				if (simpleAssignmentOp.Target is IPropertyReferenceOperation propertyReferenceOp)
				{
					propertyReference = propertyReferenceOp;
					propertyInstance = Translate<Expression>(propertyReferenceOp.Instance, argument, null) ?? obj;
					if (propertyReferenceOp.Arguments.Length > 0)
					{
						if (propertyReferenceOp.Arguments.Length != 1 || propertyInstance is null)
							return HandleTransformationFailure<List<Expression>>(initializer, "Indexed initializer target could not be translated to JavaScript.");

						var index = Translate<Expression>(propertyReferenceOp.Arguments[0].Value, argument);
						left = new MemberExpression(propertyInstance, index, computed: true, optional: false);
					}
					else
					{
						var property = new Identifier(GetInitializerMemberName(propertyReferenceOp.Property));
						left = propertyInstance is null
							? property
							: new MemberExpression(propertyInstance, property, computed: false, optional: false);
					}
				}
				else if (simpleAssignmentOp.Target is IFieldReferenceOperation fieldReferenceOp)
				{
					var fieldInstance = Translate<Expression>(fieldReferenceOp.Instance, argument, null) ?? obj;
					var field = new Identifier(GetInitializerMemberName(fieldReferenceOp.Field));
					left = fieldInstance is null
						? field
						: new MemberExpression(fieldInstance, field, computed: false, optional: false);
				}
				else
				{
					var prop = Translate<Expression>(simpleAssignmentOp.Target, argument);
					left = obj is null
						 ? prop
						 : new MemberExpression(obj, prop, computed: false, optional: false);
				}

				if (left is null)
					return HandleTransformationFailure<List<Expression>>(initializer, "Initializer target could not be translated to JavaScript.");

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
					// 对象初始化器里的 tuple 成员赋值也走统一 remap 规则，
					// 避免和普通赋值/参数传递出现不同的运行时 shape。
					// 目标协议取成员的静态类型，而不是右值字面量自己的自然名字。
					var right = TranslateTupleForTarget(simpleAssignmentOp.Value, simpleAssignmentOp.Target.Type, argument);

					if (propertyReference?.Property.SetMethod is not null && propertyInstance is not null)
					{
						var setterArguments = new List<Expression>(propertyReference.Arguments.Length + 1);
						foreach (var propertyArgument in propertyReference.Arguments)
						{
							var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
								? argument.With(Sense.OutParameter)
								: argument;
							setterArguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
						}
						setterArguments.Add(right);

						var mapperExpr = GetWhiteListExpression(propertyReference.Property.SetMethod, argument, setterArguments, propertyInstance, out _);
						if (mapperExpr is not null)
						{
							exprs.Add(mapperExpr);
							continue;
						}
					}

					var expr = new AssignmentExpression(Operator.Assignment, left, right);
					exprs.Add(expr);
				}
			}
		else if (initializer is IMemberInitializerOperation memberInitializerOp)
			{
				var right = RecursiveObjectOrCollectionInitializer(memberInitializerOp.Initializer);

				// 检查属性/字段的白名单 Inline/Import 操作
				ISymbol? memberSymbol = memberInitializerOp.InitializedMember switch
				{
					IPropertyReferenceOperation propertyReferenceOp => (ISymbol?)propertyReferenceOp.Property.SetMethod ?? propertyReferenceOp.Property,
					IFieldReferenceOperation fieldReferenceOp => fieldReferenceOp.Field,
					_ => null
				};

				if (memberSymbol is not null && obj is not null)
				{
					// 对于属性 setter，需要将 obj 和 value 作为参数
					var setterArgs = new List<Expression> { right };
					var mapperExpr = GetWhiteListExpression(memberSymbol, argument, setterArgs, obj, out var alias);
					if (mapperExpr is not null)
					{
						exprs.Add(mapperExpr);
						continue;
					}
				}

				// 普通属性/字段赋值
				var target = memberInitializerOp.InitializedMember switch
				{
					IPropertyReferenceOperation propertyReferenceOp => new Identifier(GetInitializerMemberName(propertyReferenceOp.Property)),
					IFieldReferenceOperation fieldReferenceOp => new Identifier(GetInitializerMemberName(fieldReferenceOp.Field)),
					_ => null
				};

				if (target is null)
					return HandleTransformationFailure<List<Expression>>(initializer, "Member initializer target could not be translated to JavaScript.");

				Expression left = obj is null
					 ? target
					 : new MemberExpression(obj, target, computed: false, optional: false);
				var expr = new AssignmentExpression(Operator.Assignment, left, right);
				exprs.Add(expr);
			}
			else if (initializer is IInvocationOperation invocationOp)
			{
				if (obj is null)
					return HandleTransformationFailure<List<Expression>>(initializer, "Member initializer target could not be translated to JavaScript.");

				var arguments = new List<Expression>();
				foreach (var arg in invocationOp.Arguments)
				{
					var argExpr = TranslateTupleForTarget(arg.Value, arg.Parameter?.Type, argument);
					arguments.Add(argExpr);
				}

				// 检查白名单 Inline/Import 操作
				var mapperExpr = GetWhiteListExpression(invocationOp.TargetMethod, argument, arguments, obj, out var alias);
				if (mapperExpr is not null)
				{
					exprs.Add(mapperExpr);
					continue;
				}

				// 普通方法调用
				var methodName = alias ?? GetMethodConfigOrWhiteListName(invocationOp.TargetMethod);
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
					IPropertyReferenceOperation propertyReferenceOp => new Identifier(GetInitializerMemberName(propertyReferenceOp.Property)),
					IFieldReferenceOperation fieldReferenceOp => new Identifier(GetInitializerMemberName(fieldReferenceOp.Field)),
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
	private Expression? BuildObjectOrCollectionInitializer(Expression? initExpr, IObjectOrCollectionInitializerOperation operation, SenseArgument argument)
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
	public override Node? VisitObjectCreation(IObjectCreationOperation operation, SenseArgument argument)
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
	public override Node? VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, SenseArgument argument)
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
	public override Node? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, SenseArgument argument)
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
	public override Node? VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, SenseArgument argument)
	{
		if (operation.Type is null)
			return HandleTransformationFailure<Node>(operation, "Type parameter object creation type could not be translated to JavaScript.");

		// 泛型类型参数对象创建，忽略泛型参数，当作普通对象创建
		// 使用白名单检查获取类型名称
		var typeName = GetTypeConfigOrWhiteListName(operation.Type);
		if (string.IsNullOrEmpty(typeName))
			return HandleTransformationFailure<Node>(operation, $"Type '{operation.Type.ToDisplayString()}' is not in whitelist and cannot be used for object creation.");

		var callee = new Identifier(typeName!);

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
	public override Node? VisitArrayCreation(IArrayCreationOperation operation, SenseArgument argument)
	{
		if (operation.Type is IArrayTypeSymbol arrayType && arrayType.Rank > 1)
		{
			if (operation.Initializer is not null)
				return BuildNestedArrayInitializer(operation.Initializer, argument);

			var dimensions = operation.DimensionSizes
				.Select(dimension => Translate<Expression>(dimension, argument))
				.ToArray();
			return BuildMultiDimensionalArray(dimensions, 0);
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
				if (dimension.ConstantValue.HasValue)
				{
					switch (dimension.ConstantValue.Value)
					{
						case 0:
						case 0u:
						case 0L:
						case 0UL:
							return new ArrayExpression(NodeList.From<Expression?>());
					}
				}

				var sizeNode = Translate<Expression>(dimension, argument);
				return new NewExpression(new Identifier("Array"), NodeList.From(sizeNode));
			}
		}

		return new ArrayExpression(NodeList.From(elements));
	}

	private Expression BuildNestedArrayInitializer(IArrayInitializerOperation initializer, SenseArgument argument)
	{
		var elements = new List<Expression?>(initializer.ElementValues.Length);
		foreach (var element in initializer.ElementValues)
		{
			if (element is IArrayInitializerOperation nestedInitializer)
				elements.Add(BuildNestedArrayInitializer(nestedInitializer, argument));
			else
				Translate(elements, element, argument, null);
		}

		return new ArrayExpression(NodeList.From(elements));
	}

	private static Expression BuildMultiDimensionalArray(IReadOnlyList<Expression> dimensions, int dimensionIndex)
	{
		var currentArray = new NewExpression(new Identifier("Array"), NodeList.From(dimensions[dimensionIndex]));
		if (dimensionIndex == dimensions.Count - 1)
			return currentArray;

		var fillCall = new CallExpression(
			new MemberExpression(currentArray, new Identifier("fill"), computed: false, optional: false),
			NodeList.From<Expression>(),
			optional: false);

		var nested = BuildMultiDimensionalArray(dimensions, dimensionIndex + 1);
		var mapper = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			nested,
			expression: true,
			async: false);

		return new CallExpression(
			new MemberExpression(fillCall, new Identifier("map"), computed: false, optional: false),
			NodeList.From<Expression>(mapper),
			optional: false);
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
	public override Node? VisitMemberInitializer(IMemberInitializerOperation operation, SenseArgument argument)
	{
		var target = operation.InitializedMember switch
		{
			IPropertyReferenceOperation propertyReferenceOp => new Identifier(GetInitializerMemberName(propertyReferenceOp.Property)),
			IFieldReferenceOperation fieldReferenceOp => new Identifier(GetInitializerMemberName(fieldReferenceOp.Field)),
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
	public override Node? VisitDelegateCreation(IDelegateCreationOperation operation, SenseArgument argument)
	{
		// 委托创建转换为函数引用或箭头函数
		return Visit(operation.Target, argument);
	}	
}
