using System;
using System.Linq;
using Acornima;
using Acornima.Ast;
using Jazor.Name;
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

/// <summary>
/// 编译器共享的符号、名称和 AST 辅助工具。
///
/// 这里的规则大多服务于“C# 声明侧”到“JS 运行时侧”的名称与宿主对齐，
/// 因此很多 helper 不只是字符串工具，而是运行时映射判定的一部分。
/// </summary>
public static class Util
{
    private const string ECMAScriptAssemblyName = "ECMAScript";
    private const string ECMAScriptAttributeName = "ECMAScriptAttribute";
    private const string ECMAScriptModuleAttributeName = "ECMAScriptModuleAttribute";

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
        => node.ToJavaScript(KnRJavaScriptTextFormatterOptions.Default, AstToJavaScriptOptions.Default);

    /// <summary>
    /// 以默认 writer 选项输出 ECMAScript 文本。
    /// </summary>
    public static string ToECMAScript(this Node node)
        => node.ToJavaScript(JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);

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

        // roslyn生成的隐式字段是 <PropName>k__BackingField 格式，不符合命名规范，需要处理一下
        if (symbol.Kind == SymbolKind.Field && symbol.IsImplicitlyDeclared)
        {
            var prop = ((IFieldSymbol)symbol).AssociatedSymbol!;
            var displayString = prop.OriginalDefinition.ToDisplayString(Format.NameFormat);
            return Format.HashName(displayString);
        }

        return AppendMethodOverloadSuffixIfNeeded(symbol, symbol.Name);
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

    private static bool HasAttribute(ISymbol? symbol, string attributeName)
        => symbol?.GetAttributes().Any(attr => attr.AttributeClass?.Name == attributeName) == true;

    private static bool IsRuntimeMarkerType(ISymbol? symbol)
        => HasAttribute(symbol, ECMAScriptAttributeName) ||
           HasAttribute(symbol, ECMAScriptModuleAttributeName);

    /// <summary>
    /// 判断一个类型是否属于 ECMAScript 运行时映射类型。
    ///
    /// 这里同时要求：
    /// - 类型来自 <c>ECMAScript</c> 程序集
    /// - 类型自身带有运行时标记特性
    ///
    /// 这样可以避免仅凭程序集名或类型名误判普通 CLR 类型。
    /// </summary>
    public static bool IsECMAScriptRuntimeType(ITypeSymbol? symbol)
        => symbol?.ContainingAssembly?.Name == ECMAScriptAssemblyName &&
           IsRuntimeMarkerType(symbol);

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
            _ => symbol.ContainingAssembly?.Name == ECMAScriptAssemblyName &&
                 IsECMAScriptRuntimeType(symbol.ContainingType)
        };

    /// <summary>
    /// ECMAScript 运行时宿主上的方法名默认直接视为运行时 API 名称，不再追加重载哈希。
    ///
    /// 原因是这些宿主最终对齐的是 JS 运行时对象，而不是 CLR 的 overload surface。
    /// 如果在这里追加哈希后缀，会无端扩大 C# / JS 的命名割裂。
    /// </summary>
    private static bool ShouldSkipMethodOverloadSuffix(IMethodSymbol methodSymbol)
        => methodSymbol.ContainingAssembly?.Name == ECMAScriptAssemblyName &&
           IsRuntimeMarkerType(methodSymbol.ContainingType);
}
