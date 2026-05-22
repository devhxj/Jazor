using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Discovery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue;

internal sealed class RazorVueCompilationContext
{
    public RazorVueCompilationContext(
        Compilation compilation,
        RazorVueCompilationSymbols symbols)
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
        var currentAssembly = Compilation.Assembly;
        foreach (var symbol in EnumerateNamedTypes(Compilation.GlobalNamespace))
        {
            if (RazorVueEntryClassifier.Classify(symbol, Symbols) != RazorVueEntryKind.RazorVueComponent)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, currentAssembly))
                continue;

            if (!HasCurrentCompilationSource(symbol))
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
                RazorVueEntryClassifier.FindLogicProperties(symbol),
                RazorVueEntryClassifier.FindLogicFields(symbol),
                RazorVueEntryKind.RazorVueComponent));
        }

        return builder.ToImmutable();
    }

    public RazorVueSemanticSnapshot CreateSemanticSnapshot(RazorVueComponentCandidate candidate)
        => CreateSemanticSnapshot(candidate, razorIrCarrier: null, razorSourceGeneratorDocument: null);

    internal RazorVueSemanticSnapshot CreateSemanticSnapshot(
        RazorVueComponentCandidate candidate,
        RazorSdk.RazorVueRazorIrCarrier? razorIrCarrier)
        => CreateSemanticSnapshot(candidate, razorIrCarrier, razorSourceGeneratorDocument: null);

    internal RazorVueSemanticSnapshot CreateSemanticSnapshot(
        RazorVueComponentCandidate candidate,
        RazorSdk.RazorVueRazorIrCarrier? razorIrCarrier,
        RazorSdk.RazorVueRazorSourceGeneratorDocument? razorSourceGeneratorDocument,
        ImmutableArray<string> additionalImportedNamespaces = default)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        if (candidate.EntryKind != RazorVueEntryKind.RazorVueComponent)
            throw new InvalidOperationException($"Only {nameof(RazorVueEntryKind.RazorVueComponent)} candidates can become semantic snapshots.");

        var descriptor = ResolveDescriptor(candidate, razorIrCarrier, razorSourceGeneratorDocument);
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
        var logicProperties = candidate.LogicProperties
            .Select(property => new VueLogicPropertyDescriptor(
                property.Name,
                property.SetMethod is null,
                DetermineLogicPropertyLoweringKind(Compilation, property),
                property))
            .ToImmutableArray();
        var logicFields = candidate.LogicFields
            // Preserve Roslyn field carriers for upcoming setup-side lowering.
            .Select(static field => new VueLogicFieldDescriptor(field.Name, field.IsReadOnly, field))
            .ToImmutableArray();
        var logic = logicMethods.IsDefaultOrEmpty && logicProperties.IsDefaultOrEmpty && logicFields.IsDefaultOrEmpty
            ? VueLogicDescriptor.Empty
            : new VueLogicDescriptor(logicProperties, logicFields, logicMethods);
        // Keep the first snapshot carrier tied to Roslyn locations so later
        // sourcemap/HMR work has a stable source identity anchor.
        var origins = candidate.ComponentSymbol.Locations
            .Where(static location => location.IsInSource)
            .Select(static location => RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Component))
            .ToImmutableArray();
        var importedNamespaces = CollectImportedNamespaces(candidate, additionalImportedNamespaces);

        return new RazorVueSemanticSnapshot(
            Compilation,
            candidate.ComponentSymbol,
            candidate.BuildRenderTreeMethod,
            razorIrCarrier,
            razorSourceGeneratorDocument,
            lifecycle,
            logic,
            descriptor,
            origins,
            importedNamespaces,
            candidate.OnInitializedMethod,
            candidate.OnInitializedAsyncMethod,
            candidate.OnParametersSetMethod,
            candidate.OnParametersSetAsyncMethod,
            candidate.ShouldRenderMethod,
            candidate.SetParametersAsyncMethod,
            candidate.OnAfterRenderMethod,
            candidate.OnAfterRenderAsyncMethod,
            candidate.DisposeMethod,
            candidate.DisposeAsyncMethod);
    }

    private static VueLogicPropertyLoweringKind DetermineLogicPropertyLoweringKind(
        Compilation compilation,
        IPropertySymbol property)
    {
        return RazorVueCurrentComponentValueMemberHelper.TryGetSupportedPropertyLoweringKind(compilation, property, out var loweringKind)
            ? loweringKind
            : VueLogicPropertyLoweringKind.Unsupported;
    }

    private VueComponentDescriptor ResolveDescriptor(
        RazorVueComponentCandidate candidate,
        RazorSdk.RazorVueRazorIrCarrier? razorIrCarrier,
        RazorSdk.RazorVueRazorSourceGeneratorDocument? razorSourceGeneratorDocument)
    {
        var descriptor = VueComponentDescriptorFactory.Create(candidate, this);
        var resolvedRouteTemplates = VueComponentDescriptorRouteTemplateResolver.Resolve(
            descriptor.RouteTemplates,
            razorIrCarrier?.DocumentText ?? razorSourceGeneratorDocument?.PrimaryDocument.Text.ToString());
        if (resolvedRouteTemplates.SequenceEqual(descriptor.RouteTemplates))
            return descriptor;

        return descriptor with
        {
            RouteTemplates = resolvedRouteTemplates
        };
    }

    private static ImmutableArray<string> CollectImportedNamespaces(
        RazorVueComponentCandidate candidate,
        ImmutableArray<string> additionalImportedNamespaces)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var importedNamespace in candidate.ComponentSymbol.DeclaringSyntaxReferences
                     .Select(static reference => reference.GetSyntax())
                     .OfType<TypeDeclarationSyntax>()
                     .SelectMany(static declaration =>
                         declaration.SyntaxTree.GetRoot() is CompilationUnitSyntax compilationUnit
                             ? compilationUnit.Usings
                             : Enumerable.Empty<UsingDirectiveSyntax>())
                     .Where(static directive =>
                         directive.Alias is null &&
                         !directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) &&
                         directive.Name is not null)
                     .Select(static directive => directive.Name!.ToString()))
        {
            AddImportedNamespace(importedNamespace);
        }

        if (!additionalImportedNamespaces.IsDefaultOrEmpty)
        {
            foreach (var importedNamespace in additionalImportedNamespaces)
                AddImportedNamespace(importedNamespace);
        }

        return builder.ToImmutable();

        void AddImportedNamespace(string importedNamespace)
        {
            if (string.IsNullOrWhiteSpace(importedNamespace))
                return;

            if (seen.Add(importedNamespace))
                builder.Add(importedNamespace);
        }
    }

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots()
    {
        var builder = ImmutableArray.CreateBuilder<RazorVueSemanticSnapshot>();
        foreach (var candidate in DiscoverComponentCandidates())
            builder.Add(CreateSemanticSnapshot(candidate));

        return builder.ToImmutable();
    }

    public ImmutableArray<VueComponentDescriptor> DiscoverLibraryComponents()
    {
        var builder = ImmutableArray.CreateBuilder<VueComponentDescriptor>();
        foreach (var symbol in EnumerateNamedTypes(Compilation.GlobalNamespace))
        {
            if (!RazorVueEntryClassifier.IsLibraryComponent(symbol, Symbols))
                continue;

            // Library stubs are descriptor-only authoring surfaces discovered
            // from the compilation, not [ECMAScriptModule] runtime entries.
            builder.Add(VueComponentDescriptorFactory.CreateLibraryComponent(symbol, this));
        }

        return builder.ToImmutable();
    }

    public ImmutableArray<VueComponentDescriptor> DiscoverReferencedUserComponents()
    {
        var builder = ImmutableArray.CreateBuilder<VueComponentDescriptor>();
        var currentAssembly = Compilation.Assembly;
        foreach (var assemblySymbol in GetReferencedAssemblies())
        {
            foreach (var symbol in EnumerateNamedTypes(assemblySymbol.GlobalNamespace))
            {
                if (RazorVueEntryClassifier.Classify(symbol, Symbols) != RazorVueEntryKind.RazorVueComponent)
                    continue;

                if (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, currentAssembly))
                    continue;

                builder.Add(VueComponentDescriptorFactory.CreateReferencedUserComponent(symbol, this));
            }
        }

        return builder.ToImmutable();
    }

    public VueComponentRegistry CreateComponentRegistry(ImmutableArray<VueComponentDescriptor> libraryComponents = default(ImmutableArray<VueComponentDescriptor>))
    {
        var userSnapshots = CreateSemanticSnapshots();
        var referencedUserComponents = DiscoverReferencedUserComponents();
        var discoveredLibraryComponents = DiscoverLibraryComponents();
        if (!libraryComponents.IsDefaultOrEmpty)
            discoveredLibraryComponents = MergeLibraryComponents(discoveredLibraryComponents, libraryComponents);

        if (referencedUserComponents.IsDefaultOrEmpty)
            return VueComponentRegistry.Create(userSnapshots, discoveredLibraryComponents);

        var userComponents = userSnapshots.IsDefaultOrEmpty
            ? referencedUserComponents
            : userSnapshots
                .Select(static snapshot => snapshot.Descriptor)
                .Concat(referencedUserComponents)
                .ToImmutableArray();

        return VueComponentRegistry.Create(userComponents, discoveredLibraryComponents);
    }

    private static ImmutableArray<VueComponentDescriptor> MergeLibraryComponents(
        ImmutableArray<VueComponentDescriptor> discoveredLibraryComponents,
        ImmutableArray<VueComponentDescriptor> additionalLibraryComponents)
    {
        if (discoveredLibraryComponents.IsDefaultOrEmpty)
            return additionalLibraryComponents;
        if (additionalLibraryComponents.IsDefaultOrEmpty)
            return discoveredLibraryComponents;

        var builder = ImmutableArray.CreateBuilder<VueComponentDescriptor>();
        var seenFullNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var component in discoveredLibraryComponents)
        {
            if (seenFullNames.Add(component.FullName))
                builder.Add(component);
        }

        foreach (var component in additionalLibraryComponents)
        {
            if (seenFullNames.Add(component.FullName))
                builder.Add(component);
        }

        return builder.ToImmutable();
    }

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

    private IEnumerable<IAssemblySymbol> GetReferencedAssemblies()
    {
        var seen = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);

        foreach (var reference in Compilation.References)
        {
            if (Compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol &&
                seen.Add(assemblySymbol))
            {
                yield return assemblySymbol;
            }
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

    private static bool HasCurrentCompilationSource(INamedTypeSymbol symbol)
        => symbol.Locations.Any(static location => location.IsInSource) ||
           symbol.DeclaringSyntaxReferences.Length > 0;
}
