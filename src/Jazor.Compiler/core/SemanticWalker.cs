using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Jazor.Compiler;

/// <summary>
/// C# Roslyn 操作树到 JavaScript Acornima AST 的转换器
/// <para><b>转换器功能范围</b></para>
/// 支持将方法体、静态字段初始值、属性 getter/setter、构造函数初始值设定项、局部函数、匿名函数/Lambda 转换为 Acornima AST。
/// <para><b>核心转换原则</b></para>
/// 1. <b>行为保真优先</b>：优先保证使用点可观察行为；当完整 CLR/runtime 结构等价不可得时，允许擦除或协议模拟
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

    private static bool TryMapKnownRuntimeTypeName(string runtimeTypeName, out (TypeMapper Mapper, string TypeName) mapped)
    {
        switch (runtimeTypeName)
        {
            case "String":
                mapped = (TypeMapper.String, "String");
                return true;
            case "Object":
                mapped = (TypeMapper.Object, "Object");
                return true;
            case "Array":
                mapped = (TypeMapper.Array, "Array");
                return true;
            case "Number":
                mapped = (TypeMapper.Number, "Number");
                return true;
            case "Date":
                mapped = (TypeMapper.Date, "Date");
                return true;
            case "BigInt":
                mapped = (TypeMapper.BigInt, "BigInt");
                return true;
            case "Map":
                mapped = (TypeMapper.Map, "Map");
                return true;
            case "Set":
                mapped = (TypeMapper.Set, "Set");
                return true;
            case "Boolean":
                mapped = (TypeMapper.Boolean, "Boolean");
                return true;
            default:
                mapped = default;
                return false;
        }
    }

    private static bool TryMapKnownWhiteListAlias(ITypeSymbol typeSymbol, out (TypeMapper Mapper, string TypeName) mapped)
    {
        mapped = default;
        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (!TryGetWhiteListValue(WhiteList.Types, displayName, out _, out var entry) ||
            entry.Op != ECMAScript.Contract.Op.Alias ||
            string.IsNullOrWhiteSpace(entry.Value))
            return false;

        return TryMapKnownRuntimeTypeName(entry.Value!, out mapped);
    }

    private static bool TryMapKnownEcmascriptRuntimeHost(ITypeSymbol typeSymbol, out (TypeMapper Mapper, string TypeName) mapped)
    {
        mapped = default;
        if (!Util.IsECMAScriptRuntimeType(typeSymbol))
            return false;

        var runtimeTypeName = GetTypeConfigOrWhiteListName(typeSymbol);
        if (string.IsNullOrWhiteSpace(runtimeTypeName))
            return false;

        return TryMapKnownRuntimeTypeName(runtimeTypeName!, out mapped);
    }

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
        // long、ulong、Int128、UInt128、BigInteger ->js BigInt
        // DateOnly、DateTime -> js Date
        // DateTimeOffset -> js Object wrapper
        // TimeOnly -> js Object wrapper
        // TimeSpan -> js Object wrapper
        // Array -> js array
        // IDictionary -> js Map
        // IEnumerable(非IDictionary) -> js Set
        // 其他 class -> js class
        // 其他类型不支持 -> Unknown

        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType)
            return (TypeMapper.Object, "Object");

        if (TryMapKnownEcmascriptRuntimeHost(typeSymbol, out var runtimeHost))
            return runtimeHost;

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

                    // enum 默认仍是数值域；只有显式 [String] 标记的 enum 才按字符串标量处理
                    else if (typeSymbol.TypeKind == TypeKind.Enum)
                        return Util.IsStringEnumType(typeSymbol)
                            ? (TypeMapper.String, "String")
                            : (TypeMapper.Number, "Number");

                    // Date 相关类型（SpecialType 只包含 DateTime）
                    else if (displayName == "System.DateOnly")
                        return (TypeMapper.Date, "Date");

                    else if (displayName == "System.DateTimeOffset")
                        return (TypeMapper.Object, "Object");

                    else if (IsSystemHalfType(typeSymbol) || displayName == "System.Half")
                        return (TypeMapper.Number, "Number");

                    else if (displayName == "System.TimeOnly")
                        return (TypeMapper.Object, "Object");

                    // BigInt 相关类型（SpecialType 只包含 Int64/UInt64）
                    else if (displayName == "System.Int128" ||
                        displayName == "System.UInt128" ||
                        displayName == "System.Numerics.BigInteger")
                        return (TypeMapper.BigInt, "BigInt");

                    else if (displayName == "System.TimeSpan")
                        return (TypeMapper.Object, "Object");

                    else if (displayName == "System.Collections.Generic.Dictionary<TKey, TValue>")
                        return (TypeMapper.Map, "Map");


                    else if (displayName == "System.Collections.Generic.HashSet<T>")
                        return (TypeMapper.Set, "Set");

                    // 集合类型检查
                    else if (displayName == "System.Collections.Generic.List<T>")
                        return (TypeMapper.Array, "Array");

                    else if (TryMapKnownWhiteListAlias(typeSymbol, out var whiteListAlias))
                        return whiteListAlias;

                    // 对于自定义类型，使用instanceof检查（优先于接口检查）
                    else if (typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.TypeKind == TypeKind.Class)
                    {
                        if (TryGetWhiteListValue(WhiteList.Types, displayName, out _, out var entry))
                        {
                            // 白名单中的类型
                            if (entry.Op == ECMAScript.Contract.Op.Alias && !string.IsNullOrEmpty(entry.Value))
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

    private Expression? GetWhiteListExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, out string? alias, IOperation? originOperation = null, ITypeSymbol? hostType = null)
        => GetWhiteListExpressionCore(symbol, context, arguments, instance: null, out alias, originOperation, hostType);

    private Expression? GetWhiteListExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, Expression? instance, out string? alias, IOperation? originOperation = null, ITypeSymbol? hostType = null)
        => GetWhiteListExpressionCore(symbol, context, arguments, instance, out alias, originOperation, hostType);

    private void RejectUnsupportedTypeFallback(IOperation operation, ITypeSymbol typeSymbol, string usage)
    {
        var unsupportedType = FindFirstUnsupportedExternalType(operation, typeSymbol);
        if (unsupportedType is null)
            return;

        HandleTransformationFailure<Node>(
            operation,
            $"External type '{unsupportedType.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported and cannot be used for {usage}. Only [ECMAScript]/[ECMAScriptModule] types (or nested under such types) and whitelist types are supported.");
    }

    private void RejectAmbiguousRuntimeTypeFilter(IOperation operation, ITypeSymbol typeSymbol, string usage)
    {
        if (GetMapperType(typeSymbol).Mapper != TypeMapper.Class ||
            !TryGetWhiteListTypeAlias(typeSymbol, out var runtimeAlias))
            return;

        var conflicts = FindIncompatibleWhiteListAliasTypes(operation, typeSymbol, runtimeAlias);
        if (conflicts.Count == 0)
            return;

        HandleTransformationFailure<Node>(
            operation,
            $"Type '{typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}' cannot be used for {usage} because its runtime alias '{runtimeAlias}' is shared with incompatible supported types: {string.Join(", ", conflicts)}. Configure distinct runtime types in Jazor.CLR if precise runtime filtering is required.");
    }

    private void RejectUnsupportedRuntimeFallback(IOperation operation, ISymbol symbol, string usage, ITypeSymbol? hostType = null)
    {
        if (IsSupportedExternalMember(operation, symbol, hostType))
            return;

        var unsupportedType = FindFirstUnsupportedExternalType(operation, hostType) ?? FindFirstUnsupportedConstructedMemberType(operation, symbol);
        if (unsupportedType is not null)
        {
            HandleTransformationFailure<Node>(
                operation,
                $"External type '{unsupportedType.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported and cannot be used for {usage}. Only [ECMAScript]/[ECMAScriptModule] types (or nested under such types) and whitelist types are supported.");
        }

        HandleTransformationFailure<Node>(
            operation,
            $"External member '{symbol.OriginalDefinition.ToDisplayString(Format.NameFormat)}' is not supported and cannot fall back to raw JavaScript {usage}. Only whitelist members or members declared on [ECMAScript]/[ECMAScriptModule] types are supported.");
    }

    private bool IsSupportedExternalType(IOperation operation, ITypeSymbol typeSymbol)
        => FindFirstUnsupportedExternalType(operation, typeSymbol) is null;

    private ITypeSymbol? FindFirstUnsupportedConstructedMemberType(IOperation operation, ISymbol? symbol)
    {
        return symbol switch
        {
            null => null,
            IMethodSymbol method => FindFirstUnsupportedExternalType(operation, method.ContainingType),
            IPropertySymbol property => FindFirstUnsupportedExternalType(operation, property.ContainingType),
            IFieldSymbol field => FindFirstUnsupportedExternalType(operation, field.ContainingType),
            INamedTypeSymbol namedType => FindFirstUnsupportedExternalType(operation, (ITypeSymbol)namedType),
            _ => null
        };
    }

    private ITypeSymbol? FindFirstUnsupportedExternalType(IOperation operation, ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return null;

        return IsDirectlySupportedExternalType(operation, typeSymbol) ? null : typeSymbol;
    }

    private void RejectUnsupportedNativeMapSetEqualityBoundaryIfNeeded(IOperation operation, ITypeSymbol? hostType, string usage)
    {
        if (!TryGetNativeMapSetEqualitySurface(hostType, out var equalityType, out var role))
            return;

        if (HasJsStableNativeMapSetEquality(equalityType, out var reason))
            return;

        HandleTransformationFailure<Node>(
            operation,
            $"Collection type '{hostType!.ToDisplayString(Format.NameFormat)}' cannot be used for {usage} because its {role} type '{equalityType.ToDisplayString(Format.NameFormat)}' does not have JS-stable default equality under the current native Map/Set carrier. {reason}");
    }

    private static bool TryGetNativeMapSetEqualitySurface(ITypeSymbol? hostType, out ITypeSymbol equalityType, out string role)
    {
        if (hostType is INamedTypeSymbol namedType)
        {
            var displayName = namedType.OriginalDefinition.ToDisplayString(Format.NameFormat);
            switch (displayName)
            {
                case "System.Collections.Generic.Dictionary<TKey, TValue>":
                case "System.Collections.Generic.IDictionary<TKey, TValue>":
                case "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>":
                    equalityType = namedType.TypeArguments[0];
                    role = "key";
                    return true;

                case "System.Collections.Generic.HashSet<T>":
                case "System.Collections.Generic.ISet<T>":
                case "System.Collections.ObjectModel.ReadOnlySet<T>":
                    equalityType = namedType.TypeArguments[0];
                    role = "element";
                    return true;
            }
        }

        equalityType = null!;
        role = string.Empty;
        return false;
    }

    private bool HasJsStableNativeMapSetEquality(ITypeSymbol typeSymbol, out string reason)
    {
        var normalizedType = UnwrapNullableValueType(typeSymbol);
        if (normalizedType.IsTupleType)
        {
            reason = "Tuple keys/elements lower structurally, but native JS Map/Set only compares carrier identity.";
            return false;
        }

        if (normalizedType.TypeKind == TypeKind.Enum)
        {
            reason = string.Empty;
            return true;
        }

        if (normalizedType is IArrayTypeSymbol)
        {
            reason = string.Empty;
            return true;
        }

        switch (GetMapperType(normalizedType).Mapper)
        {
            case TypeMapper.Number:
            case TypeMapper.String:
            case TypeMapper.Boolean:
            case TypeMapper.BigInt:
                reason = string.Empty;
                return true;
        }

        switch (normalizedType)
        {
            case ITypeParameterSymbol:
                reason = "Type-parameter keys/elements are not statically known to have JS-stable default equality.";
                return false;

            case INamedTypeSymbol namedType when namedType.SpecialType == SpecialType.System_Object:
                reason = "object keys/elements are not statically bounded to a JS-stable default equality contract.";
                return false;

            case INamedTypeSymbol { TypeKind: TypeKind.Interface }:
                reason = "Interface-typed keys/elements are not statically known to have JS-stable default equality.";
                return false;

            case INamedTypeSymbol { TypeKind: TypeKind.Struct }:
                reason = "Struct keys/elements use CLR value equality, but their JS carriers are compared by identity under native Map/Set.";
                return false;

            case INamedTypeSymbol { TypeKind: TypeKind.Delegate }:
                reason = "Delegate equality is not modeled by native JS function identity.";
                return false;

            case INamedTypeSymbol { TypeKind: TypeKind.Class } namedType when HasCustomDefaultEqualitySemantics(namedType):
                reason = "Reference type keys/elements with record/custom equality semantics are not preserved by native JS Map/Set identity checks.";
                return false;

            case INamedTypeSymbol { TypeKind: TypeKind.Class }:
                reason = string.Empty;
                return true;
        }

        reason = "This key/element type does not map to a JS-stable default equality contract under native Map/Set.";
        return false;
    }

    private static ITypeSymbol UnwrapNullableValueType(ITypeSymbol typeSymbol)
        => typeSymbol is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            TypeArguments.Length: 1
        } nullableType
            ? nullableType.TypeArguments[0]
            : typeSymbol;

    private static bool HasCustomDefaultEqualitySemantics(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.IsRecord || ImplementsSelfIEquatable(typeSymbol))
            return true;

        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (current.IsRecord || OverridesObjectEquals(current) || OverridesObjectGetHashCode(current))
                return true;
        }

        return false;
    }

    private static bool ImplementsSelfIEquatable(INamedTypeSymbol typeSymbol)
        => typeSymbol.AllInterfaces.Any(iface =>
            iface.OriginalDefinition.ToDisplayString(Format.NameFormat) == "System.IEquatable<T>" &&
            iface.TypeArguments.Length == 1 &&
            SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], typeSymbol));

    private static bool OverridesObjectEquals(INamedTypeSymbol typeSymbol)
        => typeSymbol
            .GetMembers("Equals")
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.IsOverride &&
                method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_Object);

    private static bool OverridesObjectGetHashCode(INamedTypeSymbol typeSymbol)
        => typeSymbol
            .GetMembers("GetHashCode")
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.IsOverride &&
                method.Parameters.Length == 0);

    private bool IsStructuralType(ITypeSymbol? typeSymbol)
        => StructuralRecordSupport.IsStructuralType(typeSymbol, IsStructuralSourceDataCarrierType);

    private bool IsStructuralMember(ISymbol? symbol)
        => StructuralRecordSupport.IsStructuralMember(symbol, IsStructuralSourceDataCarrierType);

    private bool IsNonStructuralRuntimeMember(ISymbol? symbol, ITypeSymbol? hostType = null)
        => StructuralRecordSupport.IsNonStructuralRuntimeMember(symbol, hostType, IsStructuralSourceDataCarrierType);

    private bool IsStructuralRuntimeSemanticInvocation(IInvocationOperation? invocation)
        => StructuralRecordSupport.IsStructuralRuntimeSemanticInvocation(invocation, IsStructuralSourceDataCarrierType);

    private bool IsDirectlySupportedExternalType(IOperation operation, ITypeSymbol typeSymbol)
    {
        var original = typeSymbol.OriginalDefinition;
        if (original.TypeKind is TypeKind.Array or TypeKind.Delegate or TypeKind.TypeParameter ||
            typeSymbol.IsTupleType ||
            IsStructuralType(typeSymbol))
            return true;

        if (IsSymbolDeclaredInCurrentSourceBoundary(operation, typeSymbol))
            return true;

        if (HasEcmascriptSupportMarker(typeSymbol))
            return true;

        if (original is INamedTypeSymbol namedOriginal &&
            HasEcmascriptSupportMarkerBaseType(namedOriginal))
            return true;

        return TryGetWhiteListValue(WhiteList.Types, original.ToDisplayString(Format.NameFormat), out _, out _);
    }

    private static bool TryGetWhiteListTypeAlias(ITypeSymbol typeSymbol, out string runtimeAlias)
    {
        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (TryGetWhiteListValue(WhiteList.Types, displayName, out _, out var entry) &&
            entry.Op == Op.Alias &&
            !string.IsNullOrWhiteSpace(entry.Value))
        {
            runtimeAlias = entry.Value!;
            return true;
        }

        runtimeAlias = string.Empty;
        return false;
    }

    private static bool TryGetWhiteListRuntimeValueCarrier(
        ITypeSymbol typeSymbol,
        out RuntimeValueCarrierReference runtimeValueCarrier)
    {
        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (TryGetWhiteListValue(WhiteList.Types, displayName, out _, out var entry) &&
            entry.RuntimeValueCarrier is not null)
        {
            runtimeValueCarrier = entry.RuntimeValueCarrier;
            return true;
        }

        runtimeValueCarrier = null!;
        return false;
    }

    private List<string> FindIncompatibleWhiteListAliasTypes(IOperation operation, ITypeSymbol targetType, string runtimeAlias)
    {
        var conflicts = new List<string>();
        var targetDisplayName = targetType.OriginalDefinition.ToDisplayString(Format.NameFormat);
        var targetErasedDisplayName = EraseGenericDisplayArguments(targetDisplayName);
        var compilation = operation.SemanticModel?.Compilation;

        foreach (var pair in WhiteList.Types)
        {
            var candidateDisplayName = pair.Key;
            var entry = pair.Value;
            if (entry.Op != Op.Alias ||
                !string.Equals(entry.Value, runtimeAlias, StringComparison.Ordinal) ||
                string.Equals(candidateDisplayName, targetDisplayName, StringComparison.Ordinal))
                continue;

            if (string.Equals(EraseGenericDisplayArguments(candidateDisplayName), targetErasedDisplayName, StringComparison.Ordinal))
                continue;

            if (compilation is not null &&
                TryResolveWhiteListAliasType(compilation, candidateDisplayName) is ITypeSymbol candidateType &&
                IsRuntimeAliasAssignableToTarget(candidateType, targetType))
                continue;

            conflicts.Add(candidateDisplayName);
        }

        conflicts.Sort(StringComparer.Ordinal);
        return conflicts;
    }

    private static string EraseGenericDisplayArguments(string displayName)
    {
        if (displayName.IndexOf('<') < 0)
            return displayName;

        var builder = new StringBuilder(displayName.Length);
        var depth = 0;
        foreach (var ch in displayName)
        {
            if (ch == '<')
            {
                depth++;
                continue;
            }

            if (ch == '>')
            {
                if (depth > 0)
                    depth--;
                continue;
            }

            if (depth == 0)
                builder.Append(ch);
        }

        return builder.ToString();
    }

    private static ITypeSymbol? TryResolveWhiteListAliasType(Compilation compilation, string displayName)
    {
        return displayName switch
        {
            "bool" => compilation.GetSpecialType(SpecialType.System_Boolean),
            "byte" => compilation.GetSpecialType(SpecialType.System_Byte),
            "char" => compilation.GetSpecialType(SpecialType.System_Char),
            "decimal" => compilation.GetSpecialType(SpecialType.System_Decimal),
            "double" => compilation.GetSpecialType(SpecialType.System_Double),
            "float" => compilation.GetSpecialType(SpecialType.System_Single),
            "int" => compilation.GetSpecialType(SpecialType.System_Int32),
            "long" => compilation.GetSpecialType(SpecialType.System_Int64),
            "object" => compilation.GetSpecialType(SpecialType.System_Object),
            "sbyte" => compilation.GetSpecialType(SpecialType.System_SByte),
            "short" => compilation.GetSpecialType(SpecialType.System_Int16),
            "string" => compilation.GetSpecialType(SpecialType.System_String),
            "uint" => compilation.GetSpecialType(SpecialType.System_UInt32),
            "ulong" => compilation.GetSpecialType(SpecialType.System_UInt64),
            "ushort" => compilation.GetSpecialType(SpecialType.System_UInt16),
            _ => compilation.GetTypeByMetadataName(displayName)
        };
    }

    private static bool IsRuntimeAliasAssignableToTarget(ITypeSymbol candidateType, ITypeSymbol targetType)
    {
        if (SymbolEqualityComparer.Default.Equals(candidateType.OriginalDefinition, targetType.OriginalDefinition))
            return true;

        if (candidateType is not INamedTypeSymbol namedCandidate)
            return false;

        for (var current = namedCandidate.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, targetType.OriginalDefinition))
                return true;
        }

        foreach (var @interface in namedCandidate.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, targetType.OriginalDefinition))
                return true;
        }

        return false;
    }

    private bool IsSupportedExternalMember(IOperation operation, ISymbol symbol, ITypeSymbol? hostType = null)
    {
        if (IsStructuralMember(symbol))
            return true;

        if (IsStructuralRuntimeSemanticInvocation(operation as IInvocationOperation))
            return false;

        if (Util.IsECMAScriptRecordProxyMember(symbol, hostType))
            return true;

        if (IsNonStructuralRuntimeMember(symbol, hostType))
            return false;

        if (TryGetWhiteListValue(_whiteListCompiles, symbol, out _, out _))
            return true;

        if (TryGetWhiteListValue(WhiteList.Members, symbol, out _, out _))
            return true;

        if (IsSymbolDeclaredInCurrentSourceBoundary(operation, symbol))
            return true;

        if (symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol associatedProperty } &&
            IsUnsupportedUnionProjectionProperty(associatedProperty))
            return false;

        if (symbol is IPropertySymbol property &&
            IsUnsupportedUnionProjectionProperty(property))
            return false;

        if (HasEcmascriptSupportMarker(symbol))
            return true;

        if (symbol is IFieldSymbol field && IsIntrinsicFieldFallbackAllowed(field, hostType))
            return true;

        if ((symbol is IMethodSymbol or IPropertySymbol) && IsIntrinsicCallableOrPropertyFallbackAllowed(hostType))
            return true;

        return !RequiresExplicitExternalMemberSupport(operation, symbol, hostType);
    }

    private static bool HasEcmascriptSupportMarker(ISymbol symbol)
    {
        foreach (var candidate in EnumerateSupportMarkerCandidates(symbol))
        {
            for (ISymbol? current = candidate; current is not null; current = GetSupportContainingSymbol(current))
            {
                if (current.GetAttributes().Any(static attribute =>
                    Util.IsECMAScriptSupportMarkerAttribute(attribute.AttributeClass)))
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<ISymbol> EnumerateSupportMarkerCandidates(ISymbol symbol)
    {
        for (ISymbol? current = symbol.OriginalDefinition; current is not null; current = WhiteListLookup.GetFallbackSymbol(current))
            yield return current;
    }

    private static ISymbol? GetSupportContainingSymbol(ISymbol symbol)
        => symbol is ITypeSymbol typeSymbol ? typeSymbol.ContainingType : symbol.ContainingType;

    private static bool IsSymbolDeclaredInSource(ISymbol symbol)
    {
        foreach (var candidate in EnumerateSupportMarkerCandidates(symbol))
        {
            if (candidate.Locations.Any(static location => location.IsInSource))
                return true;
        }

        return false;
    }

    private bool IsSymbolDeclaredInCurrentSourceBoundary(IOperation operation, ISymbol symbol)
    {
        if (_moduleDeclaredNames is not null)
        {
            foreach (var candidate in EnumerateSupportMarkerCandidates(symbol))
            {
                if (_moduleDeclaredNames.ContainsKey(candidate.OriginalDefinition))
                    return true;
            }
        }

        var boundaryType = TryGetCurrentSourceBoundaryType(operation);
        if (boundaryType is null)
            return false;

        foreach (var candidate in EnumerateSupportMarkerCandidates(symbol))
        {
            if (!candidate.Locations.Any(static location => location.IsInSource))
                continue;

            if (IsSymbolWithinBoundary(candidate, boundaryType))
                return true;
        }

        return false;
    }

    private static INamedTypeSymbol? TryGetCurrentSourceBoundaryType(IOperation operation)
    {
        var semanticModel = operation.SemanticModel;
        if (semanticModel is null)
            return null;

        var enclosingSymbol = semanticModel.GetEnclosingSymbol(operation.Syntax.SpanStart);
        return GetTopMostContainingType(enclosingSymbol);
    }

    private static INamedTypeSymbol? GetTopMostContainingType(ISymbol? symbol)
    {
        var current = symbol as INamedTypeSymbol ?? symbol?.ContainingType;
        while (current?.ContainingType is INamedTypeSymbol containingType)
            current = containingType;

        return current;
    }

    private static bool IsSymbolWithinBoundary(ISymbol symbol, INamedTypeSymbol boundaryType)
    {
        var currentType = symbol as ITypeSymbol ?? symbol.ContainingType;
        while (currentType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentType.OriginalDefinition, boundaryType.OriginalDefinition))
                return true;

            currentType = currentType.ContainingType;
        }

        return false;
    }

    private bool RequiresExplicitExternalMemberSupport(IOperation operation, ISymbol symbol, ITypeSymbol? hostType)
    {
        var effectiveHost = hostType ?? symbol.ContainingType;
        if (effectiveHost is null)
            return false;

        if (_moduleDeclaredNames is not null &&
            _moduleDeclaredNames.ContainsKey(effectiveHost.OriginalDefinition))
        {
            return false;
        }

        if (IsSymbolDeclaredInCurrentSourceBoundary(operation, effectiveHost))
            return false;

        if (HasEcmascriptSupportMarker(effectiveHost))
            return false;

        if (effectiveHost is INamedTypeSymbol namedEffectiveHost &&
            HasEcmascriptSupportMarkerBaseType(namedEffectiveHost))
            return false;

        if (effectiveHost.IsAnonymousType)
            return false;

        var original = effectiveHost.OriginalDefinition;
        if (original.TypeKind is TypeKind.TypeParameter or TypeKind.Delegate)
            return false;

        return true;
    }

    private static bool IsIntrinsicFieldFallbackAllowed(IFieldSymbol field, ITypeSymbol? hostType)
    {
        var effectiveHost = hostType ?? field.ContainingType;
        if (effectiveHost is null)
            return false;

        if (field.IsConst && !IsConcreteMetadataInteropHost(effectiveHost))
            return true;

        return IsTupleLikeHost(effectiveHost);
    }

    private static bool IsIntrinsicCallableOrPropertyFallbackAllowed(ITypeSymbol? hostType)
    {
        if (hostType is null)
            return false;

        var original = hostType.OriginalDefinition;
        return original.TypeKind == TypeKind.TypeParameter ||
            original.TypeKind == TypeKind.Delegate;
    }

    private static bool IsConcreteMetadataInteropHost(ITypeSymbol typeSymbol)
    {
        var original = typeSymbol.OriginalDefinition;
        if (original.TypeKind == TypeKind.TypeParameter ||
            original.TypeKind == TypeKind.Array ||
            original.TypeKind == TypeKind.Delegate ||
            original.TypeKind == TypeKind.Enum ||
            original.IsAnonymousType ||
            original.SpecialType != SpecialType.None)
            return false;

        if (IsTupleLikeHost(original))
            return false;

        return true;
    }

    private static bool IsTupleLikeHost(ITypeSymbol typeSymbol)
    {
        var original = typeSymbol.OriginalDefinition;
        if (original.IsTupleType)
            return true;

        return original is INamedTypeSymbol namedType &&
            namedType.Name == "ValueTuple" &&
            namedType.ContainingNamespace?.ToDisplayString() == "System";
    }

    private bool IsPassThroughCustomOperatorFallbackAllowed(IMethodSymbol method)
    {
        if (IsNonStructuralRuntimeMember(method))
            return false;

        if (TryGetWhiteListValue(_whiteListCompiles, method, out _, out _))
            return true;

        if (TryGetWhiteListValue(WhiteList.Members, method, out _, out var entry))
            return entry.Op is Op.Allowed or Op.Alias;

        return HasEcmascriptSupportMarker(method);
    }

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
    /// - `Compile`：拿到原始 `symbol`、当前 `context`、实例 `handler`，以及只保留显式参数的 `args`
    /// - `Alias/Inline/Import`：继续沿用历史占位符布局，实例方法把宿主拼到参数前缀
    ///
    /// 这样既能把 `Compile` 接到主分发优先级前面，又不会一次性打坏既有模板和导入规则。
    /// </summary>
    private Expression? GetWhiteListExpressionCore(
        ISymbol symbol,
        SenseArgument context,
        List<Expression> arguments,
        Expression? instance,
        out string? alias,
        IOperation? originOperation,
        ITypeSymbol? hostType)
    {
        alias = null;
        if (IsStructuralRuntimeSemanticInvocation(originOperation as IInvocationOperation))
            return null;

        var effectiveHostType = hostType ?? TryGetRuntimeMemberHostType(originOperation);
        if (!Util.IsECMAScriptRecordProxyMember(symbol, effectiveHostType) &&
            IsNonStructuralRuntimeMember(
                symbol,
                effectiveHostType))
        {
            return null;
        }

        var compileExpr = TryGetCompileExpression(symbol, context, arguments, instance, originOperation);
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

    private static ITypeSymbol? TryGetRuntimeMemberHostType(IOperation? operation)
        => operation switch
        {
            IInvocationOperation invocation =>
                invocation.Instance?.Type ?? invocation.TargetMethod.ContainingType,
            IMethodReferenceOperation methodReference =>
                methodReference.Instance?.Type ?? methodReference.Method.ContainingType,
            IPropertyReferenceOperation propertyReference =>
                propertyReference.Instance?.Type ?? propertyReference.Property.ContainingType,
            IFieldReferenceOperation fieldReference =>
                fieldReference.Instance?.Type ?? fieldReference.Field.ContainingType,
            IObjectCreationOperation objectCreation =>
                objectCreation.Type,
            _ => null
        };

    private Expression? TryGetCompileExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, Expression? instance, IOperation? originOperation)
    {
        if (!TryGetWhiteListValue(_whiteListCompiles, symbol, out _, out var compile))
            return null;

        var (handler, explicitArgs) = CreateCompileArguments(symbol, arguments, instance);
        return compile(symbol, context, handler, explicitArgs, originOperation);
    }

    private static bool TryGetWhiteListValue<T>(Dictionary<string, T> mappings, string lookupKey, out string displayString, out T value)
        where T : notnull
        => WhiteListLookup.TryGetValue(mappings, lookupKey, out displayString, out value);

    private static bool TryGetWhiteListValue<T>(Dictionary<string, T> mappings, ISymbol symbol, out string displayString, out T value)
        where T : notnull
        => WhiteListLookup.TryGetValue(mappings, symbol, out displayString, out value);

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
    private readonly Dictionary<ITypeSymbol, bool> _structuralSourceDataCarrierTypeCache =
        new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IMethodSymbol, ImmutableArray<ISymbol>> _structuralSourceDataCarrierConstructorMemberMapCache =
        new(SymbolEqualityComparer.Default);
    private readonly HashSet<IMethodSymbol> _nonStructuralSourceDataCarrierConstructors =
        new(SymbolEqualityComparer.Default);

    private readonly Action<Location, string?>? _report;

    private readonly ITypeSymbol? _moduleRootType;

    private readonly IReadOnlyDictionary<ISymbol, string>? _moduleDeclaredNames;

    private readonly Dictionary<string, Func<ISymbol, SenseArgument, Expression?, Expression?[], IOperation?, Expression?>> _whiteListCompiles;

    private readonly CancellationToken _cancellationToken;

    private UniqueNameSession? _uniqueNameSession;

    public SemanticWalkerHost? Host { get; set; }

    public bool AllowStructuralSourceDataCarrierLowering { get; set; }

    public SemanticWalker()
    {
        _whiteListCompiles = [];
        Generate(ref _whiteListCompiles);
	}

    public SemanticWalker(CancellationToken cancellationToken) : this() => _cancellationToken = cancellationToken;

    public SemanticWalker(ITypeSymbol moduleRootType) : this() => _moduleRootType = moduleRootType;

    public SemanticWalker(ITypeSymbol moduleRootType, CancellationToken cancellationToken) : this(cancellationToken) => _moduleRootType = moduleRootType;

    public SemanticWalker(ITypeSymbol moduleRootType, IReadOnlyDictionary<ISymbol, string> moduleDeclaredNames) : this()
    {
        _moduleRootType = moduleRootType;
        _moduleDeclaredNames = moduleDeclaredNames;
    }

    public SemanticWalker(ITypeSymbol moduleRootType, IReadOnlyDictionary<ISymbol, string> moduleDeclaredNames, CancellationToken cancellationToken) : this(cancellationToken)
    {
        _moduleRootType = moduleRootType;
        _moduleDeclaredNames = moduleDeclaredNames;
    }

    public SemanticWalker(bool test) : this() => _test = test;

    public SemanticWalker(bool test, CancellationToken cancellationToken) : this(cancellationToken) => _test = test;

    public SemanticWalker(Action<Location, string?> report):this() => _report = report;

    public SemanticWalker(Action<Location, string?> report, CancellationToken cancellationToken) : this(cancellationToken) => _report = report;

    private bool IsStructuralSourceDataCarrierType(INamedTypeSymbol typeSymbol)
    {
        if (!AllowStructuralSourceDataCarrierLowering)
            return false;

        if (_structuralSourceDataCarrierTypeCache.TryGetValue(typeSymbol, out var cached))
            return cached;

        if (!TryGetStructuralSourceDataCarrierMemberOrder(typeSymbol, out _))
        {
            _structuralSourceDataCarrierTypeCache[typeSymbol] = false;
            return false;
        }

        _structuralSourceDataCarrierTypeCache[typeSymbol] = true;
        return true;
    }

    private bool TryGetStructuralSourceDataCarrierMemberOrder(
        INamedTypeSymbol typeSymbol,
        out ImmutableArray<ISymbol> members)
    {
        members = default;

        if (!AllowStructuralSourceDataCarrierLowering ||
            typeSymbol.IsRecord ||
            typeSymbol.IsAnonymousType ||
            typeSymbol.IsTupleType ||
            typeSymbol.TypeKind is not (TypeKind.Class or TypeKind.Struct) ||
            !typeSymbol.Locations.Any(static location => location.IsInSource))
        {
            return false;
        }

        if (typeSymbol.TypeParameters.Length != 0 ||
            typeSymbol.IsStatic ||
            typeSymbol.IsAbstract ||
            typeSymbol.IsRefLikeType)
        {
            return false;
        }

        if (typeSymbol.BaseType is INamedTypeSymbol baseType &&
            baseType.SpecialType != SpecialType.System_Object &&
            baseType.SpecialType != SpecialType.System_ValueType)
        {
            return false;
        }

        if (typeSymbol.AllInterfaces.Length != 0)
            return false;

        var orderedMembers = new List<ISymbol>();
		foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
		{
			if (property.IsStatic ||
				property.IsIndexer ||
				property.Parameters.Length != 0 ||
				property.IsAbstract ||
				!property.Locations.Any(static location => location.IsInSource))
			{
				return false;
			}

			if (property.SetMethod is not null &&
				!property.SetMethod.IsInitOnly &&
				property.SetMethod.DeclaredAccessibility != Accessibility.Private &&
				!StructuralRecordSupport.IsSourceDeclaredAutoPropertyCandidate(property))
			{
				return false;
			}

            if (property.GetMethod is null)
                return false;

            orderedMembers.Add(property);
        }

        foreach (var field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic)
                return false;

            if (field.AssociatedSymbol is not null)
                continue;

            if (!field.Locations.Any(static location => location.IsInSource))
                return false;

            if (!field.IsReadOnly)
                return false;

            orderedMembers.Add(field);
        }

        foreach (var member in typeSymbol.GetMembers())
        {
            switch (member)
            {
                case IMethodSymbol method:
                    if (method.MethodKind is MethodKind.Constructor or MethodKind.PropertyGet or MethodKind.PropertySet)
                        continue;

                    return false;
                case IEventSymbol:
                    return false;
            }
        }

        if (orderedMembers.Count == 0)
            return false;

        orderedMembers.Sort(static (left, right) =>
        {
            var leftStart = left.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue;
            var rightStart = right.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue;
            var byLocation = leftStart.CompareTo(rightStart);
            if (byLocation != 0)
                return byLocation;

            return string.Compare(left.Name, right.Name, StringComparison.Ordinal);
        });

        members = orderedMembers.ToImmutableArray();
        return true;
    }

    private bool TryGetStructuralSourceDataCarrierConstructorMemberMap(
        IMethodSymbol constructor,
        out ImmutableArray<ISymbol> memberMap)
    {
        if (_structuralSourceDataCarrierConstructorMemberMapCache.TryGetValue(constructor, out memberMap))
            return true;

        if (_nonStructuralSourceDataCarrierConstructors.Contains(constructor))
        {
            memberMap = default;
            return false;
        }

        if (constructor.MethodKind != MethodKind.Constructor ||
            constructor.ContainingType is not INamedTypeSymbol containingType ||
            !TryGetStructuralSourceDataCarrierMemberOrder(containingType, out var orderedMembers))
        {
            _nonStructuralSourceDataCarrierConstructors.Add(constructor);
            memberMap = default;
            return false;
        }

        if (constructor.Parameters.Length > orderedMembers.Length)
        {
            _nonStructuralSourceDataCarrierConstructors.Add(constructor);
            memberMap = default;
            return false;
        }

        var mappedMembers = ImmutableArray.CreateBuilder<ISymbol>(constructor.Parameters.Length);
        var usedMemberIndices = new HashSet<int>();
        foreach (var parameter in constructor.Parameters)
        {
            var parameterType = parameter.Type;
            var matchedIndex = -1;
            for (var index = 0; index < orderedMembers.Length; index++)
            {
                if (usedMemberIndices.Contains(index))
                    continue;

                var candidate = orderedMembers[index];
                if (!string.Equals(candidate.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                    continue;

                var candidateType = candidate switch
                {
                    IPropertySymbol property => property.Type,
                    IFieldSymbol field => field.Type,
                    _ => null
                };
                if (candidateType is null ||
                    !SymbolEqualityComparer.Default.Equals(candidateType, parameterType))
                {
                    continue;
                }

                matchedIndex = index;
                break;
            }

            if (matchedIndex < 0)
            {
                _nonStructuralSourceDataCarrierConstructors.Add(constructor);
                memberMap = default;
                return false;
            }

            usedMemberIndices.Add(matchedIndex);
            mappedMembers.Add(orderedMembers[matchedIndex]);
        }

        memberMap = mappedMembers.ToImmutable();
        _structuralSourceDataCarrierConstructorMemberMapCache[constructor] = memberMap;
        return true;
    }

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

		var lineSpan = location.GetMappedLineSpan();
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

	partial void Generate(ref Dictionary<string, Func<ISymbol, SenseArgument, Expression?, Expression?[], IOperation?, Expression?>> funcs);

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
        _cancellationToken.ThrowIfCancellationRequested();

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

            case LoweringSiteKind.LockValueTemp:
                builder.Append("lock|").Append(site.Slot).Append('|');
                builder.Append(BuildSemanticNameKey(operation));
                break;

            case LoweringSiteKind.UsingResourceTemp:
                builder.Append("using|").Append(site.Slot).Append('|');
                builder.Append(BuildSemanticNameKey(operation));
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

            case LoweringSiteKind.PatternInputCache:
                builder.Append("patcache|").Append(site.Slot).Append('|');
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

    private static OperationTransformationException CreateOperationTransformationException(IOperation operation, string? message)
    {
        var exception = new OperationTransformationException(operation.Kind, message);
        AttachLocationMetadata(exception, operation.Syntax.GetLocation());
        return exception;
    }

    private static SyntaxNodeTransformationException CreateSyntaxNodeTransformationException(SyntaxNode node, string? message)
    {
        var exception = new SyntaxNodeTransformationException(node.Kind(), message);
        AttachLocationMetadata(exception, node.GetLocation());
        return exception;
    }

    private static void AttachLocationMetadata(Exception exception, Location? location)
    {
        if (location is null)
        {
            exception.Data["location.path"] = "<unknown>";
            return;
        }

        var lineSpan = location.GetLineSpan();
        var path = !string.IsNullOrWhiteSpace(lineSpan.Path)
            ? lineSpan.Path
            : location.SourceTree?.FilePath;
        if (string.IsNullOrWhiteSpace(path))
            path = "<unknown>";

        exception.Data["location.path"] = path;
        exception.Data["location.startLine"] = lineSpan.StartLinePosition.Line + 1;
        exception.Data["location.startColumn"] = lineSpan.StartLinePosition.Character + 1;
        exception.Data["location.endLine"] = lineSpan.EndLinePosition.Line + 1;
        exception.Data["location.endColumn"] = lineSpan.EndLinePosition.Character + 1;
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
        throw CreateOperationTransformationException(operation, message);
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
        throw CreateSyntaxNodeTransformationException(node, message);
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

        throw CreateOperationTransformationException(operation, message);
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

        throw CreateOperationTransformationException(operation, message);
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
