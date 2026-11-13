using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.Compiler;

/// <summary>
/// Finds source declarations used by a compilation root, without treating host metadata as
/// permission to translate an unrelated CLR assembly. Shared by lowering and early analysis.
/// </summary>
internal static class SourceTypeClosure
{
    public static ImmutableArray<INamedTypeSymbol> Collect(
        Compilation compilation,
        IEnumerable<INamedTypeSymbol> roots,
        Func<ISymbol, bool>? includeMember = null,
        CancellationToken cancellationToken = default,
        bool includeTypeAnnotations = false,
        Func<INamedTypeSymbol, bool>? includeDependency = null)
    {
        var included = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var rootTypes = roots.Select(static root => root.OriginalDefinition)
            .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var pending = new Queue<INamedTypeSymbol>();
        foreach (var root in rootTypes)
            Add(root, isRoot: true);

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var type = pending.Dequeue();
            foreach (var reference in type.DeclaringSyntaxReferences)
            {
                var declaration = reference.GetSyntax(cancellationToken);
                var model = compilation.GetSemanticModel(declaration.SyntaxTree);
                foreach (var name in declaration.DescendantNodes(node =>
                             includeMember is null || !rootTypes.Contains(type) ||
                             node is not MemberDeclarationSyntax { Parent: TypeDeclarationSyntax } member ||
                             model.GetDeclaredSymbol(member, cancellationToken) is not { } symbol ||
                             includeMember(symbol)).OfType<SimpleNameSyntax>())
                {
                    // Attributes describe declarations; their CLR implementation is not part
                    // of the emitted module. Generic arguments remain compile-time annotations.
                    if (name.Ancestors().Any(static ancestor => ancestor is AttributeSyntax))
                        continue;
                    if (!includeTypeAnnotations && name.Ancestors().Any(static ancestor => ancestor is InvocationExpressionSyntax
                        { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } }))
                        continue;
                    if (!includeTypeAnnotations && name.Ancestors().Any(static ancestor => ancestor is TypeArgumentListSyntax))
                        continue;
                    var symbol = model.GetSymbolInfo(name, cancellationToken).Symbol;
                    if (symbol is not null)
                        Add(symbol as INamedTypeSymbol ?? symbol.ContainingType);
                }
            }
        }

        return included.OrderBy(static type => type.ToDisplayString(), StringComparer.Ordinal).ToImmutableArray();

        void Add(INamedTypeSymbol? type, bool isRoot = false)
        {
            if (type is null)
                return;
            type = type.OriginalDefinition;
            if (type.IsAnonymousType ||
                !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly) ||
                type.DeclaringSyntaxReferences.Length == 0 ||
                !isRoot && (Util.HasECMAScriptSupportMarker(type) || Util.HasECMAScriptSupportMarkerBaseType(type) ||
                    includeDependency?.Invoke(type) == false))
                return;
            if (included.Add(type))
                pending.Enqueue(type);
        }
    }

    public static void ValidateModuleOwnership(Compilation compilation,
        INamedTypeSymbol moduleRoot, IEnumerable<INamedTypeSymbol> sourceTypes)
    {
        var runtimeTypes = sourceTypes.Where(static type => type.TypeKind == TypeKind.Class && !type.IsRecord)
            .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        if (runtimeTypes.Count == 0 || !Util.IsECMAScriptModuleType(moduleRoot))
            return;
        foreach (var root in EnumerateTypes(compilation.Assembly.GlobalNamespace)
                     .Where(type => Util.IsECMAScriptModuleType(type) &&
                         !SymbolEqualityComparer.Default.Equals(type, moduleRoot)))
        {
            var shared = Collect(compilation, [root]).FirstOrDefault(runtimeTypes.Contains);
            if (shared is not null)
                // Copying the same class into two modules splits static state and instanceof
                // identity. Existing module exports provide the supported shared ownership seam.
                throw new NotSupportedException($"Source class '{shared.ToDisplayString()}' is shared by generated modules '{moduleRoot.ToDisplayString()}' and '{root.ToDisplayString()}'. Declare its implementation in a shared [ECMAScriptModule] and import that module instead of duplicating runtime identity.");
        }

        static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceOrTypeSymbol owner)
        {
            foreach (var member in owner.GetMembers())
            {
                if (member is INamedTypeSymbol type)
                    yield return type;
                if (member is INamespaceOrTypeSymbol container)
                    foreach (var nested in EnumerateTypes(container))
                        yield return nested;
            }
        }
    }
}
