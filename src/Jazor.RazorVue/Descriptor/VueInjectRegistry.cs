using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Descriptor;

internal sealed class VueInjectRegistry
{
    private readonly ImmutableDictionary<string, VueInjectRegistration> _registrations;

    private VueInjectRegistry(ImmutableDictionary<string, VueInjectRegistration> registrations)
    {
        _registrations = registrations;
    }

    public static VueInjectRegistry Empty { get; } = new(ImmutableDictionary<string, VueInjectRegistration>.Empty);

    public static VueInjectRegistry Resolve(RazorVueCompilationContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var symbols = context.Symbols;
        if (symbols.VueInjectAttribute is null)
            return Empty;

        var registry = context.CreateComponentRegistry();
        var builder = ImmutableDictionary.CreateBuilder<string, VueInjectRegistration>(StringComparer.Ordinal);
        foreach (var attribute in context.Compilation.Assembly.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, symbols.VueInjectAttribute))
                continue;

            var contractType = TryReadTypeArgument(attribute, 0);
            var implementationType = TryReadTypeArgument(attribute, 1);
            if (contractType is null || implementationType is null)
                continue;

            var contractFullName = FormatFullName(contractType);
            var implementationFullName = FormatFullName(implementationType);
            var origin = CreateOrigin(attribute);

            if (builder.TryGetValue(contractFullName, out var existingRegistration))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] declares duplicate implementations for container contract '{contractFullName}'. " +
                    $"Existing implementation '{existingRegistration.ImplementationFullName}', duplicate '{implementationFullName}'.",
                    origin);
            }

            if (!registry.ComponentsByFullName.TryGetValue(implementationFullName, out var implementationDescriptor))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] maps container contract '{contractFullName}' to implementation '{implementationFullName}', but that component is not visible to the current compilation registry.",
                    origin);
            }

            if (implementationDescriptor.ContainerContractFullName is null)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] implementation '{implementationFullName}' must implement IVueContainerImplementation<{contractFullName}>.",
                    origin);
            }

            if (!string.Equals(implementationDescriptor.ContainerContractFullName, contractFullName, StringComparison.Ordinal))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] maps container contract '{contractFullName}' to implementation '{implementationFullName}', " +
                    $"but that implementation declares container contract '{implementationDescriptor.ContainerContractFullName}'.",
                    origin);
            }

            builder[contractFullName] = new VueInjectRegistration(contractFullName, implementationFullName);
        }

        return new VueInjectRegistry(builder.ToImmutable());
    }

    public VueComponentDescriptor ResolveImplementation(
        VueComponentDescriptor descriptor,
        VueComponentRegistry registry,
        RazorVueSourceOrigin? origin)
    {
        if (descriptor is null)
            throw new ArgumentNullException(nameof(descriptor));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (_registrations.IsEmpty || descriptor.ContainerContractFullName is null)
            return descriptor;

        // Inject only applies when the authored component is the container
        // contract itself. Direct references to a concrete implementation must
        // keep their own descriptor shape and must not be remapped back through
        // the contract indirection.
        if (!string.Equals(descriptor.FullName, descriptor.ContainerContractFullName, StringComparison.Ordinal))
            return descriptor;

        if (!_registrations.TryGetValue(descriptor.ContainerContractFullName, out var registration))
            return descriptor;

        if (!registry.ComponentsByFullName.TryGetValue(registration.ImplementationFullName, out var implementationDescriptor))
        {
            throw CreateInvalidContainerInjectDeclarationException(
                $"RazorVue [VueInject] maps container contract '{registration.ContractFullName}' to implementation '{registration.ImplementationFullName}', but the resolved component registry cannot find that implementation.",
                origin);
        }

        ValidateCompatibility(descriptor, implementationDescriptor, origin);
        return CreateInjectedDescriptor(descriptor, implementationDescriptor);
    }

    public void ValidateRegisteredCompatibility(
        VueComponentRegistry registry,
        Func<string, RazorVueSourceOrigin?> originFactory)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (originFactory is null)
            throw new ArgumentNullException(nameof(originFactory));
        if (_registrations.IsEmpty)
            return;

        foreach (var registration in _registrations.Values)
        {
            if (!registry.ComponentsByFullName.TryGetValue(registration.ContractFullName, out var contractDescriptor))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] maps container contract '{registration.ContractFullName}' to implementation '{registration.ImplementationFullName}', but the resolved component registry cannot find that contract.",
                    originFactory(registration.ContractFullName));
            }

            if (!registry.ComponentsByFullName.TryGetValue(registration.ImplementationFullName, out var implementationDescriptor))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] maps container contract '{registration.ContractFullName}' to implementation '{registration.ImplementationFullName}', but the resolved component registry cannot find that implementation.",
                    originFactory(registration.ContractFullName));
            }

            ValidateCompatibility(contractDescriptor, implementationDescriptor, originFactory(registration.ContractFullName));
        }
    }

    private static VueComponentDescriptor CreateInjectedDescriptor(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor)
        => new(
            Name: contractDescriptor.Name,
            FullName: contractDescriptor.FullName,
            SourceKind: implementationDescriptor.SourceKind,
            ResolutionNamespace: contractDescriptor.ResolutionNamespace,
            ImportSpecifier: implementationDescriptor.ImportSpecifier,
            ExportName: implementationDescriptor.ExportName,
            ContainerContractFullName: contractDescriptor.ContainerContractFullName,
            RouteTemplates: contractDescriptor.RouteTemplates,
            Props: MergeProps(contractDescriptor, implementationDescriptor),
            Emits: MergeEmits(contractDescriptor, implementationDescriptor),
            Slots: MergeSlots(contractDescriptor, implementationDescriptor),
            StyleDependencies: implementationDescriptor.StyleDependencies,
            PluginRequirements: implementationDescriptor.PluginRequirements,
            Flags: contractDescriptor.Flags,
            CascadingParameters: implementationDescriptor.CascadingParameters);

    private static ImmutableArray<VuePropDescriptor> MergeProps(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor)
    {
        if (contractDescriptor.Props.IsDefaultOrEmpty)
            return ImmutableArray<VuePropDescriptor>.Empty;

        var implementationProps = implementationDescriptor.Props.ToImmutableDictionary(
            static item => item.PublicName,
            static item => item,
            StringComparer.Ordinal);
        var builder = ImmutableArray.CreateBuilder<VuePropDescriptor>(contractDescriptor.Props.Length);
        foreach (var contractProp in contractDescriptor.Props)
        {
            var implementationProp = implementationProps[contractProp.PublicName];
            builder.Add(contractProp with
            {
                Name = implementationProp.Name,
                DefaultExpression = contractProp.DefaultExpression ?? implementationProp.DefaultExpression,
                DefaultSource = contractProp.DefaultExpression is not null
                    ? contractProp.DefaultSource
                    : implementationProp.DefaultSource
            });
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<VueEmitDescriptor> MergeEmits(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor)
    {
        if (contractDescriptor.Emits.IsDefaultOrEmpty)
            return ImmutableArray<VueEmitDescriptor>.Empty;

        var implementationEmits = implementationDescriptor.Emits
            .Where(static item => !string.IsNullOrWhiteSpace(item.RazorAlias))
            .ToImmutableDictionary(
                static item => item.RazorAlias!,
                static item => item,
                StringComparer.Ordinal);
        var builder = ImmutableArray.CreateBuilder<VueEmitDescriptor>(contractDescriptor.Emits.Length);
        foreach (var contractEmit in contractDescriptor.Emits)
        {
            if (string.IsNullOrWhiteSpace(contractEmit.RazorAlias))
            {
                builder.Add(contractEmit);
                continue;
            }

            var implementationEmit = implementationEmits[contractEmit.RazorAlias!];
            builder.Add(contractEmit with
            {
                Name = implementationEmit.Name
            });
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<VueSlotDescriptor> MergeSlots(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor)
    {
        if (contractDescriptor.Slots.IsDefaultOrEmpty)
            return ImmutableArray<VueSlotDescriptor>.Empty;

        var implementationSlots = implementationDescriptor.Slots.ToImmutableDictionary(
            static item => item.PublicName,
            static item => item,
            StringComparer.Ordinal);
        var builder = ImmutableArray.CreateBuilder<VueSlotDescriptor>(contractDescriptor.Slots.Length);
        foreach (var contractSlot in contractDescriptor.Slots)
        {
            var implementationSlot = implementationSlots[contractSlot.PublicName];
            builder.Add(contractSlot with
            {
                Name = implementationSlot.Name
            });
        }

        return builder.ToImmutable();
    }

    private static void ValidateCompatibility(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor,
        RazorVueSourceOrigin? origin)
    {
        ValidatePropCompatibility(contractDescriptor, implementationDescriptor, origin);
        ValidateEmitCompatibility(contractDescriptor, implementationDescriptor, origin);
        ValidateSlotCompatibility(contractDescriptor, implementationDescriptor, origin);
        ValidateFlagCompatibility(contractDescriptor, implementationDescriptor, origin);
    }

    private static void ValidatePropCompatibility(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor,
        RazorVueSourceOrigin? origin)
    {
        var implementationProps = implementationDescriptor.Props.ToImmutableDictionary(
            static item => item.PublicName,
            static item => item,
            StringComparer.Ordinal);

        foreach (var contractProp in contractDescriptor.Props)
        {
            if (!implementationProps.TryGetValue(contractProp.PublicName, out var implementationProp))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] implementation '{implementationDescriptor.FullName}' is missing compatible prop '{contractProp.PublicName}' required by container contract '{contractDescriptor.FullName}'.",
                    origin);
            }

            if (!TypeNamesEqual(contractProp.TypeName, implementationProp.TypeName))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] prop '{contractProp.PublicName}' on implementation '{implementationDescriptor.FullName}' has type '{implementationProp.TypeName}', but container contract '{contractDescriptor.FullName}' requires '{contractProp.TypeName}'.",
                    origin);
            }

            if (contractProp.Required != implementationProp.Required)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] prop '{contractProp.PublicName}' on implementation '{implementationDescriptor.FullName}' has Required={implementationProp.Required}, but container contract '{contractDescriptor.FullName}' requires Required={contractProp.Required}.",
                    origin);
            }

            if (contractProp.AcceptsBinding != implementationProp.AcceptsBinding)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] prop '{contractProp.PublicName}' on implementation '{implementationDescriptor.FullName}' has AcceptsBinding={implementationProp.AcceptsBinding}, but container contract '{contractDescriptor.FullName}' requires AcceptsBinding={contractProp.AcceptsBinding}.",
                    origin);
            }

            if (contractProp.CaptureUnmatchedValues != implementationProp.CaptureUnmatchedValues)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] prop '{contractProp.PublicName}' on implementation '{implementationDescriptor.FullName}' has CaptureUnmatchedValues={implementationProp.CaptureUnmatchedValues}, but container contract '{contractDescriptor.FullName}' requires CaptureUnmatchedValues={contractProp.CaptureUnmatchedValues}.",
                    origin);
            }

            if (contractProp.Kind != implementationProp.Kind)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] prop '{contractProp.PublicName}' on implementation '{implementationDescriptor.FullName}' has kind '{implementationProp.Kind}', but container contract '{contractDescriptor.FullName}' requires '{contractProp.Kind}'.",
                    origin);
            }
        }
    }

    private static void ValidateEmitCompatibility(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor,
        RazorVueSourceOrigin? origin)
    {
        var implementationEmits = implementationDescriptor.Emits
            .Where(static item => !string.IsNullOrWhiteSpace(item.RazorAlias))
            .ToImmutableDictionary(
                static item => item.RazorAlias!,
                static item => item,
                StringComparer.Ordinal);

        foreach (var contractEmit in contractDescriptor.Emits)
        {
            if (string.IsNullOrWhiteSpace(contractEmit.RazorAlias))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] container contract '{contractDescriptor.FullName}' declares emit '{contractEmit.Name}' without RazorAlias. Container emits must have stable authoring aliases.",
                    origin);
            }

            if (!implementationEmits.TryGetValue(contractEmit.RazorAlias!, out var implementationEmit))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] implementation '{implementationDescriptor.FullName}' is missing compatible emit '{contractEmit.RazorAlias}' required by container contract '{contractDescriptor.FullName}'.",
                    origin);
            }

            if (!TypeNamesEqual(contractEmit.PayloadTypeName, implementationEmit.PayloadTypeName))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] emit '{contractEmit.RazorAlias}' on implementation '{implementationDescriptor.FullName}' has payload '{implementationEmit.PayloadTypeName}', but container contract '{contractDescriptor.FullName}' requires '{contractEmit.PayloadTypeName}'.",
                    origin);
                }

            if (contractEmit.Kind != implementationEmit.Kind)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] emit '{contractEmit.RazorAlias}' on implementation '{implementationDescriptor.FullName}' has kind '{implementationEmit.Kind}', but container contract '{contractDescriptor.FullName}' requires '{contractEmit.Kind}'.",
                    origin);
            }
        }
    }

    private static void ValidateSlotCompatibility(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor,
        RazorVueSourceOrigin? origin)
    {
        var implementationSlots = implementationDescriptor.Slots.ToImmutableDictionary(
            static item => item.PublicName,
            static item => item,
            StringComparer.Ordinal);

        foreach (var contractSlot in contractDescriptor.Slots)
        {
            if (!implementationSlots.TryGetValue(contractSlot.PublicName, out var implementationSlot))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] implementation '{implementationDescriptor.FullName}' is missing compatible slot '{contractSlot.PublicName}' required by container contract '{contractDescriptor.FullName}'.",
                    origin);
            }

            if (contractSlot.PatternOnly != implementationSlot.PatternOnly)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] slot '{contractSlot.PublicName}' on implementation '{implementationDescriptor.FullName}' has PatternOnly={implementationSlot.PatternOnly}, but container contract '{contractDescriptor.FullName}' requires PatternOnly={contractSlot.PatternOnly}.",
                    origin);
            }

            if (contractSlot.IsDefault != implementationSlot.IsDefault)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] slot '{contractSlot.PublicName}' on implementation '{implementationDescriptor.FullName}' has IsDefault={implementationSlot.IsDefault}, but container contract '{contractDescriptor.FullName}' requires IsDefault={contractSlot.IsDefault}.",
                    origin);
            }

            if (contractSlot.Required != implementationSlot.Required)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] slot '{contractSlot.PublicName}' on implementation '{implementationDescriptor.FullName}' has Required={implementationSlot.Required}, but container contract '{contractDescriptor.FullName}' requires Required={contractSlot.Required}.",
                    origin);
            }

            if (!string.Equals(contractSlot.NamePattern ?? string.Empty, implementationSlot.NamePattern ?? string.Empty, StringComparison.Ordinal))
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] slot '{contractSlot.PublicName}' on implementation '{implementationDescriptor.FullName}' has NamePattern '{implementationSlot.NamePattern}', but container contract '{contractDescriptor.FullName}' requires '{contractSlot.NamePattern}'.",
                    origin);
            }

            if (contractSlot.Parameters.Length != implementationSlot.Parameters.Length)
            {
                throw CreateInvalidContainerInjectDeclarationException(
                    $"RazorVue [VueInject] slot '{contractSlot.PublicName}' on implementation '{implementationDescriptor.FullName}' has {implementationSlot.Parameters.Length} context parameter(s), but container contract '{contractDescriptor.FullName}' requires {contractSlot.Parameters.Length}.",
                    origin);
            }

            for (var index = 0; index < contractSlot.Parameters.Length; index++)
            {
                var contractParameter = contractSlot.Parameters[index];
                var implementationParameter = implementationSlot.Parameters[index];
                if (!TypeNamesEqual(contractParameter.TypeName, implementationParameter.TypeName))
                {
                    throw CreateInvalidContainerInjectDeclarationException(
                        $"RazorVue [VueInject] slot '{contractSlot.PublicName}' on implementation '{implementationDescriptor.FullName}' has context type '{implementationParameter.TypeName}', but container contract '{contractDescriptor.FullName}' requires '{contractParameter.TypeName}'.",
                        origin);
                    }
            }
        }
    }

    private static void ValidateFlagCompatibility(
        VueComponentDescriptor contractDescriptor,
        VueComponentDescriptor implementationDescriptor,
        RazorVueSourceOrigin? origin)
    {
        if (contractDescriptor.Flags == implementationDescriptor.Flags)
            return;

        throw CreateInvalidContainerInjectDeclarationException(
            $"RazorVue [VueInject] implementation '{implementationDescriptor.FullName}' declares flags '{implementationDescriptor.Flags}', but container contract '{contractDescriptor.FullName}' requires '{contractDescriptor.Flags}'.",
            origin);
    }

    private static bool TypeNamesEqual(string left, string right)
        => string.Equals(NormalizeTypeName(left), NormalizeTypeName(right), StringComparison.Ordinal);

    private static string NormalizeTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return string.Empty;

        var normalized = typeName.Trim();
        normalized = normalized.Replace("System.String", "string");
        normalized = normalized.Replace("System.Boolean", "bool");
        normalized = normalized.Replace("System.Byte", "byte");
        normalized = normalized.Replace("System.SByte", "sbyte");
        normalized = normalized.Replace("System.Int16", "short");
        normalized = normalized.Replace("System.UInt16", "ushort");
        normalized = normalized.Replace("System.Int32", "int");
        normalized = normalized.Replace("System.UInt32", "uint");
        normalized = normalized.Replace("System.Int64", "long");
        normalized = normalized.Replace("System.UInt64", "ulong");
        normalized = normalized.Replace("System.Single", "float");
        normalized = normalized.Replace("System.Double", "double");
        normalized = normalized.Replace("System.Decimal", "decimal");
        normalized = normalized.Replace("System.Object", "object");
        return normalized;
    }

    private static INamedTypeSymbol? TryReadTypeArgument(AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Length <= index)
            return null;

        var argument = attribute.ConstructorArguments[index];
        return argument.Kind == TypedConstantKind.Type
            ? argument.Value as INamedTypeSymbol
            : null;
    }

    private static string FormatFullName(INamedTypeSymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", string.Empty);

    private static RazorVueSourceOrigin? CreateOrigin(AttributeData attribute)
    {
        var location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();
        return location is null || !location.IsInSource
            ? null
            : RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Descriptor);
    }

    private static RazorVueCompilationIssueException CreateInvalidContainerInjectDeclarationException(
        string message,
        RazorVueSourceOrigin? origin)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.InvalidContainerInjectDeclaration,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, string.Empty, origin);
    }

    private sealed record VueInjectRegistration(
        string ContractFullName,
        string ImplementationFullName);
}
