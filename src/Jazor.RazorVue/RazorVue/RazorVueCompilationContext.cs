using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Discovery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue;

public sealed class RazorVueCompilationContext
{
    public RazorVueCompilationContext(Compilation compilation, RazorVueCompilationSymbols symbols)
    {
        Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
    }

    public Compilation Compilation { get; }

    public RazorVueCompilationSymbols Symbols { get; }

    public static RazorVueCompilationContext? TryCreate(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var symbols = RazorVueCompilationSymbols.TryCreate(compilation);
        return symbols is null ? null : new RazorVueCompilationContext(compilation, symbols);
    }

    public RazorVueEntryKind ClassifyEntry(INamedTypeSymbol symbol)
    {
        if (symbol is null)
            throw new ArgumentNullException(nameof(symbol));

        return RazorVueEntryClassifier.Classify(symbol, Symbols);
    }

    public ImmutableArray<RazorVueComponentCandidate> DiscoverComponentCandidates()
    {
        var builder = ImmutableArray.CreateBuilder<RazorVueComponentCandidate>();
        foreach (var symbol in EnumerateNamedTypes(Compilation.GlobalNamespace))
        {
            if (RazorVueEntryClassifier.Classify(symbol, Symbols) != RazorVueEntryKind.RazorVueComponent)
                continue;

            builder.Add(new RazorVueComponentCandidate(
                symbol,
                RazorVueEntryClassifier.FindBuildRenderTreeMethod(symbol),
                RazorVueEntryClassifier.FindOnInitializedMethod(symbol),
                RazorVueEntryClassifier.FindOnInitializedAsyncMethod(symbol),
                RazorVueEntryClassifier.FindOnParametersSetMethod(symbol),
                RazorVueEntryClassifier.FindOnParametersSetAsyncMethod(symbol),
                RazorVueEntryClassifier.FindOnAfterRenderMethod(symbol),
                RazorVueEntryClassifier.FindOnAfterRenderAsyncMethod(symbol),
                RazorVueEntryClassifier.FindShouldRenderMethod(symbol),
                RazorVueEntryClassifier.FindSetParametersAsyncMethod(symbol),
                RazorVueEntryClassifier.FindDisposeMethod(symbol),
                RazorVueEntryClassifier.FindDisposeAsyncMethod(symbol),
                RazorVueEntryClassifier.FindLogicMethods(symbol),
                RazorVueEntryClassifier.FindLogicFields(symbol),
                RazorVueEntryKind.RazorVueComponent));
        }

        return builder.ToImmutable();
    }

    public RazorVueSemanticSnapshot CreateSemanticSnapshot(RazorVueComponentCandidate candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        if (candidate.EntryKind != RazorVueEntryKind.RazorVueComponent)
            throw new InvalidOperationException($"Only {nameof(RazorVueEntryKind.RazorVueComponent)} candidates can become semantic snapshots.");

        var descriptor = VueComponentDescriptorFactory.Create(candidate, this);
        var lifecycle = new VueLifecycleDescriptor(
            HasOnInitialized: candidate.OnInitializedMethod is not null,
            HasOnInitializedAsync: candidate.OnInitializedAsyncMethod is not null,
            HasOnParametersSet: candidate.OnParametersSetMethod is not null,
            HasOnParametersSetAsync: candidate.OnParametersSetAsyncMethod is not null,
            HasOnAfterRender: candidate.OnAfterRenderMethod is not null,
            HasOnAfterRenderAsync: candidate.OnAfterRenderAsyncMethod is not null,
            HasShouldRender: candidate.ShouldRenderMethod is not null,
            HasSetParametersAsync: candidate.SetParametersAsyncMethod is not null,
            HasDispose: candidate.DisposeMethod is not null,
            HasDisposeAsync: candidate.DisposeAsyncMethod is not null);
        var logicMethods = candidate.LogicMethods
            .Select(static method => new VueLogicMethodDescriptor(method.Name, method.Parameters.Length, method.IsAsync, method))
            .ToImmutableArray();
        var logicFields = candidate.LogicFields
            // Preserve Roslyn field carriers for upcoming setup-side lowering.
            .Select(static field => new VueLogicFieldDescriptor(field.Name, field.IsReadOnly, field))
            .ToImmutableArray();
        var logic = logicMethods.IsDefaultOrEmpty && logicFields.IsDefaultOrEmpty
            ? VueLogicDescriptor.Empty
            : new VueLogicDescriptor(logicFields, logicMethods);
        // Keep the first snapshot carrier tied to Roslyn locations so later
        // sourcemap/HMR work has a stable source identity anchor.
        var origins = candidate.ComponentSymbol.Locations
            .Where(static location => location.IsInSource)
            .Select(static location => RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Component))
            .ToImmutableArray();
        var importedNamespaces = candidate.ComponentSymbol.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .SelectMany(static declaration =>
                declaration.SyntaxTree.GetRoot() is CompilationUnitSyntax compilationUnit
                    ? compilationUnit.Usings
                    : Enumerable.Empty<UsingDirectiveSyntax>())
            .Where(static directive => directive.Alias is null && directive.Name is not null)
            .Select(static directive => directive.Name!.ToString())
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();

        return new RazorVueSemanticSnapshot(
            Compilation,
            candidate.ComponentSymbol,
            candidate.BuildRenderTreeMethod,
            lifecycle,
            logic,
            descriptor,
            origins,
            importedNamespaces,
            candidate.OnInitializedMethod,
            candidate.OnInitializedAsyncMethod,
            candidate.OnParametersSetMethod,
            candidate.OnParametersSetAsyncMethod,
            candidate.OnAfterRenderMethod,
            candidate.OnAfterRenderAsyncMethod);
    }

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots()
    {
        var builder = ImmutableArray.CreateBuilder<RazorVueSemanticSnapshot>();
        foreach (var candidate in DiscoverComponentCandidates())
            builder.Add(CreateSemanticSnapshot(candidate));

        return builder.ToImmutable();
    }

    public VueComponentRegistry CreateComponentRegistry(ImmutableArray<VueComponentDescriptor> libraryComponents = default(ImmutableArray<VueComponentDescriptor>))
        => VueComponentRegistry.Create(CreateSemanticSnapshots(), libraryComponents);

    private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            yield return typeSymbol;
            foreach (var nestedType in EnumerateNestedTypes(typeSymbol))
                yield return nestedType;
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var childType in EnumerateNamedTypes(childNamespace))
                yield return childType;
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol typeSymbol)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            yield return nestedType;
            foreach (var nestedChild in EnumerateNestedTypes(nestedType))
                yield return nestedChild;
        }
    }
}

