#nullable enable
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	private const string VueDefaultSlotKey = "default";

	private const string VueDefaultSlotComponentParameterName = "__component";

	private const string VueDefaultSlotPropsParameterName = "__props";

	private const string VueDefaultSlotChildParameterName = "__slot0";

	public Expression? CompileVueHDefaultSlotNoProps(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
		=> CompileVueDefaultSlotComponentCallCore(symbol, context, handler, args, originOperation, hasProps: false);

	public Expression? CompileVueHDefaultSlotWithProps(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
		=> CompileVueDefaultSlotComponentCallCore(symbol, context, handler, args, originOperation, hasProps: true);

	private Expression? CompileVueDefaultSlotComponentCallCore(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation,
		bool hasProps)
	{
		var methodDisplay = symbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat);
		if (handler is not null)
			throw new InvalidOperationException($"{methodDisplay} 不应接收实例 handler。");

		Expression component;
		Expression? props;
		Expression childContent;
		if (hasProps)
		{
			if (args.Length != 3 ||
				args[0] is not Expression componentExpression ||
				args[1] is not Expression propsExpression ||
				args[2] is not Expression childExpression)
			{
				throw new InvalidOperationException($"{methodDisplay} 需要且仅需要三个显式参数。");
			}

			component = componentExpression;
			props = propsExpression;
			childContent = childExpression;
		}
		else
		{
			if (args.Length != 2 ||
				args[0] is not Expression componentExpression ||
				args[1] is not Expression childExpression)
			{
				throw new InvalidOperationException($"{methodDisplay} 需要且仅需要两个显式参数。");
			}

			component = componentExpression;
			props = null;
			childContent = childExpression;
		}

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, props, childContent, originOperation);
	}

	private Expression BuildVueDefaultSlotComponentCall(
		ISymbol symbol,
		SenseArgument context,
		Expression component,
		Expression? props,
		Expression childContent,
		IOperation? originOperation)
	{
		if (!TryBuildImportedModuleMember(symbol.ContainingType, Util.GetConfigOrSymbolName(symbol), context, out var importedH) ||
			importedH is not Expression h)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"无法为 '{symbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' 解析运行时导入。");
		}

		if (props is null)
		{
			return BuildSingleEvaluationArrowInvocation(
				new (string ParameterName, Expression Value)[]
				{
					(VueDefaultSlotComponentParameterName, component),
					(VueDefaultSlotChildParameterName, childContent)
				},
				parameters => new CallExpression(
					h,
					NodeList.From<Expression>(parameters[0], BuildVueDefaultSlotObject(parameters[1])),
					optional: false));
		}

		return BuildSingleEvaluationArrowInvocation(
			new (string ParameterName, Expression Value)[]
			{
				(VueDefaultSlotComponentParameterName, component),
				(VueDefaultSlotPropsParameterName, props),
				(VueDefaultSlotChildParameterName, childContent)
			},
			parameters => new CallExpression(
				h,
				NodeList.From<Expression>(parameters[0], parameters[1], BuildVueDefaultSlotObject(parameters[2])),
				optional: false));
	}

	private void ValidateTypedVueDefaultSlotAuthoring(ISymbol symbol, IOperation? originOperation)
	{
		if (symbol is not IMethodSymbol method ||
			!TryGetTypedVueSlotContractType(method, out var slotType))
		{
			return;
		}

		var defaultSlots = CollectTypedVueDefaultSlotMembers(slotType);
		if (defaultSlots.Count == 0)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Typed Vue slot contract '{slotType.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not declare a default slot. Use H(component, slots) / H(component, props, slots) with an explicit slot object, or declare one slot property as 'Default' / Description(\"@#default\").");
		}

		if (defaultSlots.Count > 1)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Typed Vue slot contract '{slotType.ToDisplayString(Jazor.Common.Format.NameFormat)}' declares more than one default slot. Only one property may map to 'default' via the Default naming convention or Description(\"@#default\").");
		}

		var defaultSlot = defaultSlots[0];
		if (!IsVueSlotCallbackType(defaultSlot.Type, out var isScoped))
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Jazor.Common.Format.NameFormat)}' must use VueSlotCallback or VueSlotCallback<TScope>.");
		}

		if (isScoped)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Default slot member '{defaultSlot.ToDisplayString(Jazor.Common.Format.NameFormat)}' expects slot scope. Implicit child content cannot satisfy a typed default slot context. Use H(component, slots) / H(component, props, slots) and provide an explicit slot callback.");
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

				if (string.Equals(Util.GetConfigOrSymbolName(property), "default", StringComparison.Ordinal))
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

	private static Exception CreateVueAuthoringException(IOperation? originOperation, string message)
		=> originOperation is not null
			? CreateOperationTransformationException(originOperation, message)
			: new InvalidOperationException(message);

	private static ObjectExpression BuildVueDefaultSlotObject(Expression child)
	{
		var slotCallback = new ArrowFunctionExpression(
			NodeList.From<Node>(),
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
}
