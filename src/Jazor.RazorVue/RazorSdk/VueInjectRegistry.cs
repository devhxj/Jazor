using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>Resolves authored container component contracts to their Vue implementation types per compilation.</summary>
internal sealed class VueInjectRegistry
{
    private const string VueInjectAttributeMetadataName = "ECMAScript.VueContract.VueInjectAttribute";
    private const string ContainerComponentMetadataName = "ECMAScript.VueContract.IVueContainerComponent";
    private const string ContainerImplementationMetadataName = "ECMAScript.VueContract.IVueContainerImplementation`1";
    private const string ComponentMetadataName = "Microsoft.AspNetCore.Components.IComponent";
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string EditorRequiredAttributeMetadataName = "Microsoft.AspNetCore.Components.EditorRequiredAttribute";
    private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;
    private static readonly SymbolEqualityComparer TypeComparer = SymbolEqualityComparer.IncludeNullability;
    private static readonly ConditionalWeakTable<Compilation, VueInjectRegistry> Cache = new();

    private readonly ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol> _implementations;

    private VueInjectRegistry(
        ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol> implementations)
    {
        _implementations = implementations;
    }

    public static VueInjectRegistry ForCompilation(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        return Cache.GetValue(compilation, static current => Create(current));
    }

    public INamedTypeSymbol ResolveImplementation(INamedTypeSymbol authoredComponent)
    {
        if (authoredComponent is null)
            throw new ArgumentNullException(nameof(authoredComponent));

        return _implementations.TryGetValue(authoredComponent.OriginalDefinition, out var implementation)
            ? implementation
            : authoredComponent;
    }

    private static VueInjectRegistry Create(Compilation compilation)
    {
        var attributeType = compilation.GetTypeByMetadataName(VueInjectAttributeMetadataName);
        if (attributeType is null)
            return new VueInjectRegistry(CreateImplementationBuilder().ToImmutable());

        var implementations = CreateImplementationBuilder();
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            if (!SymbolComparer.Equals(attribute.AttributeClass, attributeType))
                continue;

            var contract = ReadComponentType(attribute, 0, "contract");
            var implementation = ReadComponentType(attribute, 1, "implementation");
            ValidateRegistration(compilation, contract, implementation);

            if (implementations.TryGetValue(contract.OriginalDefinition, out var existing))
            {
                throw InvalidDeclaration(
                    contract,
                    "declares duplicate implementations '" + Display(existing) + "' and '" +
                    Display(implementation) + "'.");
            }

            implementations.Add(contract.OriginalDefinition, implementation.OriginalDefinition);
        }

        return new VueInjectRegistry(implementations.ToImmutable());
    }

    private static ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol>.Builder CreateImplementationBuilder()
        => ImmutableDictionary.CreateBuilder<INamedTypeSymbol, INamedTypeSymbol>(SymbolComparer);

    private static INamedTypeSymbol ReadComponentType(AttributeData attribute, int index, string role)
    {
        if (attribute.ConstructorArguments.Length <= index ||
            attribute.ConstructorArguments[index].Kind != TypedConstantKind.Type ||
            attribute.ConstructorArguments[index].Value is not INamedTypeSymbol componentType)
        {
            throw new InvalidOperationException(
                "RazorVue [VueInject] " + role + " argument must be a named component type.");
        }

        return componentType;
    }

    private static void ValidateRegistration(
        Compilation compilation,
        INamedTypeSymbol contract,
        INamedTypeSymbol implementation)
    {
        var containerComponent = compilation.GetTypeByMetadataName(ContainerComponentMetadataName);
        if (containerComponent is null || !Implements(contract, containerComponent))
        {
            throw InvalidDeclaration(
                contract,
                "contract must implement IVueContainerComponent.");
        }

        var component = compilation.GetTypeByMetadataName(ComponentMetadataName);
        if (component is null || !Implements(implementation, component))
        {
            throw InvalidDeclaration(
                contract,
                "implementation '" + Display(implementation) + "' must implement IComponent.");
        }

        var implementationContract = compilation.GetTypeByMetadataName(ContainerImplementationMetadataName);
        var matchingImplementationInterface = implementationContract is not null &&
            implementation.AllInterfaces.Any(candidate =>
                SymbolComparer.Equals(candidate.OriginalDefinition, implementationContract) &&
                candidate.TypeArguments.Length == 1 &&
                SymbolComparer.Equals(candidate.TypeArguments[0], contract));
        if (!matchingImplementationInterface)
        {
            throw InvalidDeclaration(
                contract,
                "implementation '" + Display(implementation) +
                "' must implement IVueContainerImplementation<" + Display(contract) + ">.");
        }

        ValidateParameters(contract, implementation);
    }

    private static void ValidateParameters(
        INamedTypeSymbol contract,
        INamedTypeSymbol implementation)
    {
        var contractParameters = GetParameters(contract);
        var implementationParameters = GetParameters(implementation);
        foreach (var pair in contractParameters)
        {
            if (!implementationParameters.TryGetValue(pair.Key, out var implementationProperty))
            {
                throw InvalidDeclaration(
                    contract,
                    "implementation '" + Display(implementation) +
                    "' is missing parameter '" + pair.Key + "'.");
            }

            var contractProperty = pair.Value;
            if (!TypeComparer.Equals(contractProperty.Type, implementationProperty.Type))
            {
                throw InvalidDeclaration(
                    contract,
                    "implementation parameter '" + pair.Key + "' has type '" +
                    Display(implementationProperty.Type) + "', but the contract requires '" +
                    Display(contractProperty.Type) + "'.");
            }

            if (IsEditorRequired(contractProperty) != IsEditorRequired(implementationProperty))
            {
                throw InvalidDeclaration(
                    contract,
                    "implementation parameter '" + pair.Key +
                    "' must preserve the contract's EditorRequired setting.");
            }

            if (CapturesUnmatchedValues(contractProperty) != CapturesUnmatchedValues(implementationProperty))
            {
                throw InvalidDeclaration(
                    contract,
                    "implementation parameter '" + pair.Key +
                    "' must preserve the contract's CaptureUnmatchedValues setting.");
            }
        }
    }

    private static ImmutableDictionary<string, IPropertySymbol> GetParameters(INamedTypeSymbol component)
    {
        var parameters = ImmutableDictionary.CreateBuilder<string, IPropertySymbol>(StringComparer.Ordinal);
        for (var current = component; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (parameters.ContainsKey(property.Name) || !HasAttribute(property, ParameterAttributeMetadataName))
                    continue;

                parameters.Add(property.Name, property);
            }
        }

        return parameters.ToImmutable();
    }

    private static bool CapturesUnmatchedValues(IPropertySymbol property)
    {
        var parameter = property.GetAttributes().First(attribute =>
            string.Equals(attribute.AttributeClass?.ToDisplayString(), ParameterAttributeMetadataName, StringComparison.Ordinal));
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

    private static bool IsEditorRequired(IPropertySymbol property)
        => HasAttribute(property, EditorRequiredAttributeMetadataName);

    private static bool HasAttribute(ISymbol symbol, string metadataName)
        => symbol.GetAttributes().Any(attribute =>
            string.Equals(attribute.AttributeClass?.ToDisplayString(), metadataName, StringComparison.Ordinal));

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol interfaceType)
        => type.AllInterfaces.Any(candidate =>
            SymbolComparer.Equals(candidate.OriginalDefinition, interfaceType.OriginalDefinition));

    private static InvalidOperationException InvalidDeclaration(INamedTypeSymbol contract, string detail)
        => new(
            "Invalid RazorVue [VueInject] declaration for container contract '" +
            Display(contract) + "': " + detail);

    private static string Display(ITypeSymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
}
