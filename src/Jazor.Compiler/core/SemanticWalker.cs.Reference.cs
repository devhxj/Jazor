using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// 负责名称引用、成员引用、方法组和运行时全局对象的 lowering。
/// </summary>
/// <remarks>
/// 引用解析必须区分 C# 源码符号、白名单宿主成员和 JavaScript 全局值；不能仅凭名字字符串
/// 猜测目标。实例方法组还可能需要绑定 receiver，避免取出方法后丢失 <c>this</c> 语义。
/// </remarks>
public partial class SemanticWalker
{
	private static readonly HashSet<string> GlobalRuntimeTypeNames = new(StringComparer.Ordinal)
	{
		"Array",
		"ArrayBuffer",
		"BigInt",
		"Boolean",
		"DataView",
		"Date",
		"Error",
		"Function",
		"Map",
		"Number",
		"Object",
		"Promise",
		"RegExp",
		"Set",
		"String",
		"Symbol",
		"WeakMap",
		"WeakSet",
	};

	// 这些 JavaScript Array 方法保证返回新的 Array；LINQ ToArray/ToList 可直接接管该结果。
	// reverse/sort/fill 等返回原 receiver 的方法不得加入，否则会破坏物化操作的复制语义。
	private static readonly HashSet<string> FreshArrayResultMethodNames = new(StringComparer.Ordinal)
	{
		"concat",
		"filter",
		"flat",
		"flatMap",
		"map",
		"slice",
		"splice",
		"toReversed",
		"toSorted",
		"toSpliced",
		"with",
	};

	private static readonly Dictionary<string, (int ParameterCount, string? ArrayMethodName, string? CallbackNullParameterName)> EnumerableArrayLikeIntrinsics = new(StringComparer.Ordinal)
	{
		["Where"] = (2, "filter", "predicate"),
		["Select"] = (2, "map", "selector"),
		["ToList"] = (1, null, null),
		["ToArray"] = (1, null, null)
	};

	/// <summary>
	/// 获取初始化器成员的名称，优先检查白名单别名
	/// 对于属性：检查 setter 的白名单别名（初始化器是设置值）
	/// 对于字段：检查字段本身的白名单别名
	/// </summary>
	private static string GetInitializerMemberName(ISymbol symbol)
	{
		// 1. 先检查白名单别名
		ISymbol? whiteListSymbol = symbol;
		if (symbol is IPropertySymbol property && property.SetMethod is not null)
			whiteListSymbol = property.SetMethod;

		if (TryGetWhiteListValue(WhiteList.Members, whiteListSymbol, out _, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		// 2. 再检查特性配置
		return Util.GetConfigOrSymbolName(symbol);
	}

	/// <summary>
	/// 获取方法的名称，优先检查白名单别名
	/// </summary>
	private static string GetMethodConfigOrWhiteListName(IMethodSymbol method)
	{
		// 1. 先检查白名单别名
		if (TryGetWhiteListValue(WhiteList.Members, method, out _, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		// 2. 再检查特性配置
		return Util.GetConfigOrSymbolName(method);
	}

	private bool TryGetCurrentModuleDeclaredName(ISymbol symbol, out string name)
	{
		name = null!;
		if (_moduleDeclaredNames is null)
			return false;

		return _moduleDeclaredNames.TryGetValue(symbol.OriginalDefinition, out name);
	}

	private string GetCurrentModuleDeclaredOrConfigName(ISymbol symbol)
		=> TryGetCurrentModuleDeclaredName(symbol, out var declaredName)
			? declaredName
			: Util.GetConfigOrSymbolName(symbol);

	private static string? GetTypeConfigOrWhiteListName(ITypeSymbol symbol)
	{
		string? name = null;

		// 先查询白名单
		var displayName = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (TryGetWhiteListValue(WhiteList.Types, displayName, out _, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			name = entry.Value!;

		// 再取特性配置
		if (string.IsNullOrEmpty(name))
		{
			if (Util.HasNameResolutionBoundary(symbol))
				return null;

			name = Util.GetSymbolConfigName(symbol) ?? symbol.Name;
		}

		return name;
	}

	private static string? GetModuleImportPath(ITypeSymbol symbol)
		=> Util.GetECMAScriptModuleImportPath(symbol);

	private static bool ShouldFlattenRuntimeNestedType(ITypeSymbol symbol)
	{
		if (symbol is not INamedTypeSymbol namedType || namedType.ContainingType is null)
			return false;

		// 当前编译器的声明侧会把用户代码中的成员类型扁平化为顶层运行时声明，
		// 因此引用侧也必须使用同一个运行时名，不能继续保留 Outer.Inner 链。
		// ECMAScript 运行时嵌套类型分两类：
		// - [ECMAScriptModule] 宿主：声明侧已经发成同模块的顶层 named export，
		//   引用侧必须按同一名字直接导入，不能依赖 Outer.Inner 模块对象。
		// - 真实 JS 宿主（例如 Intl.*）：层级本身就是运行时对象结构，仍需保留。
		if (namedType.ContainingType.IsStatic &&
			HasEcmascriptSupportMarker(namedType.ContainingType))
			return !string.IsNullOrWhiteSpace(GetEffectiveModuleImportPath(namedType));

		return true;
	}

	/// <summary>
	/// 获取类型在运行时应归属的模块路径。
	///
	/// 对于被声明侧扁平化成顶层导出的嵌套类型，模块路径并不写在嵌套类型自身，
	/// 而是写在它所属的外层模块类上。因此这里需要沿包含链向上追溯，
	/// 让引用侧也能从同一个模块导入那个扁平化后的运行时名字。
	/// </summary>
	private static string? GetEffectiveModuleImportPath(ITypeSymbol symbol)
	{
		for (var current = symbol; current is not null; current = current.ContainingType)
		{
			var modulePath = GetModuleImportPath(current);
			if (!string.IsNullOrWhiteSpace(modulePath))
				return modulePath;
		}

		return null;
	}

	private Expression? BuildFullTypeName(ITypeSymbol symbol, SenseArgument? context = null)
	{
		var namedTypeSymbol = symbol as INamedTypeSymbol;
		if (namedTypeSymbol is not null)
			Host?.ObserveTypeReference(namedTypeSymbol, context ?? SenseArgument.Default);

		if (namedTypeSymbol is not null &&
			TryGetCurrentModuleDeclaredName(namedTypeSymbol, out var moduleDeclaredTypeName))
		{
			return new Identifier(moduleDeclaredTypeName);
		}

		if (ShouldFlattenRuntimeNestedType(symbol))
		{
			var flatName = GetTypeConfigOrWhiteListName(symbol);
			if (string.IsNullOrEmpty(flatName))
				return null;

			if (GlobalRuntimeTypeNames.Contains(flatName!))
				return new Identifier(flatName!);

			var modulePath = GetEffectiveModuleImportPath(symbol);
			if (!string.IsNullOrEmpty(modulePath))
			{
				if (context is SenseArgument importContext)
					return importContext.BindImportSpecifier(modulePath!, flatName!);
			}

			return new Identifier(flatName!);
		}

		var queue = new Stack<string>();
		var type = symbol;
		while (type is not null)
		{
			if (_moduleRootType is not null &&
				SymbolEqualityComparer.Default.Equals(type, _moduleRootType))
				break;

			var name = GetTypeConfigOrWhiteListName(type);
			if (string.IsNullOrEmpty(name))
				break;

			var modulePath = GetModuleImportPath(type);
			if (!string.IsNullOrEmpty(modulePath))
			{
				if (_moduleRootType is null || !SymbolEqualityComparer.Default.Equals(type, _moduleRootType))
				{
					if (context is SenseArgument importContext)
						return importContext.BindImportSpecifier(modulePath!, name!);
				}

				queue.Push(name!);
				break;
			}

			queue.Push(name!);

			type = type.ContainingType;
		}

		Expression? expr = null;
		if (queue.Count > 0)
		{
			expr = new Identifier(queue.Pop());
			while (queue.Count > 0)
			{
				var property = new Identifier(queue.Pop());
				expr = new MemberExpression(expr, property, computed: false, optional: false);
			}
		}
		return expr;
	}

	private Expression BuildRuntimeTypeTokenExpression(IOperation operation, ITypeSymbol typeSymbol, SenseArgument argument)
	{
		if (typeSymbol is INamedTypeSymbol namedType && IsStructuralType(namedType))
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Type '{typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}' does not expose a stable runtime type token because structural lowering is in effect. Use property/positional contracts instead of typeof(...).");
		}

		if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType || typeSymbol.TypeKind == TypeKind.Interface)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Type '{typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}' does not expose a stable runtime type token in JavaScript conversion.");
		}

		var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (TryGetWhiteListRuntimeValueCarrier(typeSymbol, out _))
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Type '{displayName}' does not expose a stable runtime type token because its internal value carrier does not represent the CLR type and its mapped members.");
		}

		var (mapper, typeName) = GetMapperType(typeSymbol);
		switch (mapper)
		{
			case TypeMapper.String:
			case TypeMapper.Number:
			case TypeMapper.BigInt:
			case TypeMapper.Boolean:
			case TypeMapper.Date:
			case TypeMapper.Map:
			case TypeMapper.Set:
			case TypeMapper.Array:
				return new Identifier(typeName);
			case TypeMapper.Class:
				RejectUnsupportedTypeFallback(operation, typeSymbol, "typeof type token");
				RejectAmbiguousRuntimeTypeFilter(operation, typeSymbol, "typeof type token");
				return BuildFullTypeName(typeSymbol, argument) ?? new Identifier(typeName);
			default:
				return HandleTransformationFailure<Expression>(
					operation,
					$"Type '{typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}' does not expose a stable runtime type token in JavaScript conversion.");
		}
	}

	private bool TryBuildImportedModuleMember(ITypeSymbol? containingType, string memberName, SenseArgument? context, out Expression? expression)
	{
		expression = null;
		if (containingType is null)
			return false;

		if (string.Equals(memberName, "default", StringComparison.Ordinal))
		{
			throw new NotSupportedException(
				$"Jazor module import does not support default export. Member import from '{containingType.ToDisplayString(Format.NameFormat)}' resolves to export name 'default'. Use a named export instead.");
		}

		var modulePath = GetModuleImportPath(containingType);
		if (string.IsNullOrWhiteSpace(modulePath))
			return false;

		if (IsCurrentModuleType(containingType))
			return false;

		var importId = context?.BindImportSpecifier(modulePath!, memberName) ?? new Identifier(memberName);
		expression = importId;
		return true;
	}

	private bool IsCurrentModuleType(ITypeSymbol? type)
	{
		if (_moduleRootType is null || type is null)
			return false;

		for (var current = type; current is not null; current = current.ContainingType)
		{
			if (SymbolEqualityComparer.Default.Equals(current, _moduleRootType) ||
				SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, _moduleRootType.OriginalDefinition))
				return true;
		}

		return false;
	}

	/// <summary>
	/// 为 ECMAScript 运行时静态成员选择最终宿主。
	///
	/// 这里解决的是“声明宿主”和“真实 JS 宿主”不一定相同的问题：
	/// - 普通映射场景应优先使用运行时宿主，例如 <c>System.Console -> console</c>。
	/// - 如果静态成员声明在基类/泛型基类上，而调用点实际使用的是更具体的运行时类型，
	///   则必须保留那个更具体的宿主，例如
	///   <c>TypedArray&lt;T, TArray&gt;</c> 上声明的成员，在
	///   <c>Uint8Array.Of(...)</c> / <c>Uint8Array.BYTES_PER_ELEMENT</c> 上
	///   仍应输出到 <c>Uint8Array</c>。
	///
	/// 选择顺序：
	/// 1. 先从声明宿主推导一个稳定的运行时宿主。
	/// 2. 再从调用点语法定位 symbol，并恢复“用户真正写下的宿主类型”。
	/// 3. 只有当调用点宿主与声明宿主在继承/接口/泛型原型定义上兼容时，才允许覆盖。
	/// 4. 语义宿主无法恢复时不猜测源码标识符，交给后续 symbol-based 路径处理。
	/// </summary>
	private bool TryBuildPreferredRuntimeStaticMemberAccess(ISymbol symbol, SyntaxNode syntax, SemanticModel? semanticModel, string memberName, out Expression? expression)
	{
		expression = null;
		var isRuntime = Util.IsECMAScriptRuntimeSymbol(symbol);
		if (!isRuntime)
			return false;

		var hostType = symbol switch
		{
			IMethodSymbol { IsStatic: true } method => method.ReceiverType ?? method.ContainingType,
			_ => symbol.ContainingType
		};
		if (hostType is null)
			return false;

		var runtimeHost = TryBuildRuntimeHostExpression(hostType);
		if (runtimeHost is null)
			return false;

		// 优先用语义模型恢复调用点宿主，而不是只信语法文本。
		// 这样 using Bytes = Uint8Array 这类别名最终仍会落到 Uint8Array，而不是 Bytes。
		var sourceHostType = TryGetStaticSourceHostTypeFromSyntax(syntax, semanticModel);
		if (sourceHostType is not null &&
			IsStaticHostOverrideCompatible(sourceHostType, hostType))
		{
			var sourceRuntimeHost = TryBuildRuntimeHostExpression(sourceHostType);
			if (sourceRuntimeHost is not null)
			{
				expression = new MemberExpression(sourceRuntimeHost, new Identifier(memberName), computed: false, optional: false);
				return true;
			}
		}

		var specializedRuntimeHostType = TryGetSpecializedRuntimeHostType(hostType);
		if (specializedRuntimeHostType is not null)
		{
			expression = new MemberExpression(runtimeHost, new Identifier(memberName), computed: false, optional: false);
			return true;
		}

		return false;
	}

	/// <summary>
	/// 从静态访问的语法节点中恢复调用点宿主对应的语义类型。
	///
	/// 这里不能只取语法文本：
	/// - <c>Bytes.Of(...)</c> 里的 <c>Bytes</c> 可能是 using alias；
	/// - <c>Namespace.Type.Member</c> / <c>Outer.Inner.Member</c> 需要拿到最终绑定后的类型；
	/// - Roslyn 在属性、方法组、调用三种静态访问上给出的 syntax 颗粒度并不一致。
	/// </summary>
	private static bool IsMemberAccessNameSyntax(SyntaxNode syntax)
		=> syntax.Parent switch
		{
			MemberAccessExpressionSyntax memberAccess => ReferenceEquals(memberAccess.Name, syntax),
			QualifiedNameSyntax qualifiedName => ReferenceEquals(qualifiedName.Right, syntax),
			_ => false
		};

	private static ITypeSymbol? TryGetStaticSourceHostTypeFromSyntax(SyntaxNode syntax, SemanticModel? semanticModel)
	{
		if (semanticModel is null)
			return null;

		var effectiveSyntax = syntax switch
		{
			IdentifierNameSyntax or GenericNameSyntax when IsMemberAccessNameSyntax(syntax)
				=> syntax.Parent,
			_ => syntax
		};

		var targetSyntax = effectiveSyntax switch
		{
			InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			QualifiedNameSyntax qualifiedName => qualifiedName.Left,
			_ => null
		};
		if (targetSyntax is null)
			return null;

		var symbol = semanticModel.GetSymbolInfo(targetSyntax).Symbol;
		if (symbol is IAliasSymbol alias && alias.Target is ITypeSymbol aliasType)
			return aliasType;

		if (symbol is ITypeSymbol typeSymbol)
			return typeSymbol;

		return semanticModel.GetTypeInfo(targetSyntax).Type;
	}

	/// <summary>
	/// 判断调用点宿主是否可以安全覆盖声明宿主。
	///
	/// 允许覆盖的前提是：调用点宿主必须就是声明宿主本身，或者能通过
	/// “继承链 / 接口实现 / 泛型原型定义一致”证明两者属于同一套运行时 API。
	/// 这样既能支持基类声明、子类复用的静态成员，也能避免把无关类型错误改写到一起。
	/// </summary>
	private static bool IsStaticHostOverrideCompatible(ITypeSymbol sourceHostType, ITypeSymbol declaredHostType)
	{
		if (SymbolEqualityComparer.Default.Equals(sourceHostType, declaredHostType) ||
			SymbolEqualityComparer.Default.Equals(sourceHostType.OriginalDefinition, declaredHostType.OriginalDefinition))
			return true;

		if (sourceHostType is not INamedTypeSymbol sourceNamed)
			return false;

		for (var current = sourceNamed.BaseType; current is not null; current = current.BaseType)
		{
			if (SymbolEqualityComparer.Default.Equals(current, declaredHostType) ||
				SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, declaredHostType.OriginalDefinition))
				return true;
		}

		return sourceNamed.AllInterfaces.Any(@interface =>
			SymbolEqualityComparer.Default.Equals(@interface, declaredHostType) ||
			SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, declaredHostType.OriginalDefinition));
	}

	private static string? TryExtractExtensionReceiverDisplayName(ITypeSymbol? type)
	{
		if (type is null)
			return null;

		var display = type.OriginalDefinition.ToDisplayString(Format.NameFormat);
		const string marker = ".extension(";
		var start = display.IndexOf(marker, System.StringComparison.Ordinal);
		if (start < 0)
			return null;

		start += marker.Length;
		var end = display.LastIndexOf(')');
		if (end <= start)
			return null;

		return display.Substring(start, end - start);
	}

	private static string? TryGetTypeAliasFromWhiteList(string displayName)
	{
		if (TryGetWhiteListValue(WhiteList.Types, displayName, out _, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value;

		return null;
	}

	/// <summary>
	/// 尝试从“自类型泛型约束”里恢复真实运行时宿主。
	///
	/// 典型例子是 <c>TypedArray&lt;T, TArray&gt;</c>：
	/// 静态成员声明在泛型基类上，但第二个类型参数才是真正的 JS 构造器，
	/// 例如 <c>Uint8Array</c>、<c>BigInt64Array</c>。
	///
	/// 这里不靠类型名硬编码，而是查找“某个类型参数的约束再次引用了当前泛型定义本身”。
	/// 一旦命中这种 self-typed 约束，并且对应类型实参本身也是 ECMAScript 运行时类型，
	/// 就把它视为更具体的最终宿主。
	/// </summary>
	private static ITypeSymbol? TryGetSpecializedRuntimeHostType(ITypeSymbol? type)
	{
		if (type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
			return null;

		var originalDefinition = namedType.OriginalDefinition;
		var count = System.Math.Min(namedType.TypeParameters.Length, namedType.TypeArguments.Length);
		for (var index = 0; index < count; index++)
		{
			var typeArgument = namedType.TypeArguments[index];
			if (typeArgument.TypeKind == TypeKind.TypeParameter)
				continue;

			var typeParameter = namedType.TypeParameters[index];
			var matchesSelfTypedConstraint = typeParameter.ConstraintTypes.Any(constraintType =>
				constraintType is INamedTypeSymbol constraintNamed &&
				SymbolEqualityComparer.Default.Equals(constraintNamed.OriginalDefinition, originalDefinition));
			if (!matchesSelfTypedConstraint)
				continue;

			if (Util.IsECMAScriptRuntimeSymbol(typeArgument))
				return typeArgument;
		}

		return null;
	}

	/// <summary>
	/// 根据现有类型映射推导运行时宿主表达式。
	///
	/// 这里不引入额外的 StaticHost 表，直接复用当前设计里已经稳定存在的两类信息：
	/// - 类型别名白名单，例如 <c>System.Console -> console</c>
	/// - ECMAScript 扩展宿主上的接收者类型
	/// </summary>
	private Expression? TryBuildRuntimeHostExpression(ITypeSymbol? type, SenseArgument? context = null)
	{
		if (type is null)
			return null;

		// 先尝试恢复“语义上更具体”的运行时宿主。
		// 这能避开 C# using 类型别名等纯语法名字，把 TypedArray<T, TArray> 正确落到 Uint8Array 之类的真实 JS host。
		var specializedRuntimeHostType = TryGetSpecializedRuntimeHostType(type);
		if (specializedRuntimeHostType is not null)
		{
			var specializedHost = BuildFullTypeName(specializedRuntimeHostType, context);
			if (specializedHost is not null)
				return specializedHost;
		}

		// 优先走现有类型映射。
		// 这条路径已经覆盖白名单别名、模块导入和名称边界，能直接复用当前设计。
		var host = BuildFullTypeName(type, context);
		if (host is not null)
			return host;

		// 兜底处理 ECMAScript 扩展宿主。
		// 某些 API 的声明宿主不是最终 JS 对象本身，而是 extension(receiver) 这种桥接类型。
		var receiverDisplayName = TryExtractExtensionReceiverDisplayName(type);
		if (string.IsNullOrEmpty(receiverDisplayName))
			return null;

		var alias = TryGetTypeAliasFromWhiteList(receiverDisplayName!);
		return string.IsNullOrEmpty(alias) ? null : new Identifier(alias!);
	}

	private static string? TryGetRuntimeHostSourceName(ITypeSymbol? type)
	{
		if (type is null)
			return null;

		// 这里取“源码里最可能出现的宿主名”，用于识别 Console.Log / Array.Of 这类
		// 尚未归一化的接收端写法，然后再替换成真实运行时 host。
		if (!string.IsNullOrEmpty(type.Name))
			return type.Name;

		var receiverDisplayName = TryExtractExtensionReceiverDisplayName(type);
		if (string.IsNullOrEmpty(receiverDisplayName))
			return null;

		var simpleName = receiverDisplayName!.Split('.').Last();
		var genericIndex = simpleName.IndexOf('<');
		return genericIndex >= 0 ? simpleName.Substring(0, genericIndex) : simpleName;
	}

	private Expression? TryBuildExtensionHostTarget(IMethodSymbol method, SenseArgument? context)
	{
		if (!method.IsStatic ||
			!Util.IsECMAScriptRuntimeSymbol(method))
			return null;

		if (method.ReceiverType is null)
			return null;

		return TryBuildRuntimeHostExpression(method.ReceiverType, context);
	}

	private bool TryBuildImportedModulePropertyAccess(IPropertySymbol property, SenseArgument? context, out Expression? expression)
	{
		expression = null;
		if (!property.IsStatic || property.GetMethod is null)
			return false;

		var explicitImportName = Util.GetSymbolConfigName(property.GetMethod) ?? Util.GetSymbolConfigName(property);
		if (!string.IsNullOrEmpty(explicitImportName))
			return TryBuildImportedModuleMember(property.ContainingType, explicitImportName!, context, out expression);

		var getterName = GetMethodConfigOrWhiteListName(property.GetMethod);
		if (!TryBuildImportedModuleMember(property.ContainingType, getterName, context, out var getter) ||
			getter is null)
			return false;

		expression = new CallExpression(getter, NodeList.Empty<Expression>(), optional: false);
		return true;
	}

	private bool TryBuildCurrentModulePropertyGetterCall(IPropertySymbol property, out Expression? expression)
	{
		expression = null;
		if (!property.IsStatic || property.GetMethod is null)
			return false;

		if (!TryGetCurrentModuleDeclaredName(property.GetMethod, out var getterName))
			return false;

		expression = new CallExpression(new Identifier(getterName), NodeList.Empty<Expression>(), optional: false);
		return true;
	}

	private bool TryBuildCurrentModulePropertySetterCall(IPropertySymbol property, Expression value, out Expression? expression)
	{
		expression = null;
		if (!property.IsStatic || property.SetMethod is null)
			return false;

		if (!TryGetCurrentModuleDeclaredName(property.SetMethod, out var setterName))
			return false;

		expression = new CallExpression(new Identifier(setterName), NodeList.From(value), optional: false);
		return true;
	}

	private bool TryBuildImportedModulePropertySetterCall(IPropertySymbol property, SenseArgument? context, Expression value, out Expression? expression)
	{
		expression = null;
		if (!property.IsStatic || property.SetMethod is null)
			return false;

		var setterName = GetMethodConfigOrWhiteListName(property.SetMethod);
		if (!TryBuildImportedModuleMember(property.ContainingType, setterName, context, out var setter) ||
			setter is null)
			return false;

		expression = new CallExpression(setter, NodeList.From(value), optional: false);
		return true;
	}

	/// <summary>
	/// 当调用点把运行时宿主写成 CLR 名称时，统一替换成真实 JavaScript 宿主。
	/// 例如 <c>Console.Log</c> 的实例部分会先表现为 <c>Console</c>，
	/// 这里再归一化为 <c>console</c>。
	/// </summary>
	private Expression NormalizeRuntimeReceiverHostInstance(Expression instance, IMethodSymbol method)
	{
		var hostType = method.ReceiverType ?? method.ContainingType;
		if (hostType is null || instance is not Identifier identifier)
			return instance;

		var sourceName = TryGetRuntimeHostSourceName(hostType);
		if (string.IsNullOrEmpty(sourceName) ||
			!string.Equals(identifier.Name, sourceName, System.StringComparison.Ordinal))
			return instance;

		// 只改写“裸宿主标识符”场景，避免误伤更复杂的用户表达式。
		return TryBuildRuntimeHostExpression(hostType) ?? instance;
	}

	/// <summary>
	/// 与 <see cref="NormalizeRuntimeReceiverHostInstance"/> 类似，但作用于已经拼好的成员访问表达式。
	/// 这样方法组引用和普通调用都能共用同一套宿主归一化逻辑。
	/// </summary>
	private Expression NormalizeRuntimeReceiverHostCallee(Expression callee, IMethodSymbol method)
	{
		var hostType = method.ReceiverType ?? method.ContainingType;
		if (hostType is null ||
			callee is not MemberExpression { Object: Identifier identifier, Property: var property, Computed: var computed, Optional: var optional })
			return callee;

		var sourceName = TryGetRuntimeHostSourceName(hostType);
		if (string.IsNullOrEmpty(sourceName) ||
			!string.Equals(identifier.Name, sourceName, System.StringComparison.Ordinal))
			return callee;

		var runtimeHost = TryBuildRuntimeHostExpression(hostType);
		if (runtimeHost is null)
			return callee;

		// 保留原成员名与可选/计算属性形态，只替换宿主部分。
		return new MemberExpression(runtimeHost, property, computed, optional);
	}

	private Expression GetFieldName(IFieldSymbol symbol)
	{
		// CLR host constants have already passed through WhiteList above this fallback.
		// Keep source-defined constants on the shared literal path so there is only one
		// place that owns numeric width, string-enum and special-value semantics.
		if (symbol.IsConst)
			return BuildValueLiteral(symbol.Type, symbol.ConstantValue)!;

		return new Identifier(GetCurrentModuleDeclaredOrConfigName(symbol));
	}

	private bool IsPrivateRuntimeClassField(IFieldSymbol field)
	{
		if (field.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal ||
			field.ContainingType is null ||
			_moduleRootType is null ||
			SymbolEqualityComparer.Default.Equals(field.ContainingType.OriginalDefinition, _moduleRootType.OriginalDefinition))
		{
			return false;
		}

		return TryGetCurrentModuleDeclaredName(field.ContainingType, out _);
	}

	private static bool TryBuildStringEnumLiteral(IFieldSymbol symbol, out Expression expression)
	{
		expression = null!;
		if (!symbol.HasConstantValue ||
			symbol.ContainingType?.TypeKind != TypeKind.Enum ||
			!Util.IsStringEnumType(symbol.ContainingType))
			return false;

		expression = CreateStringLiteral(GetStringEnumLiteralText(symbol));
		return true;
	}

	private static bool TryBuildStringEnumValueLiteral(INamedTypeSymbol enumType, object value, out Expression expression)
	{
		expression = null!;
		if (!Util.IsStringEnumType(enumType))
			return false;

		foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
		{
			if (!member.HasConstantValue ||
				!Equals(member.ConstantValue, value))
			{
				continue;
			}

			expression = CreateStringLiteral(GetStringEnumLiteralText(member));
			return true;
		}

		return false;
	}

	private static string GetStringEnumLiteralText(IFieldSymbol symbol)
	{
		foreach (var attribute in symbol.GetAttributes())
		{
			if (attribute.ConstructorArguments.Length == 0)
				continue;

			if (attribute.AttributeClass?.Name == "ECMAScriptNameAttribute")
			{
				return attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
			}

			if (attribute.AttributeClass?.Name != "DescriptionAttribute")
				continue;

			var description = attribute.ConstructorArguments[0].Value?.ToString()?.Trim();
			if (description?.StartsWith("@#", System.StringComparison.Ordinal) != true)
				continue;

			return description.Substring(2);
		}

		return Util.GetSymbolConfigName(symbol) ?? symbol.Name;
	}

	private static bool IsErasedUnionProjectionProperty(IPropertySymbol property)
	{
		if (property.IsStatic ||
			property.Parameters.Length != 0 ||
			property.GetMethod is null ||
			!IsErasedUnionProjectionPropertyName(property.Name))
			return false;

		return property.ContainingType is INamedTypeSymbol namedType &&
			ImplementsErasedUnionContract(namedType);
	}

	private static bool IsErasedUnionProjectionPropertyName(string propertyName)
		=> propertyName == "Value" || propertyName.StartsWith("As", System.StringComparison.Ordinal);

	private static bool ImplementsErasedUnionContract(INamedTypeSymbol type)
	{
		return Util.IsHostErasedUnionType(type.OriginalDefinition);
	}

	private static bool IsUnsupportedUnionProjectionProperty(IPropertySymbol property)
	{
		if (property.IsStatic ||
			property.Parameters.Length != 0 ||
			property.GetMethod is null ||
			!IsErasedUnionProjectionPropertyName(property.Name) ||
			property.ContainingType is not INamedTypeSymbol namedType)
			return false;

		var originalDefinition = namedType.OriginalDefinition;
		if (Util.IsHostErasedUnionType(originalDefinition))
			return false;

		return Util.IsSystemUnionType(originalDefinition) ||
			Util.IsRuntimeIUnionType(originalDefinition);
	}

	private static Expression BuildAliasedPropertyAccess(Expression instance, string propertyName, bool optional)
	{
		return TryBuildComputedAliasProperty(propertyName, out var computedProperty)
			? new MemberExpression(instance, computedProperty, computed: true, optional: optional)
			: new MemberExpression(instance, new Identifier(propertyName), computed: false, optional: optional);
	}

	private static bool TryBuildComputedAliasProperty(string propertyName, out Expression property)
	{
		property = null!;
		if (propertyName.Length < 3 ||
			propertyName[0] != '[' ||
			propertyName[propertyName.Length - 1] != ']')
		{
			return false;
		}

		var inner = propertyName.Substring(1, propertyName.Length - 2).Trim();
		if (inner.Length == 0)
			return false;

		if (int.TryParse(inner, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var numericIndex))
		{
			property = new NumericLiteral(numericIndex, numericIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
			return true;
		}

		if ((inner[0] == '"' && inner[inner.Length - 1] == '"') ||
			(inner[0] == '\'' && inner[inner.Length - 1] == '\''))
		{
			var literal = inner.Substring(1, inner.Length - 2);
			property = CreateStringLiteral(literal);
			return true;
		}

		return false;
	}

	private static Expression BuildArrayFrom(Expression value) =>
		new CallExpression(
			new MemberExpression(new Identifier("Array"), new Identifier("from"), computed: false, optional: false),
			NodeList.From(value),
			optional: false);

	private static ThrowStatement BuildThrowErrorStatement(string message)
	{
		var errorExpression = new NewExpression(
			new Identifier("Error"),
			NodeList.From<Expression>(CreateStringLiteral(message)));
		return new ThrowStatement(errorExpression);
	}

	private static ThrowStatement BuildThrowTypeErrorStatement(string message)
	{
		var errorExpression = new NewExpression(
			new Identifier("TypeError"),
			NodeList.From<Expression>(CreateStringLiteral(message)));
		return new ThrowStatement(errorExpression);
	}

	private static ThrowStatement BuildArgumentNullThrowStatement(string parameterName)
		=> BuildArgumentNullTypeErrorThrowStatement(parameterName);

	private static ThrowStatement BuildArgumentNullTypeErrorThrowStatement(string parameterName)
		=> BuildThrowTypeErrorStatement(parameterName);

	private static bool IsEnumerableContractType(ITypeSymbol? typeSymbol)
		=> HasContractType(typeSymbol, static displayName =>
			displayName is "System.Collections.IEnumerable" or "System.Collections.Generic.IEnumerable<T>");

	private static bool IsDictionaryContractType(ITypeSymbol? typeSymbol)
		=> HasContractType(typeSymbol, static displayName =>
			displayName is "System.Collections.IDictionary" or "System.Collections.Generic.IDictionary<TKey, TValue>");

	private static bool IsSetContractType(ITypeSymbol? typeSymbol)
		=> HasContractType(typeSymbol, static displayName =>
			displayName is "System.Collections.Generic.ISet<T>");

	private static bool IsListLikeContractType(ITypeSymbol? typeSymbol)
	{
		if (IsDictionaryContractType(typeSymbol) || IsSetContractType(typeSymbol))
			return false;

		// ICollection 与 IEnumerable 归同一组，统一走可迭代物化路径；
		// 仅 IList 视为“可直接数组方法”的强契约。
		return HasContractType(typeSymbol, static displayName =>
			displayName is
				"System.Collections.IList" or
				"System.Collections.Generic.IList<T>");
	}

	private static bool HasContractType(ITypeSymbol? typeSymbol, Func<string, bool> matcher)
	{
		if (typeSymbol is null)
			return false;

		var pending = new Queue<ITypeSymbol>();
		var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
		pending.Enqueue(typeSymbol);

		while (pending.Count > 0)
		{
			var current = pending.Dequeue();
			if (!visited.Add(current))
				continue;

			var displayName = current.OriginalDefinition.ToDisplayString(Format.NameFormat);
			if (matcher(displayName))
				return true;

			foreach (var iface in current.AllInterfaces)
				pending.Enqueue(iface);

			if (current is ITypeParameterSymbol typeParameter)
			{
				foreach (var constraint in typeParameter.ConstraintTypes)
					pending.Enqueue(constraint);
			}
		}

		return false;
	}

	private bool IsConcreteArrayLikeType(ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is null)
			return false;

		return GetMapperType(typeSymbol).Mapper == TypeMapper.Array;
	}

	private static bool IsArrayProducingExpression(Expression expression)
	{
		switch (expression)
		{
			case ArrayExpression:
				return true;

			case NewExpression { Callee: Identifier { Name: "Array" } }:
				return true;

			case CallExpression { Callee: MemberExpression { Object: Identifier { Name: "Array" }, Property: Identifier { Name: "from" or "of" } } }:
				return true;

			case CallExpression { Callee: MemberExpression { Property: Identifier { Name: var methodName } } }:
				return FreshArrayResultMethodNames.Contains(methodName);

			default:
				return false;
		}
	}

	private bool TryBuildEnumerableArrayLikeIntrinsic(IInvocationOperation operation, IMethodSymbol method, List<Expression> arguments, out Expression? expression)
	{
		expression = null;
		if (!EnumerableArrayLikeIntrinsics.TryGetValue(method.Name, out var intrinsic) ||
			method.Parameters.Length != intrinsic.ParameterCount ||
			method.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Collections_Generic_IEnumerable_T ||
			method.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat) != "System.Linq.Enumerable" ||
			!TryGetWhiteListValue(WhiteList.Members, method, out _, out var memberEntry) ||
			memberEntry.Op != Op.Import)
			return false;

		var sourceOperation = UnwrapImplicitConversions(operation.Arguments[0].Value);
		var sourceExpression = arguments[0];
		var sourceIsArrayProducingExpression = IsArrayProducingExpression(sourceExpression);
		var sourceIsEnumerableContract = IsEnumerableContractType(sourceOperation.Type);
		var sourceSupportsArrayMethods =
			IsListLikeContractType(sourceOperation.Type) ||
			(IsConcreteArrayLikeType(sourceOperation.Type) && !sourceIsEnumerableContract) ||
			sourceIsArrayProducingExpression;

		var sourceParameter = new Identifier("__src");
		var sourceArgument = sourceParameter as Expression;

		// Enumerable 的 source 已由 Roslyn 绑定为 IEnumerable<T>。IEnumerable/ICollection 的
		// Array alias 不代表运行时必有 Array 方法；仅 IList、宿主 Array 或新数组结果可直通。
		var normalizedSource = sourceSupportsArrayMethods
			? sourceArgument
			: BuildArrayFrom(sourceArgument);

		Identifier? callbackParameter = null;
		Expression? callbackArgument = null;
		if (intrinsic.CallbackNullParameterName is not null)
		{
			callbackParameter = new Identifier("__callback");
			callbackArgument = callbackParameter;
		}

		var intrinsicExpression = intrinsic.ArrayMethodName is not null
			? BuildInstanceMethodCall(normalizedSource, intrinsic.ArrayMethodName, callbackArgument!)
			: sourceIsArrayProducingExpression ? sourceArgument : BuildArrayFrom(sourceArgument);

		var statements = new List<Statement>
		{
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, sourceParameter, Null),
				BuildArgumentNullThrowStatement("source"),
				null)
		};
		var parameters = new List<Node> { sourceParameter };
		var callArguments = new List<Expression> { sourceExpression };

		if (callbackParameter is not null)
		{
			parameters.Add(callbackParameter);
			callArguments.Add(arguments[1]);
			statements.Add(new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, callbackParameter, Null),
				BuildArgumentNullThrowStatement(intrinsic.CallbackNullParameterName!),
				null));
		}

		statements.Add(new ReturnStatement(intrinsicExpression));
		var body = new FunctionBody(NodeList.From(statements), strict: true);
		var iife = new ArrowFunctionExpression(
			NodeList.From(parameters),
			body,
			expression: false,
			async: false);
		expression = new CallExpression(iife, NodeList.From(callArguments), optional: false);

		return true;
	}

	private static Expression BuildInstanceMethodCall(Expression instance, string methodName, params Expression[] arguments) =>
		new CallExpression(
			new MemberExpression(instance, new Identifier(methodName), computed: false, optional: false),
			NodeList.From(arguments),
			optional: false);

private static IOperation UnwrapImplicitConversions(IOperation operation)
{
	while (operation is IConversionOperation { IsImplicit: true } conversion)
		operation = conversion.Operand;

	return operation;
}

private const string PreserveParamsArrayAttributeFullName = "ECMAScript.PreserveParamsArrayAttribute";
private const string ECMAScriptInlineAttributeFullName = "ECMAScript.ECMAScriptInlineAttribute";

private static bool HasPreserveParamsArrayAttribute(IParameterSymbol parameter)
	=> parameter.GetAttributes().Any(static attribute =>
		attribute.AttributeClass?.ToDisplayString() == PreserveParamsArrayAttributeFullName);

private static bool TryGetEcmascriptInlineTemplate(IMethodSymbol method, out string template)
{
	foreach (var attribute in method.GetAttributes())
	{
		if (attribute.AttributeClass?.ToDisplayString() != ECMAScriptInlineAttributeFullName)
			continue;

		if (attribute.ConstructorArguments.Length == 1 &&
			attribute.ConstructorArguments[0].Value is string value &&
			!string.IsNullOrWhiteSpace(value))
		{
			template = value;
			return true;
		}
	}

	template = string.Empty;
	return false;
}

private bool TryExpandEcmascriptParamsArgument(
	IMethodSymbol method,
	IArgumentOperation arg,
	SenseArgument argument,
		List<Expression> destination)
{
	if (!Util.IsECMAScriptRuntimeSymbol(method) ||
		arg.Parameter?.IsParams != true ||
		arg.Parameter.Type is not IArrayTypeSymbol arrayType)
		return false;

	if (HasPreserveParamsArrayAttribute(arg.Parameter))
		return false;

	var value = UnwrapImplicitConversions(arg.Value);
	switch (value)
	{
			case IArrayCreationOperation { Initializer: not null } arrayCreation:
				foreach (var element in arrayCreation.Initializer.ElementValues)
					destination.Add(TranslateTupleForTarget(element, arrayType.ElementType, argument));
				return true;

			case ICollectionExpressionOperation collectionExpression:
				foreach (var element in collectionExpression.Elements)
					destination.Add(Translate<Expression>(element, argument));
				return true;
		}

		var spreadTarget = TranslateTupleForTarget(arg.Value, arg.Parameter.Type, argument);
		destination.Add(new SpreadElement(spreadTarget));
		return true;
	}

	private bool TryBuildIntrinsicMethodInvocation(IInvocationOperation operation, IMethodSymbol method, Expression? instance, List<Expression> arguments, SenseArgument argument, out Expression? expression)
	{
		expression = null;
		if (method.ContainingType is null)
			return false;

		if (TryGetEcmascriptInlineTemplate(method, out var inlineTemplate))
		{
			var signature = method.OriginalDefinition.ToDisplayString(Format.NameFormat);
			var inlineArguments = CreateLegacyWhiteListArguments(method, arguments, instance);
			var importedIdentifierName = default(string);
			Identifier? importedBinding = null;
			var modulePath = method.IsStatic ? GetModuleImportPath(method.ContainingType) : null;
			if (!string.IsNullOrWhiteSpace(modulePath))
			{
				importedIdentifierName = Util.GetConfigOrSymbolName(method);
				if (!string.IsNullOrWhiteSpace(importedIdentifierName))
					importedBinding = argument.BindImportSpecifier(modulePath!, importedIdentifierName!);
			}

			expression = InstantiateInlineTemplate(
				signature,
				inlineTemplate,
				inlineArguments,
				importedIdentifierName,
				importedBinding);
			return true;
		}

		var containingType = method.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat);
		var childrenToSlotServices = new ChildrenToSlotIntrinsic.Services(
			argument,
			TryBuildImportedModuleMember,
			GetModuleImportPath,
			GetMapperType,
			EnumerateNamedTypeHierarchyBaseFirst,
			CreateOperationTransformationException);
		if (ChildrenToSlotIntrinsic.TryBuild(operation, method, arguments, childrenToSlotServices, out expression))
			return true;

		if (TryBuildEnumerableArrayLikeIntrinsic(operation, method, arguments, out expression))
			return true;

		if (method.ContainingType.SpecialType == SpecialType.System_String || containingType == "string")
		{
			if (method.IsStatic)
			{
				expression = method.Name switch
				{
					"Join" when arguments.Count == 2 =>
						BuildInstanceMethodCall(BuildArrayFrom(arguments[1]), "join", arguments[0]),
					_ => null
				};

				if (expression is not null)
					return true;
			}
			else if (instance is not null)
			{
				expression = method.Name switch
				{
					// string.Split 的“多字符分隔符数组”不能直接翻成 JS split(array)。
					// 这里只保留真正的一元字符串/字符分隔符直译；带 count/options 的重载回退到白名单/helper。
					"Split" when arguments.Count == 1 && arguments[0] is StringLiteral =>
						BuildInstanceMethodCall(instance, "split", arguments[0]),
					"PadLeft" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "padStart", arguments[0]),
					"PadLeft" when arguments.Count == 2 =>
						BuildInstanceMethodCall(instance, "padStart", arguments[0], arguments[1]),
					"PadRight" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "padEnd", arguments[0]),
					"PadRight" when arguments.Count == 2 =>
						BuildInstanceMethodCall(instance, "padEnd", arguments[0], arguments[1]),
					"ToLowerInvariant" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "toLowerCase"),
					"ToUpperInvariant" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "toUpperCase"),
					_ => null
				};

				if (expression is not null)
					return true;
			}
		}

		if (instance is null)
			return false;

		if (arguments.Count == 1 &&
			method.Name == nameof(object.ToString) &&
			arguments[0] is StringLiteral formatLiteral)
		{
			var format = formatLiteral.Value;
			if (method.ContainingType.SpecialType is SpecialType.System_Int32 or SpecialType.System_UInt32)
			{
				var isUpperHex = format == "X";
				var isLowerHex = format == "x";
				if (isUpperHex || isLowerHex)
				{
					var numericSource = method.ContainingType.SpecialType == SpecialType.System_Int32
						? new NonLogicalBinaryExpression(Operator.UnsignedRightShift, instance, new NumericLiteral(0, "0"))
						: instance;
					var hexText = BuildInstanceMethodCall(numericSource, "toString", new NumericLiteral(16, "16"));
					expression = BuildInstanceMethodCall(hexText, isUpperHex ? "toUpperCase" : "toLowerCase");
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// 处理数组元素访问操作，不支持多维数组
	/// C# 示例：
	/// array[0]        // 一维数组访问
	/// array[i, j]     // 多维数组访问（不支持）
	/// array[^1]       // 从末尾开始的索引访问
	/// 复杂情况：array[1..^4] 转换为 array.slice(1, array.length - 4)
	/// 转换结果：array[0]/不支持多维数组/array[array.length - 1]
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitArrayElementReference(IArrayElementReferenceOperation operation, SenseArgument argument)
	{
		if (operation.Indices.Length == 0)
			return HandleTransformationFailure<Node>(operation, "Array access requires at least one index.");

		var initializations = new List<Expression>();
		var expr = BuildArrayElementReadAccess(operation, argument, initializations);
		if (initializations.Count == 0)
			return WithOriginIfMissing(expr, operation);

		var expressions = new List<Expression>(initializations.Count + 1);
		expressions.AddRange(initializations);
		expressions.Add(expr);
		return WithOrigin(new SequenceExpression(NodeList.From(expressions)), operation);
	}

	private Expression BuildArrayElementReadAccess(
		IArrayElementReferenceOperation operation,
		SenseArgument argument,
		List<Expression> initializations)
	{
		Expression expr = Translate<Expression>(operation.ArrayReference, argument);
		for (var i = 0; i < operation.Indices.Length; i++)
		{
			var indexOperation = operation.Indices[i];
			if (indexOperation is IRangeOperation && i != operation.Indices.Length - 1)
			{
				return HandleTransformationFailure<Expression>(
					operation,
					"Range indexing is only supported on the final array dimension.");
			}

			expr = BuildArrayIndexAccess(operation, expr, indexOperation, argument, initializations);
		}

		return expr;
	}

	private Expression BuildArrayElementMutationTarget(
		IArrayElementReferenceOperation operation,
		SenseArgument argument,
		List<Expression> initializations,
		bool cacheForRepeatedReadWrite = false)
	{
		Expression expr = Translate<Expression>(operation.ArrayReference, argument);
		// 自定义 operator 会把复合更新展开成 target = helper(target, rhs)。
		// receiver、索引和中间维度必须先物化，确保 read/write 两端复用同一引用组成部分。
		if (cacheForRepeatedReadWrite)
		{
			expr = MaterializePropertyMutationOperand(
				expr,
				operation,
				argument,
				initializations,
				"array");
		}

		// Roslyn 只会为可赋值数组左值提供非空、非 Range 的索引集合；
		// array[Range] 是产生新数组的读取表达式，不会进入 mutation target。
		for (var i = 0; i < operation.Indices.Length; i++)
		{
			var indexOperation = operation.Indices[i];
			expr = BuildArrayIndexAccess(
				operation,
				expr,
				indexOperation,
				argument,
				initializations,
				cacheIndexForRepeatedReadWrite: cacheForRepeatedReadWrite);
			if (cacheForRepeatedReadWrite && i < operation.Indices.Length - 1)
			{
				expr = MaterializePropertyMutationOperand(
					expr,
					operation,
					argument,
					initializations,
					$"array{i + 1}");
			}
		}

		return expr;
	}

	private Expression BuildArrayIndexAccess(
		IOperation ownerOperation,
		Expression target,
		IOperation indexOperation,
		SenseArgument argument,
		List<Expression> initializations,
		bool cacheIndexForRepeatedReadWrite = false)
	{
		if (RequiresArrayReceiverCaching(indexOperation))
			target = MaterializePropertyMutationOperand(target, ownerOperation, argument, initializations, $"array{initializations.Count}");

		if (TryUnwrapArrayFromEndIndex(indexOperation, out var unary))
		{
			var lengthAccess = new MemberExpression(target, new Identifier("length"), computed: false, optional: false);
			var innerIndex = Translate<Expression>(unary.Operand, argument);
			Expression fromEndIndex = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, innerIndex);
			if (cacheIndexForRepeatedReadWrite)
			{
				fromEndIndex = MaterializePropertyMutationOperand(
					fromEndIndex,
					ownerOperation,
					argument,
					initializations,
					$"index{initializations.Count}");
			}
			return new MemberExpression(target, fromEndIndex, computed: true, optional: false);
		}

		if (indexOperation is IRangeOperation range)
		{
			var start = TryUnwrapArrayFromEndIndex(range.LeftOperand, out var leftUnary)
				? BuildArrayFromEndIndex(target, leftUnary, argument)
				: Translate<Expression>(range.LeftOperand, argument, null);

			var end = TryUnwrapArrayFromEndIndex(range.RightOperand, out var rightUnary)
				? BuildArrayFromEndIndex(target, rightUnary, argument)
				: Translate<Expression>(range.RightOperand, argument, null);

			var slice = new MemberExpression(target, new Identifier("slice"), computed: false, optional: false);
			var args = NodeList.Empty<Expression>();
			if (start is not null && end is not null)
				args = NodeList.From(start, end);
			else if (start is not null)
				args = NodeList.From(start);
			else if (end is not null)
				args = NodeList.From<Expression>(new NumericLiteral(0, "0"), end);

			return new CallExpression(slice, args, optional: false);
		}

		var indexCalculation = Translate<Expression>(indexOperation, argument);
		if (cacheIndexForRepeatedReadWrite)
		{
			indexCalculation = MaterializePropertyMutationOperand(
				indexCalculation,
				ownerOperation,
				argument,
				initializations,
				$"index{initializations.Count}");
		}
		return new MemberExpression(target, indexCalculation, computed: true, optional: false);
	}

	private static bool RequiresArrayReceiverCaching(IOperation indexOperation)
	{
		if (TryUnwrapArrayFromEndIndex(indexOperation, out _))
			return true;

		return indexOperation is IRangeOperation range &&
			(TryUnwrapArrayFromEndIndex(range.LeftOperand, out _) ||
			 TryUnwrapArrayFromEndIndex(range.RightOperand, out _));
	}

	private static bool TryUnwrapArrayFromEndIndex(IOperation? operation, out IUnaryOperation unary)
	{
		if (operation is IUnaryOperation { OperatorKind: UnaryOperatorKind.Hat } hat)
		{
			unary = hat;
			return true;
		}

		if (operation is IConversionOperation conversion)
			return TryUnwrapArrayFromEndIndex(conversion.Operand, out unary);

		unary = null!;
		return false;
	}

	private Expression BuildArrayFromEndIndex(Expression target, IUnaryOperation unary, SenseArgument argument)
	{
		var left = new MemberExpression(target, new Identifier("length"), computed: false, optional: false);
		var right = Translate<Expression>(unary.Operand, argument);
		return new NonLogicalBinaryExpression(Operator.Subtraction, left, right);
	}

	/// <summary>
	/// 处理非数组类型上的隐式 System.Index/System.Range 索引器引用。
	/// 这里不能裸写 JavaScript length/index/slice 语义，而要基于 Roslyn 提供的
	/// LengthSymbol / IndexerSymbol 做受控 lowering：
	/// - Index: 归一化为真实 int 偏移后，再走底层 indexer/read helper
	/// - Range: 归一化为 start/end-exclusive，再走底层 slice/indexer helper
	/// - 仅支持可直接归一化的语言级写法（如 `^1`、`1..^1`、隐式 int -> Index）
	/// - 显式的 System.Index/System.Range 值对象若不能在当前节点直接归一化，则拒绝生成
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitImplicitIndexerReference(IImplicitIndexerReferenceOperation operation, SenseArgument argument)
	{
		PrepareImplicitIndexerAccess(
			operation,
			operation,
			argument,
			cacheForRepeatedReadWrite: false,
			out var initializations,
			out var instance,
			out var arguments,
			out var hostType);

		var expr = BuildImplicitIndexerReadExpression(operation, instance, arguments, argument, hostType);
		if (initializations.Count == 0)
			return WithOriginIfMissing(expr, operation);

		var expressions = new List<Expression>(initializations.Count + 1);
		expressions.AddRange(initializations);
		expressions.Add(expr);
		return WithOrigin(new SequenceExpression(NodeList.From(expressions)), operation);
	}

	private void PrepareImplicitIndexerAccess(
		IImplicitIndexerReferenceOperation operation,
		IOperation ownerOperation,
		SenseArgument argument,
		bool cacheForRepeatedReadWrite,
		out List<Expression> initializations,
		out Expression instance,
		out List<Expression> arguments,
		out ITypeSymbol? hostType)
	{
		initializations = [];
		var resolvedHostType = operation.Instance.Type ?? operation.IndexerSymbol.ContainingType ?? operation.LengthSymbol.ContainingType;
		var translatedInstance = Translate<Expression>(operation.Instance, argument);

		if (cacheForRepeatedReadWrite || RequiresImplicitIndexerLengthAccess(operation.Argument))
			translatedInstance = MaterializePropertyMutationOperand(translatedInstance, ownerOperation, argument, initializations, "iinst");

		Expression? lengthExpr = null;
		Expression GetLengthExpr()
		{
			if (lengthExpr is not null)
				return lengthExpr;

			lengthExpr = BuildImplicitIndexerLengthAccess(operation, operation.LengthSymbol, translatedInstance, argument, resolvedHostType);
			return lengthExpr;
		}

		if (TryGetRangeArgument(operation.Argument, out var rangeArgument))
		{
			var startExpr = BuildImplicitRangeBoundaryExpression(rangeArgument.LeftOperand, GetLengthExpr, argument)
				?? new NumericLiteral(0, "0");
			var endExpr = BuildImplicitRangeBoundaryExpression(rangeArgument.RightOperand, GetLengthExpr, argument)
				?? GetLengthExpr();
			arguments = BuildImplicitRangeArguments(operation, operation.IndexerSymbol, startExpr, endExpr);
		}
		else
		{
			var lengthForIndex = RequiresImplicitIndexerLengthAccess(operation.Argument)
				? GetLengthExpr()
				: null;
			arguments =
			[
				BuildImplicitIndexArgumentExpression(operation, operation.Argument, lengthForIndex, argument)
			];
		}

		if (!cacheForRepeatedReadWrite)
		{
			hostType = resolvedHostType;
			instance = translatedInstance;
			return;
		}

		for (var i = 0; i < arguments.Count; i++)
			arguments[i] = MaterializePropertyMutationOperand(arguments[i], ownerOperation, argument, initializations, $"iarg{i}");

		hostType = resolvedHostType;
		instance = translatedInstance;
	}

	private Expression BuildImplicitIndexerReadExpression(
		IImplicitIndexerReferenceOperation operation,
		Expression instance,
		List<Expression> arguments,
		SenseArgument argument,
		ITypeSymbol? hostType)
	{
		var usage = TryGetRangeArgument(operation.Argument, out _)
			? "implicit range access"
			: "implicit indexer access";
		return BuildListPatternBoundAccess(
			operation,
			operation.IndexerSymbol,
			instance,
			arguments,
			argument,
			usage,
			hostType ?? operation.IndexerSymbol.ContainingType);
	}

	private void PrepareImplicitIndexerSetterAccess(
		IImplicitIndexerReferenceOperation operation,
		IOperation ownerOperation,
		SenseArgument argument,
		bool cacheForRepeatedReadWrite,
		out List<Expression> initializations,
		out Expression instance,
		out List<Expression> arguments,
		out IPropertySymbol property,
		out ITypeSymbol? hostType)
	{
		PrepareImplicitIndexerAccess(
			operation,
			ownerOperation,
			argument,
			cacheForRepeatedReadWrite,
			out initializations,
			out instance,
			out arguments,
			out hostType);

		// Assignable implicit indexer operations are bound by Roslyn to a property symbol.
		// Array element assignments use IArrayElementReferenceOperation instead.
		property = (IPropertySymbol)operation.IndexerSymbol;
		if (property.SetMethod is null)
		{
			property = HandleTransformationFailure<IPropertySymbol>(
				operation,
				$"Implicit indexer target '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not assignable because it has no setter.");
		}
	}

	private void PrepareImplicitIndexerMutationAccess(
		IImplicitIndexerReferenceOperation operation,
		IOperation ownerOperation,
		SenseArgument argument,
		out List<Expression> initializations,
		out Expression readExpression,
		out Expression instance,
		out List<Expression> arguments,
		out IPropertySymbol property)
	{
		PrepareImplicitIndexerSetterAccess(
			operation,
			ownerOperation,
			argument,
			cacheForRepeatedReadWrite: true,
			out initializations,
			out instance,
			out arguments,
			out property,
			out var hostType);

		if (property.GetMethod is null)
		{
			property = HandleTransformationFailure<IPropertySymbol>(
				operation,
				$"Implicit indexer target '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not readable because it has no getter.");
		}

		readExpression = BuildImplicitIndexerReadExpression(operation, instance, arguments, argument, hostType);
	}

	private static bool RequiresImplicitIndexerLengthAccess(IOperation argumentOperation)
	{
		if (TryGetRangeArgument(argumentOperation, out var rangeArgument))
		{
			return rangeArgument.RightOperand is null ||
				IsFromEndIndexArgument(rangeArgument.LeftOperand) ||
				IsFromEndIndexArgument(rangeArgument.RightOperand);
		}

		return IsFromEndIndexArgument(argumentOperation);
	}

	private static bool IsFromEndIndexArgument(IOperation? operation)
	{
		if (operation is null)
			return false;

		if (operation is IUnaryOperation { OperatorKind: UnaryOperatorKind.Hat })
			return true;

		return operation is IConversionOperation conversion &&
			IsFromEndIndexArgument(conversion.Operand);
	}

	private Expression BuildImplicitIndexerSetterAssignment(
		IImplicitIndexerReferenceOperation operation,
		SenseArgument argument,
		IPropertySymbol property,
		Expression instance,
		List<Expression> arguments,
		Expression value)
	{
		if (property.SetMethod is not null)
		{
			var setterArguments = new List<Expression>(arguments.Count + 1);
			setterArguments.AddRange(arguments);
			setterArguments.Add(value);

			var mapperExpr = GetWhiteListExpression(property.SetMethod, argument, setterArguments, instance, out var setterAlias);
			if (mapperExpr is not null)
				return mapperExpr;

			if (string.IsNullOrEmpty(setterAlias))
				RejectUnsupportedRuntimeFallback(operation, property.SetMethod, "implicit indexer assignment", operation.Instance.Type ?? property.ContainingType);
		}

		var target = BuildImplicitIndexerWriteTarget(operation, instance, arguments, property);
		return new AssignmentExpression(Operator.Assignment, target, value);
	}

	private Expression BuildImplicitIndexerWriteTarget(
		IImplicitIndexerReferenceOperation operation,
		Expression instance,
		List<Expression> arguments,
		IPropertySymbol property)
	{
		if (arguments.Count != 1)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"JavaScript fallback for implicit indexer assignment requires a single translated index argument, but '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' produced {arguments.Count} arguments.");
		}

		return new MemberExpression(instance, arguments[0], computed: true, optional: false);
	}

	private Expression BuildImplicitIndexerLengthAccess(
		IOperation ownerOperation,
		ISymbol lengthSymbol,
		Expression instance,
		SenseArgument argument,
		ITypeSymbol? hostType)
	{
		return BuildListPatternBoundAccess(
			ownerOperation,
			lengthSymbol,
			instance,
			[],
			argument,
			"implicit indexer length access",
			hostType ?? lengthSymbol.ContainingType);
	}

	private Expression BuildImplicitIndexArgumentExpression(
		IOperation ownerOperation,
		IOperation argumentOperation,
		Expression? lengthExpr,
		SenseArgument argument)
	{
		if (TryBuildFromEndIndexExpression(argumentOperation, lengthExpr, argument, out var fromEndExpr))
			return fromEndExpr;

		if (TryUnwrapFromStartIndexArgument(argumentOperation, out var fromStartOperand))
			return Translate<Expression>(fromStartOperand, argument);

		return HandleTransformationFailure<Expression>(
			ownerOperation,
			"Implicit indexing requires a direct numeric index, '^', or range expression. Standalone System.Index/System.Range values are not supported in JavaScript lowering.");
	}

	private static bool TryGetRangeArgument(IOperation operation, out IRangeOperation rangeOperation)
	{
		if (operation is IRangeOperation range)
		{
			rangeOperation = range;
			return true;
		}

		if (operation is IConversionOperation conversion &&
			IsSystemRangeType(conversion.Type) &&
			TryGetRangeArgument(conversion.Operand, out range))
		{
			rangeOperation = range;
			return true;
		}

		rangeOperation = null!;
		return false;
	}

	private bool TryBuildFromEndIndexExpression(
		IOperation operation,
		Expression? lengthExpr,
		SenseArgument argument,
		out Expression expr)
	{
		if (operation is IUnaryOperation unary && unary.OperatorKind == UnaryOperatorKind.Hat)
		{
			if (lengthExpr is null)
			{
				expr = HandleTransformationFailure<Expression>(
					operation,
					"From-end index '^' requires a supported Length/Count symbol.");
				return true;
			}

			expr = new NonLogicalBinaryExpression(
				Operator.Subtraction,
				lengthExpr,
				Translate<Expression>(unary.Operand, argument));
			return true;
		}

		if (operation is IConversionOperation conversion &&
			IsSystemIndexType(conversion.Type))
		{
			return TryBuildFromEndIndexExpression(conversion.Operand, lengthExpr, argument, out expr);
		}

		expr = null!;
		return false;
	}

	private static bool TryUnwrapFromStartIndexArgument(IOperation operation, out IOperation operand)
	{
		// A standalone Range carrier is not a numeric from-start index. Direct range syntax is
		// handled earlier from IRangeOperation; letting this fall through misroutes it as Slice(range).
		if (IsSystemRangeType(operation.Type))
		{
			operand = null!;
			return false;
		}

		if (operation is IConversionOperation conversion &&
			IsSystemIndexType(conversion.Type) &&
			!IsSystemIndexType(conversion.Operand.Type))
		{
			operand = conversion.Operand;
			return true;
		}

		if (!IsSystemIndexType(operation.Type))
		{
			operand = operation;
			return true;
		}

		operand = null!;
		return false;
	}

	private Expression? BuildImplicitRangeBoundaryExpression(
		IOperation? boundaryOperation,
		Func<Expression> getLengthExpr,
		SenseArgument argument)
	{
		if (boundaryOperation is null)
			return null;

		if (TryBuildFromEndIndexExpression(boundaryOperation, getLengthExpr(), argument, out var fromEndExpr))
			return fromEndExpr;

		return Translate<Expression>(boundaryOperation, argument);
	}

	private List<Expression> BuildImplicitRangeArguments(
		IOperation ownerOperation,
		ISymbol indexerSymbol,
		Expression startExpr,
		Expression endExpr)
	{
		if (indexerSymbol is not IMethodSymbol method ||
			method.Parameters.Length != 2 ||
			method.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32 ||
			method.Parameters[1].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32)
		{
			return HandleUnsupportedSliceArguments(
				ownerOperation,
				$"Implicit range symbol '{indexerSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}' must be a Slice/Substring-style method with two int parameters.");
		}

		return
		[
			startExpr,
			BuildImplicitRangeLengthExpression(startExpr, endExpr)
		];
	}

	private Expression BuildImplicitRangeLengthExpression(Expression startExpr, Expression endExpr)
	{
		if (startExpr is NumericLiteral { Raw: "0" })
			return endExpr;

		return new NonLogicalBinaryExpression(Operator.Subtraction, endExpr, startExpr);
	}

	/// <summary>
	/// 处理局部变量引用操作
	/// C# 示例：
	/// int localVar = 5;
	/// Console.WriteLine(localVar);  // localVar 引用
	/// 转换结果：localVar
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
	{
		if (Host?.RewriteLocalReference(operation, argument) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);

		return WithOrigin(new Identifier(operation.Local.Name), operation);
	}

	/// <summary>
	/// 处理参数引用操作
	/// C# 示例：
	/// void Method(int param) {
	///     Console.WriteLine(param);  // param 引用
	/// }
	/// 转换结果：param
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
	{
		if (Host?.RewriteParameterReference(operation, argument) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);

		return WithOrigin(new Identifier(operation.Parameter.Name), operation);
	}

	/// <summary>
	/// 处理字段引用操作
	/// C# 示例：
	/// obj.field        // 实例字段访问
	/// MyClass.field    // 静态字段访问
	/// 转换结果：obj.field / MyClass.field
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitFieldReference(IFieldReferenceOperation operation, SenseArgument argument)
	{
		// 处理字段的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);

		if (IsBaseInstanceReference(operation.Instance))
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"Base field access '{operation.Field.Name}' is not supported because member-class fields lower to instance-owned state rather than prototype members. Use a property or method seam instead.");
		}

		if (Host?.RewriteFieldReference(operation, argument, instance) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);

		// 检查白名单映射
		// 字段没有 GetMethod/SetMethod，直接使用字段符号进行白名单查询
		var mapperExpr = GetWhiteListExpression(operation.Field, argument, [], instance, out var alias);
		if (mapperExpr is not null)
			return WithOriginIfMissing(mapperExpr, operation);

		if (string.IsNullOrEmpty(alias) && IsStructuralMember(operation.Field))
		{
			alias = GetCurrentModuleDeclaredOrConfigName(operation.Field);
		}

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(operation, operation.Field, "field access", operation.Instance?.Type ?? operation.Field.ContainingType);

		// 对于实例字段访问，需要创建成员访问表达式
		// ImplicitReceiver 指那些语法上不需要、也不能写 this 的隐式实例引用
		if (operation.Instance is IInstanceReferenceOperation instanceReferenceOp &&
			instanceReferenceOp.ReferenceKind == InstanceReferenceKind.ImplicitReceiver)
		{
			// 隐式接收者（如对象初始化器中的字段引用）
			// 如果是常量字段，返回字面量；否则返回字段名
			var fieldExpr = GetFieldName(operation.Field);
			return WithOriginIfMissing(fieldExpr, operation);
		}

		// 获取字段名称（支持别名）
		var fieldName = string.IsNullOrEmpty(alias)
			? GetCurrentModuleDeclaredOrConfigName(operation.Field)
			: alias;

		Expression property = IsPrivateRuntimeClassField(operation.Field)
			? new PrivateIdentifier(fieldName!)
			: new Identifier(fieldName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return WithOriginIfMissing(new MemberExpression(instance, property, false, optional), operation);
		}

		// 静态成员：生成完整的限定名
		// public 静态类带[ECMAScriptModule]是模块类
		if (operation.Field.IsStatic && operation.Field.ContainingType is not null)
		{
			if (TryBuildStringEnumLiteral(operation.Field, out var stringEnumLiteral))
				return WithOriginIfMissing(stringEnumLiteral, operation);

			if (operation.Field.IsConst)
				return WithOriginIfMissing(GetFieldName(operation.Field), operation);

			if (TryBuildImportedModuleMember(operation.Field.ContainingType, fieldName!, argument, out var importedMember) &&
				importedMember is not null)
				return WithOriginIfMissing(importedMember, operation);

			var runtimeHost = TryBuildRuntimeHostExpression(operation.Field.ContainingType, argument);
			if (runtimeHost is not null)
				return WithOriginIfMissing(new MemberExpression(runtimeHost, property, computed: false, optional: false), operation);

			var containing = BuildFullTypeName(operation.Field.ContainingType, argument);
			if (containing is not null)
				return WithOriginIfMissing(new MemberExpression(containing, property, computed: false, optional: false), operation);

		}

		var fallback = operation.Instance is null
			? GetFieldName(operation.Field)
			: property;
		return WithOriginIfMissing(fallback, operation);
	}

	/// <summary>
	/// 处理属性引用操作
	/// C# 示例：
	/// obj.Property     // 实例属性访问
	/// MyClass.Property // 静态属性访问
	/// 转换结果：obj.property / MyClass.property
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitPropertyReference(IPropertyReferenceOperation operation, SenseArgument argument)
	{
		if (operation.Property.Name == "Rank" &&
			operation.Instance?.Type is IArrayTypeSymbol arrayType)
			return WithOrigin(new NumericLiteral(arrayType.Rank, arrayType.Rank.ToString()), operation);

		RejectUnsupportedNativeMapSetEqualityBoundaryIfNeeded(
			operation,
			operation.Instance?.Type ?? operation.Property.ContainingType,
			"property access");

		// 处理属性调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);
		if (instance is not null && IsErasedUnionProjectionProperty(operation.Property))
			return WithOriginIfMissing(instance, operation);

		if (operation.Property.IsIndexer &&
			operation.Property.Parameters.Length == 1 &&
			IsSystemRangeType(operation.Property.Parameters[0].Type) &&
			operation.Arguments.Length == 1 &&
			TryGetRangeArgument(operation.Arguments[0].Value, out _))
		{
			return HandleTransformationFailure<Node>(
				operation,
				$"Range-based indexer '{operation.Property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported in JavaScript conversion. Expose an int-based slice member or configure a whitelist mapping.");
		}

		var arguments = new List<Expression>(operation.Arguments.Length);
		foreach (var propertyArgument in operation.Arguments)
		{
			var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
				? argument.With(Sense.OutParameter)
				: argument;
			arguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
		}

		if (Host?.RewritePropertyReference(operation, argument, instance, arguments) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);

		// 检查白名单映射。索引器 getter 也必须先走这里，
		// 否则会绕过运行时 helper，丢失越界/缺键等 CLR 语义。
		var mapperExpr = GetWhiteListExpression(operation.Property.GetMethod!, argument, arguments, instance, out var alias, operation);
		if (mapperExpr is not null)
			return WithOriginIfMissing(mapperExpr, operation);

		if (string.IsNullOrEmpty(alias) && IsStructuralMember(operation.Property))
		{
			alias = GetCurrentModuleDeclaredOrConfigName(operation.Property);
		}

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(operation, operation.Property.GetMethod!, "property access", operation.Instance?.Type ?? operation.Property.ContainingType);

		if (instance is not null &&
			arguments.Count > 0 &&
			(operation.Property.IsIndexer || operation.Property.Parameters.Length > 0))
		{
			if (arguments.Count != 1)
				return HandleTransformationFailure<Node>(operation, "JavaScript fallback for indexers only supports a single translated index argument.");

			var indexerOptional = operation.Instance is IConditionalAccessInstanceOperation;
			return WithOriginIfMissing(new MemberExpression(instance, arguments[0], computed: true, optional: indexerOptional), operation);
		}

		// 获取方法名称
		var propertyName = string.IsNullOrEmpty(alias)
			? GetCurrentModuleDeclaredOrConfigName(operation.Property)
			: alias;

		var property = new Identifier(propertyName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return WithOriginIfMissing(
				BuildAliasedPropertyAccess(instance, propertyName!, optional),
				operation);
		}

		// todo：后续需要清理和白名单整合
		// 静态成员：生成完整的限定名（如 DateTime.Now）
		// 检查属性是否是静态成员
		if (operation.Property.IsStatic && operation.Property.ContainingType is not null)
		{
			if (TryBuildCurrentModulePropertyGetterCall(operation.Property, out var currentModuleProperty) &&
				currentModuleProperty is not null)
				return WithOriginIfMissing(currentModuleProperty, operation);

			if (TryBuildImportedModulePropertyAccess(operation.Property, argument, out var importedProperty) &&
				importedProperty is not null)
				return WithOriginIfMissing(importedProperty, operation);

			if (TryBuildPreferredRuntimeStaticMemberAccess(operation.Property, operation.Syntax, operation.SemanticModel, propertyName!, out var preferredStaticProperty) &&
				preferredStaticProperty is not null)
				return WithOriginIfMissing(preferredStaticProperty, operation);

			var runtimeHost = TryBuildRuntimeHostExpression(operation.Property.ContainingType, argument);
			if (runtimeHost is not null)
				return WithOriginIfMissing(new MemberExpression(runtimeHost, property, computed: false, optional: false), operation);

			// 生成类型标识符作为对象
			var containing = BuildFullTypeName(operation.Property.ContainingType, argument);
			if (containing is not null)
				return WithOriginIfMissing(new MemberExpression(containing, property, computed: false, optional: false), operation);
		}

		return WithOriginIfMissing(property, operation);
	}

	/// <summary>
	/// 处理方法引用操作（不调用）
	/// C# 示例：
	/// Action action = obj.Method;  // 方法引用（委托）
	/// 转换结果：obj.method
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitMethodReference(IMethodReferenceOperation operation, SenseArgument argument)
	{
		if (Host?.RewriteMethodReferencePreorder(operation, argument) is Expression preorderHostExpression)
			return WithOriginIfMissing(preorderHostExpression, operation);

		RejectUnsupportedNativeMapSetEqualityBoundaryIfNeeded(
			operation,
			operation.Instance?.Type ?? operation.Method.ContainingType,
			"method reference");

		if (operation.Method.MethodKind == MethodKind.LocalFunction)
		{
			var localFunction = new Identifier(GetCurrentModuleDeclaredOrConfigName(operation.Method));
			if (operation.Method.IsStatic || operation.Instance is null)
				return WithOriginIfMissing(localFunction, operation);

			// Roslyn exposes the captured containing instance on a non-static local-function method group.
			// The function is still a lexical declaration, so bind that declaration instead of probing this.LocalFunction.
			var capturedInstance = Translate<Expression>(operation.Instance, argument, null);
			if (capturedInstance is null)
				return WithOriginIfMissing(localFunction, operation);

			var boundLocalFunction = new CallExpression(
				new MemberExpression(localFunction, new Identifier("bind"), computed: false, optional: false),
				NodeList.From(capturedInstance),
				optional: false);
			return WithOriginIfMissing(boundLocalFunction, operation);
		}

		// 如果是白名单方法调用，需要生成本地代理方法
		// 生成代理方法参数
		var name = AllocateUniqueName(operation, argument, LoweringSite.MethodReferenceProxy());
		var count = operation.Method.Parameters.Length + (operation.Method.IsStatic ? 0 : 1);
		var args = Enumerable.Range(0, count)
			.Select(i => new Identifier($"{name}${i}") as Expression)
			.ToList();

		var valueExpr = GetWhiteListExpression(operation.Method, argument, args, out var alias, operation);
		if (valueExpr is not null)
		{
			// 生成箭头函数表达式作为代理方法
			var func = new ArrowFunctionExpression(
				NodeList.From<Node>(args),
				valueExpr,
				expression: false,
				async: false
			);

			// 方法内不含this访问，直接返回箭头函数；否则需要绑定this
			return func;
		}

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(operation, operation.Method, "method reference", operation.Instance?.Type ?? operation.Method.ContainingType);

		var instance = Translate<Expression>(operation.Instance, argument, null);
		if (Host?.RewriteMethodReference(operation, argument, instance) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);
		var methodName = string.IsNullOrEmpty(alias) ? GetCurrentModuleDeclaredOrConfigName(operation.Method) : alias;
		var initializations = new List<Expression>();
		if (instance is not null)
		{
			if (IsBaseInstanceReference(operation.Instance))
			{
				var forwardedParameters = operation.Method.Parameters
					.Select(static parameter => new Identifier(parameter.Name))
					.ToList();
				var forwardedArguments = forwardedParameters
					.Select(static parameter => (Expression)parameter)
					.ToList();
				var baseMethod = new MemberExpression(new Super(), new Identifier(methodName!), computed: false, optional: false);
				return new ArrowFunctionExpression(
					NodeList.From<Node>(forwardedParameters),
					new CallExpression(baseMethod, NodeList.From(forwardedArguments), optional: false),
					expression: true,
					async: false);
			}

			instance = NormalizeRuntimeReceiverHostInstance(instance, operation.Method);
			if (!operation.Method.IsStatic)
				instance = MaterializeMethodReferenceReceiver(instance, operation, argument, initializations);
		}
		var property = new Identifier(methodName!);
		
		Expression callee = property;
		var extensionHost = TryBuildExtensionHostTarget(operation.Method, argument);
		if (instance is null)
		{
			if (operation.Method.IsStatic)
			{
				if (TryBuildImportedModuleMember(operation.Method.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else if (TryBuildPreferredRuntimeStaticMemberAccess(operation.Method, operation.Syntax, operation.SemanticModel, methodName!, out var preferredStaticCallee) &&
					preferredStaticCallee is not null)
					callee = preferredStaticCallee;
				else if (extensionHost is not null)
					callee = new MemberExpression(extensionHost, property, computed: false, optional: false);
				else
				{
					var containing = BuildFullTypeName(operation.Method.ContainingType, argument);
					if (containing is not null)
						callee = new MemberExpression(containing, property, computed: false, optional: false);
				}
			}
		}
		else
		{
			callee = operation.Method.IsStatic && extensionHost is not null
				? new MemberExpression(extensionHost, property, computed: false, optional: false)
				: operation.Method.MethodKind != MethodKind.DelegateInvoke
				? new MemberExpression(instance, property, computed: false, optional: false)
				: instance;

			// 实例方法组必须绑定到实际接收者，而不是当前 lexical this。
			if (!operation.Method.IsStatic)
			{
				callee = new CallExpression(
					callee: new MemberExpression(callee, new Identifier("bind"), computed: false, optional: false),
					args: NodeList.From<Expression>(instance),
					false);
			}
		}

		callee = NormalizeRuntimeReceiverHostCallee(callee, operation.Method);
		if (initializations.Count > 0)
		{
			var expressions = new List<Expression>(initializations.Count + 1);
			expressions.AddRange(initializations);
			expressions.Add(callee);
			return new SequenceExpression(NodeList.From(expressions));
		}

		return callee;
	}

	private Expression MaterializeMethodReferenceReceiver(
		Expression receiver,
		IOperation ownerOperation,
		SenseArgument argument,
		List<Expression> initializations)
	{
		if (!NeedsSingleEvaluationCaching(receiver))
			return receiver;

		var tempId = new Identifier(AllocateUniqueName(ownerOperation, argument, LoweringSite.MethodReferenceReceiver()));
		argument.AddVarDeclarator(new VariableDeclarator(tempId, null), _recursionDepth);
		initializations.Add(new AssignmentExpression(Operator.Assignment, tempId, receiver));
		return tempId;
	}

	/// <summary>
	/// 处理实例引用操作（this 关键字）
	/// C# 示例：
	/// this.Property   // 引用当前实例
	/// this            // 直接使用 this
	/// 转换结果：this
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
	{
		// InstanceReferenceKind
		// ContainingTypeInstance - 语言特性：类实例引用
		// ImplicitReceiver - 语言特性：对象初始化
		// PatternInput - 语言特性：模式匹配
		// InterpolatedStringHandler - 语言特性：内插字符串 

		if (IsBaseInstanceReference(operation))
			return WithOrigin(new Super(), operation);

		if (Host?.RewriteInstanceReference(operation, argument) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);

		if (operation.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
			return new ThisExpression();

		return null;
	}

	private static bool IsBaseInstanceReference(IOperation? operation)
		=> operation is IInstanceReferenceOperation
		{
			ReferenceKind: InstanceReferenceKind.ContainingTypeInstance,
			Syntax: BaseExpressionSyntax
		};

	/// <summary>
	/// 处理方法调用操作
	/// C# 示例：
	/// obj.Method(arg1, arg2)      // 实例方法调用
	/// StaticClass.Method(arg)     // 静态方法调用
	/// obj.ExtensionMethod(arg)     // 扩展方法调用
	/// 转换结果：obj.method(arg1, arg2) / staticClass.method(arg) / obj.extensionMethod(arg)
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInvocation(IInvocationOperation operation, SenseArgument argument)
	{
		if (Host?.RewriteInvocationPreorder(operation, argument) is Expression preorderHostExpression)
			return WithOriginIfMissing(preorderHostExpression, operation);

		// 处理方法调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);
		var refParas = new List<Expression?>();
		var hasReturn = !operation.TargetMethod.ReturnsVoid;

		// 处理方法调用的参数
		var arguments = new List<Expression>();
		for (var index = 0; index < operation.Arguments.Length; index++)
		{
			var arg = operation.Arguments[index];
			// 为 out 参数传递 OutParameter 上下文
			var argContext = arg.Parameter?.RefKind is RefKind.Out
				? argument.With(Sense.OutParameter)
				: argument;

			var isTrailingEcmascriptDefaultArgument =
				Util.IsECMAScriptRuntimeSymbol(operation.TargetMethod) &&
				arg.ArgumentKind == ArgumentKind.DefaultValue &&
				operation.Arguments.Skip(index).All(static x => x.ArgumentKind == ArgumentKind.DefaultValue);
			if (isTrailingEcmascriptDefaultArgument)
				continue;

			if (Host?.RewriteInvocationArgumentPreorder(operation, arg, index, argContext) is Expression hostArgument)
			{
				arguments.Add(hostArgument);
				continue;
			}

			if (TryExpandEcmascriptParamsArgument(operation.TargetMethod, arg, argContext, arguments))
				continue;

			var right = TranslateTupleForTarget(arg.Value, arg.Parameter?.Type, argContext);
			// ref 引用 或 out 变量引用，记住顺序
			if (arg.Parameter?.RefKind is RefKind.Out or RefKind.Ref)
				refParas.Add(arg.Value is IDiscardOperation ? null : right);

			// 当作普通参数传入
			arguments.Add(right);
		}

		if (Host?.RewriteInvocation(operation, argument, instance, arguments) is Expression hostExpression)
			return WithOriginIfMissing(hostExpression, operation);

		var callExpr = BuildMethodCallExpression(
			operation,
			operation.TargetMethod,
			operation.Syntax,
			operation.SemanticModel,
			instance,
			arguments,
			argument,
			operation.Instance?.Type ?? operation.TargetMethod.ContainingType,
			allowIntrinsic: true,
			invocationOperation: operation);
		return WithOriginIfMissing(BuildInvExpr(hasReturn, callExpr, refParas, argument), operation);

		Expression BuildInvExpr(bool hasReturns, in Expression expr, in List<Expression?> refs, in SenseArgument ctx)
		{
			var expressions = new List<Expression>();
			if (refs.Count > 0)
			{
				// 如果存在ref参数，需要生成逗号表达式，方法调用存临时变量，然后返写参数
				var tempId = new Identifier(AllocateUniqueName(operation, ctx, LoweringSite.ReferenceTemp()));
				var declarator = new VariableDeclarator(tempId, null);
				ctx.AddVarDeclarator(declarator, _recursionDepth);

				expressions.Add(new AssignmentExpression(Operator.Assignment, tempId, expr));
				for (var i = 0; i < refs.Count; i++)
				{
					if (refs[i] is null)
						continue;

					var index = hasReturns ? i + 1 : i;
					var indexer = new NumericLiteral(index, index.ToString());
					var member = new MemberExpression(tempId, indexer, computed: true, optional: false);
					var assignExpr = new AssignmentExpression(Operator.Assignment, refs[i]!, member);
					expressions.Add(assignExpr);
				}
				// 最后如果有返回调用结果
				if (hasReturns)
				{
					var indexer = new NumericLiteral(0, "0");
					var member = new MemberExpression(tempId, indexer, computed: true, optional: false);
					expressions.Add(member);
				}
				return new SequenceExpression(NodeList.From(expressions));
			}

			return expr;
		}
	}

	private Expression BuildMethodCallExpression(
		IOperation ownerOperation,
		IMethodSymbol targetMethod,
		SyntaxNode syntax,
		SemanticModel? semanticModel,
		Expression? instance,
		List<Expression> arguments,
		SenseArgument argument,
		ITypeSymbol? hostType = null,
		bool allowIntrinsic = false,
		IInvocationOperation? invocationOperation = null)
	{
		RejectUnsupportedNativeMapSetEqualityBoundaryIfNeeded(
			ownerOperation,
			hostType ?? targetMethod.ContainingType,
			"method invocation");

		if (allowIntrinsic &&
			invocationOperation is not null &&
			TryBuildIntrinsicMethodInvocation(invocationOperation, targetMethod, instance, arguments, argument, out var intrinsicExpr) &&
			intrinsicExpr is not null)
			return intrinsicExpr;

		var mapperExpr = GetWhiteListExpression(targetMethod, argument, arguments, instance, out var alias, ownerOperation);
		if (mapperExpr is not null)
			return mapperExpr;

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(ownerOperation, targetMethod, "method invocation", hostType ?? targetMethod.ContainingType);

		var methodName = string.IsNullOrEmpty(alias) ? GetCurrentModuleDeclaredOrConfigName(targetMethod) : alias;
		if (instance is not null)
			instance = NormalizeRuntimeReceiverHostInstance(instance, targetMethod);

		var property = new Identifier(methodName!);
		Expression callee = property;
		var extensionHost = TryBuildExtensionHostTarget(targetMethod, argument);
		if (instance is null)
		{
			if (targetMethod.IsStatic)
			{
				if (TryBuildImportedModuleMember(targetMethod.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else if (TryBuildPreferredRuntimeStaticMemberAccess(targetMethod, syntax, semanticModel, methodName!, out var preferredStaticCallee) &&
					preferredStaticCallee is not null)
					callee = preferredStaticCallee;
				else if (extensionHost is not null)
					callee = new MemberExpression(extensionHost, property, computed: false, optional: false);
				else
				{
					var containing = BuildFullTypeName(targetMethod.ContainingType, argument);
					if (containing is not null)
						callee = new MemberExpression(containing, property, computed: false, optional: false);
				}
			}
		}
		else
		{
			callee = targetMethod.IsStatic && extensionHost is not null
				? new MemberExpression(extensionHost, property, computed: false, optional: false)
				: targetMethod.MethodKind != MethodKind.DelegateInvoke
				? new MemberExpression(instance, property, computed: false, optional: false)
				: instance;
		}

		callee = NormalizeRuntimeReceiverHostCallee(callee, targetMethod);
		return new CallExpression(callee, NodeList.From(arguments), optional: false);
	}
}
