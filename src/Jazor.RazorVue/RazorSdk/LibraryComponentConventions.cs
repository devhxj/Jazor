using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Jazor.Common;
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
    private const string InjectAttributeMetadataName = "Microsoft.AspNetCore.Components.InjectAttribute";
    private const string CascadingParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.CascadingParameterAttribute";

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

    /// <summary>
    /// Returns whether a Razor parameter receives attributes that do not match a declared
    /// component parameter. The flag belongs to the authored <c>[Parameter]</c> contract and
    /// must be preserved when the parameter is projected into the Vue setup scope.
    /// </summary>
    public static bool CapturesUnmatchedValues(IPropertySymbol property)
    {
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        var parameter = property.GetAttributes().FirstOrDefault(static attribute =>
            string.Equals(
                attribute.AttributeClass?.OriginalDefinition.ToDisplayString(),
                ParameterAttributeMetadataName,
                StringComparison.Ordinal));
        if (parameter is null)
            return false;

        foreach (var argument in parameter.NamedArguments)
        {
            if (string.Equals(argument.Key, "CaptureUnmatchedValues", StringComparison.Ordinal) &&
                argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the effective Blazor property-injection surface from the most-derived source
    /// component toward its base components. The generated Razor property for an <c>@inject</c>
    /// directive is intentionally included: it is part of the component contract even though its
    /// declaration lives in Razor SG generated C#.
    /// </summary>
    /// <remarks>
    /// 注入属性属于 Blazor 激活契约，不是参数。这里保留生成的 @inject property，后续由
    /// Vue setup 的 provide/inject adapter 赋值；页面作者不需要改写成 props 或 builder 协议。
    /// </remarks>
    public static ImmutableArray<IPropertySymbol> GetEffectiveInjectProperties(
        INamedTypeSymbol componentType)
    {
        var properties = ImmutableArray.CreateBuilder<IPropertySymbol>();
        var claimedNames = new HashSet<string>(StringComparer.Ordinal);

        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (IsInjectProperty(property) && claimedNames.Add(property.Name))
                    properties.Add(property);
            }
        }

        return properties.ToImmutable();
    }

    public static bool IsInjectProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.OriginalDefinition.ToDisplayString(),
                InjectAttributeMetadataName,
                StringComparison.Ordinal));

    /// <summary>
    /// Returns the effective Blazor cascading-parameter surface. Cascading values are a
    /// runtime activation contract, not ordinary Vue props, so the list is kept separate from
    /// the [Parameter] and [Inject] surfaces.
    /// </summary>
    public static ImmutableArray<IPropertySymbol> GetEffectiveCascadingParameterProperties(
        INamedTypeSymbol componentType)
    {
        var properties = ImmutableArray.CreateBuilder<IPropertySymbol>();
        var claimedNames = new HashSet<string>(StringComparer.Ordinal);

        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (IsCascadingParameterProperty(property) && claimedNames.Add(property.Name))
                    properties.Add(property);
            }
        }

        return properties.ToImmutable();
    }

    public static bool IsCascadingParameterProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.OriginalDefinition.ToDisplayString(),
                CascadingParameterAttributeMetadataName,
                StringComparison.Ordinal));

    /// <summary>Reads the optional Blazor cascade name without guessing from the property name.</summary>
    public static string? GetCascadingParameterName(IPropertySymbol property)
    {
        var attribute = property.GetAttributes().FirstOrDefault(static candidate =>
            string.Equals(
                candidate.AttributeClass?.OriginalDefinition.ToDisplayString(),
                CascadingParameterAttributeMetadataName,
                StringComparison.Ordinal));
        if (attribute is null)
            return null;

        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, "Name", StringComparison.Ordinal) &&
                argument.Value.Value is string name &&
                !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return null;
    }

    /// <summary>
    /// Computes the shared browser provide/inject key. Nullable annotations are intentionally
    /// absent from Format.NameFormat, matching Blazor's type-based cascade lookup semantics.
    /// </summary>
    public static string GetCascadingServiceKey(IPropertySymbol property)
    {
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        return GetCascadingServiceKey(property.Type, GetCascadingParameterName(property));
    }

    public static string GetCascadingServiceKey(ITypeSymbol type, string? name = null)
        => "jazor:cascade:" + GetCascadingTypeKey(type) + ":" + (name ?? string.Empty);

    public static string GetCascadingTypeKey(ITypeSymbol type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.ToDisplayString(Format.NameFormat);
    }

    /// <summary>
    /// Computes the host-visible Vue provide key for one Blazor injected service. The type name
    /// is canonicalized through the shared compiler display format so aliases and nullable
    /// annotations do not make two builds disagree about the provider key.
    /// </summary>
    public static string GetInjectServiceKey(IPropertySymbol property)
    {
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        var typeName = property.Type.ToDisplayString(Format.NameFormat);
        return "jazor:service:" + typeName;
    }

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
