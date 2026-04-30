#nullable enable
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public sealed partial class SemanticWalker
{
	private const string VueRuntimeTypeName = "ECMAScript.Vue3";

	private const string VueVNodeTypeName = "ECMAScript.Vue3.IVNode";

	private const string VuePropsTypeName = "ECMAScript.Vue3.VueProps";

	private const string VueSlotsTypeName = "ECMAScript.Vue3.VueSlots";

	private const string VueComponentTypeName = "ECMAScript.Vue3.IVueComponent";

	private const string VueSlotComponentTypeName = "ECMAScript.Vue3.IVueSlotComponent<TSlots>";

	private const string VueTypedComponentTypeName = "ECMAScript.Vue3.IVueComponent<TProps, TSlots>";

	private const string VueDefaultSlotKey = "default";

	private const string VueDefaultSlotComponentParameterName = "__component";

	private const string VueDefaultSlotPropsParameterName = "__props";

	private const string VueDefaultSlotChildParameterName = "__slot0";

	private bool TryBuildVueHInvocationIntrinsic(
		IInvocationOperation operation,
		IMethodSymbol method,
		List<Expression> arguments,
		SenseArgument argument,
		out Expression? expression)
	{
		expression = null;
		if (!IsVueHMethod(method))
			return false;

		if (!TryClassifyVueHDefaultSlotInvocation(method, out var defaultSlotKind))
			return false;

		if (defaultSlotKind is VueHDefaultSlotKind.TypedNoProps or VueHDefaultSlotKind.TypedWithProps)
			ValidateTypedVueDefaultSlotAuthoring(method, operation);

		if (!TryBuildImportedModuleMember(method.ContainingType, Util.GetConfigOrSymbolName(method), argument, out var importedH) ||
			importedH is not Expression h)
		{
			throw CreateVueAuthoringException(
				operation,
				$"无法为 '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' 解析运行时导入。");
		}

		expression = defaultSlotKind switch
		{
			VueHDefaultSlotKind.UntypedNoProps or VueHDefaultSlotKind.TypedNoProps
				=> BuildVueDefaultSlotComponentCall(h, arguments[0], null, arguments[1]),
			VueHDefaultSlotKind.UntypedWithProps or VueHDefaultSlotKind.TypedWithProps
				=> BuildVueDefaultSlotComponentCall(h, arguments[0], arguments[1], arguments[2]),
			_ => null
		};
		return expression is not null;
	}

	private static bool IsVueHMethod(IMethodSymbol method)
		=> method is
		{
			MethodKind: MethodKind.Ordinary,
			IsStatic: true,
			Name: "H",
			ContainingType: { }
		} &&
		method.ContainingType.ToDisplayString(Format.NameFormat) == VueRuntimeTypeName;

	private static bool TryClassifyVueHDefaultSlotInvocation(IMethodSymbol method, out VueHDefaultSlotKind kind)
	{
		kind = default;
		var parameters = method.Parameters;
		if (parameters.Length is not (2 or 3))
			return false;

		if (parameters[0].Type is not INamedTypeSymbol receiverType)
			return false;

		var receiverDisplay = receiverType.OriginalDefinition.ToDisplayString(Format.NameFormat);
		var secondDisplay = parameters[1].Type.OriginalDefinition.ToDisplayString(Format.NameFormat);
		var thirdDisplay = parameters.Length == 3
			? parameters[2].Type.OriginalDefinition.ToDisplayString(Format.NameFormat)
			: null;

		var isUntypedComponent = receiverDisplay == VueComponentTypeName;
		var isTypedSlotOnlyComponent = receiverDisplay == VueSlotComponentTypeName;
		var isTypedComponent = receiverDisplay == VueTypedComponentTypeName;
		if (!isUntypedComponent && !isTypedSlotOnlyComponent && !isTypedComponent)
			return false;

		if (parameters.Length == 2)
		{
			if (!IsVueDefaultSlotChildType(secondDisplay))
				return false;

			kind = isUntypedComponent
				? VueHDefaultSlotKind.UntypedNoProps
				: VueHDefaultSlotKind.TypedNoProps;
			return true;
		}

		if (!IsVuePropsLikeParameter(parameters[1]) || !IsVueDefaultSlotChildType(thirdDisplay))
			return false;

		kind = isUntypedComponent
			? VueHDefaultSlotKind.UntypedWithProps
			: VueHDefaultSlotKind.TypedWithProps;
		return true;
	}

	private static bool IsVueDefaultSlotChildType(string? displayName)
		=> displayName is VueVNodeTypeName or "string" or "bool" or "Number" or "ECMAScript.Number" or $"{VueVNodeTypeName}[]";

	private static bool IsVuePropsLikeParameter(IParameterSymbol parameter)
	{
		return parameter.Type switch
		{
			INamedTypeSymbol namedType => InheritsFrom(namedType, VuePropsTypeName),
			ITypeParameterSymbol typeParameter => typeParameter.ConstraintTypes
				.OfType<INamedTypeSymbol>()
				.Any(static constraint => InheritsFrom(constraint, VuePropsTypeName)),
			_ => false
		};
	}

	private static bool InheritsFrom(INamedTypeSymbol type, string baseDisplayName)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
		{
			if (current.OriginalDefinition.ToDisplayString(Format.NameFormat) == baseDisplayName)
				return true;
		}

		return false;
	}

	private static Expression BuildVueDefaultSlotComponentCall(
		Expression h,
		Expression component,
		Expression? props,
		Expression childContent)
	{
		if (props is null)
		{
			return BuildSingleEvaluationArrowInvocation(
				[
					(VueDefaultSlotComponentParameterName, component),
					(VueDefaultSlotChildParameterName, childContent)
				],
				parameters => new CallExpression(
					h,
					NodeList.From<Expression>(parameters[0], BuildVueDefaultSlotObject(parameters[1])),
					optional: false));
		}

		return BuildSingleEvaluationArrowInvocation(
			[
				(VueDefaultSlotComponentParameterName, component),
				(VueDefaultSlotPropsParameterName, props),
				(VueDefaultSlotChildParameterName, childContent)
			],
			parameters => new CallExpression(
				h,
				NodeList.From<Expression>(parameters[0], parameters[1], BuildVueDefaultSlotObject(parameters[2])),
				optional: false));
	}

	private void ValidateTypedVueDefaultSlotAuthoring(IMethodSymbol method, IOperation originOperation)
	{
		if (!TryGetTypedVueSlotContractType(method, out var slotType))
			return;

		var defaultSlots = CollectTypedVueDefaultSlotMembers(slotType);
		if (defaultSlots.Count == 0)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Typed Vue slot contract '{slotType.ToDisplayString(Format.NameFormat)}' does not declare a default slot. Use H(component, slots) / H(component, props, slots) with an explicit slot object, or declare one slot property as 'Default' / Description(\"@#default\").");
		}

		if (defaultSlots.Count > 1)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Typed Vue slot contract '{slotType.ToDisplayString(Format.NameFormat)}' declares more than one default slot. Only one property may map to 'default' via the Default naming convention or Description(\"@#default\").");
		}

		var defaultSlot = defaultSlots[0];
		if (!IsVueSlotCallbackType(defaultSlot.Type, out var isScoped))
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Format.NameFormat)}' must use VueSlotCallback or VueSlotCallback<TScope>.");
		}

		if (isScoped)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Format.NameFormat)}' expects slot scope. Implicit child content cannot satisfy a typed default slot context. Use H(component, slots) / H(component, props, slots) and provide an explicit slot callback.");
		}
	}

	private static bool TryGetTypedVueSlotContractType(IMethodSymbol method, out INamedTypeSymbol slotType)
	{
		slotType = null!;
		if (method.ContainingType?.OriginalDefinition is not INamedTypeSymbol vueHost ||
			method.Parameters.Length == 0 ||
			method.Parameters[0].Type is not INamedTypeSymbol { IsGenericType: true } parameterType)
		{
			return false;
		}

		if (method.TypeArguments.Length == 1 &&
			method.TypeArguments[0] is INamedTypeSymbol slotComponentSlotType &&
			parameterType.TypeArguments.Length == 1 &&
			vueHost.GetTypeMembers("IVueSlotComponent", 1).SingleOrDefault() is INamedTypeSymbol slotComponentContract &&
			SymbolEqualityComparer.Default.Equals(parameterType.OriginalDefinition, slotComponentContract) &&
			SymbolEqualityComparer.Default.Equals(parameterType.TypeArguments[0], method.TypeArguments[0]))
		{
			slotType = slotComponentSlotType;
			return true;
		}

		if (method.TypeArguments.Length == 2 &&
			method.TypeArguments[1] is INamedTypeSymbol componentSlotType &&
			parameterType.TypeArguments.Length == 2 &&
			vueHost.GetTypeMembers("IVueComponent", 2).SingleOrDefault() is INamedTypeSymbol typedComponentContract &&
			SymbolEqualityComparer.Default.Equals(parameterType.OriginalDefinition, typedComponentContract) &&
			SymbolEqualityComparer.Default.Equals(parameterType.TypeArguments[1], method.TypeArguments[1]))
		{
			slotType = componentSlotType;
			return true;
		}

		return false;
	}

	private static List<IPropertySymbol> CollectTypedVueDefaultSlotMembers(INamedTypeSymbol slotType)
	{
		var defaults = new List<IPropertySymbol>();
		foreach (var current in EnumerateNamedTypeHierarchyBaseFirst(slotType))
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

				if (string.Equals(Util.GetConfigOrSymbolName(property), VueDefaultSlotKey, StringComparison.Ordinal))
					defaults.Add(property);
			}
		}

		return defaults;
	}

	private static bool IsVueSlotCallbackType(ITypeSymbol type, out bool isScoped)
	{
		isScoped = false;
		if (type is not INamedTypeSymbol namedType ||
			namedType.ContainingNamespace is null ||
			namedType.ContainingNamespace.GetTypeMembers("VueSlotCallback", namedType.Arity).SingleOrDefault() is not INamedTypeSymbol expectedType ||
			!SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, expectedType))
		{
			return false;
		}

		isScoped = namedType.Arity > 0;
		return true;
	}

	private static Exception CreateVueAuthoringException(IOperation originOperation, string message)
		=> CreateOperationTransformationException(originOperation, message);

	private static ObjectExpression BuildVueDefaultSlotObject(Expression child)
	{
		var slotCallback = new ArrowFunctionExpression(
			NodeList.Empty<Node>(),
			child,
			expression: true,
			async: false);
		var slotProperty = new ObjectProperty(
			PropertyKind.Init,
			key: new Identifier(VueDefaultSlotKey),
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

	private enum VueHDefaultSlotKind
	{
		UntypedNoProps,
		UntypedWithProps,
		TypedNoProps,
		TypedWithProps
	}
}
