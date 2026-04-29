using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

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

		if (operation.Type is INamedTypeSymbol namedType &&
			IsEcmascriptRecordLike(namedType))
			return BuildEcmascriptRecordLiteral(assignObj, operation, argument);

		RejectUnsupportedTypeFallback(operation, operation.Type, "object creation");

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

		Expression? mappedConstructor = null;
		if (operation.Constructor is not null)
		{
			mappedConstructor = GetWhiteListExpression(operation.Constructor, argument, arguments, null, out _);
			if (mappedConstructor is null &&
				!IsIntrinsicObjectCreationFallbackAllowed(operation.Constructor, operation.Type))
				RejectUnsupportedRuntimeFallback(operation, operation.Constructor, "object creation", operation.Type);
		}

		// 普通对象创建
		var (mapper, typeName) = GetMapperType(operation.Type);
		// 类型引用统一走 BuildFullTypeName：
		// 用户代码中的成员类型会按声明侧规则折叠成运行时扁平名，
		// ECMAScript 宿主绑定则保留必要的层级/导入语义。
		var callee = BuildFullTypeName(operation.Type, argument) ?? new Identifier(typeName);

		Expression expr = new NewExpression(callee, NodeList.From(arguments));
		if (mapper == TypeMapper.BigInt)
			expr = new CallExpression(callee, NodeList.From(arguments), false);

		else if (mapper == TypeMapper.Array)
			expr = new ArrayExpression(NodeList.From<Expression?>(arguments));

		if (mappedConstructor is not null)
			expr = mappedConstructor;
		
		if (assignObj is not null)
			expr = new AssignmentExpression(Operator.Assignment, assignObj, expr);

		// 如果有初始化器，则需要用IIFE处理
		if (operation.Initializer?.Initializers.Length > 0)
		{
			if (operation.Type.ContainingAssembly?.Name == "ECMAScript" &&
				Util.HasNameResolutionBoundary(operation.Type))
				return RecursiveObjectOrCollectionInitializer(operation.Initializer, argument);

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

	private bool IsIntrinsicObjectCreationFallbackAllowed(IMethodSymbol constructor, ITypeSymbol constructedType)
	{
		if (constructor.MethodKind != MethodKind.Constructor)
			return false;

		var (mapper, typeName) = GetMapperType(constructedType);
		return mapper switch
		{
			TypeMapper.BigInt => constructor.Parameters.Length <= 1 &&
				constructor.Parameters.All(parameter =>
				{
					var mapper = GetMapperType(parameter.Type).Mapper;
					return mapper is TypeMapper.Number or TypeMapper.BigInt;
				}),
			TypeMapper.Class => IsNativeErrorConstructorFallbackAllowed(constructedType, typeName),
			_ => false
		};
	}

	private static bool IsNativeErrorConstructorFallbackAllowed(ITypeSymbol constructedType, string typeName)
	{
		if (typeName is not "Error" and not "TypeError")
			return false;

		for (var current = constructedType.OriginalDefinition; current is not null; current = current.BaseType)
		{
			if (current.Name == "Exception" &&
				current.ContainingNamespace?.ToDisplayString() == "System")
				return true;
		}

		return false;
	}

	private static bool IsEcmascriptRecordLike(ITypeSymbol? typeSymbol)
		=> typeSymbol is INamedTypeSymbol namedType &&
		   namedType.IsRecord &&
		   (namedType.ContainingAssembly?.Name == "ECMAScript" ||
			HasEcmascriptSupportMarker(namedType) ||
			HasEcmascriptSupportMarkerBaseType(namedType));

	private static bool HasEcmascriptSupportMarkerBaseType(INamedTypeSymbol typeSymbol)
	{
		for (var current = typeSymbol.BaseType; current is not null; current = current.BaseType)
		{
			if (HasEcmascriptSupportMarker(current))
				return true;
		}

		return false;
	}

	private Expression BuildEcmascriptRecordLiteral(Expression? assignObj, IObjectCreationOperation operation, SenseArgument argument)
	{
		if (operation.Type is not INamedTypeSymbol namedType)
			return HandleTransformationFailure<Expression>(operation, "ECMAScript record type could not be translated to JavaScript.");

		var nodes = new List<Node>();
		for (var index = 0; index < operation.Arguments.Length; index++)
		{
			var arg = operation.Arguments[index];
			if (arg.ArgumentKind == ArgumentKind.DefaultValue)
				continue;

			var parameter = arg.Parameter ??
				(operation.Constructor is not null && operation.Constructor.Parameters.Length > index
					? operation.Constructor.Parameters[index]
					: null);
			var keyName = ResolveEcmascriptRecordPropertyName(namedType, parameter, index);
			var value = TranslateTupleForTarget(arg.Value, parameter?.Type, argument);
			nodes.Add(new ObjectProperty(
				PropertyKind.Init,
				key: new Identifier(keyName),
				value: value,
				computed: false,
				shorthand: false,
				method: false));
		}

		if (operation.Initializer is not null)
			nodes.AddRange(BuildObjectLiteralMembers(operation.Initializer, argument));

		Expression expr = new ObjectExpression(NodeList.From(nodes));
		if (assignObj is not null)
			expr = new AssignmentExpression(Operator.Assignment, assignObj, expr);

		return expr;
	}

	private static string ResolveEcmascriptRecordPropertyName(INamedTypeSymbol type, IParameterSymbol? parameter, int index)
	{
		if (parameter is null)
			return $"item{index}";

		var property = type
			.GetMembers()
			.OfType<IPropertySymbol>()
			.FirstOrDefault(member =>
				!member.IsStatic &&
				string.Equals(member.Name, parameter.Name, System.StringComparison.OrdinalIgnoreCase));

		return property is null
			? parameter.Name
			: Util.GetConfigOrSymbolName(property);
	}

	private bool TryBuildCollectionLiteral(IObjectOrCollectionInitializerOperation initializer, TypeMapper mapper, SenseArgument argument, out Expression expression)
	{
		expression = null!;
		if (mapper is not (TypeMapper.Array or TypeMapper.Set or TypeMapper.Map))
			return false;

		var items = new List<Expression>();
		foreach (var init in initializer.Initializers)
		{
			if (mapper == TypeMapper.Map &&
				init is ISimpleAssignmentOperation simpleAssignment &&
				simpleAssignment.Target is IPropertyReferenceOperation propertyReference &&
				propertyReference.Arguments.Length == 1)
			{
				// 集合字面量直接落成 JS array/Set/Map 时，同样不能绕开 tuple 边界规则。
				// 这里沿用索引器/参数的目标类型做 remap，避免集合元素里的 tuple 名字透传错误。
				var key = TranslateTupleForTarget(
					propertyReference.Arguments[0].Value,
					propertyReference.Arguments[0].Parameter?.Type,
					argument);
				var value = TranslateTupleForTarget(simpleAssignment.Value, propertyReference.Property.Type, argument);
				items.Add(new ArrayExpression(NodeList.From<Expression?>(key, value)));
				continue;
			}

			if (init is not IInvocationOperation invocation)
				return false;

			if (mapper is TypeMapper.Array or TypeMapper.Set)
			{
				if (invocation.Arguments.Length != 1)
					return false;
				items.Add(TranslateTupleForTarget(
					invocation.Arguments[0].Value,
					invocation.Arguments[0].Parameter?.Type,
					argument));
			}
			else
			{
				if (invocation.Arguments.Length != 2)
					return false;

				var key = TranslateTupleForTarget(
					invocation.Arguments[0].Value,
					invocation.Arguments[0].Parameter?.Type,
					argument);
				var value = TranslateTupleForTarget(
					invocation.Arguments[1].Value,
					invocation.Arguments[1].Parameter?.Type,
					argument);
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

	private string ResolveInitializerAssignmentMemberName(IOperation operation, ISymbol symbol, string usage, ITypeSymbol? hostType = null)
	{
		var validationSymbol = symbol switch
		{
			IPropertySymbol { SetMethod: not null } property => (ISymbol)property.SetMethod,
			_ => symbol
		};

		if (TryGetWhiteListValue(WhiteList.Members, validationSymbol, out _, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		RejectUnsupportedRuntimeFallback(operation, validationSymbol, usage, hostType);
		return Util.GetConfigOrSymbolName(symbol);
	}

	private string ResolveInitializerAccessMemberName(IOperation operation, ISymbol symbol, string usage, ITypeSymbol? hostType = null)
	{
		var validationSymbol = symbol switch
		{
			IPropertySymbol { GetMethod: not null } property => (ISymbol)property.GetMethod,
			_ => symbol
		};

		if (TryGetWhiteListValue(WhiteList.Members, validationSymbol, out _, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		RejectUnsupportedRuntimeFallback(operation, validationSymbol, usage, hostType);
		return Util.GetConfigOrSymbolName(symbol);
	}

	private Expression BuildMemberInitializerReceiver(
		IMemberInitializerOperation operation,
		Expression? fallbackInstance,
		SenseArgument argument)
	{
		switch (operation.InitializedMember)
		{
			case IPropertyReferenceOperation propertyReference:
			{
				if (propertyReference.Property.GetMethod is null)
				{
					return HandleTransformationFailure<Expression>(
						operation,
						$"Member initializer target '{propertyReference.Property.ToDisplayString(Jazor.Common.Format.NameFormat)}' must have an accessible getter.");
				}

				var instance = Translate<Expression>(propertyReference.Instance, argument, null) ?? fallbackInstance;
				var arguments = new List<Expression>(propertyReference.Arguments.Length);
				foreach (var propertyArgument in propertyReference.Arguments)
				{
					var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
						? argument.With(Sense.OutParameter)
						: argument;
					arguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
				}

				var mapperExpr = GetWhiteListExpression(propertyReference.Property.GetMethod, argument, arguments, instance, out var alias);
				if (mapperExpr is not null)
					return mapperExpr;

				if (instance is not null &&
					arguments.Count > 0 &&
					(propertyReference.Property.IsIndexer || propertyReference.Property.Parameters.Length > 0))
				{
					if (arguments.Count != 1)
					{
						return HandleTransformationFailure<Expression>(
							operation,
							"JavaScript fallback for indexer member initializers only supports a single translated index argument.");
					}

					if (string.IsNullOrEmpty(alias))
					{
						ResolveInitializerAccessMemberName(
							operation,
							propertyReference.Property,
							"member initializer access",
							propertyReference.Instance?.Type ?? propertyReference.Property.ContainingType);
					}

					return new MemberExpression(instance, arguments[0], computed: true, optional: false);
				}

				var propertyName = string.IsNullOrEmpty(alias)
					? ResolveInitializerAccessMemberName(
						operation,
						propertyReference.Property,
						"member initializer access",
						propertyReference.Instance?.Type ?? propertyReference.Property.ContainingType)
					: alias!;

				if (instance is not null)
				{
					return BuildAliasedPropertyAccess(
						instance,
						propertyName,
						optional: false,
						ShouldInvokeAliasedPropertyGetter(propertyReference, propertyName));
				}

				if (propertyReference.Property.IsStatic && propertyReference.Property.ContainingType is not null)
				{
					if (TryBuildImportedModulePropertyAccess(propertyReference.Property, argument, out var importedProperty) &&
						importedProperty is not null)
						return importedProperty;

					if (TryBuildPreferredRuntimeStaticMemberAccess(propertyReference.Property, propertyReference.Syntax, propertyReference.SemanticModel, propertyName, out var preferredStaticProperty) &&
						preferredStaticProperty is not null)
						return preferredStaticProperty;

					var containing = BuildFullTypeName(propertyReference.Property.ContainingType, argument);
					if (containing is not null)
						return new MemberExpression(containing, new Identifier(propertyName), computed: false, optional: false);
				}

				return new Identifier(propertyName);
			}

			case IFieldReferenceOperation fieldReference:
			{
				var instance = Translate<Expression>(fieldReference.Instance, argument, null) ?? fallbackInstance;
				var mapperExpr = GetWhiteListExpression(fieldReference.Field, argument, [], instance, out var alias);
				if (mapperExpr is not null)
					return mapperExpr;

				var fieldName = string.IsNullOrEmpty(alias)
					? ResolveInitializerAccessMemberName(
						operation,
						fieldReference.Field,
						"member initializer access",
						fieldReference.Instance?.Type ?? fieldReference.Field.ContainingType)
					: alias!;

				if (instance is not null)
					return new MemberExpression(instance, new Identifier(fieldName), computed: false, optional: false);

				if (fieldReference.Field.IsStatic && fieldReference.Field.ContainingType is not null)
				{
					if (TryBuildImportedModuleMember(fieldReference.Field.ContainingType, fieldName, argument, out var importedMember) &&
						importedMember is not null)
						return importedMember;

					var containing = BuildFullTypeName(fieldReference.Field.ContainingType, argument);
					if (containing is not null)
						return new MemberExpression(containing, new Identifier(fieldName), computed: false, optional: false);
				}

				return new Identifier(fieldName);
			}

			default:
				return HandleTransformationFailure<Expression>(operation, "Member initializer target could not be translated to a JavaScript receiver.");
		}
	}

	private Expression MaterializeMemberInitializerReceiver(
		Expression receiver,
		IOperation ownerOperation,
		SenseArgument argument,
		List<Expression> initializations)
	{
		if (CanDuplicateReadWriteTarget(receiver))
			return receiver;

		if (!NeedsSingleEvaluationCaching(receiver))
			return receiver;

		var tempId = CreatePropertyMutationTemp(ownerOperation, argument, "init");
		initializations.Add(new AssignmentExpression(Operator.Assignment, tempId, receiver));
		return tempId;
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
						var propertyName = ResolveInitializerAssignmentMemberName(
							simpleAssignmentOp,
							propertyReferenceOp.Property,
							"object initializer property assignment",
							propertyReferenceOp.Instance?.Type ?? propertyReferenceOp.Property.ContainingType);
						var property = new Identifier(propertyName);
						left = propertyInstance is null
							? property
							: new MemberExpression(propertyInstance, property, computed: false, optional: false);
					}
				}
				else if (simpleAssignmentOp.Target is IFieldReferenceOperation fieldReferenceOp)
				{
					var fieldInstance = Translate<Expression>(fieldReferenceOp.Instance, argument, null) ?? obj;
					var fieldName = ResolveInitializerAssignmentMemberName(
						simpleAssignmentOp,
						fieldReferenceOp.Field,
						"object initializer field assignment",
						fieldReferenceOp.Instance?.Type ?? fieldReferenceOp.Field.ContainingType);
					var field = new Identifier(fieldName);
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

						var mapperExpr = GetWhiteListExpression(propertyReference.Property.SetMethod, argument, setterArguments, propertyInstance, out var setterAlias);
						if (mapperExpr is not null)
						{
							exprs.Add(mapperExpr);
							continue;
						}

						if (string.IsNullOrEmpty(setterAlias))
							RejectUnsupportedRuntimeFallback(simpleAssignmentOp, propertyReference.Property.SetMethod, "object initializer property assignment", propertyReference.Instance?.Type ?? propertyReference.Property.ContainingType);
					}

					var expr = new AssignmentExpression(Operator.Assignment, left, right);
					exprs.Add(expr);
				}
			}
			else if (initializer is IMemberInitializerOperation memberInitializerOp)
			{
				if (obj is null)
					return HandleTransformationFailure<List<Expression>>(initializer, "Member initializer target could not be translated to JavaScript.");

				var receiver = BuildMemberInitializerReceiver(memberInitializerOp, obj, argument);
				receiver = MaterializeMemberInitializerReceiver(receiver, memberInitializerOp, argument, exprs);
				var nestedExprs = BuildObjectCreationInitializer(receiver, memberInitializerOp.Initializer, argument);
				exprs.AddRange(nestedExprs);
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

				if (string.IsNullOrEmpty(alias))
					RejectUnsupportedRuntimeFallback(invocationOp, invocationOp.TargetMethod, "initializer method invocation", invocationOp.Instance?.Type ?? invocationOp.TargetMethod.ContainingType);

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
	private List<Node> BuildObjectLiteralMembers(IObjectOrCollectionInitializerOperation operation, SenseArgument argument)
	{
		var nodes = new List<Node>();
		foreach (var initializer in operation.Initializers)
		{
			Expression? target = null, value = null;
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				target = simpleAssignmentOp.Target switch
				{
					IPropertyReferenceOperation propertyReferenceOp => new Identifier(ResolveInitializerAssignmentMemberName(
						simpleAssignmentOp,
						propertyReferenceOp.Property,
						"object literal member initialization",
						propertyReferenceOp.Instance?.Type ?? propertyReferenceOp.Property.ContainingType)),
					IFieldReferenceOperation fieldReferenceOp => new Identifier(ResolveInitializerAssignmentMemberName(
						simpleAssignmentOp,
						fieldReferenceOp.Field,
						"object literal member initialization",
						fieldReferenceOp.Instance?.Type ?? fieldReferenceOp.Field.ContainingType)),
					_ => Translate<Expression>(simpleAssignmentOp.Target, argument)
				};
				value = TranslateTupleForTarget(simpleAssignmentOp.Value, simpleAssignmentOp.Target.Type, argument);
			}
			else if (initializer is IMemberInitializerOperation memberInitializerOp)
			{
				target = memberInitializerOp.InitializedMember switch
				{
					IPropertyReferenceOperation propertyReferenceOp => new Identifier(ResolveInitializerAssignmentMemberName(
						memberInitializerOp,
						propertyReferenceOp.Property,
						"object literal member initialization",
						propertyReferenceOp.Instance?.Type ?? propertyReferenceOp.Property.ContainingType)),
					IFieldReferenceOperation fieldReferenceOp => new Identifier(ResolveInitializerAssignmentMemberName(
						memberInitializerOp,
						fieldReferenceOp.Field,
						"object literal member initialization",
						fieldReferenceOp.Instance?.Type ?? fieldReferenceOp.Field.ContainingType)),
					_ => null
				};

				if (target is not null)
					value = RecursiveObjectOrCollectionInitializer(memberInitializerOp.Initializer, argument);
			}

			if (target is null || value is null)
				return HandleTransformationFailure<List<Node>>(operation, "Member initializer could not be translated to JavaScript.");

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
		return nodes;
	}

	private ObjectExpression RecursiveObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, SenseArgument argument)
		=> new(NodeList.From(BuildObjectLiteralMembers(operation, argument)));

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

		var iifeArg = EnsureScopeContext(operation, argument).EnterEmissionScope(operation, ScopeSite.ObjectInitializerIife());
		var name = AllocateUniqueName(operation, iifeArg, LoweringSite.CreationTemp());
		var obj = new Identifier(name);
		var initExprs = BuildObjectCreationInitializer(obj, operation, iifeArg);
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
		var functionBody = new FunctionBody(NodeList.From(MaterializeScopedStatements(iifeArg, statements)), strict: true);
		var arrowFunction = new ArrowFunctionExpression(
			NodeList.From<Node>(),
			functionBody,
			expression: false,
			async: ContainsAwaitOperation(operation)
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
		return HandleTransformationFailure<Node>(
			operation,
			"Type-parameter object creation ('new T()') is not supported. JavaScript output has no runtime constructor binding for C# generic type parameters, so emitting 'new T()' would be semantically invalid.");
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
		if (operation.Type is not null)
			RejectUnsupportedTypeFallback(operation, operation.Type, "array creation");

		if (operation.Type is IArrayTypeSymbol arrayType && arrayType.Rank > 1)
		{
			if (operation.Initializer is not null)
				return BuildNestedArrayInitializer(operation.Initializer, argument, arrayType.ElementType);

			var dimensions = operation.DimensionSizes
				.Select(dimension => Translate<Expression>(dimension, argument))
				.ToArray();
			return BuildMultiDimensionalArray(dimensions, 0);
		}

		var elements = new List<Expression?>();
		if (operation.Initializer is not null)
		{
			var elementTargetType = GetCollectionElementTargetType(operation.Type);
			foreach (var element in operation.Initializer.ElementValues)
			{
				elements.Add(TranslateTupleForTarget(element, elementTargetType, argument));
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

	private Expression BuildNestedArrayInitializer(IArrayInitializerOperation initializer, SenseArgument argument, ITypeSymbol? elementTargetType = null)
	{
		var elements = new List<Expression?>(initializer.ElementValues.Length);
		foreach (var element in initializer.ElementValues)
		{
			if (element is IArrayInitializerOperation nestedInitializer)
				elements.Add(BuildNestedArrayInitializer(nestedInitializer, argument, elementTargetType));
			else
				elements.Add(TranslateTupleForTarget(element, elementTargetType, argument));
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
			IPropertyReferenceOperation propertyReferenceOp => new Identifier(ResolveInitializerAssignmentMemberName(
				operation,
				propertyReferenceOp.Property,
				"member initializer assignment",
				propertyReferenceOp.Instance?.Type ?? propertyReferenceOp.Property.ContainingType)),
			IFieldReferenceOperation fieldReferenceOp => new Identifier(ResolveInitializerAssignmentMemberName(
				operation,
				fieldReferenceOp.Field,
				"member initializer assignment",
				fieldReferenceOp.Instance?.Type ?? fieldReferenceOp.Field.ContainingType)),
			_ => null
		};
		if (target is null)
			return HandleTransformationFailure<Node>(operation.InitializedMember, "Member initializer target could not be translated to JavaScript.");

		var targetType = operation.InitializedMember switch
		{
			IPropertyReferenceOperation propertyReferenceOp => propertyReferenceOp.Property.Type,
			IFieldReferenceOperation fieldReferenceOp => fieldReferenceOp.Field.Type,
			_ => null
		};
		var value = TranslateTupleForTarget(operation.Initializer, targetType, argument);
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
