using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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
	{
		foreach (var attribute in symbol.GetAttributes())
		{
			var attributeName = attribute.AttributeClass?.ToDisplayString();
			if (attributeName is not ("ECMAScript.ECMAScriptModuleAttribute" or "ECMAScript.ECMAScriptAttribute"))
				continue;

			if (attribute.ConstructorArguments.Length != 1)
				continue;

			var importArgument = attribute.ConstructorArguments[0];
			if (importArgument.Kind == TypedConstantKind.Array ||
				importArgument.Value is not string importPath ||
				string.IsNullOrWhiteSpace(importPath))
			{
				continue;
			}

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
		if (symbol is INamedTypeSymbol namedTypeSymbol &&
			TryGetCurrentModuleDeclaredName(namedTypeSymbol, out var moduleDeclaredTypeName))
		{
			return new Identifier(moduleDeclaredTypeName);
		}

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
			IdentifierNameSyntax or GenericNameSyntax when IsMemberAccessNameSyntax(syntax)
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

	private static bool IsMemberAccessNameSyntax(SyntaxNode syntax)
		=> syntax.Parent switch
		{
			MemberAccessExpressionSyntax memberAccess => ReferenceEquals(memberAccess.Name, syntax),
			QualifiedNameSyntax qualifiedName => ReferenceEquals(qualifiedName.Right, syntax),
			_ => false
		};

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

		if (string.Equals(memberName, "default", StringComparison.Ordinal))
		{
			throw new NotSupportedException(
				$"Jazor module import does not support default export. Member import from '{containingType.ToDisplayString(Format.NameFormat)}' resolves to export name 'default'. Use a named export instead.");
		}

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
	/// 某些泛型数学静态成员在调用点会先绑定成接口投影方法，
	/// 例如 <c>ushort.PopCount</c> / <c>uint.PopCount</c>。
	/// 这类符号若直接查白名单，会漏掉具体类型上的映射，随后退回错误的 runtime host。
	///
	/// 这里仅在“静态接口方法 + 能从调用点恢复具体宿主”时，尝试拉回实现面。
	/// 普通成员路径保持不变，避免扩大影响面。
	/// </summary>
	private static IMethodSymbol ResolveStaticInterfaceProjectionMethod(IMethodSymbol method, SyntaxNode syntax, SemanticModel? semanticModel)
	{
		if (!method.IsStatic || method.ContainingType?.TypeKind != TypeKind.Interface)
			return method;

		if (TryGetStaticSourceHostTypeFromSyntax(syntax, semanticModel) is not INamedTypeSymbol sourceHostType ||
			!IsStaticHostOverrideCompatible(sourceHostType, method.ContainingType))
			return method;

		if (sourceHostType.FindImplementationForInterfaceMember(method) is IMethodSymbol implementation)
			return implementation;

		foreach (var candidate in sourceHostType.GetMembers(method.Name).OfType<IMethodSymbol>())
		{
			if (candidate.IsStatic &&
				candidate.Arity == method.Arity &&
				candidate.Parameters.Length == method.Parameters.Length)
				return candidate;
		}

		return method;
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
					: new Identifier(GetCurrentModuleDeclaredOrConfigName(symbol))
			};
		}

		return new Identifier(GetCurrentModuleDeclaredOrConfigName(symbol));
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

	private static ThrowStatement BuildThrowErrorStatement(string message)
	{
		var escapedMessage = message.Replace("\\", "\\\\").Replace("\"", "\\\"");
		var errorExpression = new NewExpression(
			new Identifier("Error"),
			NodeList.From<Expression>(new StringLiteral(message, $"\"{escapedMessage}\"")));
		return new ThrowStatement(errorExpression);
	}

	private static ThrowStatement BuildArgumentNullThrowStatement(string parameterName)
		=> BuildThrowErrorStatement($"ArgumentNullException: {parameterName} is null");

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
				return methodName is
					"concat" or
					"filter" or
					"flat" or
					"flatMap" or
					"map" or
					"slice" or
					"splice" or
					"toReversed" or
					"toSorted" or
					"toSpliced";

			default:
				return false;
		}
	}

	private bool TryBuildEnumerableArrayLikeIntrinsic(IInvocationOperation operation, IMethodSymbol method, List<Expression> arguments, out Expression? expression)
	{
		expression = null;
		if (arguments.Count == 0 ||
			method.Parameters.Length == 0 ||
			!TryGetWhiteListValue(WhiteList.Members, method, out _, out var memberEntry) ||
			memberEntry.Op != Op.Import)
			return false;

		var isSupportedIntrinsicShape =
			(method.Name is "Where" or "Select" && arguments.Count == 2) ||
			(method.Name is "ToList" or "ToArray" && arguments.Count == 1);
		if (!isSupportedIntrinsicShape)
			return false;

		if (!IsEnumerableContractType(method.Parameters[0].Type))
			return false;

		var sourceOperation = UnwrapImplicitConversions(operation.Arguments[0].Value);
		var sourceExpression = arguments[0];
		var sourceIsArrayProducingExpression = IsArrayProducingExpression(sourceExpression);
		var hasArrayLikeSource =
			IsConcreteArrayLikeType(sourceOperation.Type) ||
			IsListLikeContractType(sourceOperation.Type) ||
			IsEnumerableContractType(sourceOperation.Type) ||
			sourceIsArrayProducingExpression;
		if (!hasArrayLikeSource)
			return false;

		var sourceParameter = new Identifier("__src");
		var sourceArgument = sourceParameter as Expression;

		// IEnumerable 接口入口可能在运行时是“可迭代对象”而非 Array 实例。
		// 这里先物化为 Array，再继续使用数组快路（filter/map），
		// 避免直接调用 source.filter/source.map 触发方法缺失。
		var normalizedSource = IsEnumerableContractType(sourceOperation.Type) &&
			!IsListLikeContractType(sourceOperation.Type) &&
			!sourceIsArrayProducingExpression
			? BuildArrayFrom(sourceArgument)
			: sourceArgument;

		Identifier? callbackParameter = null;
		Expression? callbackArgument = null;
		string? callbackNullParameterName = null;
		if ((method.Name == "Where" || method.Name == "Select") && arguments.Count == 2)
		{
			callbackParameter = new Identifier("__callback");
			callbackArgument = callbackParameter;
			callbackNullParameterName = method.Name == "Where" ? "predicate" : "selector";
		}

		var intrinsicExpression = method.Name switch
		{
			"Where" when arguments.Count == 2 =>
				BuildInstanceMethodCall(normalizedSource, "filter", callbackArgument!),
			"Select" when arguments.Count == 2 =>
				BuildInstanceMethodCall(normalizedSource, "map", callbackArgument!),
			"ToList" when arguments.Count == 1 =>
				sourceIsArrayProducingExpression ? sourceArgument : BuildArrayFrom(sourceArgument),
			"ToArray" when arguments.Count == 1 =>
				sourceIsArrayProducingExpression ? sourceArgument : BuildArrayFrom(sourceArgument),
			_ => null
		};
		if (intrinsicExpression is null)
			return false;

		var statements = new List<Statement>
		{
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, sourceParameter, Null),
				BuildArgumentNullThrowStatement("source"),
				null)
		};
		var parameters = new List<Node> { sourceParameter };
		var callArguments = new List<Expression> { sourceExpression };

		if (callbackParameter is not null && callbackNullParameterName is not null)
		{
			parameters.Add(callbackParameter);
			callArguments.Add(arguments[1]);
			statements.Add(new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, callbackParameter, Null),
				BuildArgumentNullThrowStatement(callbackNullParameterName),
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

		return expression is not null;
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

	private bool TryBuildIntrinsicMethodInvocation(IInvocationOperation operation, IMethodSymbol method, Expression? instance, List<Expression> arguments, SenseArgument argument, out Expression? expression)
	{
		expression = null;
		if (method.ContainingType is null)
			return false;

		var containingType = method.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (TryBuildVueHInvocationIntrinsic(operation, method, arguments, argument, out expression))
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

			expr = BuildArrayIndexAccess(operation, expr, indexOperation, argument, initializations, allowRange: true);
		}

		return expr;
	}

	private Expression BuildArrayElementMutationTarget(
		IArrayElementReferenceOperation operation,
		SenseArgument argument,
		List<Expression> initializations)
	{
		if (operation.Indices.Length == 0)
			return HandleTransformationFailure<Expression>(operation, "Array assignment requires at least one index.");

		Expression expr = Translate<Expression>(operation.ArrayReference, argument);
		for (var i = 0; i < operation.Indices.Length; i++)
		{
			var indexOperation = operation.Indices[i];
			if (indexOperation is IRangeOperation)
			{
				return HandleTransformationFailure<Expression>(
					operation,
					"Array range access is not assignable in JavaScript lowering.");
			}

			expr = BuildArrayIndexAccess(operation, expr, indexOperation, argument, initializations, allowRange: false);
		}

		return expr;
	}

	private Expression BuildArrayIndexAccess(
		IOperation ownerOperation,
		Expression target,
		IOperation indexOperation,
		SenseArgument argument,
		List<Expression> initializations,
		bool allowRange)
	{
		if (RequiresArrayReceiverCaching(indexOperation))
			target = MaterializePropertyMutationOperand(target, ownerOperation, argument, initializations, $"array{initializations.Count}");

		if (TryUnwrapArrayFromEndIndex(indexOperation, out var unary))
		{
			var lengthAccess = new MemberExpression(target, new Identifier("length"), computed: false, optional: false);
			var innerIndex = Translate<Expression>(unary.Operand, argument);
			var fromEndIndex = new NonLogicalBinaryExpression(Operator.Subtraction, lengthAccess, innerIndex);
			return new MemberExpression(target, fromEndIndex, computed: true, optional: false);
		}

		if (indexOperation is IRangeOperation range)
		{
			if (!allowRange)
			{
				return HandleTransformationFailure<Expression>(
					ownerOperation,
					"Array range access is not assignable in JavaScript lowering.");
			}

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
		var resolvedHostType = operation.Instance.Type ?? operation.IndexerSymbol?.ContainingType ?? operation.LengthSymbol?.ContainingType;
		var translatedInstance = Translate<Expression>(operation.Instance, argument);

		if (cacheForRepeatedReadWrite || RequiresImplicitIndexerLengthAccess(operation.Argument))
			translatedInstance = MaterializePropertyMutationOperand(translatedInstance, ownerOperation, argument, initializations, "iinst");

		Expression? lengthExpr = null;
		Expression GetLengthExpr()
		{
			if (lengthExpr is not null)
				return lengthExpr;

			if (operation.LengthSymbol is null)
			{
				return HandleTransformationFailure<Expression>(
					operation,
					$"Implicit index access on '{resolvedHostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported Length/Count symbol.");
			}

			lengthExpr = BuildImplicitIndexerLengthAccess(operation, operation.LengthSymbol, translatedInstance, argument, resolvedHostType);
			return lengthExpr;
		}

		if (TryGetRangeArgument(operation.Argument, out var rangeArgument))
		{
			if (operation.IndexerSymbol is null)
			{
				arguments =
				[
					HandleTransformationFailure<Expression>(
						operation,
						$"Implicit range access on '{resolvedHostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported indexer or slice symbol.")
				];
				hostType = resolvedHostType;
				instance = translatedInstance;
				return;
			}

			var startExpr = BuildImplicitRangeBoundaryExpression(rangeArgument.LeftOperand, GetLengthExpr, argument)
				?? new NumericLiteral(0, "0");
			var endExpr = BuildImplicitRangeBoundaryExpression(rangeArgument.RightOperand, GetLengthExpr, argument)
				?? GetLengthExpr();
			arguments = BuildImplicitRangeArguments(operation, operation.IndexerSymbol, startExpr, endExpr, GetLengthExpr);
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
		if (operation.IndexerSymbol is null)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Implicit index access on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported indexer symbol.");
		}

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

		property = ResolveImplicitIndexerProperty(operation);
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

	private IPropertySymbol ResolveImplicitIndexerProperty(IImplicitIndexerReferenceOperation operation)
	{
		return operation.IndexerSymbol switch
		{
			IPropertySymbol property => property,
			IMethodSymbol { AssociatedSymbol: IPropertySymbol property } => property,
			IMethodSymbol method => HandleTransformationFailure<IPropertySymbol>(
				operation,
				$"Implicit indexer symbol '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not assignable because it does not lower to a property-style setter."),
			_ => HandleTransformationFailure<IPropertySymbol>(
				operation,
				$"Implicit indexer target on '{operation.Instance.Type?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' does not expose a property-style setter.")
		};
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

	private Expression BuildImplicitIndexIndexerAccess(
		IImplicitIndexerReferenceOperation operation,
		Expression instance,
		SenseArgument argument,
		ITypeSymbol? hostType)
	{
		if (operation.IndexerSymbol is null)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Implicit index access on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported indexer symbol.");
		}

		var lengthExpr = operation.LengthSymbol is not null
			? BuildImplicitIndexerLengthAccess(operation, operation.LengthSymbol, instance, argument, hostType)
			: null;
		var indexExpr = BuildImplicitIndexArgumentExpression(operation, operation.Argument, lengthExpr, argument);
		return BuildListPatternBoundAccess(
			operation,
			operation.IndexerSymbol,
			instance,
			[indexExpr],
			argument,
			"implicit indexer access",
			hostType ?? operation.IndexerSymbol.ContainingType);
	}

	private Expression BuildImplicitRangeIndexerAccess(
		IImplicitIndexerReferenceOperation operation,
		Expression instance,
		IRangeOperation rangeOperation,
		SenseArgument argument,
		ITypeSymbol? hostType)
	{
		if (operation.IndexerSymbol is null)
		{
			return HandleTransformationFailure<Expression>(
				operation,
				$"Implicit range access on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported indexer or slice symbol.");
		}

		Expression? lengthExpr = null;

		Expression GetLengthExpr()
		{
			if (lengthExpr is not null)
				return lengthExpr;

			if (operation.LengthSymbol is null)
			{
				return HandleTransformationFailure<Expression>(
					operation,
					$"Implicit range access on '{hostType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<unknown>"}' requires a supported Length/Count symbol when using '^' or an open-ended range.");
			}

			lengthExpr = BuildImplicitIndexerLengthAccess(operation, operation.LengthSymbol, instance, argument, hostType);
			return lengthExpr;
		}

		var startExpr = BuildImplicitRangeBoundaryExpression(rangeOperation.LeftOperand, GetLengthExpr, argument)
			?? new NumericLiteral(0, "0");
		var endExpr = BuildImplicitRangeBoundaryExpression(rangeOperation.RightOperand, GetLengthExpr, argument)
			?? GetLengthExpr();
		var sliceArguments = BuildImplicitRangeArguments(operation, operation.IndexerSymbol, startExpr, endExpr, GetLengthExpr);
		return BuildListPatternBoundAccess(
			operation,
			operation.IndexerSymbol,
			instance,
			sliceArguments,
			argument,
			"implicit range access",
			hostType ?? operation.IndexerSymbol.ContainingType);
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
			"Implicit System.Index access requires a direct numeric index expression or '^' expression. Standalone System.Index values are not supported in JavaScript lowering.");
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
		Expression endExpr,
		Func<Expression> getLengthExpr)
	{
		return indexerSymbol switch
		{
			IPropertySymbol property => BuildImplicitRangePropertyArguments(ownerOperation, property, startExpr, endExpr, getLengthExpr),
			IMethodSymbol method when method.AssociatedSymbol is IPropertySymbol property => BuildImplicitRangePropertyArguments(ownerOperation, property, startExpr, endExpr, getLengthExpr),
			IMethodSymbol method => BuildImplicitRangeMethodArguments(ownerOperation, method, startExpr, endExpr, getLengthExpr),
			_ => HandleUnsupportedSliceArguments(
				ownerOperation,
				$"Unsupported implicit range indexer symbol '{indexerSymbol.Kind}' for '{indexerSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}'.")
		};
	}

	private List<Expression> BuildImplicitRangeMethodArguments(
		IOperation ownerOperation,
		IMethodSymbol method,
		Expression startExpr,
		Expression endExpr,
		Func<Expression> getLengthExpr)
	{
		if (method.Parameters.Length == 1)
		{
			if (IsSystemRangeType(method.Parameters[0].Type))
			{
				return HandleUnsupportedSliceArguments(
					ownerOperation,
					$"Range-based slice method '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported in implicit range lowering. Expose an int-based Slice/Substring overload or configure a whitelist mapping.");
			}

			if (method.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32)
			{
				return HandleUnsupportedSliceArguments(
					ownerOperation,
					$"Implicit range slice method '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' must take int-compatible parameters.");
			}

			if (!ReferenceEquals(endExpr, getLengthExpr()))
			{
				return HandleUnsupportedSliceArguments(
					ownerOperation,
					$"Slice method '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' cannot represent a bounded range because it only accepts a single int parameter.");
			}

			return [startExpr];
		}

		if (method.Parameters.Length != 2 ||
			method.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32 ||
			method.Parameters[1].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32)
		{
			return HandleUnsupportedSliceArguments(
				ownerOperation,
				$"Implicit range slice method '{method.OriginalDefinition.ToDisplayString(Format.NameFormat)}' must expose int-compatible Slice(start, length) semantics.");
		}

		return
		[
			startExpr,
			BuildImplicitRangeLengthExpression(startExpr, endExpr)
		];
	}

	private List<Expression> BuildImplicitRangePropertyArguments(
		IOperation ownerOperation,
		IPropertySymbol property,
		Expression startExpr,
		Expression endExpr,
		Func<Expression> getLengthExpr)
	{
		if (!property.IsIndexer || property.Parameters.Length == 0)
		{
			return HandleUnsupportedSliceArguments(
				ownerOperation,
				$"Unsupported implicit range property '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}'.");
		}

		if (property.Parameters.Length == 1)
		{
			if (IsSystemRangeType(property.Parameters[0].Type))
			{
				return HandleUnsupportedSliceArguments(
					ownerOperation,
					$"Range-based indexer '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported in implicit range lowering. Expose an int-based slice member or configure a whitelist mapping.");
			}

			if (property.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32)
			{
				return HandleUnsupportedSliceArguments(
					ownerOperation,
					$"Implicit range indexer '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' must take int-compatible parameters.");
			}

			if (!ReferenceEquals(endExpr, getLengthExpr()))
			{
				return HandleUnsupportedSliceArguments(
					ownerOperation,
					$"Indexer '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' cannot represent a bounded range because it only accepts a single int parameter.");
			}

			return [startExpr];
		}

		if (property.Parameters.Length != 2 ||
			property.Parameters[0].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32 ||
			property.Parameters[1].Type.OriginalDefinition.SpecialType != SpecialType.System_Int32)
		{
			return HandleUnsupportedSliceArguments(
				ownerOperation,
				$"Implicit range indexer '{property.OriginalDefinition.ToDisplayString(Format.NameFormat)}' must expose int-compatible start/length semantics.");
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

		// 检查白名单映射
		// 字段没有 GetMethod/SetMethod，直接使用字段符号进行白名单查询
		var mapperExpr = GetWhiteListExpression(operation.Field, argument, [], instance, out var alias);
		if (mapperExpr is not null)
			return WithOriginIfMissing(mapperExpr, operation);

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(operation, operation.Field, "field access", operation.Instance?.Type ?? operation.Field.ContainingType);

		// 对于实例字段访问，需要创建成员访问表达式
		// ImplicitReceiver 指那些语法上不需要、也不能写 this 的隐式实例引用
		if (operation.Instance is IInstanceReferenceOperation instanceReferenceOp &&
			instanceReferenceOp.ReferenceKind == InstanceReferenceKind.ImplicitReceiver)
		{
			// 隐式接收者（如对象初始化器中的字段引用）
			// 如果是常量字段，返回字面量；否则返回字段名
			var fieldExpr = GetFieldName(operation, operation.Field);
			return WithOriginIfMissing(fieldExpr, operation);
		}

		// 获取字段名称（支持别名）
		var fieldName = string.IsNullOrEmpty(alias)
			? GetCurrentModuleDeclaredOrConfigName(operation.Field)
			: alias;

		var property = new Identifier(fieldName!);
		if (instance is not null)
		{
			var optional = operation.Instance is IConditionalAccessInstanceOperation;
			return WithOriginIfMissing(new MemberExpression(instance, property, false, optional), operation);
		}

		// 静态成员：生成完整的限定名
		// public 静态类带[ECMAScriptModule]是模块类
		if (operation.Field.IsStatic && operation.Field.ContainingType is not null)
		{
			if (TryBuildImportedModuleMember(operation.Field.ContainingType, fieldName!, argument, out var importedMember) &&
				importedMember is not null)
				return WithOriginIfMissing(importedMember, operation);

			if (operation.Field.IsConst)
				return WithOriginIfMissing(GetFieldName(operation, operation.Field), operation);

			var runtimeHost = TryBuildRuntimeHostExpression(operation.Field.ContainingType, argument);
			if (runtimeHost is not null)
				return WithOriginIfMissing(new MemberExpression(runtimeHost, property, computed: false, optional: false), operation);

			var containing = BuildFullTypeName(operation.Field.ContainingType, argument);
			if (containing is not null)
				return WithOriginIfMissing(new MemberExpression(containing, property, computed: false, optional: false), operation);

			var qualified = TryBuildStaticQualifiedMemberFromSyntax(operation.Syntax, fieldName!);
			if (qualified is not null)
				return WithOriginIfMissing(qualified, operation);
		}

		var fallback = operation.Instance is null
			? GetFieldName(operation, operation.Field)
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

		// 检查白名单映射。索引器 getter 也必须先走这里，
		// 否则会绕过运行时 helper，丢失越界/缺键等 CLR 语义。
		var mapperExpr = GetWhiteListExpression(operation.Property.GetMethod!, argument, arguments, instance, out var alias);
		if (mapperExpr is not null)
			return WithOriginIfMissing(mapperExpr, operation);

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
				BuildAliasedPropertyAccess(instance, propertyName!, optional, ShouldInvokeAliasedPropertyGetter(operation, propertyName!)),
				operation);
		}

		// todo：后续需要清理和白名单整合
		// 静态成员：生成完整的限定名（如 DateTime.Now）
		// 检查属性是否是静态成员
		if (operation.Property.IsStatic && operation.Property.ContainingType is not null)
		{
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
		// 如果是白名单方法调用，需要生成本地代理方法
		// 生成代理方法参数
		var name = AllocateUniqueName(operation, argument, LoweringSite.MethodReferenceProxy());
		var count = operation.Method.Parameters.Length + (operation.Method.IsStatic ? 0 : 1);
		var args = Enumerable.Range(0, count)
			.Select(i => new Identifier($"{name}${i}") as Expression)
			.ToList();

		var whiteListMethod = ResolveStaticInterfaceProjectionMethod(operation.Method, operation.Syntax, operation.SemanticModel);
		var valueExpr = GetWhiteListExpression(whiteListMethod, argument, args, out var alias);
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
			RejectUnsupportedRuntimeFallback(operation, whiteListMethod, "method reference", operation.Instance?.Type ?? operation.Method.ContainingType);

		var instance = Translate<Expression>(operation.Instance, argument, null);
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

		Expression BuildInvExpr(bool hasReturns, in Expression expr, in List<Expression> refs, in SenseArgument ctx)
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
					var index = hasReturns ? i + 1 : i;
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
		var whiteListMethod = ResolveStaticInterfaceProjectionMethod(targetMethod, syntax, semanticModel);
		if (allowIntrinsic &&
			invocationOperation is not null &&
			TryBuildIntrinsicMethodInvocation(invocationOperation, whiteListMethod, instance, arguments, argument, out var intrinsicExpr) &&
			intrinsicExpr is not null)
			return intrinsicExpr;

		var mapperExpr = GetWhiteListExpression(whiteListMethod, argument, arguments, instance, out var alias, ownerOperation);
		if (mapperExpr is not null)
			return mapperExpr;

		if (string.IsNullOrEmpty(alias))
			RejectUnsupportedRuntimeFallback(ownerOperation, whiteListMethod, "method invocation", hostType ?? targetMethod.ContainingType);

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
				if (TryBuildPreferredRuntimeStaticMemberAccess(targetMethod, syntax, semanticModel, methodName!, out var preferredStaticCallee) &&
					preferredStaticCallee is not null)
					callee = preferredStaticCallee;
				else if (extensionHost is not null)
					callee = new MemberExpression(extensionHost, property, computed: false, optional: false);
				else if (TryBuildImportedModuleMember(targetMethod.ContainingType, methodName!, argument, out var importedMethod) &&
					importedMethod is not null)
					callee = importedMethod;
				else
				{
					var containing = BuildFullTypeName(targetMethod.ContainingType, argument);
					if (containing is not null)
						callee = new MemberExpression(containing, property, computed: false, optional: false);
					else if (!Util.IsECMAScriptRuntimeSymbol(targetMethod))
					{
						var qualified = TryBuildStaticQualifiedMemberFromSyntax(syntax, methodName!);
						if (qualified is not null)
							callee = qualified;
					}
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
