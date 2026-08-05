using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Resolves Vue component names from standard C# and Razor contracts.
/// 从标准 C# 与 Razor 契约解析 Vue 名称，避免为常规绑定重复引入特性 DSL。
/// </summary>
/// <remarks>
/// Member-level <c>ECMAScriptName</c>/<c>Description("@#...")</c> metadata owns
/// the concrete Roslyn symbol and therefore has priority. Legacy class-level
/// Vue descriptors remain a migration fallback only.
/// <para>
/// 成员级通用命名元数据绑定到具体 Roslyn 符号，因此优先级最高；旧的类级 Vue
/// 描述符仅作为迁移兼容层，不能覆盖成员自身声明。
/// </para>
/// </remarks>
internal static class VueLibraryComponentConventions
{
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string VueLibraryComponentAttributeMetadataName = "ECMAScript.VueContract.VueLibraryComponentAttribute";
    private const string VuePropAttributeMetadataName = "ECMAScript.VueContract.VuePropAttribute";
    private const string VueLibraryEmitAttributeMetadataName = "ECMAScript.VueContract.VueLibraryEmitAttribute";
    private const string VueSlotAttributeMetadataName = "ECMAScript.VueContract.VueSlotAttribute";

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
                attribute.AttributeClass?.ToDisplayString(),
                ParameterAttributeMetadataName,
                StringComparison.Ordinal));

    public static ImmutableDictionary<string, string> BuildParameterRuntimeNameMap(
        INamedTypeSymbol componentType)
    {
        var names = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        var owners = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);

        foreach (var property in GetEffectiveParameterProperties(componentType))
        {
            if (IsRenderFragment(property.Type) && IsPatternOnlySlot(componentType, property))
                continue;

            var runtimeName = IsRenderFragment(property.Type)
                ? GetSlotRuntimeName(componentType, property)
                : IsEventCallback(property.Type)
                    ? GetEventListenerRuntimeName(componentType, property)
                    : GetPropRuntimeName(componentType, property);

            if (owners.TryGetValue(runtimeName, out var existing))
            {
                throw new InvalidOperationException(
                    $"RazorVue component '{componentType.ToDisplayString()}' maps parameters " +
                    $"'{existing.Name}' and '{property.Name}' to the duplicate Vue name '{runtimeName}'.");
            }

            owners.Add(runtimeName, property);
            if (!string.Equals(runtimeName, ToDefaultRuntimeName(property.Name), StringComparison.Ordinal))
                names.Add(property.Name, runtimeName);
        }

        return names.ToImmutable();
    }

    public static string GetPropRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
    {
        var explicitName = Util.GetSymbolConfigName(property);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return explicitName!;

        return TryGetLegacyDescriptorName(
            componentType,
            property,
            VuePropAttributeMetadataName,
            out var descriptorName)
            ? descriptorName
            : Util.GetConfigOrSymbolName(property);
    }

    public static string GetSlotRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
    {
        var explicitName = Util.GetSymbolConfigName(property);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return explicitName!;

        if (TryGetLegacySlotName(componentType, property, out var descriptorName))
            return descriptorName;

        if (property.Name is "ChildContent" or "DefaultContent")
            return "default";

        return IsVueLibraryComponent(componentType)
            ? ToKebabCase(RemoveContentSuffix(property.Name))
            : Util.GetConfigOrSymbolName(property);
    }

    public static string GetEventListenerRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
    {
        var explicitName = Util.GetSymbolConfigName(property);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return explicitName!;

        if (TryGetLegacyDescriptorName(
                componentType,
                property,
                VueLibraryEmitAttributeMetadataName,
                out var descriptorName))
        {
            return VueDescriptorNaming.ToListenerPropertyName(descriptorName);
        }

        return TryGetModelUpdateEventName(componentType, property, out var modelEventName)
            ? VueDescriptorNaming.ToListenerPropertyName(modelEventName)
            : Util.GetConfigOrSymbolName(property);
    }

    public static string GetEmitRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
    {
        var explicitName = Util.GetSymbolConfigName(property);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return ToEmitName(explicitName!);

        if (TryGetLegacyDescriptorName(
                componentType,
                property,
                VueLibraryEmitAttributeMetadataName,
                out var descriptorName))
        {
            return descriptorName;
        }

        return TryGetModelUpdateEventName(componentType, property, out var modelEventName)
            ? modelEventName
            : ToEmitName(Util.GetConfigOrSymbolName(property));
    }

    public static bool TryGetModelUpdateEventName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        out string eventName)
    {
        eventName = string.Empty;
        if (!IsVueLibraryComponent(componentType) ||
            !IsParameterProperty(property) ||
            !IsEventCallback(property.Type) ||
            !property.Name.EndsWith("Changed", StringComparison.Ordinal))
        {
            return false;
        }

        var modelName = property.Name.Substring(0, property.Name.Length - "Changed".Length);
        var model = GetEffectiveParameterProperties(componentType)
            .FirstOrDefault(candidate => string.Equals(candidate.Name, modelName, StringComparison.Ordinal));
        if (model is null)
            return false;

        eventName = "update:" + GetPropRuntimeName(componentType, model);
        return true;
    }

    private static bool IsVueLibraryComponent(INamedTypeSymbol componentType)
        => componentType.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                VueLibraryComponentAttributeMetadataName,
                StringComparison.Ordinal));

    private static bool TryGetLegacyDescriptorName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        string attributeMetadataName,
        out string name)
    {
        // Class-level descriptors cannot identify hidden same-name symbols. They are
        // consulted only after the effective symbol and its member metadata are known.
        // 类级描述符无法区分同名隐藏成员，只能在有效符号及成员元数据确定后兼容读取。
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (!IsDescriptorFor(attribute, attributeMetadataName, property.Name))
                    continue;

                var descriptorName = GetNamedString(attribute, "Name");
                if (!string.IsNullOrWhiteSpace(descriptorName))
                {
                    name = descriptorName!;
                    return true;
                }
            }
        }

        name = string.Empty;
        return false;
    }

    private static bool TryGetLegacySlotName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        out string name)
    {
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (!IsDescriptorFor(attribute, VueSlotAttributeMetadataName, property.Name) ||
                    GetNamedBoolean(attribute, "PatternOnly") == true)
                {
                    continue;
                }

                if (GetNamedBoolean(attribute, "IsDefault") == true)
                {
                    name = "default";
                    return true;
                }

                var descriptorName = GetNamedString(attribute, "Name");
                if (!string.IsNullOrWhiteSpace(descriptorName))
                {
                    name = descriptorName!;
                    return true;
                }
            }
        }

        name = string.Empty;
        return false;
    }

    private static bool IsPatternOnlySlot(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
    {
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (IsDescriptorFor(attribute, VueSlotAttributeMetadataName, property.Name) &&
                    GetNamedBoolean(attribute, "PatternOnly") == true)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsDescriptorFor(
        AttributeData attribute,
        string attributeMetadataName,
        string publicName)
        => string.Equals(
               attribute.AttributeClass?.ToDisplayString(),
               attributeMetadataName,
               StringComparison.Ordinal) &&
           attribute.ConstructorArguments.Length > 0 &&
           attribute.ConstructorArguments[0].Value is string descriptorPublicName &&
           string.Equals(descriptorPublicName, publicName, StringComparison.Ordinal);

    private static string? GetNamedString(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is string value &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static bool? GetNamedBoolean(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return null;
    }

    private static string RemoveContentSuffix(string propertyName)
        => propertyName.EndsWith("Content", StringComparison.Ordinal) &&
           propertyName.Length > "Content".Length
            ? propertyName.Substring(0, propertyName.Length - "Content".Length)
            : propertyName;

    private static string ToKebabCase(string name)
    {
        var result = new StringBuilder(name.Length + 4);
        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (char.IsUpper(character))
            {
                var separatesWord = index > 0 &&
                    (char.IsLower(name[index - 1]) ||
                     char.IsDigit(name[index - 1]) ||
                     index + 1 < name.Length && char.IsLower(name[index + 1]));
                if (separatesWord)
                    result.Append('-');

                result.Append(char.ToLowerInvariant(character));
                continue;
            }

            result.Append(character);
        }

        return result.ToString();
    }

    private static string ToEmitName(string listenerName)
    {
        if (listenerName.Length > 2 &&
            listenerName.StartsWith("on", StringComparison.Ordinal) &&
            char.IsUpper(listenerName[2]))
        {
            var eventName = listenerName.Substring(2);
            return char.ToLowerInvariant(eventName[0]) + eventName.Substring(1);
        }

        return listenerName;
    }

    private static string ToDefaultRuntimeName(string propertyName)
        => propertyName.Length == 0
            ? propertyName
            : char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

    private static bool IsEventCallback(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var definition = namedType.OriginalDefinition;
        return string.Equals(definition.Name, "EventCallback", StringComparison.Ordinal) &&
               string.Equals(
                   definition.ContainingNamespace?.ToDisplayString(),
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
                   definition.ContainingNamespace?.ToDisplayString(),
                   "Microsoft.AspNetCore.Components",
                   StringComparison.Ordinal);
    }
}
