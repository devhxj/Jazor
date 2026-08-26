// File: Util.cs
// Purpose: Holds shared symbol, naming, type, and syntax utilities for compiler lowering.
// 只收纳跨多个 lowering 文件的纯辅助逻辑；具体 C# 语义应保留在 SemanticWalker 分部文件中。
using System;
using System.IO;
using System.Linq;
using Acornima;
using Acornima.Ast;
using ECMAScript;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.Compiler;

/// <summary>
/// 编译器共享的符号、名称和 AST 辅助工具。
///
/// 这里的规则大多服务于“C# 声明侧”到“JS 运行时侧”的名称与宿主对齐，
/// 因此很多 helper 不只是字符串工具，而是运行时映射判定的一部分。
/// </summary>
public static class Util
{
    public const string ECMAScriptAttributeMetadataName = "ECMAScript.ECMAScriptAttribute";
    public const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    public const string ECMAScriptInlineAttributeMetadataName = "ECMAScript.ECMAScriptInlineAttribute";
    public const string SystemUnionAttributeMetadataName = "System.Runtime.CompilerServices.UnionAttribute";
    public const string SystemIUnionMetadataName = "System.Runtime.CompilerServices.IUnion";
    public const string StringAttributeMetadataName = "ECMAScript.StringAttribute";

    internal static bool IsBodylessInitAccessor(IMethodSymbol method)
    {
        var isInitOnly = method.IsInitOnly;
        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not AccessorDeclarationSyntax accessor ||
                !accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
            {
                continue;
            }

            isInitOnly = true;
            if (accessor.Body is not null || accessor.ExpressionBody is not null)
                return false;
        }

        return isInitOnly;
    }

    private enum JsNameConfigKind
    {
        None,
        Stop,
        Explicit,
    }

    private readonly record struct JsNameConfig(JsNameConfigKind Kind, string? Name)
    {
        public static JsNameConfig None => new(JsNameConfigKind.None, null);
        public static JsNameConfig Stop => new(JsNameConfigKind.Stop, null);
        public static JsNameConfig Explicit(string name) => new(JsNameConfigKind.Explicit, name);
    }

    /// <summary>
    /// Raw JavaScript-name metadata found on one Roslyn symbol.
    ///
    /// This is deliberately separate from the effective name: <c>ECMAScriptName</c> has
    /// precedence over <c>Description("@#...")</c>, while the analyzer still needs to see
    /// both authored values in order to diagnose contradictory metadata.
    /// </summary>
    internal readonly record struct JavaScriptNameMetadata(
        bool HasECMAScriptNameAttribute,
        string? ECMAScriptName,
        string? DescriptionName,
        bool HasDescriptionBoundary)
    {
        public bool HasConflictingExplicitNames
            => HasECMAScriptNameAttribute &&
               !string.IsNullOrEmpty(ECMAScriptName) &&
               !string.IsNullOrEmpty(DescriptionName) &&
               !string.Equals(ECMAScriptName, DescriptionName, StringComparison.Ordinal);
    }

    /// <summary>
    /// 以仓库测试使用的 KnR 风格输出 ECMAScript 文本。
    /// </summary>
    public static string ToKnRECMAScript(this Node node)
    {
        using var writer = new LfStringWriter();
        AstToJavaScript.WriteJavaScript(node, writer, KnRJavaScriptTextFormatterOptions.Default, AstToJavaScriptOptions.Default);
        return writer.ToString();
    }

    /// <summary>
    /// 以仓库测试使用的 KnR 风格输出 ECMAScript 文本与 source map。
    /// </summary>
    /// <param name="node">待输出的 AST 根节点。</param>
    /// <param name="generatedFileName">source map 的 file 字段（通常为模块文件名）。</param>
    /// <param name="includeSourcesContent">是否内嵌 sourcesContent。</param>
    /// <param name="sourceRootPath">用于把绝对路径归一化为相对路径的根目录。</param>
    /// <param name="readSourceContent">可选源码读取回调；未提供时不会内嵌 sourcesContent。</param>
    public static GeneratedJavaScriptArtifact ToKnRECMAScriptWithSourceMap(
        this Node node,
        string generatedFileName = "module.mjs",
        bool includeSourcesContent = true,
        string? sourceRootPath = null,
        Func<string, string?>? readSourceContent = null)
        => SourceMapEmitter.Emit(
            node,
            KnRJavaScriptTextFormatterOptions.Default,
            AstToJavaScriptOptions.Default,
            generatedFileName,
            includeSourcesContent,
            sourceRootPath,
            readSourceContent);

    /// <summary>
    /// Emits a source map together with deterministic AST node positions for integrations that
    /// project compiler-generated nodes into a larger artifact.
    /// </summary>
    public static GeneratedJavaScriptLayout ToKnRECMAScriptWithSourceMapAndNodePositions(
        this Node node,
        string generatedFileName = "module.mjs",
        bool includeSourcesContent = true,
        string? sourceRootPath = null,
        Func<string, string?>? readSourceContent = null)
        => SourceMapEmitter.EmitWithNodePositions(
            node,
            KnRJavaScriptTextFormatterOptions.Default,
            AstToJavaScriptOptions.Default,
            generatedFileName,
            includeSourcesContent,
            sourceRootPath,
            readSourceContent);

    /// <summary>
    /// Emits JavaScript text together with deterministic AST node positions.
    /// </summary>
    public static GeneratedJavaScriptNodeLayout ToKnRECMAScriptWithNodePositions(this Node node)
        => SourceMapEmitter.EmitNodeLayout(
            node,
            KnRJavaScriptTextFormatterOptions.Default,
            AstToJavaScriptOptions.Default);

    /// <summary>
    /// 以默认 writer 选项输出 ECMAScript 文本。
    /// </summary>
    public static string ToECMAScript(this Node node)
    {
        using var writer = new LfStringWriter();
        AstToJavaScript.WriteJavaScript(node, writer, JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
        return writer.ToString();
    }

    /// <summary>
    /// 统一将文本内容归一化为 LF，避免生成产物随运行平台漂移。
    /// </summary>
    public static string NormalizeLineEndingsToLf(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Length == 0)
            return text;

        return text.IndexOf('\r') < 0
            ? text
            : text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private sealed class LfStringWriter : StringWriter
    {
        public LfStringWriter()
            => NewLine = "\n";
    }

    /// <summary>
    /// 以默认 writer 选项输出 ECMAScript 文本与 source map。
    /// </summary>
    /// <param name="node">待输出的 AST 根节点。</param>
    /// <param name="generatedFileName">source map 的 file 字段（通常为模块文件名）。</param>
    /// <param name="includeSourcesContent">是否内嵌 sourcesContent。</param>
    /// <param name="sourceRootPath">用于把绝对路径归一化为相对路径的根目录。</param>
    /// <param name="readSourceContent">可选源码读取回调；未提供时不会内嵌 sourcesContent。</param>
    public static GeneratedJavaScriptArtifact ToECMAScriptWithSourceMap(
        this Node node,
        string generatedFileName = "module.js",
        bool includeSourcesContent = true,
        string? sourceRootPath = null,
        Func<string, string?>? readSourceContent = null)
        => SourceMapEmitter.Emit(
            node,
            JavaScriptTextWriterOptions.Default,
            AstToJavaScriptOptions.Default,
            generatedFileName,
            includeSourcesContent,
            sourceRootPath,
            readSourceContent);

    /// <summary>
    /// 获取 ISymbol 显式配置的 JavaScript 名称。
    /// 优先级：
    /// 1. ECMAScriptNameAttribute
    /// 2. DescriptionAttribute (仅当值为 @#name 时)
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static string? GetSymbolConfigName(ISymbol symbol)
        => GetSymbolNameConfig(symbol) is { Kind: JsNameConfigKind.Explicit, Name: var name } ? name : null;

    /// <summary>
    /// 判断当前符号是否声明了名称解析边界（<c>@#</c>）。
    /// 一旦命中该边界，当前符号及其外层宿主都不再继续参与名称拼接。
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static bool HasNameResolutionBoundary(ISymbol symbol)
        => GetSymbolNameConfig(symbol).Kind == JsNameConfigKind.Stop;

    /// <summary>
    /// Resolves the producer-side CLR runtime mapping declared by <c>Jazor(Op.Import, ...)</c>.
    /// Import values name physical module exports; they are not general symbol-name metadata and
    /// must not be inferred from C# casing or confused with <c>Op.Alias</c> member access.
    /// </summary>
    internal static bool TryGetJazorImportRuntimeName(ISymbol symbol, out string runtimeName)
    {
        if (!TryGetJazorImportMapping(symbol, out _, out runtimeName))
        {
            runtimeName = string.Empty;
            return false;
        }

        return runtimeName.Length > 0;
    }

    internal static bool TryGetJazorImportMapping(
        ISymbol symbol,
        out string memberName,
        out string runtimeName)
    {
        memberName = string.Empty;
        runtimeName = string.Empty;

        var annotatedSymbol = symbol is IMethodSymbol method
            ? method.AssociatedSymbol ?? symbol
            : symbol;
        foreach (var attribute in annotatedSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name != "JazorAttribute" ||
                attribute.ConstructorArguments.Length < 2 ||
                attribute.ConstructorArguments[0].Value is null ||
                Convert.ToInt32(attribute.ConstructorArguments[0].Value) != (int)Op.Import ||
                attribute.ConstructorArguments[1].Value is not string { Length: > 0 } authoredMemberName)
            {
                continue;
            }

            memberName = authoredMemberName;
            if (attribute.ConstructorArguments.Length > 2 &&
                attribute.ConstructorArguments[2].Value is string { Length: > 0 } authoredRuntimeName)
            {
                runtimeName = authoredRuntimeName;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Reads both supported authored-name attributes without applying precedence.
    ///
    /// The effective resolver below intentionally keeps the historical rule that a blank/null
    /// <c>ECMAScriptName</c> suppresses <c>Description</c>, and that <c>Description("@#")</c>
    /// is a boundary rather than a concrete name. Keeping the raw read here gives compiler and
    /// analyzer one source of truth while still allowing diagnostics to explain the conflict.
    /// </summary>
    internal static JavaScriptNameMetadata GetJavaScriptNameMetadata(ISymbol symbol)
    {
        var hasECMAScriptName = false;
        string? ecmaScriptName = null;
        string? descriptionName = null;
        var hasDescriptionBoundary = false;

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.ConstructorArguments.Length == 0)
                continue;

            var attributeName = attribute.AttributeClass?.Name;
            if (attributeName == "ECMAScriptNameAttribute")
            {
                // AttributeUsage disallows duplicates, but keeping the first value preserves the
                // old resolver's source-order behavior for invalid/recovered Roslyn symbols.
                if (!hasECMAScriptName)
                {
                    hasECMAScriptName = true;
                    ecmaScriptName = ((string?)attribute.ConstructorArguments[0].Value)?.Trim();
                }

                continue;
            }

            if (attributeName != "DescriptionAttribute")
                continue;

            var description = ((string?)attribute.ConstructorArguments[0].Value)?.Trim();
            if (description?.StartsWith("@#", StringComparison.Ordinal) != true)
                continue;

            var name = description.Substring(2);
            if (name.Length == 0)
            {
                descriptionName = null;
                hasDescriptionBoundary = true;
            }
            else
            {
                descriptionName = name;
                hasDescriptionBoundary = false;
            }
        }

        return new JavaScriptNameMetadata(
            hasECMAScriptName,
            ecmaScriptName,
            descriptionName,
            hasDescriptionBoundary);
    }

    private static JsNameConfig GetSymbolNameConfig(ISymbol symbol)
    {
        var metadata = GetJavaScriptNameMetadata(symbol);
        if (metadata.HasECMAScriptNameAttribute)
        {
            return string.IsNullOrEmpty(metadata.ECMAScriptName)
                ? JsNameConfig.None
                : JsNameConfig.Explicit(metadata.ECMAScriptName!);
        }

        if (metadata.HasDescriptionBoundary)
            return JsNameConfig.Stop;

        return string.IsNullOrEmpty(metadata.DescriptionName)
            ? JsNameConfig.None
            : JsNameConfig.Explicit(metadata.DescriptionName!);
    }

    /// <summary>
    /// 获取最终用于输出的符号名。
    ///
    /// 规则顺序：
    /// 1. 优先使用显式配置名。
    /// 2. Roslyn 隐式 backing field 改写为稳定哈希名，避免泄漏 CLR 内部格式。
    /// 3. 普通方法仅在确实存在重载时追加哈希后缀。
    /// </summary>
    public static string GetConfigOrSymbolName(ISymbol symbol)
    {
        var name = GetSymbolConfigName(symbol);
        if (!string.IsNullOrEmpty(name))
            return name!;

        // Roslyn 生成的自动属性 backing field 形如 <PropName>k__BackingField，
        // 这里需要收敛成稳定哈希名；但不能把所有隐式字段都当成 backing field，
        // 例如 tuple 元素字段同样是隐式声明，仍应保留其运行时成员名。
        string? tupleFallbackName = null;
        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsImplicitlyDeclared)
        {
            if (fieldSymbol.AssociatedSymbol is IPropertySymbol propertySymbol)
            {
                var displayString = propertySymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
                return Format.HashName(displayString);
            }

            if (fieldSymbol.CorrespondingTupleField is IFieldSymbol tupleField &&
                !SymbolEqualityComparer.Default.Equals(tupleField, fieldSymbol))
                tupleFallbackName = tupleField.Name;
        }

        return AppendMethodOverloadSuffixIfNeeded(symbol, tupleFallbackName ?? symbol.Name);
    }

    public static string GetMemberConstructorHelperName(IMethodSymbol symbol)
        => $"$ctor_{Format.HashName(symbol.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_')}";

    /// <summary>
    /// Gets the stable JavaScript helper name for a runtime-class indexer accessor.
    /// JavaScript has no class indexer declaration syntax, so both declaration and use sites
    /// bind to the Roslyn accessor symbol instead of attempting a raw <c>instance[index]</c> fallback.
    /// </summary>
    public static string GetMemberIndexerAccessorHelperName(IMethodSymbol symbol)
    {
        var prefix = symbol.MethodKind switch
        {
            MethodKind.PropertyGet => "$get_",
            MethodKind.PropertySet => "$set_",
            _ => throw new ArgumentException("A runtime-class indexer helper requires a property getter or setter accessor.", nameof(symbol))
        };

        return prefix + Format.HashName(symbol.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_');
    }

    private static string AppendMethodOverloadSuffixIfNeeded(ISymbol symbol, string name)
    {
        if (symbol is IMethodSymbol methodSymbol)
        {
            if (ShouldSkipMethodOverloadSuffix(methodSymbol))
                return name;

            // A module may intentionally expose one raw overload beside explicitly named
            // overloads (for example style(...) + [ECMAScriptName("styleIn")]). Only the
            // unconfigured overloads compete for the raw C# name; adding a hash merely because
            // an explicitly renamed sibling exists would change a stable public entry point.
            var overloads = methodSymbol.ContainingType?.GetMembers(methodSymbol.Name)
                .OfType<IMethodSymbol>()
                .ToArray();
            var hasOverloadCollision = overloads is { Length: > 1 };
            if (hasOverloadCollision &&
                IsECMAScriptModuleType(methodSymbol.ContainingType))
            {
                hasOverloadCollision = overloads!.Count(static method => GetSymbolConfigName(method) is null) > 1;
            }

            if (hasOverloadCollision)
            {
                var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
                return $"{name}{Format.HashName(displayString)}";
            }
        }

        return name;
    }

    public static bool IsECMAScriptSupportMarkerAttribute(INamedTypeSymbol? symbol)
        => symbol?.ToDisplayString() is ECMAScriptAttributeMetadataName or ECMAScriptModuleAttributeMetadataName;

    public static bool IsECMAScriptSupportMarkerAttributeData(AttributeData? attribute)
    {
        if (attribute?.AttributeClass is not { } attributeClass)
            return false;

        var metadataName = attributeClass.ToDisplayString();
        if (metadataName == ECMAScriptModuleAttributeMetadataName)
            return true;
        if (metadataName != ECMAScriptAttributeMetadataName)
            return false;

        if (attribute.ConstructorArguments.Length >= 2 &&
            attribute.ConstructorArguments[1].Value is int transform)
        {
            return transform is (int)Transform.Allow or (int)Transform.Import;
        }

        return true;
    }

    internal static string? GetECMAScriptModuleImportPath(ITypeSymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            var metadataName = attribute.AttributeClass?.ToDisplayString();
            if (metadataName == ECMAScriptAttributeMetadataName)
            {
                if (attribute.ConstructorArguments.Length == 0 ||
                    attribute.ConstructorArguments[0].Value is not string externalPath ||
                    string.IsNullOrWhiteSpace(externalPath))
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length >= 2 &&
                    attribute.ConstructorArguments[1].Value is int transform)
                {
                    if (transform == (int)Transform.Component)
                        continue;
                    if (transform != (int)Transform.Import)
                    {
                        throw new NotSupportedException(
                            $"ECMAScript transform value '{transform}' is not supported for module imports.");
                    }
                }

                return ECMAScriptModulePath.ValidateExternalImportSpecifier(externalPath);
            }

            if (metadataName != ECMAScriptModuleAttributeMetadataName ||
                attribute.ConstructorArguments.Length != 1)
            {
                continue;
            }

            var importArgument = attribute.ConstructorArguments[0];
            if (importArgument.Kind == TypedConstantKind.Array ||
                importArgument.Value is not string importPath ||
                string.IsNullOrWhiteSpace(importPath))
            {
                continue;
            }

            return ECMAScriptModulePath.NormalizeImportSpecifier(importPath);
        }

        return null;
    }

    internal static bool IsExternalECMAScriptImport(ITypeSymbol symbol)
    {
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != ECMAScriptAttributeMetadataName ||
                    attribute.ConstructorArguments.Length == 0 ||
                    attribute.ConstructorArguments[0].Value is not string importSpecifier ||
                    string.IsNullOrWhiteSpace(importSpecifier))
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length < 2)
                    return true;

                return attribute.ConstructorArguments[1].Value is int transform &&
                       transform == (int)Transform.Import;
            }
        }

        return false;
    }

    public static bool HasECMAScriptSupportMarker(ISymbol? symbol)
    {
        if (symbol is null)
            return false;

        for (ISymbol? candidate = symbol.OriginalDefinition; candidate is not null; candidate = WhiteListLookup.GetFallbackSymbol(candidate))
        {
            for (ISymbol? current = candidate; current is not null; current = GetSupportContainingSymbol(current))
            {
                if (current.GetAttributes().Any(static attribute =>
                    IsECMAScriptSupportMarkerAttributeData(attribute)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static ISymbol? GetSupportContainingSymbol(ISymbol symbol)
        => symbol is ITypeSymbol typeSymbol ? typeSymbol.ContainingType : symbol.ContainingType;

    public static bool HasECMAScriptSupportMarkerBaseType(INamedTypeSymbol typeSymbol)
    {
        for (var current = typeSymbol.BaseType; current is not null; current = current.BaseType)
        {
            if (HasECMAScriptSupportMarker(current))
                return true;
        }

        return false;
    }

    public static bool IsObjectLiteralHostType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        if (StructuralRecordSupport.IsStructuralRecordType(namedType))
            return true;

        if (!HasECMAScriptSupportMarker(namedType) &&
            !HasECMAScriptSupportMarkerBaseType(namedType))
        {
            return false;
        }

        return HasNameResolutionBoundary(namedType);
    }

    public static bool IsECMAScriptRecordProxyMember(ISymbol? symbol, ITypeSymbol? hostType = null)
    {
        if (symbol is null)
            return false;

        var effectiveHost = hostType ?? symbol.ContainingType;
        if (!StructuralRecordSupport.IsStructuralRecordType(effectiveHost))
            return false;

        if (!HasDirectECMAScriptSupportMarker(effectiveHost) &&
            (effectiveHost is not INamedTypeSymbol namedHost ||
             !HasDirectECMAScriptSupportMarkerBaseType(namedHost)))
        {
            return false;
        }

        return symbol switch
        {
            IPropertySymbol property => IsECMAScriptRecordProxyProperty(property),
            IMethodSymbol { AssociatedSymbol: IPropertySymbol property } => IsECMAScriptRecordProxyProperty(property),
            IMethodSymbol method => IsECMAScriptRecordProxyMethod(method),
            _ => false
        };
    }

    private static bool HasDirectECMAScriptSupportMarker(ISymbol? symbol)
    {
        var original = symbol?.OriginalDefinition;
        return original is not null &&
               original.GetAttributes().Any(static attribute =>
                   IsECMAScriptSupportMarkerAttributeData(attribute));
    }

    private static bool HasDirectECMAScriptSupportMarkerBaseType(INamedTypeSymbol typeSymbol)
    {
        for (var current = typeSymbol.BaseType; current is not null; current = current.BaseType)
        {
            if (HasDirectECMAScriptSupportMarker(current))
                return true;
        }

        return false;
    }

    private static bool IsECMAScriptRecordProxyProperty(IPropertySymbol property)
        => property.IsIndexer ||
           property.Parameters.Length > 0 ||
           GetSymbolConfigName(property) is not null ||
           property.GetMethod?.IsExtern == true ||
           property.SetMethod?.IsExtern == true;

    private static bool IsECMAScriptRecordProxyMethod(IMethodSymbol method)
        => method.MethodKind == MethodKind.Ordinary &&
           (method.IsExtern ||
            GetSymbolConfigName(method) is not null ||
            HasECMAScriptInlineTemplate(method));

    private static bool HasECMAScriptInlineTemplate(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != ECMAScriptInlineAttributeMetadataName)
                continue;

            // NuGet metadata does not reliably preserve IsExtern。inline attribute 才是实际的
            // lowering contract，因此仅为带有效模板的成员保留此 narrow proxy allowance。
            var template = attribute.ConstructorArguments.FirstOrDefault().Value as string;
            return !string.IsNullOrWhiteSpace(template);
        }

        return false;
    }

    public static bool IsSystemUnionMarkerAttribute(INamedTypeSymbol? symbol)
        => symbol?.ToDisplayString() == SystemUnionAttributeMetadataName;

    public static bool IsRuntimeIUnionType(INamedTypeSymbol? symbol)
    {
        if (symbol is null)
            return false;

        return symbol.AllInterfaces.Any(@interface =>
            @interface.OriginalDefinition.ToDisplayString(Format.NameFormat) == SystemIUnionMetadataName);
    }

    public static bool IsStringEnumMarkerAttribute(INamedTypeSymbol? symbol)
        => symbol?.ToDisplayString() == StringAttributeMetadataName;

    public static bool IsStringEnumType(ITypeSymbol? symbol)
        => symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum } namedType &&
           namedType.GetAttributes().Any(attribute => IsStringEnumMarkerAttribute(attribute.AttributeClass));

    public static bool IsSystemUnionType(INamedTypeSymbol? symbol)
    {
        if (symbol is null)
            return false;

        return symbol.GetAttributes().Any(attr => IsSystemUnionMarkerAttribute(attr.AttributeClass)) ||
            IsNativeUnionSyntaxType(symbol);
    }

    private static bool IsNativeUnionSyntaxType(INamedTypeSymbol symbol)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var kind = reference.GetSyntax().Kind().ToString();
            if (string.Equals(kind, "UnionDeclaration", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static bool IsHostErasedUnionType(INamedTypeSymbol? symbol)
    {
        if (symbol is null)
            return false;

        return IsSystemUnionType(symbol) &&
            IsECMAScriptRuntimeType(symbol) &&
            IsRuntimeIUnionType(symbol);
    }

    private static bool IsRuntimeMarkerType(ISymbol? symbol)
        => symbol?.GetAttributes().Any(static attribute =>
            attribute.AttributeClass?.ToDisplayString() == ECMAScriptAttributeMetadataName) == true;

    /// <summary>
    /// 判断一个类型是否属于 ECMAScript 运行时映射类型。
    ///
    /// 这里同时要求：
    /// - 类型自身带有运行时标记特性
    ///
    /// 这样可以避免仅凭程序集名或类型名误判普通 CLR 类型，并允许外部库
    /// 通过同一标记机制参与运行时映射。
    /// </summary>
    public static bool IsECMAScriptRuntimeType(ITypeSymbol? symbol)
        => HasECMAScriptSupportMarker(symbol);

    /// <summary>
    /// 判断一个符号是否属于 ECMAScript 运行时映射域。
    ///
    /// 对成员符号，这里不是检查成员自己是否带标记，而是检查其宿主类型是否是
    /// ECMAScript 运行时类型。这样字段、属性、方法可以共享同一套运行时宿主规则，
    /// 例如静态宿主选择、方法名后缀跳过、全局对象映射等。
    /// </summary>
    public static bool IsECMAScriptRuntimeSymbol(ISymbol? symbol)
        => symbol switch
        {
            null => false,
            ITypeSymbol typeSymbol => IsECMAScriptRuntimeType(typeSymbol),
            _ => IsECMAScriptRuntimeType(symbol.ContainingType)
        };

    /// <summary>
    /// ECMAScript 运行时宿主上的方法名默认直接视为运行时 API 名称，不再追加重载哈希。
    ///
    /// 原因是这些宿主最终对齐的是 JS 运行时对象，而不是 CLR 的 overload surface。
    /// 如果在这里追加哈希后缀，会无端扩大 C# / JS 的命名割裂。
    /// </summary>
    private static bool ShouldSkipMethodOverloadSuffix(IMethodSymbol methodSymbol)
        => IsRuntimeMarkerType(methodSymbol.ContainingType);

    private static bool IsECMAScriptModuleType(ITypeSymbol? symbol)
        => symbol?.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() == ECMAScriptModuleAttributeMetadataName) == true;
}
