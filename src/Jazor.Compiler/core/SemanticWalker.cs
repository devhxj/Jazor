using Acornima;
using Acornima.Ast;
using Jazor.Name;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Jazor.Compiler;

/// <summary>
/// C# Roslyn 操作树到 JavaScript Acornima AST 的转换器
/// <para><b>转换器功能范围</b></para>
/// 支持将方法体、静态字段初始值、属性 getter/setter、构造函数初始值设定项、局部函数、匿名函数/Lambda 转换为 Acornima AST。
/// <para><b>核心转换原则</b></para>
/// 1. <b>语义等价性</b>：确保 C# 和 JavaScript 之间的语义完全等价，禁止任何形式的简化处理
/// 2. <b>直接AST构造</b>：必须直接构造目标AST节点，禁止使用Parser进行解析
/// 3. <b>空值安全处理</b>：构造AST节点时必须先检查输入值是否为null，避免NullReferenceException
/// 4. <b>编译时优化</b>：利用C#强类型系统的编译时信息直接生成最简AST，避免不必要的运行时检测
/// 5. <b>方法复用原则</b>：优先复用现有的Visit方法，避免为相似语义创建多个独立生成方法
/// <para><b>性能优化策略</b></para>
/// - 利用编译时类型信息避免运行时类型检测
/// - 对于强类型到弱类型转换，依赖编译时类型安全
/// - 生成最简洁的JavaScript代码，避免复杂的IIFE包装（除非必要）
/// - 递归深度控制，防止栈溢出
/// </summary>
public sealed partial class SemanticWalker : OperationVisitor<SenseArgument, Node?>, IWhiteList
{
    // SourceOrigin is stored on Node.UserData; these literals must not be shared
    // across unrelated operation trees, otherwise later assignments can overwrite
    // earlier origins.
    private static NullLiteral Null => new("null");

    private static Identifier Undefined => new("undefined");

    private static MemberExpression IsArrayExpr
        => new(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    private static (TypeMapper Mapper, string TypeName) GetMapperType(ITypeSymbol typeSymbol)
    {
        // 类型映射
        // object、匿名类型、元组类型 -> js object
        // string -> js string
        // byte、sbyte、short、ushort、int、uint、decimal、double、float -> js Number
        // long、ulong、Int128、UInt128、timestamp、BigInteger ->js BigInt
        // DateOnly、DateTime -> js Date
        // DateTimeOffset -> js Object wrapper
        // TimeOnly -> js BigInt（ticks）
        // Array -> js array
        // IDictionary -> js Map
        // IEnumerable(非IDictionary) -> js Set
        // 其他 class -> js class
        // 其他类型不支持 -> Unknown

        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType)
            return (TypeMapper.Object, "Object");

        // 使用 SpecialType 进行基础类型检查，更加类型安全和高效
        switch (typeSymbol.OriginalDefinition.SpecialType)
        {
            case SpecialType.System_Char:
            case SpecialType.System_String:
                return (TypeMapper.String, "String");
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return (TypeMapper.Number, "Number");
            case SpecialType.System_Boolean:
                return (TypeMapper.Boolean, "Boolean");
            case SpecialType.System_Object:
                return (TypeMapper.Object, "Object");
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return (TypeMapper.BigInt, "BigInt");
            case SpecialType.System_DateTime:
                return (TypeMapper.Date, "Date");
            default:
                {
                    // Array 类型检查
                    if (typeSymbol.TypeKind == TypeKind.Array)
                        return (TypeMapper.Array, "Array");

                    // Enum 类型映射到 Number
                    else if (typeSymbol.TypeKind == TypeKind.Enum)
                        return (TypeMapper.Number, "Number");

                    // Date 相关类型（SpecialType 只包含 DateTime）
                    else if (displayName == "System.DateOnly")
                        return (TypeMapper.Date, "Date");

                    else if (displayName == "System.DateTimeOffset")
                        return (TypeMapper.Object, "Object");

                    else if (IsSystemHalfType(typeSymbol) || displayName == "System.Half")
                        return (TypeMapper.Number, "Number");

                    else if (displayName == "System.TimeOnly")
                        return (TypeMapper.BigInt, "BigInt");

                    // BigInt 相关类型（SpecialType 只包含 Int64/UInt64）
                    else if (displayName == "System.Int128" ||
                        displayName == "System.UInt128" ||
                        displayName == "System.TimeSpan" ||
                        displayName == "System.Numerics.BigInteger")
                        return (TypeMapper.BigInt, "BigInt");

                    else if (
                        displayName == "System.Collections.Generic.Dictionary<TKey, TValue>" ||
                        displayName == "System.Collections.Generic.IDictionary<TKey, TValue>")
                        return (TypeMapper.Map, "Map");


                    else if (displayName == "System.Collections.Generic.HashSet<T>")
                        return (TypeMapper.Set, "Set");

                    // 集合类型检查
                    else if (displayName == "System.Collections.Generic.List<T>" ||
                        displayName == "System.Collections.Generic.IList" ||
                        displayName == "System.Collections.Generic.IList<T>" ||
                        displayName == "System.Collections.Generic.IEnumerable" ||
                        displayName == "System.Collections.Generic.IEnumerable<T>" ||
                        displayName == "System.Collections.Generic.ICollection<T>")
                        return (TypeMapper.Array, "Array");

                    // 对于自定义类型，使用instanceof检查（优先于接口检查）
                    else if (typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.TypeKind == TypeKind.Class)
                    {
                        if (WhiteList.Types.TryGetValue(displayName, out var entry))
                        {
                            // 白名单中的类型
                            if (entry.Op == Common.Op.Alias && !string.IsNullOrEmpty(entry.Value))
                            {
                                var mapper = entry.Value! switch
                                {
                                    "String" => TypeMapper.String,
                                    "Object" => TypeMapper.Object,
                                    "Array" => TypeMapper.Array,
                                    "Number" => TypeMapper.Number,
                                    "Date" => TypeMapper.Date,
                                    "BigInt" => TypeMapper.BigInt,
                                    "Map" => TypeMapper.Map,
                                    "Set" => TypeMapper.Set,
                                    _ => TypeMapper.Class
                                };
                                return (mapper, entry.Value!);
                            }
                            // Op.Allowed 等其他情况，使用原始名称
                            // 例如：System.Nullable<T>, void 等
                        }

                        // 不在白名单中或 Op != Alias 的自定义类型
                        // 这些类型应该已经被 Analyzer 验证过（被 [ECMAScript] 标记）
                        // 使用 GetTypeConfigOrWhiteListName 获取正确的名称（支持特性配置）
                        var configName = GetTypeConfigOrWhiteListName(typeSymbol);
                        return (TypeMapper.Class, configName ?? typeSymbol.Name);
                    }
                }
                break;
        }

        // 未知类型，使用白名单检查获取名称
        var unknownName = GetTypeConfigOrWhiteListName(typeSymbol);
        return (TypeMapper.Unknown, unknownName ?? typeSymbol.Name);
    }

    private static bool IsSystemHalfType(ITypeSymbol? typeSymbol)
        => typeSymbol?.OriginalDefinition is { Name: "Half" } original &&
           original.ContainingNamespace?.ToDisplayString() == "System";

    private static ISymbol GetWhiteListSymbol(IMemberReferenceOperation operation, bool isRead = true)
    {
        if (operation is IPropertyReferenceOperation propertyReferenceOp)
        {
            if (isRead && propertyReferenceOp.Property.GetMethod is not null)
                return propertyReferenceOp.Property.GetMethod;
            else if (!isRead && propertyReferenceOp.Property.SetMethod is not null)
                return propertyReferenceOp.Property.SetMethod;
        }

        return operation.Member;
    }

    private Expression? GetWhiteListExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, out string? alias)
        => GetWhiteListExpressionCore(symbol, context, arguments, instance: null, out alias);

    private Expression? GetWhiteListExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, Expression? instance, out string? alias)
        => GetWhiteListExpressionCore(symbol, context, arguments, instance, out alias);

    /// <summary>
    /// 统一消费白名单成员映射。
    ///
    /// 这里解决的是 consumer 侧“已经声明好的 Op 应该按什么顺序尝试”，
    /// 不是 producer 侧“新增成员时该选哪个 Op”。
    ///
    /// producer 侧应优先选择：
    /// `Allowed/Alias -> Inline -> Import -> Compile`。
    /// 这里仍然把 `Compile` 放在 consumer 分发最前面，
    /// 是因为凡是已经进入 `_whiteListCompiles` 的成员，都应被视为编译器内部保留特例。
    ///
    /// 可以把两边理解成：
    /// - producer：先决定“这个成员本质上属于模板、运行时 helper，还是编译器特例”
    /// - consumer：一旦成员已经被 producer 明确标成 Compile，就先信任这条最窄的特例路径
    ///
    /// 这里刻意把 `Compile` 和旧的 `Alias/Inline/Import` 分成两套参数语义：
    /// - `Compile`：`handler` 表示实例宿主，`args` 只保留显式参数
    /// - `Alias/Inline/Import`：继续沿用历史占位符布局，实例方法把宿主拼到参数前缀
    ///
    /// 这样既能把 `Compile` 接到主分发优先级前面，又不会一次性打坏既有模板和导入规则。
    /// </summary>
    private Expression? GetWhiteListExpressionCore(ISymbol symbol, SenseArgument context, List<Expression> arguments, Expression? instance, out string? alias)
    {
        alias = null;

        var compileExpr = TryGetCompileExpression(symbol, arguments, instance);
        if (compileExpr is not null)
            return compileExpr;

        var legacyArguments = CreateLegacyWhiteListArguments(symbol, arguments, instance);
        if (TryGetWhiteListValue(WhiteList.Members, symbol, out var displayString, out var entry))
        {
            if (entry.Op == Op.Alias)
                alias = entry.Value!;

            else if (entry.Op == Op.Inline)
                return InstantiateInlineTemplate(displayString, entry.Value!, legacyArguments);

            else if (entry.Op == Op.Import)
            {
                // Import 仍沿用历史参数语义：实例宿主拼到实参数组前缀。
                var id = context.BindImportSpecifier(entry.Path!, entry.Value!);
                return new CallExpression(id, NodeList.From(legacyArguments), optional: false);
            }
        }

        return null;
    }

    private Expression? TryGetCompileExpression(ISymbol symbol, List<Expression> arguments, Expression? instance)
    {
        if (!TryGetWhiteListValue(_whiteListCompiles, symbol, out _, out var compile))
            return null;

        var (handler, explicitArgs) = CreateCompileArguments(symbol, arguments, instance);
        return compile(handler, explicitArgs);
    }

    private static bool TryGetWhiteListValue<T>(Dictionary<string, T> mappings, ISymbol symbol, out string displayString, out T value)
        where T : notnull
    {
        foreach (var candidate in EnumerateWhiteListLookupSymbols(symbol))
        {
            var rawDisplayString = candidate.OriginalDefinition.ToDisplayString(Format.NameFormat);
            foreach (var lookupKey in EnumerateWhiteListLookupKeys(rawDisplayString))
            {
                displayString = lookupKey;
                if (mappings.TryGetValue(lookupKey, out value))
                    return true;
            }

            if (candidate is IMethodSymbol method &&
                (method.IsExtensionMethod || method.ReducedFrom is not null))
            {
                var extensionSource = method.ReducedFrom?.OriginalDefinition ?? method.OriginalDefinition;
                var staticDisplayString = extensionSource.ToDisplayString(Format.StaticExtensionNameFormat);
                foreach (var lookupKey in EnumerateWhiteListLookupKeys(staticDisplayString))
                {
                    displayString = lookupKey;
                    if (mappings.TryGetValue(lookupKey, out value))
                        return true;
                }
            }

            if (candidate is IMethodSymbol supplementalMethod)
            {
                var synthesizedStaticKey = TryBuildMethodWhiteListKey(supplementalMethod);
                if (!string.IsNullOrEmpty(synthesizedStaticKey))
                {
                    foreach (var lookupKey in EnumerateWhiteListLookupKeys(synthesizedStaticKey!))
                    {
                        displayString = lookupKey;
                        if (mappings.TryGetValue(lookupKey, out value))
                            return true;
                    }
                }
            }
        }

        displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        value = default!;
        return false;
    }

    private static string? TryBuildMethodWhiteListKey(IMethodSymbol method)
    {
        var source = method.ReducedFrom?.OriginalDefinition ?? method.OriginalDefinition;
        if (source.ContainingType is null)
            return null;

        var builder = new StringBuilder();
        if (source.IsStatic)
            builder.Append("static ");

        builder.Append(source.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat));
        builder.Append('.');
        builder.Append(source.Name);

        if (source.TypeParameters.Length > 0)
        {
            builder.Append('<');
            for (var i = 0; i < source.TypeParameters.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(source.TypeParameters[i].Name);
            }

            builder.Append('>');
        }

        builder.Append('(');
        for (var i = 0; i < source.Parameters.Length; i++)
        {
            if (i > 0)
                builder.Append(", ");

            var parameter = source.Parameters[i];
            if (parameter.RefKind == RefKind.Ref)
                builder.Append("ref ");
            else if (parameter.RefKind == RefKind.Out)
                builder.Append("out ");
            else if (parameter.RefKind == RefKind.In)
                builder.Append("in ");

            if (parameter.IsParams)
                builder.Append("params ");

            builder.Append(parameter.Type.OriginalDefinition.ToDisplayString(Format.NameFormat));
        }

        builder.Append(')');
        return builder.ToString();
    }

    private static IEnumerable<ISymbol> EnumerateWhiteListLookupSymbols(ISymbol symbol)
    {
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        foreach (var candidate in EnumerateWithOverrideFallback(symbol.OriginalDefinition))
        {
            if (seen.Add(candidate))
                yield return candidate;
        }

        // 某些泛型数学静态成员在语义模型里会先绑定到接口投影符号，
        // 但白名单声明的是具体类型上的实现成员。
        // 这里按“同容器、同名字、同参数形状”再回退一次，把 lookup 拉回稳定的实现面。
        foreach (var candidate in EnumerateContainingTypeImplementationCandidates(symbol))
        {
            foreach (var fallback in EnumerateWithOverrideFallback(candidate))
            {
                if (seen.Add(fallback))
                    yield return fallback;
            }
        }
    }

    private static IEnumerable<ISymbol> EnumerateWithOverrideFallback(ISymbol symbol)
    {
        for (ISymbol? current = symbol; current is not null; current = GetWhiteListFallbackSymbol(current))
            yield return current;
    }

    private static IEnumerable<ISymbol> EnumerateContainingTypeImplementationCandidates(ISymbol symbol)
    {
        if (symbol.ContainingType is null)
            yield break;

        if (symbol is IMethodSymbol method)
        {
            foreach (var candidate in symbol.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>())
            {
                if (!IsCompatibleMethodCandidate(method, candidate))
                    continue;

                yield return candidate.OriginalDefinition;
            }

            yield break;
        }

        if (symbol is IPropertySymbol property)
        {
            foreach (var candidate in symbol.ContainingType.GetMembers(property.Name).OfType<IPropertySymbol>())
            {
                if (!IsCompatiblePropertyCandidate(property, candidate))
                    continue;

                yield return candidate.OriginalDefinition;
            }
        }
    }

    private static bool IsCompatibleMethodCandidate(IMethodSymbol source, IMethodSymbol candidate)
    {
        if (source.MethodKind != candidate.MethodKind ||
            source.Name != candidate.Name ||
            source.IsStatic != candidate.IsStatic ||
            source.Arity != candidate.Arity ||
            source.Parameters.Length != candidate.Parameters.Length)
            return false;

        for (var i = 0; i < source.Parameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                    source.Parameters[i].Type.OriginalDefinition,
                    candidate.Parameters[i].Type.OriginalDefinition))
                return false;
        }

        return true;
    }

    private static bool IsCompatiblePropertyCandidate(IPropertySymbol source, IPropertySymbol candidate)
    {
        if (source.Name != candidate.Name ||
            source.IsStatic != candidate.IsStatic ||
            source.Parameters.Length != candidate.Parameters.Length)
            return false;

        for (var i = 0; i < source.Parameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                    source.Parameters[i].Type.OriginalDefinition,
                    candidate.Parameters[i].Type.OriginalDefinition))
                return false;
        }

        return SymbolEqualityComparer.Default.Equals(
            source.Type.OriginalDefinition,
            candidate.Type.OriginalDefinition);
    }

    private static ISymbol? GetWhiteListFallbackSymbol(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol { ReducedFrom: not null } method => method.ReducedFrom.OriginalDefinition,
            IMethodSymbol { OverriddenMethod: not null } method => method.OverriddenMethod.OriginalDefinition,
            IPropertySymbol { OverriddenProperty: not null } property => property.OverriddenProperty.OriginalDefinition,
            IEventSymbol { OverriddenEvent: not null } @event => @event.OverriddenEvent.OriginalDefinition,
            _ => null
        };

    private static IEnumerable<string> EnumerateWhiteListLookupKeys(string displayString)
    {
        yield return displayString;

        var normalizedExtensionDisplay = NormalizeExtensionThisParameterDisplay(displayString);
        if (normalizedExtensionDisplay is { Length: > 0 } &&
            !string.Equals(normalizedExtensionDisplay, displayString, StringComparison.Ordinal))
            yield return normalizedExtensionDisplay;

        var normalizedStaticDisplay = NormalizeStaticAbstractLikeDisplay(displayString);
        if (normalizedStaticDisplay is { Length: > 0 } &&
            !string.Equals(normalizedStaticDisplay, displayString, StringComparison.Ordinal))
            yield return normalizedStaticDisplay;

        const string virtualPrefix = "virtual ";
        const string overridePrefix = "override ";
        const string abstractPrefix = "abstract ";

        if (displayString.StartsWith(virtualPrefix, StringComparison.Ordinal))
        {
            yield return displayString.Substring(virtualPrefix.Length);
            yield break;
        }

        if (displayString.StartsWith(overridePrefix, StringComparison.Ordinal))
        {
            yield return displayString.Substring(overridePrefix.Length);
            yield return virtualPrefix + displayString.Substring(overridePrefix.Length);
            yield break;
        }

        if (displayString.StartsWith(abstractPrefix, StringComparison.Ordinal))
        {
            yield return displayString.Substring(abstractPrefix.Length);
            yield return virtualPrefix + displayString.Substring(abstractPrefix.Length);
            yield break;
        }

        yield return virtualPrefix + displayString;
        yield return overridePrefix + displayString;
        yield return abstractPrefix + displayString;

        static string? NormalizeExtensionThisParameterDisplay(string text)
        {
            var normalized = text
                .Replace("(this ", "(")
                .Replace(", this ", ", ");

            return string.Equals(normalized, text, StringComparison.Ordinal) ? null : normalized;
        }

        static string? NormalizeStaticAbstractLikeDisplay(string text)
        {
            const string staticAbstractPrefix = "static abstract ";
            const string staticVirtualPrefix = "static virtual ";
            const string staticOverridePrefix = "static override ";
            const string staticSealedPrefix = "static sealed ";

            if (text.StartsWith(staticAbstractPrefix, StringComparison.Ordinal))
                return "static " + text.Substring(staticAbstractPrefix.Length);
            if (text.StartsWith(staticVirtualPrefix, StringComparison.Ordinal))
                return "static " + text.Substring(staticVirtualPrefix.Length);
            if (text.StartsWith(staticOverridePrefix, StringComparison.Ordinal))
                return "static " + text.Substring(staticOverridePrefix.Length);
            if (text.StartsWith(staticSealedPrefix, StringComparison.Ordinal))
                return "static " + text.Substring(staticSealedPrefix.Length);

            return null;
        }
    }

    private static List<Expression> CreateLegacyWhiteListArguments(ISymbol symbol, List<Expression> arguments, Expression? instance)
    {
        if (symbol.IsStatic || instance is null)
            return arguments;

        var legacyArguments = new List<Expression>(arguments.Count + 1) { instance };
        legacyArguments.AddRange(arguments);
        return legacyArguments;
    }

    private static (Expression? Handler, Expression?[] Args) CreateCompileArguments(ISymbol symbol, List<Expression> arguments, Expression? instance)
    {
        if (symbol.IsStatic)
            return (null, ToNullableExpressionArray(arguments));

        // 普通调用路径会显式传入 instance；
        // 方法组等路径可能还保留“宿主在参数前缀”的旧布局，这里顺手拆开。
        if (instance is not null)
            return (instance, ToNullableExpressionArray(arguments));

        if (arguments.Count == 0)
            throw new InvalidOperationException($"Jazor 无法为实例成员 {symbol.Name} 绑定 Compile handler。");

        var compileArgs = new Expression?[arguments.Count - 1];
        for (var i = 1; i < arguments.Count; i++)
            compileArgs[i - 1] = arguments[i];

        return (arguments[0], compileArgs);
    }

    private static Expression?[] ToNullableExpressionArray(List<Expression> arguments)
    {
        var result = new Expression?[arguments.Count];
        for (var i = 0; i < arguments.Count; i++)
            result[i] = arguments[i];

        return result;
    }

    private int _recursionDepth;

    /// <summary>
    /// 调试标识
    /// </summary>
    private readonly bool _test;

    private readonly Dictionary<string, string> _testNameAliases = new(System.StringComparer.Ordinal);
    private readonly Dictionary<IOperation, string> _semanticNameKeyCache =
        new(ReferenceEqualityComparer<IOperation>.Instance);

    private readonly Action<Location, string?>? _report;

    private readonly ITypeSymbol? _moduleRootType;

    private readonly Dictionary<string, Func<Expression?, Expression?[], Expression?>> _whiteListCompiles;

    private UniqueNameSession? _uniqueNameSession;

    public SemanticWalker()
    {
        _whiteListCompiles = [];
        Generate(ref _whiteListCompiles);
	}

    public SemanticWalker(ITypeSymbol moduleRootType) : this() => _moduleRootType = moduleRootType;

    public SemanticWalker(bool test) : this() => _test = test;

    public SemanticWalker(Action<Location, string?> report):this() => _report = report;

	private static SourceOrigin CreateOrigin(IOperation operation, bool isSynthetic = false, string? name = null)
	{
		if (operation is null)
			throw new ArgumentNullException(nameof(operation));

		return CreateOrigin(operation.Syntax.GetLocation(), isSynthetic, name);
	}

	private static SourceOrigin CreateOrigin(Location location, bool isSynthetic = false, string? name = null)
	{
		if (location is null)
			throw new ArgumentNullException(nameof(location));

		var lineSpan = location.GetLineSpan();
		var sourcePath = !string.IsNullOrWhiteSpace(lineSpan.Path)
			? lineSpan.Path
			: location.SourceTree?.FilePath;

		return new SourceOrigin(
			SourcePath: sourcePath,
			StartLine: lineSpan.StartLinePosition.Line,
			StartColumn: lineSpan.StartLinePosition.Character,
			EndLine: lineSpan.EndLinePosition.Line,
			EndColumn: lineSpan.EndLinePosition.Character,
			Name: name,
			IsSynthetic: isSynthetic);
	}

	private static T WithOrigin<T>(T node, IOperation operation)
		where T : Node
	{
		if (node is null)
			throw new ArgumentNullException(nameof(node));

		node.UserData = CreateOrigin(operation);
		return node;
	}

	private static T WithOriginIfMissing<T>(T node, IOperation operation)
		where T : Node
	{
		if (node is null)
			throw new ArgumentNullException(nameof(node));

		if (node.UserData is not SourceOrigin)
			node.UserData = CreateOrigin(operation);

		return node;
	}

	private static T WithSyntheticOrigin<T>(T node, IOperation operation, string? name = null)
		where T : Node
	{
		if (node is null)
			throw new ArgumentNullException(nameof(node));

		node.UserData = CreateOrigin(operation, isSynthetic: true, name);
		return node;
	}

	partial void Generate(ref Dictionary<string, Func<Expression?, Expression?[], Expression?>> funcs);

	[DebuggerStepThrough]
    public static void EnsureSufficientExecutionStack(int recursionDepth)
    {
        if (recursionDepth > 20)
            RuntimeHelpers.EnsureSufficientExecutionStack();
    }

    /// <summary>
    /// 方法体、静态字段初始值、属性 getter/setter、构造函数初始值设定项、局部函数、匿名函数/Lambda 转换为 Acornima AST。
    /// </summary>
    /// <param name="operation">BlockSyntax对应的IOperation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Node? Visit(IOperation? operation, SenseArgument argument)
    {
        if (operation is null)
            return null;

        _recursionDepth++;
        try
        {
            EnsureSufficientExecutionStack(_recursionDepth);
            var scopedArgument = EnsureScopeContext(operation, argument);
            var result = operation.Accept(this, scopedArgument);
            if (result is null)
                return null;

            // Always anchor the operation root to the current operation.
            // Child nodes keep their own origins (when present), and sourcemap
            // emission still prefers innermost captures for same generated
            // positions.
            return WithOrigin(result, operation);
        }
        finally
        {
            _recursionDepth--;
            if (_recursionDepth == 0)
            {
                _uniqueNameSession = null;
                _semanticNameKeyCache.Clear();
                _testNameAliases.Clear();
            }
        }
    }

    private SenseArgument EnsureScopeContext(IOperation operation, SenseArgument argument, ScopeSite? explicitRootSite = null)
    {
        if (argument.ScopeContext is not null)
            return argument;

        if (_recursionDepth > 1)
            throw new InvalidOperationException($"Jazor 在访问 {operation.Kind} 时丢失了发射作用域上下文。");

        var rootSite = explicitRootSite ?? ResolveRootScopeSite(argument.Sense);
        _uniqueNameSession = new UniqueNameSession(operation, rootSite);
        return argument.WithScope(_uniqueNameSession.RootScope);
    }

    private static ScopeSite ResolveRootScopeSite(Sense sense)
        => sense switch
        {
            Sense.FunctionBody => ScopeSite.FunctionBody(),
            Sense.StaticBlock => ScopeSite.StaticBlock(),
            _ => ScopeSite.RootFragment()
        };

    private string AllocateUniqueName(IOperation operation, SenseArgument argument, LoweringSite site)
    {
        var scopedArgument = EnsureScopeContext(operation, argument);
        var owner = CreateLoweringNameOwner(operation, site, scopedArgument);
        var canonicalName = scopedArgument.AllocateName(owner, site);
        if (!_test)
            return canonicalName;

        if (_testNameAliases.TryGetValue(canonicalName, out var alias))
            return alias;

        alias = $"v${_testNameAliases.Count}";
        _testNameAliases.Add(canonicalName, alias);
        return alias;
    }

    private LoweringNameOwner CreateLoweringNameOwner(IOperation operation, LoweringSite site, SenseArgument argument)
    {
        var session = argument.ScopeContext?.Session ?? _uniqueNameSession;
        if (session is null)
            throw new InvalidOperationException($"Jazor 无法为 {operation.Kind} 创建稳定命名 owner，因为会话尚未初始化。");

        return new(
            BuildLoweringOwnerStableKey(operation, site),
            session.GetOperationIdentity(operation));
    }

    private string BuildLoweringOwnerStableKey(IOperation operation, LoweringSite site)
    {
        var builder = new StringBuilder();
        builder.Append("owner|").Append(site.Kind).Append('|');

        switch (site.Kind)
        {
            case LoweringSiteKind.CreationTemp:
                builder.Append("creation");
                break;

            case LoweringSiteKind.SwitchExpressionInput:
                if (operation is ISwitchExpressionOperation switchExpression)
                    builder.Append(BuildSemanticNameKey(switchExpression.Value));
                else
                    builder.Append(BuildSemanticNameKey(operation));
                break;

            case LoweringSiteKind.SwitchPatternInput:
                if (operation is ISwitchOperation @switch)
                    builder.Append(BuildSemanticNameKey(@switch.Value));
                else
                    builder.Append(BuildSemanticNameKey(operation));
                break;

            case LoweringSiteKind.MethodReferenceProxy:
                if (operation is IMethodReferenceOperation methodReference)
                {
                    var resolvedMethod = ResolveStaticInterfaceProjectionMethod(
                        methodReference.Method,
                        methodReference.Syntax,
                        methodReference.SemanticModel);
                    builder.Append(DescribeStableSymbol(resolvedMethod)).Append('|');
                    builder.Append(methodReference.Instance is null
                        ? "<null>"
                        : BuildSemanticNameKey(methodReference.Instance));
                }
                else
                    builder.Append(BuildSemanticNameKey(operation));
                break;

            case LoweringSiteKind.MultiCatchParameter:
                builder.Append("mcatch");
                break;

            case LoweringSiteKind.SyntheticCatchParameter:
                builder.Append("scatch");
                break;

            default:
                builder.Append(BuildSemanticNameKey(operation));
                break;
        }

        return UniqueNameSession.HashHex(builder.ToString(), 24);
    }

    private string BuildSemanticNameKey(IOperation operation)
    {
        if (_semanticNameKeyCache.TryGetValue(operation, out var cached))
            return cached;

        var builder = new StringBuilder();
        builder.Append("kind=").Append(operation.Kind);
        AppendSemanticNameType(builder, operation.Type);
        AppendSemanticNameConstant(builder, operation.ConstantValue);

        switch (operation)
        {
            case ILocalReferenceOperation localReference:
                builder.Append("|local=").Append(localReference.Local.Name);
                break;

            case IParameterReferenceOperation parameterReference:
                builder.Append("|param=").Append(parameterReference.Parameter.Ordinal);
                builder.Append(':').Append(parameterReference.Parameter.Name);
                break;

            case IInstanceReferenceOperation instanceReference:
                builder.Append("|refkind=").Append(instanceReference.ReferenceKind);
                break;

            case IMethodReferenceOperation methodReference:
                builder.Append("|method=").Append(DescribeStableSymbol(methodReference.Method));
                builder.Append("|instance=").Append(methodReference.Instance is null
                    ? "<null>"
                    : BuildSemanticNameKey(methodReference.Instance));
                break;

            case IPropertyReferenceOperation propertyReference:
                builder.Append("|property=").Append(DescribeStableSymbol(propertyReference.Property));
                builder.Append("|instance=").Append(propertyReference.Instance is null
                    ? "<null>"
                    : BuildSemanticNameKey(propertyReference.Instance));
                foreach (var propertyArgument in propertyReference.Arguments)
                    builder.Append("|arg=").Append(BuildSemanticNameKey(propertyArgument));
                break;

            case IMemberReferenceOperation memberReference:
                builder.Append("|member=").Append(DescribeStableSymbol(memberReference.Member));
                builder.Append("|instance=").Append(memberReference.Instance is null
                    ? "<null>"
                    : BuildSemanticNameKey(memberReference.Instance));
                break;

            case IInvocationOperation invocation:
                builder.Append("|method=").Append(DescribeStableSymbol(invocation.TargetMethod));
                builder.Append("|instance=").Append(invocation.Instance is null
                    ? "<null>"
                    : BuildSemanticNameKey(invocation.Instance));
                builder.Append("|virtual=").Append(invocation.IsVirtual);
                foreach (var argument in invocation.Arguments)
                    builder.Append("|arg=").Append(BuildSemanticNameKey(argument));
                break;

            case IArgumentOperation argument:
                builder.Append("|argkind=").Append(argument.ArgumentKind);
                builder.Append("|ref=").Append(argument.Parameter?.RefKind);
                builder.Append("|param=").Append(argument.Parameter?.Ordinal.ToString(CultureInfo.InvariantCulture) ?? "<none>");
                builder.Append("|value=").Append(BuildSemanticNameKey(argument.Value));
                break;

            case IConversionOperation conversion:
                builder.Append("|implicit=").Append(conversion.IsImplicit);
                builder.Append("|checked=").Append(conversion.IsChecked);
                builder.Append("|exists=").Append(conversion.Conversion.Exists);
                builder.Append("|identity=").Append(conversion.Conversion.IsIdentity);
                builder.Append("|numeric=").Append(conversion.Conversion.IsNumeric);
                builder.Append("|reference=").Append(conversion.Conversion.IsReference);
                builder.Append("|operator=").Append(DescribeStableSymbol(conversion.OperatorMethod));
                builder.Append("|operand=").Append(BuildSemanticNameKey(conversion.Operand));
                break;

            case IObjectCreationOperation objectCreation:
                builder.Append("|ctor=").Append(DescribeStableSymbol(objectCreation.Constructor));
                foreach (var argument in objectCreation.Arguments)
                    builder.Append("|arg=").Append(BuildSemanticNameKey(argument));
                builder.Append("|init=").Append(objectCreation.Initializer is null
                    ? "<none>"
                    : BuildSemanticNameKey(objectCreation.Initializer));
                break;

            case IArrayElementReferenceOperation arrayElementReference:
                builder.Append("|array=").Append(BuildSemanticNameKey(arrayElementReference.ArrayReference));
                foreach (var index in arrayElementReference.Indices)
                    builder.Append("|index=").Append(BuildSemanticNameKey(index));
                break;

            case IBinaryOperation binary:
                builder.Append("|operator=").Append(binary.OperatorKind);
                builder.Append("|lifted=").Append(binary.IsLifted);
                builder.Append("|checked=").Append(binary.IsChecked);
                builder.Append("|method=").Append(DescribeStableSymbol(binary.OperatorMethod));
                builder.Append("|left=").Append(BuildSemanticNameKey(binary.LeftOperand));
                builder.Append("|right=").Append(BuildSemanticNameKey(binary.RightOperand));
                break;

            case IUnaryOperation unary:
                builder.Append("|operator=").Append(unary.OperatorKind);
                builder.Append("|lifted=").Append(unary.IsLifted);
                builder.Append("|checked=").Append(unary.IsChecked);
                builder.Append("|method=").Append(DescribeStableSymbol(unary.OperatorMethod));
                builder.Append("|operand=").Append(BuildSemanticNameKey(unary.Operand));
                break;

            case IIncrementOrDecrementOperation incrementOrDecrement:
                builder.Append("|postfix=").Append(incrementOrDecrement.IsPostfix);
                builder.Append("|lifted=").Append(incrementOrDecrement.IsLifted);
                builder.Append("|checked=").Append(incrementOrDecrement.IsChecked);
                builder.Append("|method=").Append(DescribeStableSymbol(incrementOrDecrement.OperatorMethod));
                builder.Append("|target=").Append(BuildSemanticNameKey(incrementOrDecrement.Target));
                break;

            case ITupleBinaryOperation tupleBinary:
                builder.Append("|tupleop=").Append(tupleBinary.OperatorKind);
                builder.Append("|left=").Append(BuildSemanticNameKey(tupleBinary.LeftOperand));
                builder.Append("|right=").Append(BuildSemanticNameKey(tupleBinary.RightOperand));
                break;

            case IDeconstructionAssignmentOperation deconstruction:
                builder.Append("|target=").Append(BuildSemanticNameKey(deconstruction.Target));
                builder.Append("|value=").Append(BuildSemanticNameKey(deconstruction.Value));
                break;

            case ICompoundAssignmentOperation compoundAssignment:
                builder.Append("|operator=").Append(compoundAssignment.OperatorKind);
                builder.Append("|lifted=").Append(compoundAssignment.IsLifted);
                builder.Append("|checked=").Append(compoundAssignment.IsChecked);
                builder.Append("|method=").Append(DescribeStableSymbol(compoundAssignment.OperatorMethod));
                builder.Append("|target=").Append(BuildSemanticNameKey(compoundAssignment.Target));
                builder.Append("|value=").Append(BuildSemanticNameKey(compoundAssignment.Value));
                break;

            case IIsTypeOperation isType:
                builder.Append("|value=").Append(BuildSemanticNameKey(isType.ValueOperand));
                builder.Append("|checked=").Append(isType.TypeOperand?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<null>");
                builder.Append("|negated=").Append(isType.IsNegated);
                break;

            case IRelationalPatternOperation relationalPattern:
                builder.Append("|relop=").Append(relationalPattern.OperatorKind);
                builder.Append("|value=").Append(BuildSemanticNameKey(relationalPattern.Value));
                break;

            case IBinaryPatternOperation binaryPattern:
                builder.Append("|patop=").Append(binaryPattern.OperatorKind);
                builder.Append("|left=").Append(BuildSemanticNameKey(binaryPattern.LeftPattern));
                builder.Append("|right=").Append(BuildSemanticNameKey(binaryPattern.RightPattern));
                break;

            case IPropertySubpatternOperation propertySubpattern:
                builder.Append("|member=").Append(BuildSemanticNameKey(propertySubpattern.Member));
                builder.Append("|pattern=").Append(BuildSemanticNameKey(propertySubpattern.Pattern));
                break;

            case IDeclarationPatternOperation declarationPattern:
                builder.Append("|matched=").Append(declarationPattern.MatchedType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<null>");
                builder.Append("|declared=").Append(DescribeStableSymbol(declarationPattern.DeclaredSymbol));
                builder.Append("|matchesnull=").Append(declarationPattern.MatchesNull);
                break;

            case ITypePatternOperation typePattern:
                builder.Append("|matched=").Append(typePattern.MatchedType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<null>");
                break;

            case IRecursivePatternOperation recursivePattern:
                builder.Append("|matched=").Append(recursivePattern.MatchedType?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<null>");
                builder.Append("|deconstruct=").Append(DescribeStableSymbol(recursivePattern.DeconstructSymbol));
                builder.Append("|declared=").Append(DescribeStableSymbol(recursivePattern.DeclaredSymbol));
                foreach (var subpattern in recursivePattern.DeconstructionSubpatterns)
                    builder.Append("|decon=").Append(BuildSemanticNameKey(subpattern));
                foreach (var subpattern in recursivePattern.PropertySubpatterns)
                    builder.Append("|prop=").Append(BuildSemanticNameKey(subpattern));
                break;

            case IListPatternOperation listPattern:
                builder.Append("|declared=").Append(DescribeStableSymbol(listPattern.DeclaredSymbol));
                builder.Append("|length=").Append(DescribeStableSymbol(listPattern.LengthSymbol));
                builder.Append("|indexer=").Append(DescribeStableSymbol(listPattern.IndexerSymbol));
                foreach (var pattern in listPattern.Patterns)
                    builder.Append("|item=").Append(BuildSemanticNameKey(pattern));
                break;

            case ISlicePatternOperation slicePattern:
                builder.Append("|slice=").Append(DescribeStableSymbol(slicePattern.SliceSymbol));
                builder.Append("|pattern=").Append(slicePattern.Pattern is null
                    ? "<null>"
                    : BuildSemanticNameKey(slicePattern.Pattern));
                break;

            case IInterpolatedStringTextOperation interpolatedText:
                builder.Append("|text=").Append(interpolatedText.Text.ConstantValue.Value as string ?? string.Empty);
                break;

            case IAnonymousFunctionOperation anonymousFunction:
                builder.Append("|symbol=").Append(DescribeStableSymbol(anonymousFunction.Symbol));
                break;

            case ILocalFunctionOperation localFunction:
                builder.Append("|symbol=").Append(DescribeStableSymbol(localFunction.Symbol));
                break;

            default:
                foreach (var child in operation.ChildOperations)
                {
                    if (child is not null)
                        builder.Append("|child=").Append(BuildSemanticNameKey(child));
                }
                break;
        }

        cached = UniqueNameSession.HashHex(builder.ToString(), 20);
        _semanticNameKeyCache.Add(operation, cached);
        return cached;
    }

    private static void AppendSemanticNameType(StringBuilder builder, ITypeSymbol? type)
    {
        builder.Append("|type=");
        builder.Append(type is null
            ? "<null>"
            : type.OriginalDefinition.ToDisplayString(Format.NameFormat));
    }

    private static void AppendSemanticNameConstant(StringBuilder builder, Optional<object?> constantValue)
    {
        if (!constantValue.HasValue)
            return;

        builder.Append("|const=");
        builder.Append(FormatSemanticConstant(constantValue.Value));
    }

    private static string FormatSemanticConstant(object? value)
    {
        if (value is null)
            return "<null>";

        return value switch
        {
            string text => "\"" + text + "\"",
            char c => "'" + c.ToString() + "'",
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? value.GetType().FullName ?? "<unknown>"
        };
    }

    private static string DescribeStableSymbol(ISymbol? symbol)
        => symbol is null
            ? "<null>"
            : symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);

    private static List<Statement> MaterializeScopedStatements(SenseArgument context, IEnumerable<Statement> pendingStatements)
    {
        var statements = new List<Statement>();
        if (context.HasVarDeclarator)
        {
            var declarators = context.FlushVarDeclarator();
            if (declarators.Count > 0)
                statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
        }

        statements.AddRange(pendingStatements);
        return statements;
    }

    /// <summary>
    /// 操作无法转换时的兜底方法，提供详细的错误信息，包括操作类型
    /// </summary>
    /// <param name="operation">无法转换的Operation</param>
    /// <param name="message">错误信息</param>
    /// <returns></returns>
    /// <exception cref="OperationTransformationException">当操作无法转换时抛出</exception>
    private T HandleTransformationFailure<T>(IOperation operation, string? message)
    {
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);
        throw new OperationTransformationException(operation.Kind, message);
    }

    /// <summary>
    /// 语法无法转换时的兜底方法
    /// </summary>
    /// <param name="operation">无法转换的Operation</param>
    /// <param name="message">错误信息</param>
    /// <returns></returns>
    /// <exception cref="SyntaxNodeTransformationException"></exception>
    private Node HandleTransformationFailure(SyntaxNode node, string? message)
    {
        var location = node.GetLocation();
        _report?.Invoke(location, message);
        throw new SyntaxNodeTransformationException(node.Kind(), message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为Expression
    /// <para>
    /// </summary>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <returns>转换后的Expression节点，如果操作为null或转换结果为null则抛出异常</returns>
    /// <exception cref="OperationTransformationException"></exception>
    private Expression TranslateExpression(IOperation operation, SenseArgument argument)
    {
        var node = Visit(operation, argument);
        if (node is Expression result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(Expression).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        throw new OperationTransformationException(operation.Kind, message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为指定类型的AST节点
    /// <para>
    /// 此方法是类型安全的访问器，用于处理可能为null的操作，确保转换结果符合预期的节点类型。
    /// 如果操作为null、转换结果为null或无法转换，抛出异常。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望返回的AST节点类型</typeparam>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <returns>转换后的指定类型AST节点，如果操作为null或转换结果为null则抛出异常</returns>
    /// <exception cref="OperationTransformationException">当操作不为null但无法转换为目标类型时抛出</exception>
    private T Translate<T>(IOperation operation, SenseArgument argument) where T : INode
    {
        var node = Visit(operation, argument);
        if (node is T result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        throw new OperationTransformationException(operation.Kind, message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为指定类型的AST节点
    /// <para>
    /// 此方法是类型安全的访问器，用于处理可能为null的操作，确保转换结果符合预期的节点类型。
    /// 如果操作为null、转换结果为null或无法转换，返回默认值而不是抛出异常。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望返回的AST节点类型</typeparam>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <param name="defaultValue">为空时的默认值</param>
    /// <returns>转换后的指定类型AST节点，如果操作为null或转换结果为null则返回默认值</returns>
    private T? Translate<T>(IOperation? operation, SenseArgument argument, T? defaultValue) where T : INode
    {
        if (operation is null)
            return defaultValue;

        var node = Visit(operation, argument);
        if (node is null)
            return defaultValue;

        if (node is T result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        return defaultValue;
    }

    /// <summary>
    /// 安全访问操作并将其添加到集合中
    /// <para>
    /// 此方法用于将操作转换为指定类型的AST节点，并将成功转换的节点添加到集合中。
    /// 如果操作为null或转换结果为null，则跳过处理，不抛出异常。
    /// 如果转换结果类型不匹配，会记录错误信息但不中断处理流程。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望的AST节点类型</typeparam>
    /// <param name="target">用于存放成功转换的AST节点的集合</param>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    private void Translate<T>(ICollection<T> target, IOperation? operation, SenseArgument argument) where T : INode
    {
        if (operation is null)
            return;

        var node = Visit(operation, argument);
        if (node is null)
            return;

        if (node is T item)
            target.Add(item);
        else
        {
            var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
            var location = operation.Syntax.GetLocation();
            _report?.Invoke(location, message);
        }
    }

    /// <summary>
    /// 安全访问操作并将其添加到集合中
    /// <para>
    /// 此方法用于将操作转换为指定类型的AST节点，并将成功转换的节点添加到集合中。
    /// 如果操作为null或转换结果为null，则跳过处理，不抛出异常。
    /// 如果转换结果类型不匹配，会记录错误信息但不中断处理流程。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望的AST节点类型</typeparam>
    /// <param name="target">用于存放成功转换的AST节点的集合</param>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <param name="defaultValue">为空时的默认值</param>
    private void Translate<T>(ICollection<T?> target, IOperation? operation, SenseArgument argument, T? defaultValue) where T : INode
    {
        if (operation is null)
            return;

        var node = Visit(operation, argument);
        if (node is null)
            return;

        if (node is T item)
            target.Add(item);
        else
        {
            var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
            var location = operation.Syntax.GetLocation();
            _report?.Invoke(location, message);
        }
    }
}
