using System;
using System.Linq;
using Acornima;
using Acornima.Ast;
using Jazor.Name;
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

/// <summary>
/// 
/// </summary>
public static class Util
{
    private const string ECMAScriptAssemblyName = "ECMAScript";
    private const string ECMAScriptAttributeName = "ECMAScriptAttribute";

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
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static string ToKnRECMAScript(this Node node)
        => node.ToJavaScript(KnRJavaScriptTextFormatterOptions.Default, AstToJavaScriptOptions.Default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
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

    private static bool ShouldSkipMethodOverloadSuffix(IMethodSymbol methodSymbol)
        => methodSymbol.ContainingAssembly?.Name == ECMAScriptAssemblyName &&
           HasAttribute(methodSymbol.ContainingType, ECMAScriptAttributeName);
}
