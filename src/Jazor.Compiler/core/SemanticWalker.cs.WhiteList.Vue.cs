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
	public Expression? Compile_4d891df0d6540e03(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Vue.IVNode) 不应接收实例 handler。");
		if (args.Length != 2 || args[0] is not Expression component || args[1] is not Expression child)
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Vue.IVNode) 需要且仅需要两个显式参数。");

		return BuildVueDefaultSlotComponentCall(symbol, context, component, null, child, originOperation);
	}

	public Expression? Compile_bf7eb3c58b5fa599(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 不应接收实例 handler。");
		if (args.Length != 2 || args[0] is not Expression component || args[1] is not Expression children)
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 需要且仅需要两个显式参数。");

		return BuildVueDefaultSlotComponentCall(symbol, context, component, null, children, originOperation);
	}

	public Expression? Compile_304e553be1ef96b9(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Vue.VueProps, ECMAScript.Vue.IVNode) 不应接收实例 handler。");
		if (args.Length != 3 ||
			args[0] is not Expression component ||
			args[1] is not Expression props ||
			args[2] is not Expression child)
		{
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Vue.VueProps, ECMAScript.Vue.IVNode) 需要且仅需要三个显式参数。");
		}

		return BuildVueDefaultSlotComponentCall(symbol, context, component, props, child, originOperation);
	}

	public Expression? Compile_9fe39c798bb82645(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Vue.VueProps, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 不应接收实例 handler。");
		if (args.Length != 3 ||
			args[0] is not Expression component ||
			args[1] is not Expression props ||
			args[2] is not Expression children)
		{
			throw new InvalidOperationException("ECMAScript.Vue.H(ECMAScript.Vue.IVueComponent, ECMAScript.Vue.VueProps, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 需要且仅需要三个显式参数。");
		}

		return BuildVueDefaultSlotComponentCall(symbol, context, component, props, children, originOperation);
	}

	public Expression? Compile_218982270c602b63(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H<TSlots>(ECMAScript.Vue.IVueSlotComponent<TSlots>, ECMAScript.Vue.IVNode) 不应接收实例 handler。");
		if (args.Length != 2 || args[0] is not Expression component || args[1] is not Expression child)
			throw new InvalidOperationException("ECMAScript.Vue.H<TSlots>(ECMAScript.Vue.IVueSlotComponent<TSlots>, ECMAScript.Vue.IVNode) 需要且仅需要两个显式参数。");

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, null, child, originOperation);
	}

	public Expression? Compile_12f66773377f7470(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H<TSlots>(ECMAScript.Vue.IVueSlotComponent<TSlots>, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 不应接收实例 handler。");
		if (args.Length != 2 || args[0] is not Expression component || args[1] is not Expression children)
			throw new InvalidOperationException("ECMAScript.Vue.H<TSlots>(ECMAScript.Vue.IVueSlotComponent<TSlots>, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 需要且仅需要两个显式参数。");

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, null, children, originOperation);
	}

	public Expression? Compile_7144b71f1860ef44(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, ECMAScript.Vue.IVNode) 不应接收实例 handler。");
		if (args.Length != 2 || args[0] is not Expression component || args[1] is not Expression child)
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, ECMAScript.Vue.IVNode) 需要且仅需要两个显式参数。");

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, null, child, originOperation);
	}

	public Expression? Compile_1026633d034d6dff(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 不应接收实例 handler。");
		if (args.Length != 2 || args[0] is not Expression component || args[1] is not Expression children)
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 需要且仅需要两个显式参数。");

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, null, children, originOperation);
	}

	public Expression? Compile_ee7c23968f63b7e2(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, TProps, ECMAScript.Vue.IVNode) 不应接收实例 handler。");
		if (args.Length != 3 ||
			args[0] is not Expression component ||
			args[1] is not Expression props ||
			args[2] is not Expression child)
		{
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, TProps, ECMAScript.Vue.IVNode) 需要且仅需要三个显式参数。");
		}

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, props, child, originOperation);
	}

	public Expression? Compile_0700e0010fb8b9a9(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation)
	{
		if (handler is not null)
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, TProps, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 不应接收实例 handler。");
		if (args.Length != 3 ||
			args[0] is not Expression component ||
			args[1] is not Expression props ||
			args[2] is not Expression children)
		{
			throw new InvalidOperationException("ECMAScript.Vue.H<TProps, TSlots>(ECMAScript.Vue.IVueComponent<TProps, TSlots>, TProps, ECMAScript.Either<string, ECMAScript.Number, bool, ECMAScript.Vue.IVNode, ECMAScript.Vue.IVNode[]>) 需要且仅需要三个显式参数。");
		}

		ValidateTypedVueDefaultSlotAuthoring(symbol, originOperation);
		return BuildVueDefaultSlotComponentCall(symbol, context, component, props, children, originOperation);
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

		var componentId = new Identifier("__component");
		var childId = new Identifier("__slot0");
		var slotObject = BuildVueDefaultSlotObject(childId);

		if (props is null)
		{
			var call = new CallExpression(h, NodeList.From<Expression>(componentId, slotObject), optional: false);
			var iife = new ArrowFunctionExpression(
				NodeList.From<Node>(componentId, childId),
				call,
				expression: true,
				async: false);
			return new CallExpression(iife, NodeList.From<Expression>(component, childContent), optional: false);
		}

		var propsId = new Identifier("__props");
		var callWithProps = new CallExpression(h, NodeList.From<Expression>(componentId, propsId, slotObject), optional: false);
		var iifeWithProps = new ArrowFunctionExpression(
			NodeList.From<Node>(componentId, propsId, childId),
			callWithProps,
			expression: true,
			async: false);
		return new CallExpression(iifeWithProps, NodeList.From<Expression>(component, props, childContent), optional: false);
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
				$"Typed Vue slot contract '{slotType.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not declare a default slot. Use H(component, slots) / H(component, props, slots) with an explicit slot object, or mark one slot property with Description(\"@#default\").");
		}

		if (defaultSlots.Count > 1)
		{
			throw CreateVueAuthoringException(
				originOperation,
				$"Typed Vue slot contract '{slotType.ToDisplayString(Jazor.Common.Format.NameFormat)}' declares more than one default slot. Only one property may map to Description(\"@#default\").");
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

		if (method.TypeArguments.Length == 1 &&
			method.TypeArguments[0] is INamedTypeSymbol slotComponentSlotType &&
			method.Parameters.Length > 0 &&
			method.Parameters[0].Type is INamedTypeSymbol { IsGenericType: true } parameterType &&
			string.Equals(
				parameterType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat),
				"ECMAScript.Vue.IVueSlotComponent<TSlots>",
				StringComparison.Ordinal))
		{
			slotType = slotComponentSlotType;
			return true;
		}

		if (method.TypeArguments.Length == 2 &&
			method.TypeArguments[1] is INamedTypeSymbol componentSlotType &&
			method.Parameters.Length > 0 &&
			method.Parameters[0].Type is INamedTypeSymbol { IsGenericType: true } componentType &&
			string.Equals(
				componentType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat),
				"ECMAScript.Vue.IVueComponent<TProps, TSlots>",
				StringComparison.Ordinal))
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
			namedType.Name != "VueSlotCallback" ||
			namedType.ContainingNamespace?.ToDisplayString() != "ECMAScript")
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
			key: new Identifier("default"),
			value: slotCallback,
			computed: false,
			shorthand: false,
			method: false);
		return new ObjectExpression(NodeList.From<Node>(slotProperty));
	}
}
