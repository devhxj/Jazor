using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Name;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
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

		var displayString = whiteListSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (WhiteList.Members.TryGetValue(displayString, out var entry) &&
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
		var displayString = method.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (WhiteList.Members.TryGetValue(displayString, out var entry) &&
			entry.Op == Op.Alias &&
			!string.IsNullOrEmpty(entry.Value))
			return entry.Value!;

		// 2. 再检查特性配置
		return Util.GetConfigOrSymbolName(method);
	}

	private static string? GetTypeConfigOrWhiteListName(ITypeSymbol symbol)
	{
		string? name = null;

		// 先查询白名单
		var displayName = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (WhiteList.Types.TryGetValue(displayName, out var entry) &&
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
	{
		foreach (var attribute in symbol.GetAttributes())
		{
			if (attribute.AttributeClass?.ToDisplayString() != "ECMAScript.ECMAScriptModuleAttribute")
				continue;

			if (attribute.ConstructorArguments.Length == 1 &&
				attribute.ConstructorArguments[0].Value is string importPath &&
				!string.IsNullOrWhiteSpace(importPath))
				return importPath;
		}

		return null;
	}

	private static bool ShouldFlattenRuntimeNestedType(ITypeSymbol symbol)
	{
		if (symbol is not INamedTypeSymbol namedType || namedType.ContainingType is null)
			return false;

		// 当前编译器的声明侧会把用户代码中的成员类型扁平化为顶层运行时声明，
		// 因此引用侧也必须使用同一个运行时名，不能继续保留 Outer.Inner 链。
		// ECMAScript 绑定里的静态宿主类型例外，它们本身就是运行时宿主层级的一部分。
		if (namedType.ContainingAssembly?.Name == "ECMAScript" && namedType.ContainingType.IsStatic)
			return false;

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
		if (ShouldFlattenRuntimeNestedType(symbol))
		{
			var flatName = GetTypeConfigOrWhiteListName(symbol);
			if (string.IsNullOrEmpty(flatName))
				return null;

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

			type = SymbolEqualityComparer.Default.Equals(type, symbol.ContainingType)
				? null : symbol.ContainingType;
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

	/// <summary>
	/// 从静态成员访问语法中提取“宿主”部分。
	/// 例如：
	/// - <c>Array.Of(...)</c> -> <c>Array</c>
	/// - <c>Uint8Array.BYTES_PER_ELEMENT</c> -> <c>Uint8Array</c>
	/// 这里故意保留调用点写下来的宿主，因为某些运行时 API 的成员声明在泛型基类上，
	/// 但真实 JavaScript 宿主应当是调用点上的具体类型。
	/// </summary>
	private Expression? TryBuildStaticMemberTargetFromSyntax(SyntaxNode syntax)
	{
		// Roslyn 在不同静态访问形态下给到的 syntax 颗粒度不一致。
		// 这里先把 "名字节点" 提升回完整成员访问节点，后面才能稳定提取宿主。
		var effectiveSyntax = syntax switch
		{
			IdentifierNameSyntax or GenericNameSyntax when syntax.Parent is MemberAccessExpressionSyntax or QualifiedNameSyntax
				=> syntax.Parent,
			_ => syntax
		};

		ExpressionSyntax? targetSyntax = effectiveSyntax switch
		{
			InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			QualifiedNameSyntax qualifiedName => qualifiedName.Left,
			_ => null
		};

		if (targetSyntax is null)
			return null;

		var target = TryBuildStaticHostExpressionFromSyntax(targetSyntax);
		if (target is null)
			target = ConvertFromSyntaxNode(targetSyntax) as Expression;
		return target;
	}

	private Expression? TryBuildStaticQualifiedMemberFromSyntax(SyntaxNode syntax, string memberName)
	{
		var target = TryBuildStaticMemberTargetFromSyntax(syntax);
		if (target is null)
			return null;

		return new MemberExpression(target, new Identifier(memberName), computed: false, optional: false);
	}

	private static Expression? TryBuildStaticHostExpressionFromSyntax(SyntaxNode syntax) =>
		syntax switch
		{
			IdentifierNameSyntax identifier => new Identifier(identifier.Identifier.ValueText),
			GenericNameSyntax generic => new Identifier(generic.Identifier.ValueText),
			MemberAccessExpressionSyntax memberAccess => TryBuildMemberAccessHostExpression(memberAccess),
			QualifiedNameSyntax qualifiedName => TryBuildQualifiedNameHostExpression(qualifiedName),
			AliasQualifiedNameSyntax aliasQualifiedName => new MemberExpression(
				new Identifier(aliasQualifiedName.Alias.Identifier.ValueText),
				new Identifier(aliasQualifiedName.Name.Identifier.ValueText),
				computed: false,
				optional: false),
			_ => null
		};

	private static Expression? TryBuildMemberAccessHostExpression(MemberAccessExpressionSyntax memberAccess)
	{
		var receiver = TryBuildStaticHostExpressionFromSyntax(memberAccess.Expression);
		if (receiver is null)
			return null;

		return new MemberExpression(
			receiver,
			new Identifier(memberAccess.Name.Identifier.ValueText),
			computed: false,
			optional: false);
	}

	private static Expression? TryBuildQualifiedNameHostExpression(QualifiedNameSyntax qualifiedName)
	{
		var left = TryBuildStaticHostExpressionFromSyntax(qualifiedName.Left);
		if (left is null)
			return null;

		return new MemberExpression(
			left,
			new Identifier(qualifiedName.Right.Identifier.ValueText),
			computed: false,
			optional: false);
	}

	private bool TryBuildImportedModuleMember(ITypeSymbol? containingType, string memberName, SenseArgument? context, out Expression? expression)
	{
		expression = null;
		if (containingType is null)
			return false;

		var modulePath = GetModuleImportPath(containingType);
		if (string.IsNullOrWhiteSpace(modulePath))
			return false;

		if (_moduleRootType is not null &&
			SymbolEqualityComparer.Default.Equals(containingType, _moduleRootType))
			return false;

		var importId = context?.BindImportSpecifier(modulePath!, memberName) ?? new Identifier(memberName);
		expression = importId;
		return true;
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
	/// 2. 再从调用点语法 + 语义里恢复“用户真正写下的宿主类型”。
	/// 3. 只有当调用点宿主与声明宿主在继承/接口/泛型原型定义上兼容时，才允许覆盖。
	/// 4. 两边都恢复不完整时，才退回语法宿主，避免把具体类型降成抽象基类。
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

		var syntaxHost = TryBuildStaticMemberTargetFromSyntax(syntax);
		var specializedRuntimeHostType = TryGetSpecializedRuntimeHostType(hostType);
		if (syntaxHost is null)
		{
			// 没有可复用的语法宿主时，只在“声明宿主能恢复出更具体的运行时宿主”场景下强制改写。
			// 否则交给普通路径处理，避免为所有静态成员都重复输出一层宿主。
			if (specializedRuntimeHostType is null)
				return false;

			expression = new MemberExpression(runtimeHost, new Identifier(memberName), computed: false, optional: false);
			return true;
		}

		// 两边已经一致时，通常交给普通路径继续输出即可。
		// 但“具体宿主是从泛型约束恢复出来”的场景例外：普通路径会退回声明宿主，
		// 例如又变回 TypedArray.BYTES_PER_ELEMENT，因此这里仍要显式输出运行时宿主。
		if (syntaxHost.ToECMAScript() == runtimeHost.ToECMAScript())
		{
			if (specializedRuntimeHostType is null)
				return false;

			expression = new MemberExpression(runtimeHost, new Identifier(memberName), computed: false, optional: false);
			return true;
		}

		// 泛型基类上的静态成员经常被具体运行时子类型复用。
		// 但如果语义信息已经能恢复出真实宿主，就不再保留调用点文本，
		// 否则像 using Bytes = Uint8Array 这种 C# 别名会被错误发成 Bytes.of。
		// 只有在拿不到语义化具体宿主时，才退回调用点宿主，避免 Uint8Array.of 被降成 TypedArray.of。
		// 其他普通静态宿主则优先采用运行时映射后的真实 host，例如 System.Console -> console。
		var preferredHost = hostType is INamedTypeSymbol { IsGenericType: true } && specializedRuntimeHostType is null
			? syntaxHost
			: runtimeHost;
		expression = new MemberExpression(preferredHost, new Identifier(memberName), computed: false, optional: false);
		return true;
	}

	/// <summary>
	/// 从静态访问的语法节点中恢复调用点宿主对应的语义类型。
	///
	/// 这里不能只取语法文本：
	/// - <c>Bytes.Of(...)</c> 里的 <c>Bytes</c> 可能是 using alias；
	/// - <c>Namespace.Type.Member</c> / <c>Outer.Inner.Member</c> 需要拿到最终绑定后的类型；
	/// - Roslyn 在属性、方法组、调用三种静态访问上给出的 syntax 颗粒度并不一致。
	/// </summary>
	private static ITypeSymbol? TryGetStaticSourceHostTypeFromSyntax(SyntaxNode syntax, SemanticModel? semanticModel)
	{
		if (semanticModel is null)
			return null;

		var effectiveSyntax = syntax switch
		{
			IdentifierNameSyntax or GenericNameSyntax when syntax.Parent is MemberAccessExpressionSyntax or QualifiedNameSyntax
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
		if (WhiteList.Types.TryGetValue(displayName, out var entry) &&
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
			method.ContainingAssembly?.Name != "ECMAScript")
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

		var getterName = GetMethodConfigOrWhiteListName(property.GetMethod);
		if (!TryBuildImportedModuleMember(property.ContainingType, getterName, context, out var getter) ||
			getter is null)
			return false;

		expression = new CallExpression(getter, NodeList.Empty<Expression>(), optional: false);
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

	private Expression GetFieldName(IOperation includeOp, IFieldSymbol symbol)
	{
		if (TryBuildECMAScriptEnumLiteral(symbol, out var enumLiteral))
			return enumLiteral;

		// 检查是否是特殊常量字段（如 double.PositiveInfinity, double.NaN 等）
		if (symbol.ContainingType is not null && symbol.IsConst)
		{
			// 处理特殊常量字段
			return (symbol.Name, symbol.ContainingType.SpecialType) switch
			{
				// 浮点类型特殊常量
				(nameof(double.PositiveInfinity), SpecialType.System_Double) or
				(nameof(float.PositiveInfinity), SpecialType.System_Single) => new Identifier("Infinity"),

				(nameof(double.NegativeInfinity), SpecialType.System_Double) or
				(nameof(float.NegativeInfinity), SpecialType.System_Single) => new Identifier("-Infinity"),

				(nameof(double.NaN), SpecialType.System_Double) or
				(nameof(float.NaN), SpecialType.System_Single) => new Identifier("NaN"),

				(nameof(double.Epsilon), SpecialType.System_Double) or
				(nameof(float.Epsilon), SpecialType.System_Single) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("EPSILON"), computed: false, optional: false),

				// double 的最大/最小值与 JavaScript Number 范围一致
				(nameof(double.MaxValue), SpecialType.System_Double) =>
					new MemberExpression(
						new Identifier("Number"),
						new Identifier("MAX_VALUE"), computed: false, optional: false),
				(nameof(double.MinValue), SpecialType.System_Double) =>
					new NonUpdateUnaryExpression(
						Operator.UnaryNegation,
						new MemberExpression(
							new Identifier("Number"),
							new Identifier("MAX_VALUE"), computed: false, optional: false)),

				// float 的边界值需要保留 C# 单精度语义，不能退化成 JS 的 double 极值
				(nameof(float.MaxValue), SpecialType.System_Single) =>
					new NumericLiteral(float.MaxValue, float.MaxValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)),
				(nameof(float.MinValue), SpecialType.System_Single) =>
					new NumericLiteral(float.MinValue, float.MinValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture)),

				// long 的边界值在当前映射中属于 bigint
				(nameof(long.MaxValue), SpecialType.System_Int64) =>
					new BigIntLiteral(new System.Numerics.BigInteger(long.MaxValue), $"{long.MaxValue}n"),
				(nameof(long.MinValue), SpecialType.System_Int64) =>
					new BigIntLiteral(new System.Numerics.BigInteger(long.MinValue), $"{long.MinValue}n"),

				// decimal 最大/最小值保持为精确数值字面量
				(nameof(decimal.MaxValue), SpecialType.System_Decimal) =>
					new NumericLiteral((double)decimal.MaxValue, decimal.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
				(nameof(decimal.MinValue), SpecialType.System_Decimal) =>
					new NumericLiteral((double)decimal.MinValue, decimal.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),

				// 其他整数类型（int, short, sbyte 等）保持原样，会作为字面量处理
				_ => symbol.HasConstantValue
					? BuildValueLiteral(symbol.Type, symbol.ConstantValue) ?? Null
					: new Identifier(Util.GetConfigOrSymbolName(symbol))
			};
		}

		return new Identifier(Util.GetConfigOrSymbolName(symbol));
	}

	private static bool TryBuildECMAScriptEnumLiteral(IFieldSymbol symbol, out Expression expression)
	{
		expression = null!;
		if (!symbol.HasConstantValue ||
			symbol.ContainingType?.TypeKind != TypeKind.Enum ||
			symbol.ContainingAssembly?.Name != "ECMAScript")
			return false;

		var alias = Util.GetSymbolConfigName(symbol);
		if (string.IsNullOrEmpty(alias))
			return false;

		expression = new StringLiteral(alias!, $"\"{alias!.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
		return true;
	}

	private static bool IsDateLikeType(ITypeSymbol? type)
	{
		if (type is null)
			return false;

		var displayName = type.OriginalDefinition.ToDisplayString(Format.NameFormat);
		return type.SpecialType == SpecialType.System_DateTime ||
			displayName == "System.DateOnly";
	}

	private static bool ShouldInvokeAliasedPropertyGetter(IPropertyReferenceOperation operation, string alias)
	{
		if (operation.Instance is null || operation.Arguments.Length != 0 || string.IsNullOrEmpty(alias))
			return false;

		if (!IsDateLikeType(operation.Instance.Type))
			return false;

		return alias is "getDate" or "getHours" or "getMilliseconds" or "getMinutes" or "getSeconds" or "getFullYear";
	}

	private static Expression BuildAliasedPropertyAccess(Expression instance, string propertyName, bool optional, bool invoke)
	{
		var member = new MemberExpression(instance, new Identifier(propertyName), computed: false, optional: optional);
		if (!invoke)
			return member;

		return new CallExpression(member, NodeList.Empty<Expression>(), optional: false);
	}

	private static Expression BuildArrayFrom(Expression value) =>
		new CallExpression(
			new MemberExpression(new Identifier("Array"), new Identifier("from"), computed: false, optional: false),
			NodeList.From(value),
			optional: false);

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

	private bool TryExpandEcmascriptParamsArgument(
		IMethodSymbol method,
		IArgumentOperation arg,
		SenseArgument argument,
		List<Expression> destination)
	{
		if (method.ContainingAssembly?.Name != "ECMAScript" ||
			arg.Parameter?.IsParams != true ||
			arg.Parameter.Type is not IArrayTypeSymbol arrayType)
			return false;

		var value = UnwrapImplicitConversions(arg.Value);
		switch (value)
		{
			case IArrayCreationOperation { Initializer: not null } arrayCreation:
				foreach (var element in arrayCreation.Initializer.ElementValues)
					destination.Add(TranslateTupleForTarget(element, arrayType.ElementType, argument));
				return true;

			case IArrayInitializerOperation arrayInitializer:
				foreach (var element in arrayInitializer.ElementValues)
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

	private static bool TryBuildIntrinsicMethodInvocation(IMethodSymbol method, Expression? instance, List<Expression> arguments, out Expression? expression)
	{
		expression = null;
		if (method.ContainingType is null)
			return false;

		var containingType = method.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat);
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
					// 这里只保留显然是一元字符串分隔符的直译；其余情况回退到白名单/helper。
					"Split" when arguments.Count >= 1 && arguments[0] is StringLiteral =>
						BuildInstanceMethodCall(instance, "split", arguments[0]),
					"PadLeft" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "padStart", arguments[0]),
					"PadLeft" when arguments.Count == 2 =>
						BuildInstanceMethodCall(instance, "padStart", arguments[0], arguments[1]),
					"PadRight" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "padEnd", arguments[0]),
					"PadRight" when arguments.Count == 2 =>
						BuildInstanceMethodCall(instance, "padEnd", arguments[0], arguments[1]),
					"ToCharArray" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "split", new StringLiteral("", "\"\"")),
					"ToCharArray" when arguments.Count == 2 =>
						BuildInstanceMethodCall(
							BuildInstanceMethodCall(
								instance,
								"substring",
								arguments[0],
								new NonLogicalBinaryExpression(Operator.Addition, arguments[0], arguments[1])),
							"split",
							new StringLiteral("", "\"\"")),
					"ToLowerInvariant" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "toLowerCase"),
					"ToUpperInvariant" when arguments.Count == 0 =>
						BuildInstanceMethodCall(instance, "toUpperCase"),
					"Remove" when arguments.Count == 1 =>
						BuildInstanceMethodCall(instance, "slice", new NumericLiteral(0, "0"), arguments[0]),
					"Remove" when arguments.Count == 2 =>
						new NonLogicalBinaryExpression(
							Operator.Addition,
							BuildInstanceMethodCall(instance, "slice", new NumericLiteral(0, "0"), arguments[0]),
							BuildInstanceMethodCall(
								instance,
								"slice",
								new NonLogicalBinaryExpression(Operator.Addition, arguments[0], arguments[1]))),
					"Insert" when arguments.Count == 2 =>
						new NonLogicalBinaryExpression(
							Operator.Addition,
							new NonLogicalBinaryExpression(
								Operator.Addition,
								BuildInstanceMethodCall(instance, "slice", new NumericLiteral(0, "0"), arguments[0]),
								arguments[1]),
							BuildInstanceMethodCall(instance, "slice", arguments[0])),
					_ => null
				};

				if (expression is not null)
					return true;
			}
		}

		if (containingType == "System.Linq.Enumerable")
		{
			expression = method.Name switch
			{
				"Where" when arguments.Count == 2 =>
					new CallExpression(
						new MemberExpression(
							BuildArrayFrom(arguments[0]),
							new Identifier("filter"),
							computed: false,
							optional: false),
						NodeList.From(arguments[1]),
						optional: false),
				"Select" when arguments.Count == 2 =>
					new CallExpression(
						new MemberExpression(
							BuildArrayFrom(arguments[0]),
							new Identifier("map"),
							computed: false,
							optional: false),
						NodeList.From(arguments[1]),
						optional: false),
				"ToList" when arguments.Count == 1 =>
					BuildArrayFrom(arguments[0]),
				_ => null
			};

			if (expression is not null)
				return true;
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

		Expression expr = Translate<Expression>(operation.ArrayReference, argument);
		for (var i = 0; i < operation.Indices.Length; i++)
		{
			var indexOperation = operation.Indices[i];
			if (indexOperation is IRangeOperation && i != operation.Indices.Length - 1)
				return HandleTransformationFailure<Node>(operation, "Range indexing is only supported on the final array dimension.");

			expr = BuildArrayIndexAccess(expr, indexOperation);
		}
		return expr;

		Expression BuildArrayIndexAccess(Expression target, IOperation indexOperation)
		{
			if (indexOperation is IUnaryOperation unary && unary.OperatorKind == UnaryOperatorKind.Hat)
			{
				var lengthAccess = new MemberExpression(target, new Identifier("length"), computed: false, optional: false);
				var innerIndex = Translate<Expression>(unary.Operand, argument);
				var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, innerIndex);
				return new MemberExpression(target, indexCalculation, computed: true, optional: false);
			}
			else if (indexOperation is IImplicitIndexerReferenceOperation implicitIndexer)
			{
				var instance = Translate<Expression>(implicitIndexer.Instance, argument);
				var indexArgument = Translate<Expression>(implicitIndexer.Argument, argument);
				var lengthAccess = new MemberExpression(instance, new Identifier("length"), computed: false, optional: false);
				if (implicitIndexer.Argument is IUnaryOperation indexUnaryOp && indexUnaryOp.OperatorKind == UnaryOperatorKind.Hat)
					indexArgument = Translate<Expression>(indexUnaryOp.Operand, argument);
				var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, indexArgument);
				return new MemberExpression(instance, indexCalculation, computed: true, optional: false);
			}
			else if (indexOperation is IRangeOperation range)
			{
				var start = range.LeftOperand is IUnaryOperation leftUnary && leftUnary.OperatorKind == UnaryOperatorKind.Hat
					? UnaryHat(target, leftUnary)
					: Translate<Expression>(range.LeftOperand, argument, null);

				var end = range.RightOperand is IUnaryOperation rightUnary && rightUnary.OperatorKind == UnaryOperatorKind.Hat
					? UnaryHat(target, rightUnary)
					: Translate<Expression>(range.RightOperand, argument, null);

				var slice = new MemberExpression(target, new Identifier("slice"), computed: false, optional: false);
				var args = NodeList.Empty<Expression>();
				if (start is not null && end is not null)
				{
					var adjustedEnd = new NonLogicalBinaryExpression(Operator.Addition, end, new NumericLiteral(1, "1"));
					args = NodeList.From(start, adjustedEnd);
				}
				else if (start is not null)
				{
					args = NodeList.From(start);
				}
				else if (end is not null)
				{
					var adjustedEnd = new NonLogicalBinaryExpression(Operator.Addition, end, new NumericLiteral(1, "1"));
					args = NodeList.From<Expression>(new NumericLiteral(0, "0"), adjustedEnd);
				}

				return new CallExpression(slice, args, optional: false);
			}
			else
			{
				var indexCalculation = Translate<Expression>(indexOperation, argument);
				return new MemberExpression(target, indexCalculation, computed: true, optional: false);
			}
		}

		Expression UnaryHat(Expression obj, IUnaryOperation unary)
		{
			var left = new MemberExpression(obj, new Identifier("length"), computed: false, optional: false);
			var right = Translate<Expression>(unary.Operand, argument);
			return new NonLogicalBinaryExpression(Operator.Subtraction, left, right);
		}
	}

	/// <summary>
	/// 处理隐式索引器引用操作
	/// C# 示例：
	/// array[^1]                           // 从末尾开始的索引
	/// array[^n]                           // 从末尾开始的第n个位置
	/// array[^0]                           // 从末尾开始的第0个位置（等同于array.length）
	/// 转换结果：直接生成最简单的 array[array.length - n] 表达式
	/// 利用C#强类型系统，避免不必要的运行时检测，生成高效简洁的代码
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitImplicitIndexerReference(IImplicitIndexerReferenceOperation operation, SenseArgument argument)
	{
		// 隐式索引器引用的直接AST转换，生成最简洁的代码
		var instance = Translate<Expression>(operation.Instance, argument);
		var indexArgument = Translate<Expression>(operation.Argument, argument);
		// 生成 array.length 访问
		var lengthAccess = new MemberExpression(instance, new Identifier("length"), computed: false, optional: false);
		if (operation.Argument is IUnaryOperation indexUnaryOp && indexUnaryOp.OperatorKind == UnaryOperatorKind.Hat)
			indexArgument = Translate<Expression>(indexUnaryOp.Operand, argument);
		// 处理从末尾开始的索引（^n），转换为 length - n
		// 普通索引计算，不是从末尾开始的索引
		// 这种情况可能出现在显式使用 Index.FromEnd() 等场景
		var indexCalculation = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, indexArgument);

		// 直接返回数组访问表达式：array[array.length - n]
		return new MemberExpression(instance, indexCalculation, computed: true, optional: false);
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
		return new Identifier(operation.Local.Name);
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
		return new Identifier(operation.Parameter.Name);
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

		// 检查白名单映射
		// 字段没有 GetMethod/SetMethod，直接使用字段符号进行白名单查询
		var mapperExpr = GetWhiteListExpression(operation.Field, argument, [], instance, out var alias);
		if (mapperExpr is not null)
			return mapperExpr;

		// 对于实例字段访问，需要创建成员访问表达式
		// ImplicitReceiver 指那些语法上不需要、也不能写 this 的隐式实例引用
		if (operation.Instance is IInstanceReferenceOperation instanceReferenceOp &&
			instanceReferenceOp.ReferenceKind == InstanceReferenceKind.ImplicitReceiver)
		{
			// 隐式接收者（如对象初始化器中的字段引用）
			// 如果是常量字段，返回字面量；否则返回字段名
			var fieldExpr = GetFieldName(operation, operation.Field);
			return fieldExpr;
		}

		// 获取字段名称（支持别名）
		var fieldName = string.IsNullOrEmpty(alias)
			? operation.Field.Name
			: alias;

		var property = new Identifier(fieldName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return new MemberExpression(instance, property, false, optional);
		}

		// 静态成员：生成完整的限定名
		// public 静态类带[ECMAScriptModule]是模块类
		if (operation.Field.IsStatic && operation.Field.ContainingType is not null)
		{
			if (TryBuildImportedModuleMember(operation.Field.ContainingType, fieldName!, argument, out var importedMember) &&
				importedMember is not null)
				return importedMember;

			if (operation.Field.IsConst)
				return GetFieldName(operation, operation.Field);

			var containing = BuildFullTypeName(operation.Field.ContainingType, argument);
			if (containing is not null)
				return new MemberExpression(containing, property, computed: false, optional: false);

			var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, fieldName!);
			if (qualified is not null)
				return qualified;
		}

		return operation.Instance is null
			? GetFieldName(operation, operation.Field)
			: property;
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
			return new NumericLiteral(arrayType.Rank, arrayType.Rank.ToString());

		// 处理属性调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);
		var arguments = new List<Expression>(operation.Arguments.Length);
		foreach (var propertyArgument in operation.Arguments)
		{
			var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
				? argument.With(Sense.OutParameter)
				: argument;
			arguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
		}

		if (instance is not null &&
			arguments.Count > 0 &&
			(operation.Property.IsIndexer || operation.Property.Parameters.Length > 0))
		{
			var indexerOptional = operation.Instance is IConditionalAccessInstanceOperation;
			return new MemberExpression(instance, arguments[0], computed: true, optional: indexerOptional);
		}

		// 检查白名单映射
		var mapperExpr = GetWhiteListExpression(operation.Property.GetMethod!, argument, arguments, instance, out var alias);
		if (mapperExpr is not null)
			return mapperExpr;

		// 获取方法名称
		var propertyName = string.IsNullOrEmpty(alias)
			? Util.GetConfigOrSymbolName(operation.Property)
			: alias;

		var property = new Identifier(propertyName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return BuildAliasedPropertyAccess(instance, propertyName!, optional, ShouldInvokeAliasedPropertyGetter(operation, propertyName!));
		}

		// todo：后续需要清理和白名单整合
		// 静态成员：生成完整的限定名（如 DateTime.Now）
		// 检查属性是否是静态成员
		if (operation.Property.IsStatic && operation.Property.ContainingType is not null)
		{
			if (TryBuildImportedModulePropertyAccess(operation.Property, argument, out var importedProperty) &&
				importedProperty is not null)
				return importedProperty;

			if (TryBuildPreferredRuntimeStaticMemberAccess(operation.Property, operation.Syntax, operation.SemanticModel, propertyName!, out var preferredStaticProperty) &&
				preferredStaticProperty is not null)
				return preferredStaticProperty;

			// 生成类型标识符作为对象
			var containing = BuildFullTypeName(operation.Property.ContainingType, argument);
			if (containing is not null)
				return new MemberExpression(containing, property, computed: false, optional: false);
		}

		return property;
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
		// 如果是白名单方法调用，需要生成本地代理方法
		// 生成代理方法参数
		var name = GetUniqueName(operation);
		var count = operation.Method.Parameters.Length + (operation.Method.IsStatic ? 0 : 1);
		var args = Enumerable.Range(0, count)
			.Select(i => new Identifier($"{name}${i}") as Expression)
			.ToList();

		var valueExpr = GetWhiteListExpression(operation.Method, argument, args, out var alias);
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

		var instance = Translate<Expression>(operation.Instance, argument, null);
		var methodName = string.IsNullOrEmpty(alias) ? Util.GetConfigOrSymbolName(operation.Method) : alias;
		if (instance is not null)
			instance = NormalizeRuntimeReceiverHostInstance(instance, operation.Method);
		var property = new Identifier(methodName!);
		
		Expression callee = property;
		var extensionHost = TryBuildExtensionHostTarget(operation.Method, argument);
		if (instance is null)
		{
			if (operation.Method.IsStatic)
			{
				if (TryBuildPreferredRuntimeStaticMemberAccess(operation.Method, operation.Syntax, operation.SemanticModel, methodName!, out var preferredStaticCallee) &&
					preferredStaticCallee is not null)
					callee = preferredStaticCallee;
				else if (extensionHost is not null)
					callee = new MemberExpression(extensionHost, property, computed: false, optional: false);
				else if (TryBuildImportedModuleMember(operation.Method.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else
				{
					var containing = BuildFullTypeName(operation.Method.ContainingType, argument);
					if (containing is not null)
						callee = new MemberExpression(containing, property, computed: false, optional: false);
					else if (!Util.IsECMAScriptRuntimeSymbol(operation.Method))
					{
						var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, methodName!);
						if (qualified is not null)
							callee = qualified;
					}
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
		return callee;
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

		if (operation.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
			return new ThisExpression();

		return null;
	}

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
		// 处理方法调用的实例对象
		var instance = Translate<Expression>(operation.Instance, argument, null);
		var refParas = new List<Expression>();
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
				operation.TargetMethod.ContainingAssembly?.Name == "ECMAScript" &&
				arg.ArgumentKind == ArgumentKind.DefaultValue &&
				operation.Arguments.Skip(index).All(static x => x.ArgumentKind == ArgumentKind.DefaultValue);
			if (isTrailingEcmascriptDefaultArgument)
				continue;

			if (TryExpandEcmascriptParamsArgument(operation.TargetMethod, arg, argContext, arguments))
				continue;

			var right = TranslateTupleForTarget(arg.Value, arg.Parameter?.Type, argContext);
			// ref 引用 或 out 变量引用，记住顺序
			if (arg.Parameter?.RefKind is RefKind.Out or RefKind.Ref)
				refParas.Add(right);

			// 当作普通参数传入
			arguments.Add(right);
		}

		// 检查白名单映射
		var mapperExpr = GetWhiteListExpression(operation.TargetMethod, argument, arguments, instance, out var alias);
		if (mapperExpr is not null)
			return BuildInvExpr(hasReturn, mapperExpr, refParas, argument);

		if (TryBuildIntrinsicMethodInvocation(operation.TargetMethod, instance, arguments, out var intrinsicExpr) &&
			intrinsicExpr is not null)
			return BuildInvExpr(hasReturn, intrinsicExpr, refParas, argument);

		// 判断方法调用的类型
		var methodName = string.IsNullOrEmpty(alias) ? Util.GetConfigOrSymbolName(operation.TargetMethod) : alias;
		if (instance is not null)
			instance = NormalizeRuntimeReceiverHostInstance(instance, operation.TargetMethod);
		var property = new Identifier(methodName!);
		Expression callee = property;
		var extensionHost = TryBuildExtensionHostTarget(operation.TargetMethod, argument);
		if (instance is null)
		{
			if (operation.TargetMethod.IsStatic)
			{
				if (TryBuildPreferredRuntimeStaticMemberAccess(operation.TargetMethod, operation.Syntax, operation.SemanticModel, methodName!, out var preferredStaticCallee) &&
					preferredStaticCallee is not null)
					callee = preferredStaticCallee;
				else if (extensionHost is not null)
					callee = new MemberExpression(extensionHost, property, computed: false, optional: false);
				else if (TryBuildImportedModuleMember(operation.TargetMethod.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else
				{
					var containing = BuildFullTypeName(operation.TargetMethod.ContainingType, argument);
					if (containing is not null)
						callee = new MemberExpression(containing, property, computed: false, optional: false);
					else if (!Util.IsECMAScriptRuntimeSymbol(operation.TargetMethod))
					{
						var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, methodName!);
						if (qualified is not null)
							callee = qualified;
					}
				}
			}
		}
		else
		{
			callee = operation.TargetMethod.IsStatic && extensionHost is not null
				? new MemberExpression(extensionHost, property, computed: false, optional: false)
				: operation.TargetMethod.MethodKind != MethodKind.DelegateInvoke
				? new MemberExpression(instance, property, computed: false, optional: false)
				: instance;
		}

		callee = NormalizeRuntimeReceiverHostCallee(callee, operation.TargetMethod);
		var callExpr = new CallExpression(callee, NodeList.From(arguments), optional: false);
		return BuildInvExpr(hasReturn, callExpr, refParas, argument);

		Expression BuildInvExpr(bool hasReturns, in Expression expr, in List<Expression> refs, in SenseArgument ctx)
		{
			var expressions = new List<Expression>();
			if (refs.Count > 0)
			{
				// 如果存在ref参数，需要生成逗号表达式，方法调用存临时变量，然后返写参数
				var tempId = new Identifier(GetUniqueName(operation));
				var declarator = new VariableDeclarator(tempId, null);
				ctx.AddVarDeclarator(declarator, _recursionDepth);

				expressions.Add(new AssignmentExpression(Operator.Assignment, tempId, expr));
				for (var i = 0; i < refs.Count; i++)
				{
					var index = hasReturns ? i + 1 : 0;
					var indexer = new NumericLiteral(index, index.ToString());
					var member = new MemberExpression(tempId, indexer, computed: true, optional: false);
					var assignExpr = new AssignmentExpression(Operator.Assignment, refs[i], member);
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
}
