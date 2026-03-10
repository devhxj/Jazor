using System;
using System.Linq;
using Jazor.Name;
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

internal static class Util
{
    /// <summary>
    /// 获取ISymbol的 JavaScript 名称
    /// 优先级：
    /// 1. ECMAScriptNameAttribute
    /// 2. DescriptionAttribute (以 @# 开头)
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static string? GetSymbolConfigName(ISymbol symbol)
    {
        // todo:属性的别名如何处理（因为存在get、set）
        var useDescription = true;
        string? configName = null, description = null;
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
                    description = desc.Substring(2);
            }
        }

        return useDescription ? description : configName;
    }

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
        else if (symbol is IMethodSymbol methodSymbol)
        {
            // 需要判断是否存在方法重载
            if (methodSymbol.ContainingType.GetMembers(methodSymbol.Name)
                .Count(m => m.Kind == SymbolKind.Method) > 1)
            {
                var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
                return $"{symbol.Name}{Format.HashName(displayString)}";
            }
        }

        return symbol.Name;
    }
}
