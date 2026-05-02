#nullable enable
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

internal static class ChildrenToSlotIntrinsic
{
	private const string DefaultSlotKey = "default";

	private const string DefaultSlotComponentParameterName = "__component";

	private const string DefaultSlotPropsParameterName = "__props";

	private const string DefaultSlotChildParameterName = "__slot0";

	public static bool TryBuild(
		IInvocationOperation operation,
		IMethodSymbol method,
		IReadOnlyList<Expression> arguments,
		Services services,
		out Expression? expression)
	{
		expression = null;
		if (!IsRenderFactoryMethod(method, services))
			return false;

		if (!TryClassifyDefaultSlotInvocation(method, services, out var defaultSlotKind, out var hostContracts))
			return false;

		if (arguments.Count != method.Parameters.Length)
			return false;

		if (defaultSlotKind is DefaultSlotInvocationKind.TypedNoProps or DefaultSlotInvocationKind.TypedWithProps)
			ValidateTypedDefaultSlotAuthoring(method, hostContracts, operation, services);

		if (!services.TryBuildImportedModuleMember(method.ContainingType, Util.GetConfigOrSymbolName(method), services.Argument, out var importedRenderFactory) ||
			importedRenderFactory is not Expression renderFactory)
		{
			throw services.CreateException(
				operation,
				$"无法为 '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' 解析运行时导入。");
		}

		expression = defaultSlotKind switch
		{
			DefaultSlotInvocationKind.UntypedNoProps or DefaultSlotInvocationKind.TypedNoProps
				=> BuildDefaultSlotComponentCall(renderFactory, arguments[0], null, arguments[1]),
			DefaultSlotInvocationKind.UntypedWithProps or DefaultSlotInvocationKind.TypedWithProps
				=> BuildDefaultSlotComponentCall(renderFactory, arguments[0], arguments[1], arguments[2]),
			_ => null
		};
		return expression is not null;
	}

	private static bool IsRenderFactoryMethod(IMethodSymbol method, Services services)
		=> method is
		{
			MethodKind: MethodKind.Ordinary,
			IsStatic: true,
			ContainingType: { }
		} &&
		string.Equals(Util.GetConfigOrSymbolName(method), "h", StringComparison.Ordinal) &&
		!string.IsNullOrWhiteSpace(services.GetModuleImportPath(method.ContainingType));

	private static bool TryClassifyDefaultSlotInvocation(
		IMethodSymbol method,
		Services services,
		out DefaultSlotInvocationKind kind,
		out HostContracts hostContracts)
	{
		kind = default;
		hostContracts = null!;
		if (!TryResolveHostContracts(method, out hostContracts))
			return false;

		var parameters = method.Parameters;
		if (parameters.Length is not (2 or 3))
			return false;

		if (!TryClassifyReceiver(parameters[0].Type, hostContracts, out var hasTypedSlots))
			return false;

		if (parameters.Length == 2)
		{
			if (!IsDefaultSlotChildType(parameters[1].Type, hostContracts, services))
				return false;

			kind = hasTypedSlots ? DefaultSlotInvocationKind.TypedNoProps : DefaultSlotInvocationKind.UntypedNoProps;
			return true;
		}

		if (!IsPropsLikeParameter(parameters[1], hostContracts) ||
			!IsDefaultSlotChildType(parameters[2].Type, hostContracts, services))
			return false;

		kind = hasTypedSlots ? DefaultSlotInvocationKind.TypedWithProps : DefaultSlotInvocationKind.UntypedWithProps;
		return true;
	}

	private static bool TryResolveHostContracts(IMethodSymbol method, out HostContracts contracts)
	{
		contracts = null!;
		if (method.ContainingType?.OriginalDefinition is not INamedTypeSymbol hostType)
			return false;

		var node = GetHostTypeMember(hostType, "IVNode", 0);
		if (node is null)
			return false;

		contracts = new HostContracts(
			node,
			GetHostTypeMember(hostType, "VueProps", 0),
			GetHostTypeMember(hostType, "VueChild", 0),
			GetHostTypeMember(hostType, "IVueComponent", 0),
			GetHostTypeMember(hostType, "IVueComponent", 1),
			GetHostTypeMember(hostType, "IVueSlotComponent", 1),
			GetHostTypeMember(hostType, "IVueComponent", 2));
		return true;
	}

	private static INamedTypeSymbol? GetHostTypeMember(INamedTypeSymbol hostType, string name, int arity)
		=> hostType.GetTypeMembers(name, arity).SingleOrDefault();

	private static bool TryClassifyReceiver(
		ITypeSymbol receiverType,
		HostContracts hostContracts,
		out bool hasTypedSlots)
	{
		hasTypedSlots = false;
		if (receiverType is not INamedTypeSymbol namedType)
			return false;

		if (IsSameOriginalDefinition(namedType, hostContracts.Component) ||
			IsSameOriginalDefinition(namedType, hostContracts.PropsComponent))
		{
			return true;
		}

		if (IsSameOriginalDefinition(namedType, hostContracts.SlotComponent) ||
			IsSameOriginalDefinition(namedType, hostContracts.TypedComponent))
		{
			hasTypedSlots = true;
			return true;
		}

		return false;
	}

	private static bool IsDefaultSlotChildType(ITypeSymbol type, HostContracts hostContracts, Services services)
	{
		if (hostContracts.Child is not null &&
			type is INamedTypeSymbol childType &&
			InheritsFrom(childType, hostContracts.Child))
		{
			return true;
		}

		if (IsSameOriginalDefinition(type, hostContracts.Node))
			return true;

		if (type is IArrayTypeSymbol arrayType &&
			IsSameOriginalDefinition(arrayType.ElementType, hostContracts.Node))
			return true;

		if (type is INamedTypeSymbol { Name: "Number", Arity: 0 })
			return true;

		return services.GetMapperType(type).Mapper is TypeMapper.String or TypeMapper.Number or TypeMapper.Boolean;
	}

	private static bool IsPropsLikeParameter(IParameterSymbol parameter, HostContracts hostContracts)
	{
		if (hostContracts.Props is null)
			return false;

		return parameter.Type switch
		{
			INamedTypeSymbol namedType => InheritsFrom(namedType, hostContracts.Props),
			ITypeParameterSymbol typeParameter => typeParameter.ConstraintTypes
				.OfType<INamedTypeSymbol>()
				.Any(constraint => InheritsFrom(constraint, hostContracts.Props)),
			_ => false
		};
	}

	private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
		{
			if (IsSameOriginalDefinition(current, baseType))
				return true;
		}

		return false;
	}

	private static bool IsSameOriginalDefinition(ITypeSymbol? candidate, INamedTypeSymbol? expected)
		=> candidate is INamedTypeSymbol candidateNamed &&
		   expected is not null &&
		   SymbolEqualityComparer.Default.Equals(candidateNamed.OriginalDefinition, expected.OriginalDefinition);

	private static Expression BuildDefaultSlotComponentCall(
		Expression renderFactory,
		Expression component,
		Expression? props,
		Expression childContent)
	{
		if (IsDirectDefaultSlotChildSafe(childContent))
		{
			var arguments = props is null
				? NodeList.From<Expression>(component, BuildDefaultSlotObject(childContent))
				: NodeList.From<Expression>(component, props, BuildDefaultSlotObject(childContent));
			return new CallExpression(renderFactory, arguments, optional: false);
		}

		if (props is null)
		{
			return BuildSingleEvaluationArrowInvocation(
				[
					(DefaultSlotComponentParameterName, component),
					(DefaultSlotChildParameterName, childContent)
				],
				parameters => new CallExpression(
					renderFactory,
					NodeList.From<Expression>(parameters[0], BuildDefaultSlotObject(parameters[1])),
					optional: false));
		}

		return BuildSingleEvaluationArrowInvocation(
			[
				(DefaultSlotComponentParameterName, component),
				(DefaultSlotPropsParameterName, props),
				(DefaultSlotChildParameterName, childContent)
			],
			parameters => new CallExpression(
				renderFactory,
				NodeList.From<Expression>(parameters[0], parameters[1], BuildDefaultSlotObject(parameters[2])),
				optional: false));
	}

	private static bool IsDirectDefaultSlotChildSafe(Expression childContent)
		=> childContent is StringLiteral or BooleanLiteral or NumericLiteral or BigIntLiteral or NullLiteral;

	private static void ValidateTypedDefaultSlotAuthoring(
		IMethodSymbol method,
		HostContracts hostContracts,
		IOperation originOperation,
		Services services)
	{
		if (!TryGetTypedSlotContractType(method, hostContracts, out var slotType))
			return;

		var defaultSlots = CollectTypedDefaultSlotMembers(slotType, services);
		if (defaultSlots.Count == 0)
		{
			throw services.CreateException(
				originOperation,
				$"Typed slot contract '{slotType.ToDisplayString(Format.NameFormat)}' does not declare a default slot. Use H(component, slots) / H(component, props, slots) with an explicit slot object, or declare one slot property as 'Default' / Description(\"@#default\").");
		}

		if (defaultSlots.Count > 1)
		{
			throw services.CreateException(
				originOperation,
				$"Typed slot contract '{slotType.ToDisplayString(Format.NameFormat)}' declares more than one default slot. Only one property may map to 'default' via the Default naming convention or Description(\"@#default\").");
		}

		var defaultSlot = defaultSlots[0];
		if (!TryClassifySlotDelegate(defaultSlot.Type, hostContracts, out var isScoped))
		{
			throw services.CreateException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Format.NameFormat)}' must be a delegate returning the host IVNode type.");
		}

		if (isScoped)
		{
			throw services.CreateException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Format.NameFormat)}' expects slot scope. Implicit child content cannot satisfy a typed default slot context. Use H(component, slots) / H(component, props, slots) and provide an explicit slot callback.");
		}
	}

	private static bool TryGetTypedSlotContractType(
		IMethodSymbol method,
		HostContracts hostContracts,
		out INamedTypeSymbol slotType)
	{
		slotType = null!;
		if (method.Parameters.Length == 0 ||
			method.Parameters[0].Type is not INamedTypeSymbol { IsGenericType: true } parameterType)
		{
			return false;
		}

		if (method.TypeArguments.Length == 1 &&
			method.TypeArguments[0] is INamedTypeSymbol slotComponentSlotType &&
			parameterType.TypeArguments.Length == 1 &&
			IsSameOriginalDefinition(parameterType, hostContracts.SlotComponent) &&
			SymbolEqualityComparer.Default.Equals(parameterType.TypeArguments[0], method.TypeArguments[0]))
		{
			slotType = slotComponentSlotType;
			return true;
		}

		if (method.TypeArguments.Length == 2 &&
			method.TypeArguments[1] is INamedTypeSymbol componentSlotType &&
			parameterType.TypeArguments.Length == 2 &&
			IsSameOriginalDefinition(parameterType, hostContracts.TypedComponent) &&
			SymbolEqualityComparer.Default.Equals(parameterType.TypeArguments[1], method.TypeArguments[1]))
		{
			slotType = componentSlotType;
			return true;
		}

		return false;
	}

	private static List<IPropertySymbol> CollectTypedDefaultSlotMembers(INamedTypeSymbol slotType, Services services)
	{
		var defaults = new List<IPropertySymbol>();
		foreach (var current in services.EnumerateNamedTypeHierarchyBaseFirst(slotType))
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

				if (string.Equals(Util.GetConfigOrSymbolName(property), DefaultSlotKey, StringComparison.Ordinal))
					defaults.Add(property);
			}
		}

		return defaults;
	}

	private static bool TryClassifySlotDelegate(ITypeSymbol type, HostContracts hostContracts, out bool isScoped)
	{
		isScoped = false;
		if (type is not INamedTypeSymbol namedType ||
			namedType.TypeKind != TypeKind.Delegate ||
			namedType.DelegateInvokeMethod is not { } invoke ||
			!IsSameOriginalDefinition(invoke.ReturnType, hostContracts.Node))
		{
			return false;
		}

		isScoped = invoke.Parameters.Length > 0;
		return true;
	}

	private static ObjectExpression BuildDefaultSlotObject(Expression child)
	{
		var slotCallback = new ArrowFunctionExpression(
			NodeList.Empty<Node>(),
			child,
			expression: true,
			async: false);
		var slotProperty = new ObjectProperty(
			PropertyKind.Init,
			key: new Identifier(DefaultSlotKey),
			value: slotCallback,
			computed: false,
			shorthand: false,
			method: false);
		return new ObjectExpression(NodeList.From<Node>(slotProperty));
	}

	private static Expression BuildSingleEvaluationArrowInvocation(
		IReadOnlyList<(string ParameterName, Expression Value)> inputs,
		Func<Identifier[], Expression> bodyFactory)
	{
		var parameters = new Identifier[inputs.Count];
		var parameterNodes = new Node[inputs.Count];
		var values = new Expression[inputs.Count];
		for (var index = 0; index < inputs.Count; index++)
		{
			var parameter = new Identifier(inputs[index].ParameterName);
			parameters[index] = parameter;
			parameterNodes[index] = parameter;
			values[index] = inputs[index].Value;
		}

		var arrow = new ArrowFunctionExpression(
			NodeList.From(parameterNodes),
			bodyFactory(parameters),
			expression: true,
			async: false);
		return new CallExpression(arrow, NodeList.From(values), optional: false);
	}

	private enum DefaultSlotInvocationKind
	{
		UntypedNoProps,
		UntypedWithProps,
		TypedNoProps,
		TypedWithProps
	}

	private sealed class HostContracts
	{
		public HostContracts(
			INamedTypeSymbol node,
			INamedTypeSymbol? props,
			INamedTypeSymbol? child,
			INamedTypeSymbol? component,
			INamedTypeSymbol? propsComponent,
			INamedTypeSymbol? slotComponent,
			INamedTypeSymbol? typedComponent)
		{
			Node = node;
			Props = props;
			Child = child;
			Component = component;
			PropsComponent = propsComponent;
			SlotComponent = slotComponent;
			TypedComponent = typedComponent;
		}

		public INamedTypeSymbol Node { get; }

		public INamedTypeSymbol? Props { get; }

		public INamedTypeSymbol? Child { get; }

		public INamedTypeSymbol? Component { get; }

		public INamedTypeSymbol? PropsComponent { get; }

		public INamedTypeSymbol? SlotComponent { get; }

		public INamedTypeSymbol? TypedComponent { get; }
	}

	public readonly struct Services
	{
		public Services(
			SenseArgument argument,
			ImportedModuleMemberBuilder tryBuildImportedModuleMember,
			ModuleImportPathResolver getModuleImportPath,
			TypeMapperResolver getMapperType,
			NamedTypeHierarchyEnumerator enumerateNamedTypeHierarchyBaseFirst,
			ExceptionFactory createException)
		{
			Argument = argument;
			TryBuildImportedModuleMember = tryBuildImportedModuleMember;
			GetModuleImportPath = getModuleImportPath;
			GetMapperType = getMapperType;
			EnumerateNamedTypeHierarchyBaseFirst = enumerateNamedTypeHierarchyBaseFirst;
			CreateException = createException;
		}

		public SenseArgument Argument { get; }

		public ImportedModuleMemberBuilder TryBuildImportedModuleMember { get; }

		public ModuleImportPathResolver GetModuleImportPath { get; }

		public TypeMapperResolver GetMapperType { get; }

		public NamedTypeHierarchyEnumerator EnumerateNamedTypeHierarchyBaseFirst { get; }

		public ExceptionFactory CreateException { get; }
	}

	public delegate bool ImportedModuleMemberBuilder(
		ITypeSymbol? containingType,
		string memberName,
		SenseArgument? context,
		out Expression? expression);

	public delegate string? ModuleImportPathResolver(ITypeSymbol symbol);

	public delegate (TypeMapper Mapper, string TypeName) TypeMapperResolver(ITypeSymbol symbol);

	public delegate IEnumerable<INamedTypeSymbol> NamedTypeHierarchyEnumerator(INamedTypeSymbol type);

	public delegate Exception ExceptionFactory(IOperation operation, string message);
}
