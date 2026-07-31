using System;
using System.IO;
using System.Linq;
using Acornima;
using Acornima.Ast;
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

    private static JsNameConfig GetSymbolNameConfig(ISymbol symbol)
    {
        // todo:属性的别名如何处理（因为存在get、set）
        var useDescription = true;
        string? configName = null;
        JsNameConfig description = JsNameConfig.None;
        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.ConstructorArguments.Length == 0)
                continue;

            //ECMAScriptNameAttribute 优先级最高，找到后直接返回
            if (attr.AttributeClass?.Name == "ECMAScriptNameAttribute")
            {
                useDescription = false;
                configName = attr.ConstructorArguments[0].Value?.ToString()?.Trim();
                break;
            }
            else if (attr.AttributeClass?.Name == "DescriptionAttribute")
            {
                var desc = attr.ConstructorArguments[0].Value?.ToString()?.Trim();
                if (desc?.StartsWith("@#") == true)
                {
                    var name = desc.Substring(2);
                    description = string.IsNullOrEmpty(name)
                        ? JsNameConfig.Stop
                        : JsNameConfig.Explicit(name);
                }
            }
        }

        if (!useDescription)
            return string.IsNullOrEmpty(configName)
                ? JsNameConfig.None
                : JsNameConfig.Explicit(configName!);

        return description;
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

        var fallbackName = tupleFallbackName ?? symbol.Name;
        if (ShouldUseJsMemberNamingFallback(symbol))
            fallbackName = ConvertPascalCaseIdentifierToJsNaming(fallbackName);

        return AppendMethodOverloadSuffixIfNeeded(symbol, fallbackName);
    }

    public static string GetMemberConstructorHelperName(IMethodSymbol symbol)
        => $"$ctor_{Format.HashName(symbol.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_')}";

    private static bool ShouldUseJsMemberNamingFallback(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol methodSymbol => methodSymbol.MethodKind is not MethodKind.LocalFunction
                and not MethodKind.AnonymousFunction
                and not MethodKind.LambdaMethod,
            IPropertySymbol or IFieldSymbol or IEventSymbol => true,
            _ => false,
        };

    internal static string ConvertPascalCaseIdentifierToJsNaming(string name)
    {
        if (string.IsNullOrEmpty(name) ||
            !char.IsUpper(name[0]))
            return name;

        if (name.Length == 1)
            return char.ToLowerInvariant(name[0]).ToString();

        var chars = name.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);

        for (var index = 1; index < chars.Length; index++)
        {
            if (!char.IsUpper(chars[index]))
                break;

            var hasNext = index + 1 < chars.Length;
            if (hasNext && !char.IsUpper(chars[index + 1]))
                break;

            chars[index] = char.ToLowerInvariant(chars[index]);
        }

        return new string(chars);
    }

    private static string AppendMethodOverloadSuffixIfNeeded(ISymbol symbol, string name)
    {
        if (symbol is IMethodSymbol methodSymbol)
        {
            if (ShouldSkipMethodOverloadSuffix(methodSymbol))
                return name;

            // 需要判断是否存在方法重载
            if (methodSymbol.ContainingType is not null &&
                methodSymbol.ContainingType.GetMembers(methodSymbol.Name)
                .Count(m => m.Kind == SymbolKind.Method) > 1)
            {
                var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
                return $"{name}{Format.HashName(displayString)}";
            }
        }

        return name;
    }

    public static bool IsECMAScriptSupportMarkerAttribute(INamedTypeSymbol? symbol)
        => symbol?.ToDisplayString() is ECMAScriptAttributeMetadataName or ECMAScriptModuleAttributeMetadataName;

    internal static string? GetECMAScriptModuleImportPath(ITypeSymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (!IsECMAScriptSupportMarkerAttribute(attribute.AttributeClass) ||
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

    public static bool HasECMAScriptSupportMarker(ISymbol? symbol)
    {
        if (symbol is null)
            return false;

        for (ISymbol? candidate = symbol.OriginalDefinition; candidate is not null; candidate = WhiteListLookup.GetFallbackSymbol(candidate))
        {
            for (ISymbol? current = candidate; current is not null; current = GetSupportContainingSymbol(current))
            {
                if (current.GetAttributes().Any(static attribute =>
                    IsECMAScriptSupportMarkerAttribute(attribute.AttributeClass)))
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
                   IsECMAScriptSupportMarkerAttribute(attribute.AttributeClass));
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
           (method.IsExtern || GetSymbolConfigName(method) is not null);

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
        => symbol?.GetAttributes().Any(attr => IsECMAScriptSupportMarkerAttribute(attr.AttributeClass)) == true;

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
}
