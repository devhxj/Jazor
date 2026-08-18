using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Resolves the already-declared Vue component names from Razor contracts.
/// 从 Razor 契约读取已声明的 Vue 名称，不推断新的协议名称。
/// </summary>
/// <remarks>
/// Member-level <c>ECMAScriptName</c>/<c>Description("@#...")</c> metadata owns
/// the concrete Roslyn symbol.
/// <para>
/// 成员级通用命名元数据绑定到具体 Roslyn 符号。
/// </para>
/// </remarks>
internal static class LibraryComponentConventions
{
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";

    /// <summary>
    /// Returns the effective Razor parameter contract from derived type to base type.
    /// 返回从派生类到基类合并后的有效 Razor 参数契约。
    /// </summary>
    public static ImmutableArray<IPropertySymbol> GetEffectiveParameterProperties(INamedTypeSymbol componentType)
    {
        var properties = ImmutableArray.CreateBuilder<IPropertySymbol>();
        var claimedNames = new HashSet<string>(StringComparer.Ordinal);

        // Only a derived [Parameter] claims the public parameter name. A same-name
        // non-parameter member must not erase an inherited Razor parameter.
        // 只有派生 [Parameter] 才接管公开参数名；普通同名成员不能抹掉继承参数。
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (IsParameterProperty(property) && claimedNames.Add(property.Name))
                    properties.Add(property);
            }
        }

        return properties.ToImmutable();
    }

    public static bool IsParameterProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass!.ToDisplayString(),
                ParameterAttributeMetadataName,
                StringComparison.Ordinal));

    public static ImmutableDictionary<string, string> BuildParameterRuntimeNameMap(
        INamedTypeSymbol componentType)
    {
        var names = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        // Vue slots are passed as the third h(...) argument, while props and listeners
        // share its second argument. Prop/slot 同名是合法 Vue 契约；监听器仍须与 prop 共用冲突域。
        var propOwners = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
        var slotOwners = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);

        foreach (var property in GetEffectiveParameterProperties(componentType))
        {
            var isSlot = IsRenderFragment(property.Type);
            var runtimeName = isSlot
                ? GetSlotRuntimeName(componentType, property)
                : IsEventCallback(property.Type)
                    ? GetEventListenerRuntimeName(componentType, property)
                    : GetPropRuntimeName(property);
            var owners = isSlot ? slotOwners : propOwners;

            if (owners.TryGetValue(runtimeName, out var existing))
            {
                throw new InvalidOperationException(
                    $"RazorVue component '{componentType.ToDisplayString()}' maps parameters " +
                    $"'{existing.Name}' and '{property.Name}' to the duplicate Vue name '{runtimeName}'.");
            }

            owners.Add(runtimeName, property);
            // A map records only an actual ABI difference. Unannotated component
            // parameters retain the source name on every direct-render path.
            if (!string.Equals(runtimeName, property.Name, StringComparison.Ordinal))
                names.Add(property.Name, runtimeName);
        }

        return names.ToImmutable();
    }

    public static string GetPropRuntimeName(IPropertySymbol property)
        => Util.GetConfigOrSymbolName(property);

    public static string GetSlotRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
        => Util.GetConfigOrSymbolName(property);

    public static string GetEventListenerRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
        => Util.GetConfigOrSymbolName(property);

    private static bool IsEventCallback(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var definition = namedType.OriginalDefinition;
        return string.Equals(definition.Name, "EventCallback", StringComparison.Ordinal) &&
               string.Equals(
                   definition.ContainingNamespace!.ToDisplayString(),
                   "Microsoft.AspNetCore.Components",
                   StringComparison.Ordinal);
    }

    private static bool IsRenderFragment(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var definition = namedType.OriginalDefinition;
        return string.Equals(definition.Name, "RenderFragment", StringComparison.Ordinal) &&
               string.Equals(
                   definition.ContainingNamespace!.ToDisplayString(),
                   "Microsoft.AspNetCore.Components",
                   StringComparison.Ordinal);
    }
}
