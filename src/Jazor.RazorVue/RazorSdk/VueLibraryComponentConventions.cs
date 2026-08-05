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
/// the concrete Roslyn symbol. Class-level emit metadata is retained only where
/// the raw Vue event name cannot be reconstructed from a listener property name.
/// <para>
/// 成员级通用命名元数据绑定到具体 Roslyn 符号；只有监听器属性名无法无损还原
/// Vue 原始事件名时，才继续读取类级 emit 元数据。
/// </para>
/// </remarks>
internal static class VueLibraryComponentConventions
{
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string VueLibraryComponentAttributeMetadataName = "ECMAScript.VueContract.VueLibraryComponentAttribute";
    private const string VueLibraryEmitAttributeMetadataName = "ECMAScript.VueContract.VueLibraryEmitAttribute";

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
            var runtimeName = IsRenderFragment(property.Type)
                ? GetSlotRuntimeName(componentType, property)
                : IsEventCallback(property.Type)
                    ? GetEventListenerRuntimeName(componentType, property)
                    : GetPropRuntimeName(property);

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

    public static string GetPropRuntimeName(IPropertySymbol property)
    {
        var explicitName = Util.GetSymbolConfigName(property);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return explicitName!;

        return Util.GetConfigOrSymbolName(property);
    }

    public static string GetSlotRuntimeName(
        INamedTypeSymbol componentType,
        IPropertySymbol property)
    {
        var explicitName = Util.GetSymbolConfigName(property);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return explicitName!;

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

        if (TryGetEmitDescriptorName(componentType, property, out var descriptorName))
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

        if (TryGetEmitDescriptorName(componentType, property, out var descriptorName))
        {
            return descriptorName;
        }

        if (TryGetModelUpdateEventName(componentType, property, out var modelEventName))
            return modelEventName;

        return TryGetConventionalEventName(property, out var eventName)
            ? eventName
            : ToEmitName(Util.GetConfigOrSymbolName(property));
    }

    public static bool TryGetModelUpdateEventName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        out string eventName)
    {
        eventName = string.Empty;
        if (!IsParameterProperty(property) ||
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

        eventName = "update:" + GetPropRuntimeName(model);
        return true;
    }

    private static bool TryGetConventionalEventName(
        IPropertySymbol property,
        out string eventName)
    {
        eventName = string.Empty;
        if (!IsParameterProperty(property) ||
            !IsEventCallback(property.Type) ||
            property.Name.Length <= 2 ||
            !property.Name.StartsWith("On", StringComparison.Ordinal) ||
            !char.IsUpper(property.Name[2]))
        {
            return false;
        }

        eventName = ToKebabCase(property.Name.Substring(2));
        return true;
    }

    private static bool IsVueLibraryComponent(INamedTypeSymbol componentType)
        => componentType.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                VueLibraryComponentAttributeMetadataName,
                StringComparison.Ordinal));

    private static bool TryGetEmitDescriptorName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        out string name)
    {
        // Emit metadata carries the raw event name, but it still resolves only after
        // the effective parameter symbol is known so hidden members stay deterministic.
        // Emit 元数据承载原始事件名，但仍须先确定有效参数符号，保证隐藏成员解析稳定。
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (!IsDescriptorFor(attribute, VueLibraryEmitAttributeMetadataName, property.Name))
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
