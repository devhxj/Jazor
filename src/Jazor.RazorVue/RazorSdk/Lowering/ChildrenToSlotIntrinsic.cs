#nullable enable
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// 将 Razor children 调用投影为 Vue slot intrinsic 表达式。
/// </summary>
/// <remarks>
/// 这是 compiler 识别的窄语义入口，用于把官方 Razor 生成代码中的 children 传递转换为
/// 最终 render-function slot 形状。它不建立通用的中间 marker protocol，也不处理普通调用。
/// </remarks>
internal static class ChildrenToSlotIntrinsic
{
	private const string DefaultSlotComponentParameterName = "__component";

	private const string DefaultSlotPropsParameterName = "__props";

	private const string DefaultSlotChildParameterName = "__slot0";

	public static bool TryBuild(
		IInvocationOperation operation,
		IMethodSymbol method,
		IReadOnlyList<Expression> arguments,
		SemanticInvocationLoweringContext context,
		out Expression? expression)
		=> TryBuildCore(operation, method, arguments, new SemanticServices(context), out expression);

	internal static bool TryBuild(
		IInvocationOperation operation,
		IMethodSymbol method,
		IReadOnlyList<Expression> arguments,
		Services services,
		out Expression? expression)
		=> TryBuildCore(operation, method, arguments, services, out expression);

	private static bool TryBuildCore(
		IInvocationOperation operation,
		IMethodSymbol method,
		IReadOnlyList<Expression> arguments,
		IContext context,
		out Expression? expression)
	{
		// Recognition is deliberately structural and narrow. Returning false delegates normal
		// invocations to SemanticWalker; a recognized invalid slot contract gets an actionable error.
		// 只认领精确的 Razor children 传输形状，避免把普通 h 调用误当作 slot protocol。
		expression = null;
		if (method.ContainingType is not { } containingType ||
			!IsRenderFactoryMethod(method, containingType, context))
			return false;

		if (!TryClassifyDefaultSlotInvocation(method, context, out var defaultSlotKind, out var hostContracts))
			return false;

		if (arguments.Count != method.Parameters.Length)
			return false;


		// Untyped implicit children would lose the authored slot ABI. Require an explicit slots
		// object unless the component contract supplies a typed default slot declaration.
		// 无类型 children 不能猜测 slot 名；只有已声明 typed default slot 才允许隐式传递。
		var defaultSlotName = defaultSlotKind is DefaultSlotInvocationKind.TypedNoProps or DefaultSlotInvocationKind.TypedWithProps
			? ValidateTypedDefaultSlotAuthoring(method, hostContracts, operation, context)
			: throw context.CreateException(
				operation,
				"Implicit component child content has no explicit slot contract. Use H(component, slots) / H(component, props, slots) and declare the slot key in the supplied slots object.");

		if (!context.TryBuildImportedModuleMember(containingType, Util.GetConfigOrSymbolName(method), out var importedRenderFactory) ||
			importedRenderFactory is not Expression renderFactory)
		{
			throw context.CreateException(
				operation,
				$"无法为 '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' 解析运行时导入。");
		}

		expression = defaultSlotKind switch
		{
			DefaultSlotInvocationKind.UntypedNoProps or DefaultSlotInvocationKind.TypedNoProps
				=> BuildDefaultSlotComponentCall(renderFactory, arguments[0], null, arguments[1], defaultSlotName),
			DefaultSlotInvocationKind.UntypedWithProps or DefaultSlotInvocationKind.TypedWithProps
				=> BuildDefaultSlotComponentCall(renderFactory, arguments[0], arguments[1], arguments[2], defaultSlotName),
			_ => null
		};
		return expression is not null;
	}

	private static bool IsRenderFactoryMethod(
		IMethodSymbol method,
		INamedTypeSymbol containingType,
		IContext context)
		=> method is
		{
			MethodKind: MethodKind.Ordinary,
			IsStatic: true
		} &&
		string.Equals(Util.GetConfigOrSymbolName(method), "h", StringComparison.Ordinal) &&
		!string.IsNullOrWhiteSpace(context.GetModuleImportPath(containingType));

	private static bool TryClassifyDefaultSlotInvocation(
		IMethodSymbol method,
		IContext context,
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
			if (!IsDefaultSlotChildType(parameters[1].Type, hostContracts, context))
				return false;

			kind = hasTypedSlots ? DefaultSlotInvocationKind.TypedNoProps : DefaultSlotInvocationKind.UntypedNoProps;
			return true;
		}

		if (!IsPropsLikeParameter(parameters[1], hostContracts) ||
			!IsDefaultSlotChildType(parameters[2].Type, hostContracts, context))
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

		var propsComponent = GetHostTypeMember(hostType, "IVueComponent", 1);
		var slotComponent = GetHostTypeMember(hostType, "IVueSlotComponent", 1);
		var typedComponent = GetHostTypeMember(hostType, "IVueComponent", 2);

		contracts = new HostContracts(
			node,
			GetHostTypeMember(hostType, "VueProps", 0),
			GetHostTypeMember(hostType, "VueChild", 0),
			GetHostTypeMember(hostType, "IVueComponent", 0) ?? ResolveUntypedComponentContract(propsComponent, slotComponent, typedComponent),
			propsComponent,
			slotComponent,
			typedComponent);
		return true;
	}

	private static INamedTypeSymbol? GetHostTypeMember(INamedTypeSymbol hostType, string name, int arity)
		=> hostType.GetTypeMembers(name, arity).SingleOrDefault();

	private static INamedTypeSymbol? ResolveUntypedComponentContract(params INamedTypeSymbol?[] candidates)
	{
		foreach (var candidate in candidates)
		{
			if (candidate is null)
				continue;

			foreach (var @interface in candidate.AllInterfaces)
			{
				if (string.Equals(@interface.Name, "IVueComponent", StringComparison.Ordinal) &&
					@interface.Arity == 0)
				{
					return @interface.OriginalDefinition;
				}
			}
		}

		return null;
	}

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

	private static bool IsDefaultSlotChildType(
		ITypeSymbol type,
		HostContracts hostContracts,
		IContext context)
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

		if (hostContracts.Node is not null &&
			type is INamedTypeSymbol namedCollection &&
			namedCollection.IsGenericType &&
			namedCollection.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T &&
			IsSameOriginalDefinition(namedCollection.TypeArguments[0], hostContracts.Node))
		{
			return true;
		}

		if (type is INamedTypeSymbol { Name: "Number", Arity: 0 })
			return true;

		return context.GetTypeMapping(type).Mapper is TypeMapper.String or TypeMapper.Number or TypeMapper.Boolean;
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
		Expression childContent,
		string slotName)
	{
		if (IsDirectDefaultSlotChildSafe(childContent))
		{
			var arguments = props is null
				? NodeList.From<Expression>(component, BuildDefaultSlotObject(childContent, slotName))
				: NodeList.From<Expression>(component, props, BuildDefaultSlotObject(childContent, slotName));
			return new CallExpression(renderFactory, arguments, optional: false);
		}

		if (props is null)
		{
			return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
				[
					(DefaultSlotComponentParameterName, component),
					(DefaultSlotChildParameterName, childContent)
				],
				parameters => new CallExpression(
					renderFactory,
					NodeList.From<Expression>(parameters[0], BuildDefaultSlotObject(parameters[1], slotName)),
					optional: false));
		}

		return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
			[
				(DefaultSlotComponentParameterName, component),
				(DefaultSlotPropsParameterName, props),
				(DefaultSlotChildParameterName, childContent)
			],
			parameters => new CallExpression(
				renderFactory,
				NodeList.From<Expression>(parameters[0], parameters[1], BuildDefaultSlotObject(parameters[2], slotName)),
				optional: false));
	}

	private static bool IsDirectDefaultSlotChildSafe(Expression childContent)
		=> childContent is StringLiteral or BooleanLiteral or NumericLiteral or BigIntLiteral or NullLiteral;

	private static string ValidateTypedDefaultSlotAuthoring(
		IMethodSymbol method,
		HostContracts hostContracts,
		IOperation originOperation,
		IContext context)
	{
		if (!TryGetTypedSlotContractType(method, hostContracts, out var slotType))
			throw context.CreateException(
				originOperation,
				"Implicit component child content requires a typed slot contract. Use H(component, slots) / H(component, props, slots) instead.");

		var defaultSlots = CollectTypedDefaultSlotMembers(slotType, context);
		if (defaultSlots.Count == 0)
		{
			throw context.CreateException(
				originOperation,
				$"Typed slot contract '{slotType.ToDisplayString(Format.NameFormat)}' does not declare an explicit default slot. Use H(component, slots) / H(component, props, slots) with an explicit slot object, or map one slot property with Description(\"@#default\") / ECMAScriptName(\"default\").");
		}

		if (defaultSlots.Count > 1)
		{
			throw context.CreateException(
				originOperation,
				$"Typed slot contract '{slotType.ToDisplayString(Format.NameFormat)}' declares more than one explicit default slot. Only one property may map to 'default'.");
		}

		var defaultSlot = defaultSlots[0];
		if (!TryClassifySlotDelegate(defaultSlot.Type, hostContracts, out var isScoped))
		{
			throw context.CreateException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Format.NameFormat)}' must be a delegate returning the host IVNode type.");
		}

		if (isScoped)
		{
			throw context.CreateException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Format.NameFormat)}' expects slot scope. Implicit child content cannot satisfy a typed default slot context. Use H(component, slots) / H(component, props, slots) and provide an explicit slot callback.");
		}

		return Util.GetSymbolConfigName(defaultSlot)!;
	}

	private static bool TryGetTypedSlotContractType(
		IMethodSymbol method,
		HostContracts hostContracts,
		out INamedTypeSymbol slotType)
	{
		slotType = null!;
		if (method.Parameters[0].Type is not INamedTypeSymbol { IsGenericType: true } parameterType)
		{
			return false;
		}

		if (parameterType.TypeArguments.Length == 1 &&
			IsSameOriginalDefinition(parameterType, hostContracts.SlotComponent) &&
			parameterType.TypeArguments[0] is INamedTypeSymbol slotComponentSlotType)
		{
			slotType = slotComponentSlotType;
			return true;
		}

		if (parameterType.TypeArguments.Length == 2 &&
			IsSameOriginalDefinition(parameterType, hostContracts.TypedComponent) &&
			parameterType.TypeArguments[1] is INamedTypeSymbol componentSlotType)
		{
			slotType = componentSlotType;
			return true;
		}

		return false;
	}

	private static List<IPropertySymbol> CollectTypedDefaultSlotMembers(
		INamedTypeSymbol slotType,
		IContext context)
	{
		var defaults = new List<IPropertySymbol>();
		foreach (var current in context.EnumerateNamedTypeHierarchyBaseFirst(slotType))
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

				if (string.Equals(Util.GetSymbolConfigName(property), "default", StringComparison.Ordinal))
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

	private static ObjectExpression BuildDefaultSlotObject(Expression child, string slotName)
	{
		var slotCallback = new ArrowFunctionExpression(
			NodeList.Empty<Node>(),
			child,
			expression: true,
			async: false);
		var slotProperty = new ObjectProperty(
			PropertyKind.Init,
			key: JavaScriptAstFactory.IsJavaScriptIdentifierName(slotName)
				? new Identifier(slotName)
				: JavaScriptAstFactory.CreateStringLiteral(slotName),
			value: slotCallback,
			computed: false,
			shorthand: false,
			method: false);
		return new ObjectExpression(NodeList.From<Node>(slotProperty));
	}

	/// <summary>Distinguishes the supported default-slot callback signatures before AST projection.</summary>
	private enum DefaultSlotInvocationKind
	{
		UntypedNoProps,
		UntypedWithProps,
		TypedNoProps,
		TypedWithProps
	}

	/// <summary>Cached Roslyn symbols for the narrow Vue child/slot host contract.</summary>
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

	/// <summary>Compiler services required by this intrinsic without depending on a concrete walker host.</summary>
	private interface IContext
	{
		bool TryBuildImportedModuleMember(
			ITypeSymbol containingType,
			string memberName,
			out Expression? expression);

		string? GetModuleImportPath(ITypeSymbol symbol);

		SemanticTypeMapping GetTypeMapping(ITypeSymbol symbol);

		IEnumerable<INamedTypeSymbol> EnumerateNamedTypeHierarchyBaseFirst(INamedTypeSymbol type);

		Exception CreateException(IOperation operation, string message);
	}

	private readonly struct SemanticServices : IContext
	{
		private readonly SemanticInvocationLoweringContext _context;

		public SemanticServices(SemanticInvocationLoweringContext context)
		{
			_context = context;
		}

		public bool TryBuildImportedModuleMember(
			ITypeSymbol containingType,
			string memberName,
			out Expression? expression)
			=> _context.TryBuildImportedModuleMember(containingType, memberName, out expression);

		public string? GetModuleImportPath(ITypeSymbol symbol)
			=> _context.GetModuleImportPath(symbol);

		public SemanticTypeMapping GetTypeMapping(ITypeSymbol symbol)
			=> _context.GetTypeMapping(symbol);

		public IEnumerable<INamedTypeSymbol> EnumerateNamedTypeHierarchyBaseFirst(INamedTypeSymbol type)
			=> _context.EnumerateNamedTypeHierarchyBaseFirst(type);

		public Exception CreateException(IOperation operation, string message)
			=> _context.CreateException(operation, message);
	}

	internal readonly struct Services : IContext
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

		bool IContext.TryBuildImportedModuleMember(
			ITypeSymbol containingType,
			string memberName,
			out Expression? expression)
			=> TryBuildImportedModuleMember(containingType, memberName, Argument, out expression);

		string? IContext.GetModuleImportPath(ITypeSymbol symbol)
			=> GetModuleImportPath(symbol);

		SemanticTypeMapping IContext.GetTypeMapping(ITypeSymbol symbol)
		{
			var (mapper, typeName) = GetMapperType(symbol);
			return new SemanticTypeMapping(mapper, typeName);
		}

		IEnumerable<INamedTypeSymbol> IContext.EnumerateNamedTypeHierarchyBaseFirst(INamedTypeSymbol type)
			=> EnumerateNamedTypeHierarchyBaseFirst(type);

		Exception IContext.CreateException(IOperation operation, string message)
			=> CreateException(operation, message);
	}

	internal delegate bool ImportedModuleMemberBuilder(
		ITypeSymbol containingType,
		string memberName,
		SenseArgument context,
		out Expression? expression);

	internal delegate string? ModuleImportPathResolver(ITypeSymbol symbol);

	internal delegate (TypeMapper Mapper, string TypeName) TypeMapperResolver(ITypeSymbol symbol);

	internal delegate IEnumerable<INamedTypeSymbol> NamedTypeHierarchyEnumerator(INamedTypeSymbol type);

	internal delegate Exception ExceptionFactory(IOperation operation, string message);

}
