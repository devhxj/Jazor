using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Resolves Vue library conventions that standard C# and Razor already express.
/// 解析标准 C# 与 Razor 已可表达的 Vue 组件约定，避免为常规绑定重复引入特性 DSL。
/// </summary>
/// <remarks>
/// `X` + `XChanged` is Razor's ordinary two-way-binding contract. For an external
/// Vue component it maps to the Vue `update:x` event without requiring a second
/// attribute DSL. Explicit Vue metadata still wins for non-conventional names.
/// <para>
/// `X` + `XChanged` 是 Razor 的常规双向绑定契约；对于外部 Vue 组件，它映射为
/// `update:x` 事件。只有 C# 无法忠实表示的名称才需要显式元数据，且显式元数据优先。
/// </para>
/// </remarks>
internal static class VueLibraryComponentConventions
{
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string VueLibraryComponentAttributeMetadataName = "ECMAScript.VueContract.VueLibraryComponentAttribute";
    private const string VuePropAttributeMetadataName = "ECMAScript.VueContract.VuePropAttribute";
    private const string VueSlotAttributeMetadataName = "ECMAScript.VueContract.VueSlotAttribute";

    public static void AddInferredModelUpdateNames(
        INamedTypeSymbol componentType,
        IDictionary<string, string> names)
    {
        if (!IsVueLibraryComponent(componentType))
            return;

        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (names.ContainsKey(property.Name) ||
                    !TryGetModelUpdateEventName(componentType, property, out var eventName))
                {
                    continue;
                }

                names.Add(property.Name, VueDescriptorNaming.ToListenerPropertyName(eventName));
            }
        }
    }

    public static void AddInferredSlotNames(
        INamedTypeSymbol componentType,
        IDictionary<string, string> names)
    {
        if (!IsVueLibraryComponent(componentType))
            return;

        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (names.ContainsKey(property.Name) ||
                    !TryGetSlotName(componentType, property, out var slotName))
                {
                    continue;
                }

                names.Add(property.Name, slotName);
            }
        }
    }

    public static bool TryGetModelUpdateEventName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        out string eventName)
    {
        eventName = string.Empty;
        if (!IsVueLibraryComponent(componentType) ||
            !IsParameter(property) ||
            !IsEventCallback(property.Type) ||
            !property.Name.EndsWith("Changed", StringComparison.Ordinal))
        {
            return false;
        }

        var modelName = property.Name.Substring(0, property.Name.Length - "Changed".Length);
        if (modelName.Length == 0 || FindParameter(componentType, modelName) is not IPropertySymbol model)
            return false;

        eventName = "update:" + GetRuntimePropertyName(componentType, model);
        return true;
    }

    private static bool TryGetSlotName(
        INamedTypeSymbol componentType,
        IPropertySymbol property,
        out string slotName)
    {
        slotName = string.Empty;
        if (!IsParameter(property) ||
            !IsRenderFragment(property.Type) ||
            HasExplicitSlotDescriptor(componentType, property))
        {
            return false;
        }

        slotName = property.Name switch
        {
            "ChildContent" or "DefaultContent" => "default",
            _ => ToKebabCase(RemoveContentSuffix(property.Name))
        };
        return slotName.Length > 0;
    }

    private static bool IsVueLibraryComponent(INamedTypeSymbol componentType)
        => componentType.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                VueLibraryComponentAttributeMetadataName,
                StringComparison.Ordinal));

    private static IPropertySymbol? FindParameter(INamedTypeSymbol componentType, string name)
    {
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            var property = current.GetMembers(name).OfType<IPropertySymbol>()
                .FirstOrDefault(IsParameter);
            if (property is not null)
                return property;
        }

        return null;
    }

    private static string GetRuntimePropertyName(INamedTypeSymbol componentType, IPropertySymbol property)
    {
        // A declared VueProp name is an intentional exception to C# camel-case naming.
        // 显式 VueProp.Name 是对 C# camel-case 约定的有意例外，必须优先保留。
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (!string.Equals(
                        attribute.AttributeClass?.ToDisplayString(),
                        VuePropAttributeMetadataName,
                        StringComparison.Ordinal) ||
                    attribute.ConstructorArguments.Length == 0 ||
                    attribute.ConstructorArguments[0].Value is not string publicName ||
                    !string.Equals(publicName, property.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var argument in attribute.NamedArguments)
                {
                    if (string.Equals(argument.Key, "Name", StringComparison.Ordinal) &&
                        argument.Value.Value is string name &&
                        !string.IsNullOrWhiteSpace(name))
                    {
                        return name.Trim();
                    }
                }
            }
        }

        return char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
    }

    private static bool HasExplicitSlotDescriptor(INamedTypeSymbol componentType, IPropertySymbol property)
    {
        // Dot-qualified and other non-conventional slot names cannot be inferred safely.
        // 含点号等非常规插槽名无法安全推导，保留显式 VueSlot 描述符。
        for (INamedTypeSymbol? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                if (string.Equals(
                        attribute.AttributeClass?.ToDisplayString(),
                        VueSlotAttributeMetadataName,
                        StringComparison.Ordinal) &&
                    attribute.ConstructorArguments.Length > 0 &&
                    attribute.ConstructorArguments[0].Value is string publicName &&
                    string.Equals(publicName, property.Name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
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

    private static bool IsParameter(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                ParameterAttributeMetadataName,
                StringComparison.Ordinal));

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
