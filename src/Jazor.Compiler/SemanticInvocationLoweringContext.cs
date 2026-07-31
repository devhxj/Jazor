using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Exposes the narrow compiler services required by host-owned invocation intrinsics.
/// </summary>
/// <remarks>
/// The context is scoped to one invocation lowering. It keeps import binding, type mapping,
/// hierarchy traversal, and diagnostic creation inside compiler-owned implementations while
/// allowing a host extension to build its final ESTree expression without accessing walker state.
/// </remarks>
public sealed class SemanticInvocationLoweringContext
{
    private readonly ImportedModuleMemberBuilder _importedModuleMemberBuilder;
    private readonly ModuleImportPathResolver _moduleImportPathResolver;
    private readonly TypeMappingResolver _typeMappingResolver;
    private readonly NamedTypeHierarchyEnumerator _namedTypeHierarchyEnumerator;
    private readonly ExceptionFactory _exceptionFactory;

    internal SemanticInvocationLoweringContext(
        SenseArgument argument,
        ImportedModuleMemberBuilder importedModuleMemberBuilder,
        ModuleImportPathResolver moduleImportPathResolver,
        TypeMappingResolver typeMappingResolver,
        NamedTypeHierarchyEnumerator namedTypeHierarchyEnumerator,
        ExceptionFactory exceptionFactory)
    {
        Argument = argument;
        _importedModuleMemberBuilder = importedModuleMemberBuilder;
        _moduleImportPathResolver = moduleImportPathResolver;
        _typeMappingResolver = typeMappingResolver;
        _namedTypeHierarchyEnumerator = namedTypeHierarchyEnumerator;
        _exceptionFactory = exceptionFactory;
    }

    public SenseArgument Argument { get; }

    public bool TryBuildImportedModuleMember(
        ITypeSymbol? containingType,
        string memberName,
        out Expression? expression)
        => _importedModuleMemberBuilder(containingType, memberName, Argument, out expression);

    public string? GetModuleImportPath(ITypeSymbol symbol)
        => _moduleImportPathResolver(symbol);

    public SemanticTypeMapping GetTypeMapping(ITypeSymbol symbol)
    {
        var (mapper, typeName) = _typeMappingResolver(symbol);
        return new SemanticTypeMapping(mapper, typeName);
    }

    public IEnumerable<INamedTypeSymbol> EnumerateNamedTypeHierarchyBaseFirst(INamedTypeSymbol type)
        => _namedTypeHierarchyEnumerator(type);

    public OperationTransformationException CreateException(IOperation operation, string message)
        => _exceptionFactory(operation, message);

    internal delegate bool ImportedModuleMemberBuilder(
        ITypeSymbol? containingType,
        string memberName,
        SenseArgument? context,
        out Expression? expression);

    internal delegate string? ModuleImportPathResolver(ITypeSymbol symbol);

    internal delegate (TypeMapper Mapper, string TypeName) TypeMappingResolver(ITypeSymbol symbol);

    internal delegate IEnumerable<INamedTypeSymbol> NamedTypeHierarchyEnumerator(INamedTypeSymbol type);

    internal delegate OperationTransformationException ExceptionFactory(IOperation operation, string message);
}

public readonly record struct SemanticTypeMapping(TypeMapper Mapper, string TypeName);
