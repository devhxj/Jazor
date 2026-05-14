using System.Collections.Immutable;
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

        if (!_registrations.TryGetValue(descriptor.ContainerContractFullName, out var registration))
            return descriptor;

        if (!registry.ComponentsByFullName.TryGetValue(registration.ImplementationFullName, out var implementationDescriptor))
        {
            throw CreateInvalidContainerInjectDeclarationException(
                $"RazorVue [VueInject] maps container contract '{registration.ContractFullName}' to implementation '{registration.ImplementationFullName}', but the resolved component registry cannot find that implementation.",
                origin);
        }

        return implementationDescriptor;
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
