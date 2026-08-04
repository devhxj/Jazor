using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// 负责把 C# 的对象、数组、集合、匿名函数及相关初始化语义降为 JavaScript AST。
/// </summary>
/// <remarks>
/// 创建语义与普通成员访问不同：它可能同时涉及构造函数选择、属性初始化、spread 展开和
/// structural record 擦除。这里必须保留初始化顺序；不能因为目标是对象字面量就跳过必要的
/// 构造函数或把不支持的外部类型静默当成普通对象。
/// </remarks>
public partial class SemanticWalker
{
	private const string SpreadAttributeFullName = "ECMAScript.SpreadAttribute";
	private const string SymbolFullName = "ECMAScript.Symbol";

	/// <summary>
	/// 构建对象创建表达式
	/// </summary>
	/// <param name="operation"></param>
	/// <param name="argument"></param>
	/// <returns></returns>
	private Expression BuildObjectCreation(IObjectCreationOperation operation, SenseArgument argument)
	{
		// A legal IObjectCreationOperation is fully bound. Type, constructor, and each argument's
		// parameter are Roslyn contracts; null only belongs to invalid-operation recovery trees.
		var type = (INamedTypeSymbol)operation.Type!;
		var constructor = operation.Constructor!;

		if (ShouldLowerStructurally(type))
			return BuildStructuralLiteral(operation, argument);

		RejectUnsupportedTypeFallback(operation, type, "object creation");
		RejectUnsupportedNativeMapSetEqualityBoundaryIfNeeded(operation, type, "object creation");

		var loweredArguments = new List<LoweredBoundArgument>(operation.Arguments.Length);
		var lastSuppliedParameterOrdinal = GetLastSuppliedParameterOrdinal(operation.Arguments);
		for (var index = 0; index < operation.Arguments.Length; index++)
		{
			var arg = operation.Arguments[index];
			if (arg.ArgumentKind == ArgumentKind.DefaultValue &&
				arg.Parameter!.Ordinal > lastSuppliedParameterOrdinal)
				continue;
			// 构造器参数同样属于 tuple 边界。具名参数和 ref/out 位置都通过同一条
			// bound-argument lowering；前者保持源码求值顺序，后者保证回写目标不重算。
			loweredArguments.Add(LowerBoundArgument(operation, arg, argument));
		}
		var orderedArguments = CanonicalizeBoundArguments(operation, loweredArguments, argument);
		var arguments = orderedArguments.Select(static item => item.Value).ToList();
		var refParameters = orderedArguments
			.Where(static item => item.Operation.Parameter!.RefKind is RefKind.Out or RefKind.Ref)
			.Select(static item => item.WriteBackTarget)
			.ToList();

		if (IsObjectLiteralHostType(type))
		{
			if (arguments.Count != 0)
			{
				return HandleTransformationFailure<Expression>(
					operation,
					$"Object-literal host type '{type.ToDisplayString(Format.NameFormat)}' does not support constructor arguments.");
			}

			return operation.Initializer?.Initializers.Length > 0
				? RecursiveObjectOrCollectionInitializer(operation.Initializer, argument)
				: new ObjectExpression(NodeList.Empty<Node>());
		}

		var mappedConstructor = GetWhiteListExpression(constructor, argument, arguments, null, out _, operation);
		if (mappedConstructor is null &&
			!IsIntrinsicObjectCreationFallbackAllowed(constructor, type))
			RejectUnsupportedRuntimeFallback(operation, constructor, "object creation", type);

		var usesMemberConstructorRefOutSink =
			refParameters.Count > 0 &&
			mappedConstructor is null &&
			TryGetCurrentModuleDeclaredName(type, out _);
		if (refParameters.Count > 0 && !usesMemberConstructorRefOutSink)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Constructor '{constructor.OriginalDefinition.ToDisplayString(Format.NameFormat)}' has ref/out parameters but does not participate in the current-module constructor sink protocol.");
		}

		// 普通对象创建
		var (mapper, typeName) = GetMapperType(type);
		// 类型引用统一走 BuildFullTypeName：
		// 用户代码中的成员类型会按声明侧规则折叠成运行时扁平名，
		// ECMAScript 宿主绑定则保留必要的层级/导入语义。
		var callee = BuildFullTypeName(type, argument) ?? new Identifier(typeName);
		if (ShouldEmitMemberConstructorSelector(constructor))
		{
			var helperName = Util.GetMemberConstructorHelperName(constructor);
			arguments.Insert(0, CreateStringLiteral(helperName));
		}

		Expression expr = usesMemberConstructorRefOutSink
			? BuildMemberConstructorRefOutCreation(operation, callee, arguments, refParameters, argument)
			: new NewExpression(callee, NodeList.From(arguments));
		if (mapper == TypeMapper.BigInt)
			expr = new CallExpression(callee, NodeList.From(arguments), false);

		if (mappedConstructor is not null)
			expr = mappedConstructor;
		
		// 如果有初始化器，则需要用IIFE处理
		if (operation.Initializer?.Initializers.Length > 0)
		{
			// A mapped constructor may validate arguments, establish private runtime state, or select
			// host equality semantics. Replacing it with a physical literal would also bypass the
			// bound Add/indexer mappings, so the literal fast path is valid only for native hosts.
			if (mappedConstructor is null &&
				TryBuildCollectionLiteral(operation.Initializer, mapper, argument, out var collectionLiteral))
			{
				expr = collectionLiteral;
			}
			else
			{
				// Nested object creation must complete before the outer property/indexer setter runs.
				// JavaScript evaluates an assignment target before its RHS, so wrapping only after
				// the IIFE also preserves receiver/key evaluation order without rereading the target.
				expr = BuildObjectOrCollectionInitializer(expr, operation.Initializer, argument)!;
			}
		}

		return expr;
	}

	private Expression BuildMemberConstructorRefOutCreation(
		IObjectCreationOperation operation,
		Expression callee,
		List<Expression> arguments,
		IReadOnlyList<Expression?> refParameters,
		SenseArgument argument)
	{
		var sink = new Identifier(AllocateUniqueName(operation, argument, new LoweringSite(LoweringSiteKind.CreationTemp, "refout-sink")));
		var instance = new Identifier(AllocateUniqueName(operation, argument, new LoweringSite(LoweringSiteKind.CreationTemp, "refout-instance")));
		argument.AddVarDeclarator(new VariableDeclarator(sink, null), _recursionDepth);
		argument.AddVarDeclarator(new VariableDeclarator(instance, null), _recursionDepth);

		var sinkInitialization = new AssignmentExpression(
			Operator.Assignment,
			sink,
			new ArrayExpression(NodeList.Empty<Expression?>()));
		arguments.Add(sinkInitialization);

		var expressions = new List<Expression>
		{
			new AssignmentExpression(
				Operator.Assignment,
				instance,
				new NewExpression(callee, NodeList.From(arguments)))
		};
		for (var index = 0; index < refParameters.Count; index++)
		{
			if (refParameters[index] is null)
				continue;

			expressions.Add(new AssignmentExpression(
				Operator.Assignment,
				refParameters[index]!,
				new MemberExpression(
					sink,
					new NumericLiteral(index, index.ToString(System.Globalization.CultureInfo.InvariantCulture)),
					computed: true,
					optional: false)));
		}

		expressions.Add(instance);
		return new SequenceExpression(NodeList.From(expressions));
	}

	private bool ShouldEmitMemberConstructorSelector(IMethodSymbol constructor)
	{
		var containingType = constructor.ContainingType!;

		if (containingType.InstanceConstructors.Count(static ctor => !ctor.IsImplicitlyDeclared) <= 1)
			return false;

		if (TryGetCurrentModuleDeclaredName(containingType, out _))
			return true;

		for (var current = containingType.ContainingType; current is not null; current = current.ContainingType)
		{
			if (current.GetAttributes().Any(static attribute =>
				attribute.AttributeClass!.ToDisplayString() == Util.ECMAScriptModuleAttributeMetadataName))
				return true;
		}

		return false;
	}

	private bool IsIntrinsicObjectCreationFallbackAllowed(IMethodSymbol constructor, ITypeSymbol constructedType)
	{
		var (mapper, typeName) = GetMapperType(constructedType);
		return mapper switch
		{
			// Parameterized BigInteger/Int128 constructors are owned by explicit CLR mappings.
			// The only intrinsic gap is the legal parameterless BigInteger default constructor.
			TypeMapper.BigInt => constructor.Parameters.Length == 0,
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

	private bool ShouldLowerStructurally(ITypeSymbol? typeSymbol)
		=> IsStructuralType(typeSymbol);

	private static bool HasEcmascriptSupportMarkerBaseType(INamedTypeSymbol typeSymbol)
		=> Util.HasECMAScriptSupportMarkerBaseType(typeSymbol);

	private static bool IsObjectLiteralHostType(ITypeSymbol? typeSymbol)
		=> Util.IsObjectLiteralHostType(typeSymbol);

	private Expression BuildStructuralLiteral(IObjectCreationOperation operation, SenseArgument argument)
	{
		var namedType = (INamedTypeSymbol)operation.Type!;

		var memberOrder = BuildStructuralMemberOrderMap(namedType);
		var members = new List<(Node Node, string Name, int Order)>();
		for (var index = 0; index < operation.Arguments.Length; index++)
		{
			var arg = operation.Arguments[index];
			if (arg.ArgumentKind == ArgumentKind.DefaultValue)
				continue;

			var parameter = arg.Parameter!;
			if (IsStaticallyKnownNull(arg.Value))
				continue;

			ResolveStructuralRuntimeMember(
				namedType,
				parameter,
				out var structuralMember,
				out var keyName,
				out var targetType);

			var property = structuralMember as IPropertySymbol;
			if (property is not null && TryGetSpreadAttribute(property, out _))
			{
				AppendExpandedRecordMembers(property, arg.Value, argument, members, memberOrder, operation);
				continue;
			}

			var value = TranslateTupleForTarget(arg.Value, targetType, argument);
			members.Add((new ObjectProperty(
				PropertyKind.Init,
				key: CreateObjectPropertyKey(keyName),
				value: value,
				computed: false,
				shorthand: false,
				method: false),
				keyName,
				GetRecordStructuralMemberOrder(memberOrder, keyName)));
		}

		if (operation.Initializer is not null)
		{
			var initializerNodes = BuildObjectLiteralMembers(operation.Initializer, argument, expandRecordMembers: true);
			for (var index = 0; index < initializerNodes.Count; index++)
			{
				members.Add((
					initializerNodes[index].Node,
					initializerNodes[index].Name,
				GetRecordStructuralMemberOrder(memberOrder, initializerNodes[index].OrderSymbol)));
			}
		}

		AppendInferredRecordMembers(namedType, operation.Initializer, memberOrder, members, operation);

		return new ObjectExpression(NodeList.From(members.Select(static member => member.Node)));
	}

	private Dictionary<string, int> BuildStructuralMemberOrderMap(INamedTypeSymbol type)
	{
		var order = new Dictionary<string, int>(System.StringComparer.Ordinal);
		var members = EnumerateNamedTypeHierarchyBaseFirst(type)
			.SelectMany(static current => current.GetMembers().OfType<IPropertySymbol>());

		var index = 0;
		foreach (var member in members)
		{
			if (member.IsStatic)
				continue;

			var memberName = Util.GetConfigOrSymbolName(member);
			if (!order.ContainsKey(memberName))
			{
				order.Add(memberName, index);
				index++;
			}
		}

		return order;
	}

	private static void ResolveStructuralRuntimeMember(
		INamedTypeSymbol type,
		IParameterSymbol parameter,
		out ISymbol member,
		out string memberName,
		out ITypeSymbol memberType)
	{
		var property = ResolveStructuralRuntimeProperty(type, parameter);
		member = property is not null ? property : parameter;
		memberName = Util.GetConfigOrSymbolName(member);
		memberType = property?.Type ?? parameter.Type;
	}

	private static int GetRecordStructuralMemberOrder(IReadOnlyDictionary<string, int> memberOrder, string memberName)
		=> memberOrder.TryGetValue(memberName, out var order) ? order : int.MaxValue;

	private static int GetRecordStructuralMemberOrder(IReadOnlyDictionary<string, int> memberOrder, ISymbol? memberSymbol)
	{
		if (memberSymbol is null)
			return int.MaxValue;

		return GetRecordStructuralMemberOrder(memberOrder, Util.GetConfigOrSymbolName(memberSymbol));
	}

	private static bool IsStaticallyKnownNull(IOperation? operation)
		=> operation is not null &&
		   operation.ConstantValue.HasValue &&
		   operation.ConstantValue.Value is null;

	private bool ShouldOmitStaticNullObjectLiteralMember(
		IOperation initializer,
		ISymbol? orderSymbol,
		ITypeSymbol? hostType)
	{
		if (!IsStaticallyKnownNull(initializer))
			return false;

		if (orderSymbol is IPropertySymbol propertySymbol &&
			(TryGetSpreadAttribute(propertySymbol, out _) ||
			 propertySymbol.Parameters.Length == 0 &&
			 propertySymbol.ContainingType is INamedTypeSymbol containingRecord &&
			 ShouldLowerStructurally(containingRecord)))
		{
			return true;
		}

		return IsVueDictionaryHostType(hostType);
	}

	private static bool IsVueDictionaryHostType(ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedType)
			return false;

		for (var current = namedType; current is not null; current = current.BaseType)
		{
			var display = current.OriginalDefinition.ToDisplayString(Format.NameFormat);
			if (display is "ECMAScript.Vue3.VueDictionary" or "ECMAScript.Vue3.VueDictionary<TValue>")
			{
				return true;
			}
		}

		return false;
	}

	private static IPropertySymbol? ResolveStructuralRuntimeProperty(
		INamedTypeSymbol type,
		IParameterSymbol parameter)
	{
		// A derived record may forward base positional properties through its primary constructor.
		// Resolve the direct property first, then inherited instance properties, so construction and
		// positional-pattern lowering agree on the same runtime key.
		for (var current = type; current is not null; current = current.BaseType)
		{
			var property = current
				.GetMembers()
				.OfType<IPropertySymbol>()
				.FirstOrDefault(member =>
					!member.IsStatic &&
					string.Equals(member.Name, parameter.Name, System.StringComparison.OrdinalIgnoreCase));
			if (property is not null)
				return property;
		}

		return null;
	}

	private static bool TryGetSpreadAttribute(IPropertySymbol property, out AttributeData attribute)
	{
		foreach (var candidate in property.GetAttributes())
		{
			if (candidate.AttributeClass!.ToDisplayString() == SpreadAttributeFullName)
			{
				attribute = candidate;
				return true;
			}
		}

		attribute = null!;
		return false;
	}

	private void AppendExpandedRecordMembers(
		IPropertySymbol property,
		IOperation valueOperation,
		SenseArgument argument,
		List<(Node Node, string Name, int Order)> members,
		IReadOnlyDictionary<string, int> memberOrder,
		IOperation originOperation)
	{
		var expandedExpression = TranslateTupleForTarget(valueOperation, property.Type, argument);
		if (expandedExpression is ObjectExpression literal)
		{
			foreach (var propertyNode in literal.Properties)
			{
				var nodeInfo = CreateObjectLiteralNode(propertyNode, null);
				members.Add((
					nodeInfo.Node,
					nodeInfo.Name,
					GetObjectLiteralNodeOrder(memberOrder, nodeInfo)));
			}

			return;
		}

		members.Add((
			new SpreadElement(expandedExpression),
			string.Empty,
			GetRecordStructuralMemberOrder(memberOrder, property)));
	}

	private static int GetObjectLiteralNodeOrder(IReadOnlyDictionary<string, int> memberOrder, ObjectLiteralNode node)
		=> string.IsNullOrEmpty(node.Name)
			? int.MaxValue
			: GetRecordStructuralMemberOrder(memberOrder, node.Name);

	private void AppendInferredRecordMembers(
		INamedTypeSymbol type,
		IObjectOrCollectionInitializerOperation? initializer,
		IReadOnlyDictionary<string, int> memberOrder,
		List<(Node Node, string Name, int Order)> members,
		IOperation originOperation)
	{
		var seenContractMembers = new HashSet<string>(StringComparer.Ordinal);
		foreach (var current in EnumerateNamedTypeHierarchyBaseFirst(type))
		{
			foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
			{
				if (property.IsStatic ||
					property.IsIndexer)
				{
					continue;
				}

				var memberName = Util.GetConfigOrSymbolName(property);
				if (!seenContractMembers.Add(memberName) ||
					members.Any(member => string.Equals(member.Name, memberName, System.StringComparison.Ordinal)))
				{
					continue;
				}

				ObjectProperty? inferredMember = null;
				if (TryGetPropsAttribute(property, out var typeArgumentIndex))
					inferredMember = BuildPropsInferenceMember(type, property, typeArgumentIndex, originOperation);
				else if (TryGetEmitsAttribute(property, out var sourceMemberName))
					inferredMember = BuildEmitsInferenceMember(type, property, sourceMemberName, initializer, originOperation);

				if (inferredMember is null)
					continue;

				var memberOrderValue = GetRecordStructuralMemberOrder(memberOrder, memberName);
				var insertIndex = members.FindIndex(member => member.Order > memberOrderValue);
				var orderedMember = (WithOriginIfMissing(inferredMember, originOperation), memberName, memberOrderValue);
				if (insertIndex < 0)
					members.Add(orderedMember);
				else
					members.Insert(insertIndex, orderedMember);
			}
		}
	}

	private static ISymbol? GetObjectInitializerMemberSymbol(IOperation initializer)
		=> initializer switch
		{
			ISimpleAssignmentOperation { Target: IMemberReferenceOperation memberReference }
				=> memberReference.Member,
			IMemberInitializerOperation { InitializedMember: IMemberReferenceOperation memberReference }
				=> memberReference.Member,
			_ => null
		};

	private static string GetObjectInitializerMemberName(IOperation initializer)
	{
		var symbol = GetObjectInitializerMemberSymbol(initializer);
		return symbol is null
			? string.Empty
			: Util.GetConfigOrSymbolName(symbol);
	}

	private static bool TryGetPropsAttribute(IPropertySymbol property, out int typeArgumentIndex)
	{
		foreach (var attribute in property.GetAttributes())
		{
			if (TryReadPropsAttribute(attribute, out typeArgumentIndex))
				return true;
		}

		typeArgumentIndex = default;
		return false;
	}

	private static bool TryReadPropsAttribute(AttributeData attribute, out int typeArgumentIndex)
	{
		typeArgumentIndex = PropsAttribute.DefaultTypeArgumentIndex;
		if (attribute.AttributeClass!.ToDisplayString() != typeof(PropsAttribute).FullName)
			return false;

		foreach (var namedArgument in attribute.NamedArguments)
		{
			if (string.Equals(namedArgument.Key, nameof(PropsAttribute.TypeArgumentIndex), StringComparison.Ordinal))
				typeArgumentIndex = (int)namedArgument.Value.Value!;
		}

		return true;
	}

	private static bool TryGetEmitsAttribute(IPropertySymbol property, out string sourceMemberName)
	{
		foreach (var attribute in property.GetAttributes())
		{
			if (TryReadEmitsAttribute(attribute, out sourceMemberName))
				return true;
		}

		sourceMemberName = null!;
		return false;
	}

	private static bool TryReadEmitsAttribute(AttributeData attribute, out string sourceMemberName)
	{
		sourceMemberName = EmitsAttribute.DefaultSourceMemberName;
		if (attribute.AttributeClass!.ToDisplayString() != typeof(EmitsAttribute).FullName)
			return false;

		foreach (var namedArgument in attribute.NamedArguments)
		{
			if (string.Equals(namedArgument.Key, nameof(EmitsAttribute.SourceMemberName), StringComparison.Ordinal))
				sourceMemberName = (string)namedArgument.Value.Value!;
		}

		return true;
	}

	private ObjectProperty BuildPropsInferenceMember(
		INamedTypeSymbol recordType,
		IPropertySymbol targetProperty,
		int typeArgumentIndex,
		IOperation originOperation)
	{
		if (targetProperty.Type is not IArrayTypeSymbol
			{
				ElementType.SpecialType: SpecialType.System_String
			})
		{
			return HandleTransformationFailure<ObjectProperty>(
				originOperation,
				$"[Props] can only be applied to string[] members, but '{targetProperty.ToDisplayString(Format.NameFormat)}' has type '{targetProperty.Type.ToDisplayString(Format.NameFormat)}'.");
		}

		if (typeArgumentIndex < 0)
		{
			return HandleTransformationFailure<ObjectProperty>(
				originOperation,
				$"[Props] on '{targetProperty.ToDisplayString(Format.NameFormat)}' must declare a non-negative TypeArgumentIndex.");
		}

		if (!TryCollectTypeArgumentPublicInstancePropertyNames(recordType, typeArgumentIndex, out var values))
		{
			return HandleTransformationFailure<ObjectProperty>(
				originOperation,
				$"[Props] requires '{recordType.ToDisplayString(Format.NameFormat)}' to provide a named generic type argument at index {typeArgumentIndex} for property-name inference.");
		}

		return BuildStringArrayRecordMember(targetProperty, values);
	}

	private ObjectProperty BuildEmitsInferenceMember(
		INamedTypeSymbol recordType,
		IPropertySymbol targetProperty,
		string sourceMemberName,
		IObjectOrCollectionInitializerOperation? initializer,
		IOperation originOperation)
	{
		if (targetProperty.Type is not IArrayTypeSymbol
			{
				ElementType.SpecialType: SpecialType.System_String
			})
		{
			return HandleTransformationFailure<ObjectProperty>(
				originOperation,
				$"[Emits] can only be applied to string[] members, but '{targetProperty.ToDisplayString(Format.NameFormat)}' has type '{targetProperty.Type.ToDisplayString(Format.NameFormat)}'.");
		}

		var values = CollectEmitNames(recordType, initializer, originOperation, targetProperty, sourceMemberName);
		return BuildStringArrayRecordMember(targetProperty, values);
	}

	private static bool TryCollectTypeArgumentPublicInstancePropertyNames(
		INamedTypeSymbol recordType,
		int typeArgumentIndex,
		out List<string> names)
	{
		names = null!;
		if (typeArgumentIndex < 0 ||
			recordType.TypeArguments.Length <= typeArgumentIndex ||
			recordType.TypeArguments[typeArgumentIndex] is not INamedTypeSymbol sourceType)
			return false;

		names = CollectPublicInstancePropertyNames(sourceType);
		return true;
	}

	private List<string> CollectEmitNames(
		INamedTypeSymbol recordType,
		IObjectOrCollectionInitializerOperation? initializer,
		IOperation originOperation,
		IPropertySymbol targetProperty,
		string sourceMemberName)
	{
		if (string.IsNullOrWhiteSpace(sourceMemberName))
		{
			return HandleTransformationFailure<List<string>>(
				originOperation,
				$"[Emits] on '{targetProperty.ToDisplayString(Format.NameFormat)}' must declare a non-empty SourceMemberName.");
		}

		if (!TryResolveInstanceProperty(recordType, sourceMemberName, out var sourceProperty))
		{
			return HandleTransformationFailure<List<string>>(
				originOperation,
				$"[Emits] source member '{sourceMemberName}' configured on '{targetProperty.ToDisplayString(Format.NameFormat)}' was not found on '{recordType.ToDisplayString(Format.NameFormat)}'.");
		}

		if (initializer is null ||
			!TryGetInitializerAssignedValue(initializer, sourceProperty, out var setupValue))
		{
			return new List<string>();
		}

		if (!TryResolveEmitInferenceRoot(setupValue, out var rootOperation, out var emitContextParameter))
		{
			return HandleTransformationFailure<List<string>>(
				originOperation,
				$"[Emits] could not analyze the setup callback assigned in '{recordType.ToDisplayString(Format.NameFormat)}'. Use an inline lambda or a source-declared method group, or set Emits explicitly.");
		}

		if (emitContextParameter is null)
			return new List<string>();

		var names = new List<string>();
		var seen = new HashSet<string>(System.StringComparer.Ordinal);
		foreach (var operation in EnumerateSelfAndDescendants(rootOperation))
		{
			if (operation is IParameterReferenceOperation parameterReference &&
				SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, emitContextParameter) &&
				!IsSupportedEmitContextUsage(parameterReference))
			{
				return HandleTransformationFailure<List<string>>(
					originOperation,
					$"[Emits] only supports direct setup-context member usage in '{recordType.ToDisplayString(Format.NameFormat)}'. If the context is passed around or aliased, set Emits explicitly.");
			}

			if (operation is not IInvocationOperation invocation ||
				!string.Equals(invocation.TargetMethod.Name, "Emit", System.StringComparison.Ordinal) ||
				!IsEmitContextInvocation(invocation.Instance, emitContextParameter))
			{
				continue;
			}

			if (invocation.Arguments.Length == 0 ||
				invocation.Arguments[0].Value.ConstantValue is not { HasValue: true, Value: string emitName } ||
				string.IsNullOrWhiteSpace(emitName))
			{
				return HandleTransformationFailure<List<string>>(
					originOperation,
					$"[Emits] requires literal non-empty event names in setup emit calls for '{recordType.ToDisplayString(Format.NameFormat)}'. Use context.Emit(\"event\") or set Emits explicitly.");
			}

			if (seen.Add(emitName))
				names.Add(emitName);
		}

		return names;
	}

	private static bool TryResolveInstanceProperty(INamedTypeSymbol recordType, string clrPropertyName, out IPropertySymbol property)
	{
		for (var current = recordType; current is not null; current = current.BaseType)
		{
			var match = current.GetMembers(clrPropertyName)
				.OfType<IPropertySymbol>()
				.FirstOrDefault(static candidate => !candidate.IsStatic && !candidate.IsIndexer);
			if (match is not null)
			{
				property = match;
				return true;
			}
		}

		property = null!;
		return false;
	}

	private static bool TryGetInitializerAssignedValue(
		IObjectOrCollectionInitializerOperation initializer,
		IPropertySymbol targetProperty,
		out IOperation value)
	{
		foreach (var item in initializer.Initializers)
		{
			if (item is not ISimpleAssignmentOperation
				{
					Target: IPropertyReferenceOperation { Property: var property },
					Value: var assignedValue
				})
			{
				continue;
			}

			if (SymbolEqualityComparer.Default.Equals(property.OriginalDefinition, targetProperty.OriginalDefinition))
			{
				value = assignedValue;
				return true;
			}
		}

		value = null!;
		return false;
	}

	private bool TryResolveEmitInferenceRoot(
		IOperation operation,
		out IOperation rootOperation,
		out IParameterSymbol? emitContextParameter)
	{
		switch (UnwrapEmitInferenceOperation(operation))
		{
			case IMethodReferenceOperation methodReference:
				emitContextParameter = FindEmitContextParameter(methodReference.Method.Parameters);
				return TryGetMethodOperationRoot(methodReference.Method, operation.SemanticModel!, out rootOperation);
			case IAnonymousFunctionOperation anonymousFunction:
				rootOperation = anonymousFunction.Body;
				emitContextParameter = FindEmitContextParameter(anonymousFunction.Symbol.Parameters);
				return true;
			default:
				rootOperation = null!;
				emitContextParameter = null;
				return false;
		}
	}

	private static IOperation UnwrapEmitInferenceOperation(IOperation operation)
	{
		var current = operation;
		while (true)
		{
			switch (current)
			{
				case IConversionOperation conversion:
					current = conversion.Operand;
					continue;
				case IDelegateCreationOperation delegateCreation:
					current = delegateCreation.Target;
					continue;
				default:
					return current;
			}
		}
	}

	private bool TryGetMethodOperationRoot(
		IMethodSymbol method,
		SemanticModel semanticModel,
		out IOperation rootOperation)
	{
		foreach (var reference in method.DeclaringSyntaxReferences)
		{
			var syntax = reference.GetSyntax(_cancellationToken);
			var declarationModel = semanticModel.Compilation.GetSemanticModel(syntax.SyntaxTree);

			// A legal source declaration body owned by this compilation always has one
			// Roslyn operation root. Metadata/abstract methods still fall through below.
			switch (syntax)
			{
				case MethodDeclarationSyntax methodDeclaration when methodDeclaration.Body is not null:
					rootOperation = declarationModel.GetOperation(methodDeclaration.Body, _cancellationToken)!;
					return true;
				case MethodDeclarationSyntax methodDeclaration when methodDeclaration.ExpressionBody is not null:
					rootOperation = declarationModel.GetOperation(methodDeclaration.ExpressionBody.Expression, _cancellationToken)!;
					return true;
				case LocalFunctionStatementSyntax localFunction when localFunction.Body is not null:
					rootOperation = declarationModel.GetOperation(localFunction.Body, _cancellationToken)!;
					return true;
				case LocalFunctionStatementSyntax localFunction:
					// A valid local function always has either the block handled above or an expression body.
					rootOperation = declarationModel.GetOperation(localFunction.ExpressionBody!.Expression, _cancellationToken)!;
					return true;
			}
		}

		rootOperation = null!;
		return false;
	}

	private static IParameterSymbol? FindEmitContextParameter(IEnumerable<IParameterSymbol> parameters)
		=> parameters.FirstOrDefault(static parameter =>
			parameter.Type is INamedTypeSymbol namedType &&
			EnumerateNamedTypeHierarchyBaseFirst(namedType)
				.SelectMany(static current => current.GetMembers("Emit").OfType<IMethodSymbol>())
				.Any(static method =>
					!method.IsStatic &&
					method.Parameters.Length > 0 &&
					method.Parameters[0].Type.SpecialType == SpecialType.System_String));

	private static bool IsEmitContextInvocation(IOperation? instance, IParameterSymbol emitContextParameter)
		=> UnwrapEmitInvocationInstance(instance) is IParameterReferenceOperation parameterReference &&
		   SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, emitContextParameter);

	private static IOperation? UnwrapEmitInvocationInstance(IOperation? operation)
	{
		var current = operation;
		while (true)
		{
			switch (current)
			{
				case IConversionOperation conversion:
					current = conversion.Operand;
					continue;
				default:
					return current;
			}
		}
	}

	private static bool IsSupportedEmitContextUsage(IParameterReferenceOperation parameterReference)
	{
		IOperation current = parameterReference;
		while (true)
		{
			switch (current.Parent)
			{
				case IConversionOperation conversion when ReferenceEquals(conversion.Operand, current):
					current = conversion;
					continue;
				case IInvocationOperation invocation when ReferenceEquals(invocation.Instance, current):
					return true;
				case IPropertyReferenceOperation propertyReference when ReferenceEquals(propertyReference.Instance, current):
					return true;
				case IFieldReferenceOperation fieldReference when ReferenceEquals(fieldReference.Instance, current):
					return true;
				default:
					return false;
			}
		}
	}

	private static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation rootOperation)
	{
		yield return rootOperation;
		foreach (var operation in rootOperation.Descendants())
			yield return operation;
	}

	private static ObjectProperty BuildStringArrayRecordMember(IPropertySymbol targetProperty, IEnumerable<string> values)
	{
		var elements = values
			.Select(static name => (Expression?)JavaScriptAstFactory.CreateStringLiteral(name));

		return new ObjectProperty(
			PropertyKind.Init,
			key: CreateObjectPropertyKey(Util.GetConfigOrSymbolName(targetProperty)),
			value: new ArrayExpression(NodeList.From(elements)),
			computed: false,
			shorthand: false,
			method: false);
	}

	private static List<string> CollectPublicInstancePropertyNames(INamedTypeSymbol type)
	{
		var names = new List<string>();
		var seen = new HashSet<string>(System.StringComparer.Ordinal);
		foreach (var current in EnumerateNamedTypeHierarchyBaseFirst(type))
		{
			foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
			{
				if (property.IsStatic ||
					property.IsIndexer ||
					property.DeclaredAccessibility != Accessibility.Public ||
					property.GetMethod is null)
				{
					continue;
				}

				var name = Util.GetConfigOrSymbolName(property);
				if (seen.Add(name))
					names.Add(name);
			}
		}

		return names;
	}

	private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypeHierarchyBaseFirst(INamedTypeSymbol type)
	{
		var types = new Stack<INamedTypeSymbol>();
		for (var current = type; current is not null; current = current.BaseType)
			types.Push(current);

		while (types.Count > 0)
			yield return types.Pop();
	}

	private static Expression CreateObjectPropertyKey(string name)
	{
		// Object literal keys accept JavaScript IdentifierName, which is wider than
		// binding identifiers and still allows keywords like "class" without quoting.
		return JavaScriptAstFactory.IsJavaScriptIdentifierName(name)
			? new Identifier(name)
			: CreateStringLiteral(name);
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
					propertyReference.Arguments[0].Parameter!.Type,
					argument);
				var value = TranslateTupleForTarget(simpleAssignment.Value, propertyReference.Property.Type, argument);
				items.Add(new ArrayExpression(NodeList.From<Expression?>(key, value)));
				continue;
			}

			if (init is not IInvocationOperation invocation)
				return false;

			if (mapper is TypeMapper.Array or TypeMapper.Set)
			{
				// Bound List/Set collection initializers select their one-argument Add contract.
				items.Add(TranslateTupleForTarget(
					invocation.Arguments[0].Value,
					invocation.Arguments[0].Parameter!.Type,
					argument));
			}
			else
			{
				// Bound Map collection initializers select Dictionary.Add(key, value).
				var key = TranslateTupleForTarget(
					invocation.Arguments[0].Value,
					invocation.Arguments[0].Parameter!.Type,
					argument);
				var value = TranslateTupleForTarget(
					invocation.Arguments[1].Value,
					invocation.Arguments[1].Parameter!.Type,
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

	private bool TryTranslateObjectLiteralPropertyKey(
		IOperation operation,
		ITypeSymbol? keyType,
		SenseArgument argument,
		out Expression key,
		out bool computed)
	{
		if (operation.ConstantValue is { HasValue: true, Value: string literalKey })
		{
			key = CreateObjectPropertyKey(literalKey);
			computed = false;
			return true;
		}

		if (TryCreateNumericObjectPropertyKey(operation, keyType, out key, out computed))
		{
			return true;
		}

		if (IsObjectLiteralComputedKeyType(keyType))
		{
			key = TranslateTupleForTarget(operation, keyType, argument);
			computed = true;
			return true;
		}

		key = null!;
		computed = false;
		return false;
	}

	private static bool TryCreateNumericObjectPropertyKey(
		IOperation operation,
		ITypeSymbol? keyType,
		out Expression key,
		out bool computed)
	{
		key = null!;
		computed = false;
		if (keyType is not INamedTypeSymbol namedType ||
			namedType.OriginalDefinition.ToDisplayString(Format.NameFormat) != "ECMAScript.Number")
		{
			return false;
		}

		if (!TryExtractNumericObjectKeyLiteral(operation, out var literal))
			return false;

		key = literal;
		computed = literal is not NumericLiteral;
		return true;
	}

	private static bool TryExtractNumericObjectKeyLiteral(
		IOperation operation,
		out Expression literal)
	{
		if (operation.ConstantValue is { HasValue: true, Value: not null } constantValue &&
			TryCreateNumericLiteral(constantValue.Value, out literal))
		{
			return true;
		}

		if (operation is IConversionOperation conversion &&
			TryExtractNumericObjectKeyLiteral(conversion.Operand, out literal))
		{
			return true;
		}

		literal = null!;
		return false;
	}

	private static bool TryCreateNumericLiteral(
		object value,
		out Expression literal)
	{
		var createdLiteral = value switch
		{
			byte numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			sbyte numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			short numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			ushort numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			int numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			uint numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			float numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)),
			double numberValue => JavaScriptAstFactory.CreateNumericExpression(numberValue, numberValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)),
			decimal numberValue => JavaScriptAstFactory.CreateNumericExpression(System.Convert.ToDouble(numberValue), numberValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
			_ => null
		};

		if (createdLiteral is null)
		{
			literal = null!;
			return false;
		}

		literal = createdLiteral;
		return true;
	}

	private static bool IsObjectLiteralComputedKeyType(ITypeSymbol? keyType)
		=> keyType is INamedTypeSymbol namedType &&
		   namedType.OriginalDefinition.ToDisplayString() == SymbolFullName;

	private static bool IsObjectLiteralNumericKeyType(ITypeSymbol? keyType)
		=> keyType is INamedTypeSymbol namedType &&
		   namedType.OriginalDefinition.ToDisplayString(Format.NameFormat) == "ECMAScript.Number";

	private void RejectUnsupportedDynamicObjectLiteralKey(IOperation operation, ITypeSymbol? hostType, string usage)
	{
		var hostDisplay = hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>";
		HandleTransformationFailure<Node>(
			operation,
			$"unsupported dynamic object key in {usage} for '{hostDisplay}'. Object-literal host types only support compile-time string literal keys, plus computed Symbol keys when the indexer/Add contract explicitly declares ECMAScript.Symbol.");
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

		if (!IsStructuralMember(symbol))
			RejectUnsupportedRuntimeFallback(operation, validationSymbol, usage, hostType);

		return GetCurrentModuleDeclaredOrConfigName(symbol);
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

		if (!IsStructuralMember(symbol))
			RejectUnsupportedRuntimeFallback(operation, validationSymbol, usage, hostType);

		return GetCurrentModuleDeclaredOrConfigName(symbol);
	}

	private Expression BuildMemberInitializerReceiver(
		IMemberInitializerOperation operation,
		Expression? fallbackInstance,
		SenseArgument argument)
	{
		// A bound nested member initializer targets either a property/indexer or a field.
		if (operation.InitializedMember is IPropertyReferenceOperation propertyReference)
		{
			var instance = Translate<Expression>(propertyReference.Instance, argument, null) ?? fallbackInstance!;
			var arguments = new List<Expression>(propertyReference.Arguments.Length);
			foreach (var propertyArgument in propertyReference.Arguments)
				arguments.Add(Translate<Expression>(propertyArgument.Value, argument));

			var mapperExpr = GetWhiteListExpression(propertyReference.Property.GetMethod!, argument, arguments, instance, out var alias, propertyReference);
			if (mapperExpr is not null)
				return mapperExpr;

			if (arguments.Count > 0)
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
						propertyReference.Instance!.Type!);
			}

				return new MemberExpression(instance, arguments[0], computed: true, optional: false);
			}

			var propertyName = string.IsNullOrEmpty(alias)
				? ResolveInitializerAccessMemberName(
					operation,
					propertyReference.Property,
					"member initializer access",
					propertyReference.Instance!.Type!)
				: alias!;

			return BuildAliasedPropertyAccess(
				instance,
				propertyName,
				optional: false);
		}

		var fieldReference = (IFieldReferenceOperation)operation.InitializedMember;
		var fieldInstance = Translate<Expression>(fieldReference.Instance, argument, null) ?? fallbackInstance!;
		var fieldMapperExpr = GetWhiteListExpression(fieldReference.Field, argument, [], fieldInstance, out var fieldAlias, fieldReference);
		if (fieldMapperExpr is not null)
			return fieldMapperExpr;

		var fieldName = string.IsNullOrEmpty(fieldAlias)
			? ResolveInitializerAccessMemberName(
				operation,
				fieldReference.Field,
				"member initializer access",
				fieldReference.Instance!.Type!)
			: fieldAlias!;

		return BuildFieldAccess(fieldInstance, fieldReference.Field, fieldName, optional: false);
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
	private List<Expression> BuildObjectCreationInitializer(Expression obj, IObjectOrCollectionInitializerOperation initializers, SenseArgument argument)
	{
		var exprs = new List<Expression>();
		// Roslyn binds valid entries as property/field assignments, nested member initializers,
		// or Add invocations; bound arguments always carry their selected parameter.
		foreach (var initializer in initializers.Initializers)
		{
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				Expression left;
				IPropertyReferenceOperation? propertyReference = null;
				Expression? propertyInstance = null;
				if (simpleAssignmentOp.Target is IPropertyReferenceOperation propertyReferenceOp)
				{
					propertyReference = propertyReferenceOp;
					propertyInstance = Translate<Expression>(propertyReferenceOp.Instance, argument, null) ?? obj;
					if (propertyReferenceOp.Arguments.Length > 0)
					{
						if (propertyReferenceOp.Arguments.Length != 1)
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
							propertyReferenceOp.Instance!.Type!);
						left = BuildAliasedPropertyAccess(propertyInstance, propertyName, optional: false);
					}
				}
				else
				{
					var fieldReferenceOp = (IFieldReferenceOperation)simpleAssignmentOp.Target;
					var fieldInstance = Translate<Expression>(fieldReferenceOp.Instance, argument, null) ?? obj;
					var fieldName = ResolveInitializerAssignmentMemberName(
						simpleAssignmentOp,
						fieldReferenceOp.Field,
						"object initializer field assignment",
						fieldReferenceOp.Instance!.Type!);
					left = BuildFieldAccess(fieldInstance, fieldReferenceOp.Field, fieldName, optional: false);
				}

				// Nested creation produces one RHS expression (usually an IIFE). It must still flow
				// through the same mapped setter path as every other initializer assignment.
				var right = simpleAssignmentOp.Value is IObjectCreationOperation subObjectCreationOp &&
					subObjectCreationOp.Initializer is not null
					? BuildObjectCreation(subObjectCreationOp, argument)
					: TranslateTupleForTarget(simpleAssignmentOp.Value, simpleAssignmentOp.Target.Type, argument);

				if (propertyReference is not null)
				{
					var setMethod = propertyReference.Property.SetMethod!;
					var setterArguments = new List<Expression>(propertyReference.Arguments.Length + 1);
					foreach (var propertyArgument in propertyReference.Arguments)
						setterArguments.Add(Translate<Expression>(propertyArgument.Value, argument));
					setterArguments.Add(right);

					var mapperExpr = GetWhiteListExpression(setMethod, argument, setterArguments, propertyInstance!, out var setterAlias, propertyReference);
					if (mapperExpr is not null)
					{
						exprs.Add(mapperExpr);
						continue;
					}

					if (string.IsNullOrEmpty(setterAlias))
						RejectUnsupportedRuntimeFallback(simpleAssignmentOp, setMethod, "object initializer property assignment", propertyReference.Instance!.Type!);
				}

				exprs.Add(new AssignmentExpression(Operator.Assignment, left, right));
			}
			else if (initializer is IMemberInitializerOperation memberInitializerOp)
			{
				var receiver = BuildMemberInitializerReceiver(memberInitializerOp, obj, argument);
				receiver = MaterializeMemberInitializerReceiver(receiver, memberInitializerOp, argument, exprs);
				var nestedExprs = BuildObjectCreationInitializer(receiver, memberInitializerOp.Initializer, argument);
				exprs.AddRange(nestedExprs);
			}
			else
			{
				var invocationOp = (IInvocationOperation)initializer;
				var arguments = new List<Expression>();
				foreach (var arg in invocationOp.Arguments)
				{
					var argExpr = TranslateTupleForTarget(arg.Value, arg.Parameter!.Type, argument);
					arguments.Add(argExpr);
				}

				// 检查白名单 Inline/Import 操作
				var mapperExpr = GetWhiteListExpression(invocationOp.TargetMethod, argument, arguments, obj, out var alias, invocationOp);
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
		}

		return exprs;
	}

	/// <summary>
	/// 此处主要处理IMemberInitializerOperation.Initializer中或可能嵌套的对象或集合初始化器操作
	/// </summary>
	/// <param name="operation"></param>
	/// <returns>转换为字面量对象</returns>
	private List<ObjectLiteralNode> BuildObjectLiteralMembers(
		IObjectOrCollectionInitializerOperation operation,
		SenseArgument argument,
		bool expandRecordMembers = false)
	{
		var nodes = new List<ObjectLiteralNode>();
		// 合法 object initializer 只包含成员赋值/成员初始化，collection initializer 则绑定为 Add invocation。
		// Member targets、receiver type 和 invocation argument parameter 均由 Roslyn 完整绑定；
		// 无效源码在 lowering 前已被拒绝，因此这里不保留未绑定 operation 的 fallback。
		foreach (var initializer in operation.Initializers)
		{
			Expression target, value;
			ISymbol? orderSymbol;
			if (initializer is ISimpleAssignmentOperation simpleAssignmentOp)
			{
				var memberReference = (IMemberReferenceOperation)simpleAssignmentOp.Target;
				orderSymbol = memberReference.Member;
				var hostType = memberReference.Instance!.Type!;
				if (expandRecordMembers &&
					ShouldOmitStaticNullObjectLiteralMember(simpleAssignmentOp.Value, orderSymbol, hostType))
				{
					continue;
				}

				if (TryBuildObjectLiteralIndexerProperty(simpleAssignmentOp, argument, out var indexerProperty))
				{
					nodes.Add(indexerProperty);
					continue;
				}

				target = CreateObjectPropertyKey(ResolveInitializerAssignmentMemberName(
					simpleAssignmentOp,
					memberReference.Member,
					"object literal member initialization",
					hostType));
				if (expandRecordMembers &&
					orderSymbol is IPropertySymbol propertySymbol &&
					TryGetSpreadAttribute(propertySymbol, out _))
				{
					AppendExpandedInitializerMembers(propertySymbol, simpleAssignmentOp.Value, argument, nodes);
					continue;
				}

				value = TranslateTupleForTarget(simpleAssignmentOp.Value, simpleAssignmentOp.Target.Type, argument);
			}
			else if (initializer is IMemberInitializerOperation memberInitializerOp)
			{
				var memberReference = (IMemberReferenceOperation)memberInitializerOp.InitializedMember;
				orderSymbol = memberReference.Member;
				target = CreateObjectPropertyKey(ResolveInitializerAssignmentMemberName(
					memberInitializerOp,
					memberReference.Member,
					"object literal member initialization",
					memberReference.Instance!.Type!));
				value = RecursiveObjectOrCollectionInitializer(memberInitializerOp.Initializer, argument);
			}
			else
			{
				var invocationOp = (IInvocationOperation)initializer;
				if (TryBuildObjectLiteralAddProperty(invocationOp, argument, out var addProperty))
				{
					if (expandRecordMembers &&
						ShouldOmitStaticNullObjectLiteralMember(
							invocationOp.Arguments[1].Value,
							null,
							invocationOp.Instance!.Type!))
					{
						continue;
					}

					nodes.Add(addProperty);
					continue;
				}

				return HandleTransformationFailure<List<ObjectLiteralNode>>(
					invocationOp,
					$"Object-literal collection initializer '{invocationOp.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat)}' requires an instance Add(key, value) member whose key is string, numeric, or ECMAScript.Symbol.");
			}

			var prop = new ObjectProperty(
				PropertyKind.Init,
				key: target,
				value: value,
				computed: false,
				shorthand: false,
				method: false);
			nodes.Add(new ObjectLiteralNode(prop, GetObjectInitializerMemberName(initializer), orderSymbol));
		}
		return nodes;
	}

	private bool TryBuildObjectLiteralIndexerProperty(
		ISimpleAssignmentOperation assignment,
		SenseArgument argument,
		out ObjectLiteralNode node)
	{
		node = default;
		var hostType = assignment.Target switch
		{
			IPropertyReferenceOperation propertyReferenceOperation => propertyReferenceOperation.Instance!.Type!,
			_ => null
		};

		if (assignment.Target is not IPropertyReferenceOperation propertyReference ||
			propertyReference.Arguments.Length != 1 ||
			!IsObjectLiteralHostType(hostType))
		{
			return false;
		}

		var keyType = propertyReference.Arguments[0].Parameter!.Type;
		if (!TryTranslateObjectLiteralPropertyKey(propertyReference.Arguments[0].Value, keyType, argument, out var key, out var computed))
		{
			RejectUnsupportedDynamicObjectLiteralKey(
				propertyReference.Arguments[0].Value,
				hostType,
				"object-literal indexer initialization");
			return false;
		}

		var value = TranslateTupleForTarget(assignment.Value, assignment.Target.Type, argument);
		var property = new ObjectProperty(
			PropertyKind.Init,
			key: key,
			value: value,
			computed: computed,
			shorthand: false,
			method: false);
		node = new ObjectLiteralNode(property, GetObjectLiteralNodeName(property), propertyReference.Property);
		return true;
	}

	private bool TryBuildObjectLiteralAddProperty(
		IInvocationOperation invocation,
		SenseArgument argument,
		out ObjectLiteralNode node)
	{
		node = default;
		if (!IsObjectLiteralAddInvocation(invocation))
			return false;

		var keyType = invocation.Arguments[0].Parameter!.Type;
		if (!TryTranslateObjectLiteralPropertyKey(invocation.Arguments[0].Value, keyType, argument, out var key, out var computed))
		{
			RejectUnsupportedDynamicObjectLiteralKey(
				invocation.Arguments[0].Value,
				invocation.Instance!.Type!,
				"object-literal Add(key, ...) initialization");
			return false;
		}

		var value = TranslateTupleForTarget(invocation.Arguments[1].Value, invocation.Arguments[1].Parameter!.Type, argument);
		var property = new ObjectProperty(
			PropertyKind.Init,
			key: key,
			value: value,
			computed: computed,
			shorthand: false,
			method: false);
		node = new ObjectLiteralNode(property, GetObjectLiteralNodeName(property), null);
		return true;
	}

	private static bool IsObjectLiteralAddInvocation(IInvocationOperation invocation)
	{
		var targetMethod = invocation.TargetMethod;
		if (targetMethod is
			not
			{
				MethodKind: MethodKind.Ordinary,
				IsStatic: false,
				Name: "Add"
			} ||
			targetMethod.Parameters.Length != 2 ||
			invocation.Arguments.Length != 2 ||
			targetMethod.Parameters[0].RefKind != RefKind.None ||
			targetMethod.Parameters[1].RefKind != RefKind.None)
		{
			return false;
		}

		var keyType = targetMethod.Parameters[0].Type;
		if (keyType.SpecialType != SpecialType.System_String &&
			!IsObjectLiteralNumericKeyType(keyType) &&
			!IsObjectLiteralComputedKeyType(keyType))
		{
			return false;
		}

		return IsObjectLiteralHostType(invocation.Instance!.Type!);
	}

	private void AppendExpandedInitializerMembers(
		IPropertySymbol property,
		IOperation valueOperation,
		SenseArgument argument,
		List<ObjectLiteralNode> nodes)
	{
		var expandedExpression = TranslateTupleForTarget(valueOperation, property.Type, argument);
		if (expandedExpression is ObjectExpression literal)
		{
			foreach (var propertyNode in literal.Properties)
				nodes.Add(CreateObjectLiteralNode(propertyNode, null));
			return;
		}

		nodes.Add(new ObjectLiteralNode(new SpreadElement(expandedExpression), string.Empty, property));
	}

	private ObjectExpression RecursiveObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, SenseArgument argument)
		=> new(NodeList.From(BuildObjectLiteralMembers(operation, argument).Select(static node => node.Node)));

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
	{
		if (Host?.RewriteObjectCreationPreorder(operation, argument) is Expression preorderHostExpression)
			return WithOriginIfMissing(preorderHostExpression, operation);

		if (Host is not null && Host.ShouldRewriteObjectCreation(operation))
		{
			var arguments = operation.Arguments
				.Select(arg => Translate<Expression>(arg.Value, argument))
				.ToArray();
			if (Host.RewriteObjectCreation(operation, argument, arguments) is Expression hostExpression)
				return WithOriginIfMissing(hostExpression, operation);
		}

		return BuildObjectCreation(operation, argument);
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
			// Roslyn binds every legal anonymous-object member as a simple assignment.
			var simpleAssignment = (ISimpleAssignmentOperation)initializer;
			var value = Translate<Expression>(simpleAssignment.Value, argument);
			var key = Translate<Expression>(simpleAssignment.Target, argument);
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

		return new ObjectExpression(NodeList.From(properties));
	}

	private static string GetObjectLiteralNodeName(Node node)
		=> node switch
		{
			ObjectProperty
			{
				Key: Identifier { Name: var name }
			} => name,
			ObjectProperty
			{
				Key: StringLiteral { Value: var name }
			} => name,
			_ => string.Empty
		};

	private static ObjectLiteralNode CreateObjectLiteralNode(Node node, ISymbol? orderSymbol)
		=> new(node, GetObjectLiteralNodeName(node), orderSymbol);

	private readonly record struct ObjectLiteralNode(Node Node, string Name, ISymbol? OrderSymbol);

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
		var arrayType = (IArrayTypeSymbol)operation.Type!;
		RejectUnsupportedTypeFallback(operation, arrayType, "array creation");

		if (arrayType.Rank > 1)
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
			// A valid rank-one array creation without an initializer has exactly one bound dimension.
			var dimension = operation.DimensionSizes[0];
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
		Expression target;
		ITypeSymbol targetType;
		if (operation.InitializedMember is IPropertyReferenceOperation propertyReference)
		{
			target = CreateObjectPropertyKey(ResolveInitializerAssignmentMemberName(
				operation,
				propertyReference.Property,
				"member initializer assignment",
				propertyReference.Instance!.Type!));
			targetType = propertyReference.Property.Type;
		}
		else
		{
			var fieldReference = (IFieldReferenceOperation)operation.InitializedMember;
			target = CreateObjectPropertyKey(ResolveInitializerAssignmentMemberName(
				operation,
				fieldReference.Field,
				"member initializer assignment",
				fieldReference.Instance!.Type!));
			targetType = fieldReference.Field.Type;
		}

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
