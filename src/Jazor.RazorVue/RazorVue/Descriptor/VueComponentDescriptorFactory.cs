using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;
using Jazor.RazorVue.Artifacts;
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
        return CreateDescriptor(
            candidate.ComponentSymbol,
            context.Symbols,
            VueComponentSourceKind.UserComponent,
            GetUserImportSpecifier(candidate.ComponentSymbol, context.Symbols),
            "default",
            [],
            []);
    }

    public static VueComponentDescriptor CreateLibraryComponent(INamedTypeSymbol componentSymbol, RazorVueCompilationContext context)
    {
        if (componentSymbol is null)
            throw new ArgumentNullException(nameof(componentSymbol));
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var symbols = context.Symbols;
        var metadata = GetLibraryMetadata(componentSymbol, symbols);
        return CreateDescriptor(
            componentSymbol,
            symbols,
            VueComponentSourceKind.LibraryComponent,
            metadata.ImportSpecifier,
            metadata.ExportName,
            metadata.StyleDependencies,
            metadata.PluginRequirements);
    }

    private static VueComponentDescriptor CreateDescriptor(
        INamedTypeSymbol componentSymbol,
        RazorVueCompilationSymbols symbols,
        VueComponentSourceKind sourceKind,
        string importSpecifier,
        string exportName,
        ImmutableArray<string> styleDependencies,
        ImmutableArray<string> pluginRequirements)
    {
        var parameterProperties = GetParameterProperties(componentSymbol, symbols);
        var bindPairs = GetBindableParameterNames(parameterProperties, symbols);
        var authoringMetadata = sourceKind == VueComponentSourceKind.LibraryComponent
            ? GetLibraryAuthoringMetadata(componentSymbol, symbols, parameterProperties)
            : LibraryAuthoringMetadata.Empty;

        var props = ImmutableArray.CreateBuilder<VuePropDescriptor>();
        var emits = ImmutableArray.CreateBuilder<VueEmitDescriptor>();
        var slots = ImmutableArray.CreateBuilder<VueSlotDescriptor>();
        var bindableParameters = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in parameterProperties)
        {
            authoringMetadata.PropOverrides.TryGetValue(property.Name, out var propOverride);
            authoringMetadata.SlotOverrides.TryGetValue(property.Name, out var slotOverride);

            if (IsRenderFragment(property.Type, symbols))
            {
                if (propOverride is not null)
                {
                    throw CreateInvalidLibraryComponentDeclarationException(
                        componentSymbol,
                        $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryProp] to regular [Parameter] properties. '{property.Name}' is a slot parameter.");
                }

                slots.Add(CreateSlotDescriptor(property, symbols, slotOverride));
                continue;
            }

            if (slotOverride is not null)
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibrarySlot] to RenderFragment parameters. '{property.Name}' is not a slot parameter.");
            }

            if (IsEventCallback(property.Type, symbols))
            {
                if (propOverride is not null)
                {
                    throw CreateInvalidLibraryComponentDeclarationException(
                        componentSymbol,
                        $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryProp] to regular [Parameter] properties. '{property.Name}' is an event callback parameter.");
                }

                continue;
            }

            var publicName = property.Name;
            var inferredAcceptsBinding = bindPairs.Contains(publicName);
            var acceptsBinding = propOverride?.AcceptsBinding ?? inferredAcceptsBinding;
            var kind = propOverride is not null && propOverride.HasKindOverride
                ? propOverride.Kind
                : acceptsBinding
                    ? VuePropKind.Model
                    : VuePropKind.Normal;
            props.Add(new VuePropDescriptor(
                Name: propOverride?.Name ?? ToLowerCamelCase(publicName),
                PublicName: publicName,
                TypeName: FormatTypeName(property.Type),
                Required: propOverride?.Required ?? false,
                AcceptsBinding: acceptsBinding,
                DefaultExpression: propOverride?.DefaultExpression,
                Kind: kind));

            if (acceptsBinding)
                bindableParameters.Add(publicName);
        }

        foreach (var property in parameterProperties)
        {
            authoringMetadata.EmitOverrides.TryGetValue(property.Name, out var emitOverride);
            authoringMetadata.SlotOverrides.TryGetValue(property.Name, out var slotOverride);

            if (emitOverride is not null && IsRenderFragment(property.Type, symbols))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryEmit] to EventCallback parameters. '{property.Name}' is a slot parameter.");
            }

            if (IsEventCallback(property.Type, symbols))
            {
                emits.Add(CreateEmitDescriptor(property, bindableParameters, symbols, emitOverride));
                continue;
            }

            if (emitOverride is not null)
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryEmit] to EventCallback parameters. '{property.Name}' is not an event callback parameter.");
            }
        }

        if (slots.Count(static slot => slot.IsDefault) > 1)
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' declares more than one default slot.");
        }

        return new VueComponentDescriptor(
            Name: componentSymbol.Name,
            FullName: FormatFullName(componentSymbol),
            SourceKind: sourceKind,
            ResolutionNamespace: GetResolutionNamespace(componentSymbol),
            ImportSpecifier: importSpecifier,
            ExportName: exportName,
            Props: props.ToImmutable(),
            Emits: emits.ToImmutable(),
            Slots: slots.ToImmutable(),
            StyleDependencies: styleDependencies,
            PluginRequirements: pluginRequirements,
            Flags: authoringMetadata.Flags);
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
        RazorVueCompilationSymbols symbols,
        LibraryEmitOverride? emitOverride)
    {
        var payloadTypeName = GetEventPayloadTypeName(property.Type, symbols);
        var emitKind = emitOverride is not null && emitOverride.HasKindOverride
            ? emitOverride.Kind
            : VueEmitKind.Normal;
        if (property.Name.EndsWith("Changed", StringComparison.Ordinal))
        {
            var parameterName = property.Name.Substring(0, property.Name.Length - "Changed".Length);
            if (bindPairs.Contains(parameterName))
            {
                return new VueEmitDescriptor(
                    Name: emitOverride?.Name ?? $"update:{ToLowerCamelCase(parameterName)}",
                    PayloadTypeName: emitOverride?.PayloadTypeName ?? payloadTypeName,
                    RazorAlias: property.Name,
                    Kind: emitOverride is not null && emitOverride.HasKindOverride
                        ? emitOverride.Kind
                        : VueEmitKind.ModelUpdate);
            }
        }

        return new VueEmitDescriptor(
            Name: emitOverride?.Name ?? ToEmitName(property.Name),
            PayloadTypeName: emitOverride?.PayloadTypeName ?? payloadTypeName,
            RazorAlias: property.Name,
            Kind: emitOverride?.Kind ?? emitKind);
    }

    private static VueSlotDescriptor CreateSlotDescriptor(
        IPropertySymbol property,
        RazorVueCompilationSymbols symbols,
        LibrarySlotOverride? slotOverride)
    {
        var isDefault = slotOverride?.IsDefault ?? string.Equals(property.Name, "ChildContent", StringComparison.Ordinal);
        var parameters = ImmutableArray<VueSlotParameterDescriptor>.Empty;

        if (symbols.RenderFragmentOfT is not null &&
            property.Type is INamedTypeSymbol namedType &&
            Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragmentOfT) &&
            namedType.TypeArguments.Length == 1)
        {
            parameters =
            [
                new VueSlotParameterDescriptor(
                    slotOverride?.ContextParameterName ?? "context",
                    slotOverride?.ContextTypeName ?? FormatTypeName(namedType.TypeArguments[0]))
            ];
        }

        return new VueSlotDescriptor(
            Name: slotOverride?.Name ?? (isDefault ? "default" : ToLowerCamelCase(property.Name)),
            PublicName: property.Name,
            IsDefault: isDefault,
            Parameters: parameters,
            Required: slotOverride?.Required ?? false);
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

    private static string GetUserImportSpecifier(INamedTypeSymbol componentSymbol, RazorVueCompilationSymbols symbols)
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

    private static LibraryComponentMetadata GetLibraryMetadata(INamedTypeSymbol componentSymbol, RazorVueCompilationSymbols symbols)
    {
        if (symbols.VueLibraryComponentAttribute is null)
            throw new InvalidOperationException("VueLibraryComponentAttribute could not be resolved from the compilation.");

        var componentAttribute = componentSymbol.GetAttributes()
            .FirstOrDefault(attribute => Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryComponentAttribute));

        if (componentAttribute is null ||
            componentAttribute.ConstructorArguments.Length < 2 ||
            componentAttribute.ConstructorArguments[0].Value is not string importSpecifier ||
            string.IsNullOrWhiteSpace(importSpecifier) ||
            componentAttribute.ConstructorArguments[1].Value is not string exportName ||
            string.IsNullOrWhiteSpace(exportName))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' must declare [VueLibraryComponent(importSpecifier, exportName)].");
        }

        // Library imports are external package contracts, not generated module paths.
        // Library authoring metadata feeds both compile-time lowering and the
        // host-facing catalog/manifest contract.
        var styleDependencies = GetLibraryStyleDependencies(componentSymbol, symbols);
        var pluginRequirements = GetLibraryPluginRequirements(componentSymbol, symbols);
        return new LibraryComponentMetadata(importSpecifier.Trim(), exportName.Trim(), styleDependencies, pluginRequirements);
    }

    private static LibraryAuthoringMetadata GetLibraryAuthoringMetadata(
        INamedTypeSymbol componentSymbol,
        RazorVueCompilationSymbols symbols,
        ImmutableArray<IPropertySymbol> parameterProperties)
    {
        var parameterLookup = parameterProperties.ToImmutableDictionary(
            static property => property.Name,
            static property => property,
            StringComparer.Ordinal);

        return new LibraryAuthoringMetadata(
            GetLibraryPropOverrides(componentSymbol, symbols, parameterLookup),
            GetLibraryEmitOverrides(componentSymbol, symbols, parameterLookup),
            GetLibrarySlotOverrides(componentSymbol, symbols, parameterLookup),
            GetLibraryComponentFlags(componentSymbol, symbols));
    }

    private static ImmutableDictionary<string, LibraryPropOverride> GetLibraryPropOverrides(
        INamedTypeSymbol componentSymbol,
        RazorVueCompilationSymbols symbols,
        ImmutableDictionary<string, IPropertySymbol> parameterLookup)
    {
        if (symbols.VueLibraryPropAttribute is null)
            return ImmutableDictionary<string, LibraryPropOverride>.Empty.WithComparers(StringComparer.Ordinal);

        var builder = ImmutableDictionary.CreateBuilder<string, LibraryPropOverride>(StringComparer.Ordinal);
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryPropAttribute))
                continue;

            var publicName = GetRequiredConstructorStringArgument(attribute, 0, componentSymbol, "VueLibraryProp");
            var property = GetRequiredParameter(componentSymbol, parameterLookup, publicName, "VueLibraryProp");
            if (IsEventCallback(property.Type, symbols) || IsRenderFragment(property.Type, symbols))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryProp] to regular [Parameter] properties. '{publicName}' is not a prop parameter.");
            }

            if (builder.ContainsKey(publicName))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' declares duplicate [VueLibraryProp] metadata for '{publicName}'.");
            }

            builder[publicName] = new LibraryPropOverride(
                GetOptionalNamedStringArgument(attribute, "Name", componentSymbol, "VueLibraryProp"),
                GetOptionalNamedBoolArgument(attribute, "Required"),
                GetOptionalNamedBoolArgument(attribute, "AcceptsBinding"),
                GetOptionalNamedStringArgument(attribute, "DefaultExpression", componentSymbol, "VueLibraryProp"),
                attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is int propKind
                    ? (VuePropKind)propKind
                    : VuePropKind.Normal,
                attribute.ConstructorArguments.Length >= 2);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, LibraryEmitOverride> GetLibraryEmitOverrides(
        INamedTypeSymbol componentSymbol,
        RazorVueCompilationSymbols symbols,
        ImmutableDictionary<string, IPropertySymbol> parameterLookup)
    {
        if (symbols.VueLibraryEmitAttribute is null)
            return ImmutableDictionary<string, LibraryEmitOverride>.Empty.WithComparers(StringComparer.Ordinal);

        var builder = ImmutableDictionary.CreateBuilder<string, LibraryEmitOverride>(StringComparer.Ordinal);
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryEmitAttribute))
                continue;

            var razorAlias = GetRequiredConstructorStringArgument(attribute, 0, componentSymbol, "VueLibraryEmit");
            var property = GetRequiredParameter(componentSymbol, parameterLookup, razorAlias, "VueLibraryEmit");
            if (!IsEventCallback(property.Type, symbols))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryEmit] to EventCallback parameters. '{razorAlias}' is not an event callback parameter.");
            }

            if (builder.ContainsKey(razorAlias))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' declares duplicate [VueLibraryEmit] metadata for '{razorAlias}'.");
            }

            builder[razorAlias] = new LibraryEmitOverride(
                GetOptionalNamedStringArgument(attribute, "Name", componentSymbol, "VueLibraryEmit"),
                GetOptionalNamedStringArgument(attribute, "PayloadTypeName", componentSymbol, "VueLibraryEmit"),
                attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is int emitKind
                    ? (VueEmitKind)emitKind
                    : VueEmitKind.Normal,
                attribute.ConstructorArguments.Length >= 2);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, LibrarySlotOverride> GetLibrarySlotOverrides(
        INamedTypeSymbol componentSymbol,
        RazorVueCompilationSymbols symbols,
        ImmutableDictionary<string, IPropertySymbol> parameterLookup)
    {
        if (symbols.VueLibrarySlotAttribute is null)
            return ImmutableDictionary<string, LibrarySlotOverride>.Empty.WithComparers(StringComparer.Ordinal);

        var builder = ImmutableDictionary.CreateBuilder<string, LibrarySlotOverride>(StringComparer.Ordinal);
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibrarySlotAttribute))
                continue;

            var publicName = GetRequiredConstructorStringArgument(attribute, 0, componentSymbol, "VueLibrarySlot");
            var property = GetRequiredParameter(componentSymbol, parameterLookup, publicName, "VueLibrarySlot");
            if (!IsRenderFragment(property.Type, symbols))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibrarySlot] to RenderFragment parameters. '{publicName}' is not a slot parameter.");
            }

            if (builder.ContainsKey(publicName))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' declares duplicate [VueLibrarySlot] metadata for '{publicName}'.");
            }

            var slotName = GetOptionalNamedStringArgument(attribute, "Name", componentSymbol, "VueLibrarySlot");
            var isDefault = GetOptionalNamedBoolArgument(attribute, "IsDefault");
            if (isDefault == true && slotName is not null && !string.Equals(slotName, "default", StringComparison.Ordinal))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' must use slot name 'default' when [VueLibrarySlot] marks '{publicName}' as the default slot.");
            }

            var contextTypeName = GetOptionalNamedStringArgument(attribute, "ContextTypeName", componentSymbol, "VueLibrarySlot");
            if (contextTypeName is not null && !IsTypedRenderFragment(property.Type, symbols))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' can only declare an explicit slot context type for RenderFragment<T> parameters. '{publicName}' is not typed child content.");
            }

            var contextParameterName = contextTypeName is null
                ? null
                : GetOptionalNamedStringArgument(attribute, "ContextParameterName", componentSymbol, "VueLibrarySlot") ?? "context";

            builder[publicName] = new LibrarySlotOverride(
                slotName,
                isDefault,
                GetOptionalNamedBoolArgument(attribute, "Required"),
                contextTypeName,
                contextParameterName);
        }

        return builder.ToImmutable();
    }

    private static VueComponentFlags GetLibraryComponentFlags(
        INamedTypeSymbol componentSymbol,
        RazorVueCompilationSymbols symbols)
    {
        if (symbols.VueLibraryComponentFlagsAttribute is null)
            return VueComponentFlags.None;

        var flags = VueComponentFlags.None;
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryComponentFlagsAttribute))
                continue;

            if (attribute.ConstructorArguments.Length != 1 || attribute.ConstructorArguments[0].Value is not int flagValue)
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' must declare [VueLibraryComponentFlags(flags)].");
            }

            flags |= (VueComponentFlags)flagValue;
        }

        return flags;
    }

    private static ImmutableArray<string> GetLibraryStyleDependencies(INamedTypeSymbol componentSymbol, RazorVueCompilationSymbols symbols)
    {
        if (symbols.VueLibraryStyleAttribute is null)
            return [];

        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryStyleAttribute))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length != 1 ||
                attribute.ConstructorArguments[0].Value is not string styleSpecifier ||
                string.IsNullOrWhiteSpace(styleSpecifier))
            {
                throw CreateInvalidLibraryStyleDependencyDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' has an invalid [VueLibraryStyle(styleSpecifier)] declaration.");
            }

            var normalizedStyleSpecifier = styleSpecifier.Trim();
            if (!seen.Add(normalizedStyleSpecifier))
            {
                throw CreateInvalidLibraryStyleDependencyDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' declares duplicate style dependency '{normalizedStyleSpecifier}'.");
            }

            builder.Add(normalizedStyleSpecifier);
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<string> GetLibraryPluginRequirements(INamedTypeSymbol componentSymbol, RazorVueCompilationSymbols symbols)
    {
        if (symbols.VueLibraryPluginRequirementAttribute is null)
            return [];

        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryPluginRequirementAttribute))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length != 1 ||
                attribute.ConstructorArguments[0].Value is not string requirementId ||
                string.IsNullOrWhiteSpace(requirementId))
            {
                throw CreateInvalidLibraryPluginRequirementDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' has an invalid [VueLibraryPluginRequirement(requirementId)] declaration.");
            }

            var normalizedRequirementId = requirementId.Trim();
            if (!seen.Add(normalizedRequirementId))
            {
                throw CreateInvalidLibraryPluginRequirementDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' declares duplicate plugin requirement '{normalizedRequirementId}'.");
            }

            builder.Add(normalizedRequirementId);
        }

        return builder.ToImmutable();
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

    private static string GetRequiredConstructorStringArgument(
        AttributeData attribute,
        int index,
        INamedTypeSymbol componentSymbol,
        string attributeName)
    {
        if (attribute.ConstructorArguments.Length <= index ||
            attribute.ConstructorArguments[index].Value is not string value ||
            string.IsNullOrWhiteSpace(value))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' has an invalid [{attributeName}] declaration.");
        }

        return value.Trim();
    }

    private static string? GetOptionalNamedStringArgument(
        AttributeData attribute,
        string name,
        INamedTypeSymbol componentSymbol,
        string attributeName)
    {
        foreach (var pair in attribute.NamedArguments)
        {
            if (!string.Equals(pair.Key, name, StringComparison.Ordinal))
                continue;

            if (pair.Value.Value is not string value || string.IsNullOrWhiteSpace(value))
            {
                throw CreateInvalidLibraryComponentDeclarationException(
                    componentSymbol,
                    $"Library component '{FormatFullName(componentSymbol)}' has an invalid [{attributeName}] {name} value.");
            }

            return value.Trim();
        }

        return null;
    }

    private static bool? GetOptionalNamedBoolArgument(AttributeData attribute, string name)
    {
        foreach (var pair in attribute.NamedArguments)
        {
            if (string.Equals(pair.Key, name, StringComparison.Ordinal) &&
                pair.Value.Value is bool value)
            {
                return value;
            }
        }

        return null;
    }

    private static bool IsTypedRenderFragment(ITypeSymbol typeSymbol, RazorVueCompilationSymbols symbols)
        => symbols.RenderFragmentOfT is not null &&
           typeSymbol is INamedTypeSymbol namedType &&
           Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragmentOfT);

    private static IPropertySymbol GetRequiredParameter(
        INamedTypeSymbol componentSymbol,
        ImmutableDictionary<string, IPropertySymbol> parameterLookup,
        string publicName,
        string attributeName)
    {
        if (parameterLookup.TryGetValue(publicName, out var property))
            return property;

        throw CreateInvalidLibraryComponentDeclarationException(
            componentSymbol,
            $"Library component '{FormatFullName(componentSymbol)}' applies [{attributeName}] to unknown [Parameter] property '{publicName}'.");
    }

    private static RazorVueCompilationIssueException CreateInvalidLibraryComponentDeclarationException(
        INamedTypeSymbol componentSymbol,
        string message)
        => CreateLibraryMetadataIssueException(componentSymbol, RazorVueIssueCode.InvalidLibraryComponentDeclaration, message);

    private static RazorVueCompilationIssueException CreateInvalidLibraryStyleDependencyDeclarationException(
        INamedTypeSymbol componentSymbol,
        string message)
        => CreateLibraryMetadataIssueException(componentSymbol, RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration, message);

    private static RazorVueCompilationIssueException CreateInvalidLibraryPluginRequirementDeclarationException(
        INamedTypeSymbol componentSymbol,
        string message)
        => CreateLibraryMetadataIssueException(componentSymbol, RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration, message);

    private static RazorVueCompilationIssueException CreateLibraryMetadataIssueException(
        INamedTypeSymbol componentSymbol,
        RazorVueIssueCode issueCode,
        string message)
    {
        var issue = new RazorVueCompilationIssue(
            issueCode,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        var location = componentSymbol.Locations.FirstOrDefault(static item => item.IsInSource) ?? Location.None;
        var origin = location == Location.None
            ? null
            : RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Descriptor);
        return new RazorVueCompilationIssueException(issue, FormatFullName(componentSymbol), origin);
    }

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

    private sealed record LibraryComponentMetadata(
        string ImportSpecifier,
        string ExportName,
        ImmutableArray<string> StyleDependencies,
        ImmutableArray<string> PluginRequirements);

    private sealed record LibraryAuthoringMetadata(
        ImmutableDictionary<string, LibraryPropOverride> PropOverrides,
        ImmutableDictionary<string, LibraryEmitOverride> EmitOverrides,
        ImmutableDictionary<string, LibrarySlotOverride> SlotOverrides,
        VueComponentFlags Flags)
    {
        public static LibraryAuthoringMetadata Empty { get; } = new(
            ImmutableDictionary<string, LibraryPropOverride>.Empty.WithComparers(StringComparer.Ordinal),
            ImmutableDictionary<string, LibraryEmitOverride>.Empty.WithComparers(StringComparer.Ordinal),
            ImmutableDictionary<string, LibrarySlotOverride>.Empty.WithComparers(StringComparer.Ordinal),
            VueComponentFlags.None);
    }

    private sealed record LibraryPropOverride(
        string? Name,
        bool? Required,
        bool? AcceptsBinding,
        string? DefaultExpression,
        VuePropKind Kind,
        bool HasKindOverride);

    private sealed record LibraryEmitOverride(
        string? Name,
        string? PayloadTypeName,
        VueEmitKind Kind,
        bool HasKindOverride);

    private sealed record LibrarySlotOverride(
        string? Name,
        bool? IsDefault,
        bool? Required,
        string? ContextTypeName,
        string? ContextParameterName);
}

