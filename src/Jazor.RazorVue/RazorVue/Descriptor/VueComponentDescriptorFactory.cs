using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Descriptor;

internal static class VueComponentDescriptorFactory
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;
    private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static VueComponentDescriptor Create(RazorVueComponentCandidate candidate, RazorVueCompilationContext context)
    {
        var componentSymbol = candidate.ComponentSymbol;
        var symbols = context.Symbols;
        var parameterProperties = GetParameterProperties(componentSymbol, symbols);
        var bindPairs = GetBindableParameterNames(parameterProperties, symbols);

        var props = ImmutableArray.CreateBuilder<VuePropDescriptor>();
        var emits = ImmutableArray.CreateBuilder<VueEmitDescriptor>();
        var slots = ImmutableArray.CreateBuilder<VueSlotDescriptor>();

        foreach (var property in parameterProperties)
        {
            if (IsRenderFragment(property.Type, symbols))
            {
                slots.Add(CreateSlotDescriptor(property, symbols));
                continue;
            }

            if (IsEventCallback(property.Type, symbols))
            {
                emits.Add(CreateEmitDescriptor(property, bindPairs, symbols));
                continue;
            }

            var publicName = property.Name;
            var acceptsBinding = bindPairs.Contains(publicName);
            props.Add(new VuePropDescriptor(
                Name: ToLowerCamelCase(publicName),
                PublicName: publicName,
                TypeName: FormatTypeName(property.Type),
                Required: false,
                AcceptsBinding: acceptsBinding,
                DefaultExpression: null,
                Kind: acceptsBinding ? VuePropKind.Model : VuePropKind.Normal));
        }

        return new VueComponentDescriptor(
            Name: componentSymbol.Name,
            FullName: FormatFullName(componentSymbol),
            SourceKind: VueComponentSourceKind.UserComponent,
            ResolutionNamespace: GetResolutionNamespace(componentSymbol),
            ImportSpecifier: GetImportSpecifier(componentSymbol, symbols),
            ExportName: "default",
            Props: props.ToImmutable(),
            Emits: emits.ToImmutable(),
            Slots: slots.ToImmutable(),
            StyleDependencies: [],
            Flags: VueComponentFlags.None);
    }

    private static ImmutableArray<IPropertySymbol> GetParameterProperties(INamedTypeSymbol componentSymbol, RazorVueCompilationSymbols symbols)
    {
        if (symbols.ParameterAttribute is null)
            return [];

        var builder = ImmutableArray.CreateBuilder<IPropertySymbol>();
        var seenNames = new HashSet<string>(StringComparer.Ordinal);
        for (var current = componentSymbol; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is not IPropertySymbol property || property.IsStatic || !seenNames.Add(property.Name))
                    continue;

                if (property.GetAttributes().Any(attribute => Comparer.Equals(attribute.AttributeClass, symbols.ParameterAttribute)))
                    builder.Add(property);
            }
        }

        return builder.ToImmutable();
    }

    private static HashSet<string> GetBindableParameterNames(
        ImmutableArray<IPropertySymbol> parameterProperties,
        RazorVueCompilationSymbols symbols)
    {
        var parameterNames = new HashSet<string>(
            parameterProperties.Select(static property => property.Name),
            StringComparer.Ordinal);
        var builder = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in parameterProperties)
        {
            if (!IsEventCallback(property.Type, symbols) ||
                !property.Name.EndsWith("Changed", StringComparison.Ordinal))
            {
                continue;
            }

            var parameterName = property.Name.Substring(0, property.Name.Length - "Changed".Length);
            if (parameterNames.Contains(parameterName))
                builder.Add(parameterName);
        }

        return builder;
    }

    private static VueEmitDescriptor CreateEmitDescriptor(
        IPropertySymbol property,
        HashSet<string> bindPairs,
        RazorVueCompilationSymbols symbols)
    {
        var payloadTypeName = GetEventPayloadTypeName(property.Type, symbols);
        if (property.Name.EndsWith("Changed", StringComparison.Ordinal))
        {
            var parameterName = property.Name.Substring(0, property.Name.Length - "Changed".Length);
            if (bindPairs.Contains(parameterName))
            {
                return new VueEmitDescriptor(
                    Name: $"update:{ToLowerCamelCase(parameterName)}",
                    PayloadTypeName: payloadTypeName,
                    RazorAlias: property.Name,
                    Kind: VueEmitKind.ModelUpdate);
            }
        }

        return new VueEmitDescriptor(
            Name: ToEmitName(property.Name),
            PayloadTypeName: payloadTypeName,
            RazorAlias: property.Name,
            Kind: VueEmitKind.Normal);
    }

    private static VueSlotDescriptor CreateSlotDescriptor(IPropertySymbol property, RazorVueCompilationSymbols symbols)
    {
        var isDefault = string.Equals(property.Name, "ChildContent", StringComparison.Ordinal);
        var parameters = ImmutableArray<VueSlotParameterDescriptor>.Empty;

        if (symbols.RenderFragmentOfT is not null &&
            property.Type is INamedTypeSymbol namedType &&
            Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragmentOfT) &&
            namedType.TypeArguments.Length == 1)
        {
            parameters = [new VueSlotParameterDescriptor("context", FormatTypeName(namedType.TypeArguments[0]))];
        }

        return new VueSlotDescriptor(
            Name: isDefault ? "default" : ToLowerCamelCase(property.Name),
            IsDefault: isDefault,
            Parameters: parameters,
            Required: false);
    }

    private static bool IsEventCallback(ITypeSymbol typeSymbol, RazorVueCompilationSymbols symbols)
        => typeSymbol is INamedTypeSymbol namedType &&
           ((symbols.EventCallback is not null && Comparer.Equals(namedType.OriginalDefinition, symbols.EventCallback)) ||
            (symbols.EventCallbackOfT is not null && Comparer.Equals(namedType.OriginalDefinition, symbols.EventCallbackOfT)));

    private static bool IsRenderFragment(ITypeSymbol typeSymbol, RazorVueCompilationSymbols symbols)
        => typeSymbol is INamedTypeSymbol namedType &&
           ((symbols.RenderFragment is not null && Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragment)) ||
            (symbols.RenderFragmentOfT is not null && Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragmentOfT)));

    private static string GetEventPayloadTypeName(ITypeSymbol typeSymbol, RazorVueCompilationSymbols symbols)
    {
        if (symbols.EventCallbackOfT is not null &&
            typeSymbol is INamedTypeSymbol namedType &&
            Comparer.Equals(namedType.OriginalDefinition, symbols.EventCallbackOfT) &&
            namedType.TypeArguments.Length == 1)
        {
            return FormatTypeName(namedType.TypeArguments[0]);
        }

        return "void";
    }

    private static string GetResolutionNamespace(INamedTypeSymbol componentSymbol)
        => componentSymbol.ContainingNamespace?.IsGlobalNamespace == false
            ? componentSymbol.ContainingNamespace.ToDisplayString()
            : string.Empty;

    private static string GetImportSpecifier(INamedTypeSymbol componentSymbol, RazorVueCompilationSymbols symbols)
    {
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.ECMAScriptModuleAttribute))
                continue;

            if (attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string importPath &&
                !string.IsNullOrWhiteSpace(importPath))
            {
                return NormalizeImportPath(importPath);
            }
        }

        var assemblyName = componentSymbol.ContainingAssembly?.Name ?? "Jazor.Assembly";
        var namespaceName = GetResolutionNamespace(componentSymbol).Replace('.', '/');
        var fileName = $"{componentSymbol.Name}.mjs";

        return string.IsNullOrEmpty(namespaceName)
            ? $"{assemblyName}/{fileName}"
            : $"{assemblyName}/{namespaceName}/{fileName}";
    }

    private static string NormalizeImportPath(string importPath)
    {
        var normalized = importPath.Replace('\\', '/').Trim();
        var extension = normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                        normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"{normalized}.mjs";

        return extension;
    }

    private static string FormatTypeName(ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(TypeDisplayFormat);

    private static string FormatFullName(INamedTypeSymbol componentSymbol)
        => componentSymbol.ToDisplayString(TypeDisplayFormat);

    private static string ToEmitName(string propertyName)
    {
        if (propertyName.StartsWith("On", StringComparison.Ordinal) &&
            propertyName.Length > 2 &&
            char.IsUpper(propertyName[2]))
        {
            return ToLowerCamelCase(propertyName.Substring(2));
        }

        return ToLowerCamelCase(propertyName);
    }

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}

