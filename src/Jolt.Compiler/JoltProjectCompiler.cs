using Jazor.Compiler;
using Jolt.Syntax;
using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Jolt.Compiler;

public static class JoltProjectCompiler
{
    public static IReadOnlyDictionary<string, string> ProjectAuthorDeclarations(IEnumerable<JoltSource> sources,
        JoltCompileOptions options, CancellationToken cancellationToken = default)
    {
        var analysis = Build(sources, options, cancellationToken);
        return analysis.Documents.ToDictionary(document => document.Projection.Document.Path, document =>
        {
            var state = document.Module.GetTypeMembers(document.Projection.StateName).Single();
            return JoltTypeScriptDeclaration.Emit(document.Contract, document.Projection, document.Module,
                state, document.Projection.ExposedMethods(state)).Content;
        }, StringComparer.Ordinal);
    }
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);
    private static readonly SyntaxTree Authoring = CSharpSyntaxTree.ParseText("""
        namespace JoltAuthoring;
        [System.AttributeUsage(System.AttributeTargets.Property)]
        internal sealed class ParameterAttribute : System.Attribute { }
        [System.AttributeUsage(System.AttributeTargets.Property)]
        internal sealed class EditorRequiredAttribute : System.Attribute { }
        [System.AttributeUsage(System.AttributeTargets.Method)]
        internal sealed class ExposeAttribute : System.Attribute { }
        [System.AttributeUsage(System.AttributeTargets.Method)]
        internal sealed class SetupAttribute : System.Attribute { }
        internal sealed class RenderedFragment { }
        internal delegate RenderedFragment RenderFragment();
        internal delegate RenderedFragment RenderFragment<in T>(T value);
        """, ParseOptions);

    internal static readonly Lazy<ImmutableArray<MetadataReference>> PlatformReferences = new(() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray());

    public static JoltAnalysis Analyze(IEnumerable<JoltSource> sources, JoltCompileOptions? options = null, CancellationToken cancellationToken = default)
    {
        var analysis = Build(sources, options ?? new(), cancellationToken);
        return new JoltAnalysis(analysis.Documents.Select(document => document.Public).ToArray(), analysis.Diagnostics)
        {
            LocalComponents = analysis.LocalComponents,
            NpmValueDefinitions = analysis.NpmValues?.Definitions ?? new Dictionary<ISymbol, NpmDefinition>(SymbolEqualityComparer.Default),
        };
    }

    public static JoltCompileResult Compile(IEnumerable<JoltSource> sources, JoltCompileOptions? options = null, CancellationToken cancellationToken = default)
        => CompileAsync(sources, options, cancellationToken).GetAwaiter().GetResult();

    public static async Task<JoltCompileResult> CompileAsync(IEnumerable<JoltSource> sources, JoltCompileOptions? options = null, CancellationToken cancellationToken = default)
    {
        var analysis = Build(sources, options ?? new(), cancellationToken);
        if (analysis.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            return new([], analysis.Diagnostics);

        var modules = new List<JoltModule>();
        foreach (var document in analysis.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var projection = document.Projection;
            var state = document.Module.GetTypeMembers(projection.StateName).Single();
            var host = new VueLowering(projection, document.Contract) { StateType = state,
                VueSourceRootRelative = options?.VueSourceRootRelative ?? "." };
            var optionsForModule = new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: symbol => !symbol.Name.StartsWith(Projection.IntrinsicPrefix, StringComparison.Ordinal) &&
                    !symbol.Name.StartsWith("__JoltScope", StringComparison.Ordinal),
                Host: analysis.NpmValues is { } npmValues ? new CompositeSemanticWalkerHost(host, npmValues) : host,
                RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties);
            try
            {
                var converter = new AstConverter(document.Module, document.Public.SemanticModel, optionsForModule);
                var module = await converter.Convert(cancellationToken).ConfigureAwait(false);
                if (module is null)
                    throw new InvalidOperationException("Jazor did not produce a component state module.");
                modules.Add(host.Emit(module, document.Module, Util.GetConfigOrSymbolName(state.GetMembers(Projection.Render).Single()),
                    projection.ExposedMethods(state), projection.SetupMethod));
            }
            catch (OperationTransformationException error)
            {
                analysis.Diagnostics.Add(Error("JOLT2001", Map(projection, error.SourceLocation), error.Message));
            }
            catch (SymbolTransformationException error)
            {
                analysis.Diagnostics.Add(Error("JOLT2001", Map(projection, error.SourceLocation), error.Message));
            }
        }

        return analysis.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            ? new([], analysis.Diagnostics)
            : new(modules, analysis.Diagnostics) { RouterModule = analysis.RouterModule };
    }

    private static (List<BoundDocument> Documents, List<Diagnostic> Diagnostics, JoltLocalComponent[] LocalComponents,
        JoltNpmValueLowering? NpmValues, string? RouterModule) Build(
        IEnumerable<JoltSource> sources, JoltCompileOptions options, CancellationToken cancellationToken)
    {
        var diagnostics = new List<Diagnostic>();
        var projections = new List<Projection>();
        var names = new HashSet<(string Namespace, string Name)>();
        if (SyntaxFactory.ParseName(options.RootNamespace).ContainsDiagnostics || string.IsNullOrWhiteSpace(options.RootNamespace))
        {
            diagnostics.Add(Error("JOLT1013", Location.None, $"Invalid component namespace '{options.RootNamespace}'."));
            return ([], diagnostics, [], null, null);
        }
        var projectImports = JoltProjectImports.Parse(sources, cancellationToken);
        foreach (var document in projectImports.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (JoltProjectImports.IsImportsFile(document.Path))
            {
                var importText = SourceText.From(document.Source);
                Location ImportLocation(TextSpan span) => Location.Create(document.Path, span, importText.Lines.GetLinePositionSpan(span));
                foreach (var diagnostic in document.Diagnostics)
                    diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, ImportLocation(diagnostic.Span), diagnostic.Message));
                foreach (var node in document.Nodes.Where(node => node switch
                {
                    JoltUsing => false,
                    JoltText text => !string.IsNullOrWhiteSpace(text.Text),
                    _ => true,
                }))
                    diagnostics.Add(Error("JOLT1027", ImportLocation(node.Span), "_Imports.jazor accepts only @using directives."));
                continue;
            }
            var parts = document.Path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            string name = Path.GetFileNameWithoutExtension(parts.LastOrDefault() ?? "");
            string componentNamespace = string.Join('.', parts.SkipLast(1).Prepend(options.RootNamespace));
            var imports = projectImports.ImportsFor(document).DistinctBy(import => import.Code).ToArray();
            var projection = new Projection(document, name, componentNamespace, imports, diagnostics);
            var parameters = new HashSet<string>(StringComparer.Ordinal);
            foreach (var parameter in document.Nodes.OfType<JoltTypeParameter>())
                if (!parameters.Add(parameter.Name))
                    diagnostics.Add(Error("JOLT1020", projection.Location(parameter.NameSpan), $"Type parameter '{parameter.Name}' is declared more than once."));
            foreach (var diagnostic in document.Diagnostics)
            {
                if (options.Router && diagnostic.Id == "JOLT1022" &&
                    document.Nodes.OfType<JoltRouteDirective>().Any(directive => directive.NameSpan == diagnostic.Span))
                    continue;
                diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, projection.Location(diagnostic.Span), diagnostic.Message));
            }
            if (!SyntaxFacts.IsValidIdentifier(name) || parts.SkipLast(1).Any(part => !SyntaxFacts.IsValidIdentifier(part)))
                diagnostics.Add(Error("JOLT1011", projection.Location(default), $"Component path '{document.Path}' must contain valid C# identifiers."));
            else if (!names.Add((componentNamespace, name)))
                diagnostics.Add(Error("JOLT1012", projection.Location(default), $"Component '{componentNamespace}.{name}' is declared in more than one file."));
            else
                projections.Add(projection);
        }

        var references = options.References is null ? PlatformReferences.Value : PlatformReferences.Value.AddRange(options.References);
        var namespacesByPath = projections.ToDictionary(projection => projection.Document.Path, projection => projection.Namespace, StringComparer.Ordinal);
        var valueNamespaces = projections.SelectMany(projection => projection.Imports)
            .Where(import => import.IsNpmValue).Select(import => import.NpmNamespace!).Distinct(StringComparer.Ordinal).ToArray();
        var valueProjection = valueNamespaces.Length != 0 || options.NpmValues.Count != 0
            ? options.NpmProjectionCacheDirectory is { } cache && options.NpmValues.Count != 0
                ? JoltNpmProjectionCache.LoadComponent(cache, valueNamespaces, options.NpmValues, ParseOptions, cancellationToken)
                : new JoltNpmValueProjection(valueNamespaces, options.NpmValues, ParseOptions)
            : null;
        var sharedTrees = new List<SyntaxTree> { Authoring };
        if (valueProjection is not null)
            sharedTrees.Add(valueProjection.Tree);
        if (options.VueComponents.Count != 0)
            sharedTrees.Add(CSharpSyntaxTree.ParseText(string.Join('\n', options.VueComponents.Select(component => component.LocalNamespace)
                .Distinct(StringComparer.Ordinal).Select(ns => $"namespace {ns} {{ }}")), ParseOptions));
        var compilation = CSharpCompilation.Create("JoltComponents", projections.Select(projection => projection.Build(null, ParseOptions)).Concat(sharedTrees),
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var local = projections.Select(projection => ComponentContract.FromLocal(projection,
            compilation.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}+{projection.StateMetadataName}")!, compilation)).ToArray();
        var npm = (options.NpmComponents ?? []).Select(ComponentContract.FromNpm).ToArray();
        var vue = options.VueComponents.Select(ComponentContract.FromNpm).ToArray();
        Dictionary<Projection, IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding>>? typeArguments = null;
        // Bind authored outlet values before the final render projection so consumer slot
        // lambdas can share the compiler's actual C# types, including foreach locals.
        if (local.Any(component => component.Slots.Any(slot => slot.Scope is { Count: > 0 })))
        {
            int diagnosticsBeforeProbe = diagnostics.Count;
            var probeTrees = ProjectTrees();
            var probe = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(probeTrees.Keys.Concat(sharedTrees));
            local = probeTrees.Select(pair => ComponentContract.FromLocal(pair.Value,
                probe.GetTypeByMetadataName($"{pair.Value.Namespace}.{pair.Value.ModuleName}+{pair.Value.StateMetadataName}")!,
                probe, probe.GetSemanticModel(pair.Key))).ToArray();
            diagnostics.RemoveRange(diagnosticsBeforeProbe, diagnostics.Count - diagnosticsBeforeProbe);
        }
        if (local.Concat(npm).Any(component => component.TypeParameters.Count != 0))
        {
            var before = diagnostics.Count;
            var typeTrees = ProjectTrees(probeTypeArguments: true);
            var probe = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(typeTrees.Keys.Concat(sharedTrees));
            typeArguments = typeTrees.ToDictionary(pair => pair.Value, pair => pair.Value.BindTypeArguments(probe.GetSemanticModel(pair.Key)));
            diagnostics.RemoveRange(before, diagnostics.Count - before);

            // A scoped child can supply a nested component's input or forward it
            // to an outlet. Rebind that dependency chain with the preceding pass's
            // specialized scope. Each link occupies a component tag or outlet.
            var genericNames = local.Concat(npm).Where(component => component.TypeParameters.Count != 0)
                .Select(component => component.Name).ToHashSet(StringComparer.Ordinal);
            var nestedSites = projections.Sum(projection => JoltTree.Elements(projection.Document)
                .Where(element => element.Name == "template"
                    ? element.Attributes.Any(attribute => attribute.Name == "context") &&
                        element.Attributes.Any(attribute => attribute.Name.StartsWith('#'))
                    : element.Name.Length > 0 && char.IsUpper(element.Name[0]) &&
                        element.Attributes.Any(attribute => attribute.Name == "Context"))
                .Sum(element => JoltTree.Elements(element.Children)
                    .Count(child => child.Name == "slot" || genericNames.Contains(child.Name))));
            for (int pass = 0; pass <= nestedSites && nestedSites > 0; pass++)
            {
                before = diagnostics.Count;
                typeTrees = ProjectTrees(probeTypeArguments: true);
                probe = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(typeTrees.Keys.Concat(sharedTrees));
                var rebound = typeTrees.ToDictionary(pair => pair.Value, pair => pair.Value.BindTypeArguments(probe.GetSemanticModel(pair.Key)));
                var contracts = typeTrees.Select(pair => ComponentContract.FromLocal(pair.Value,
                    probe.GetTypeByMetadataName($"{pair.Value.Namespace}.{pair.Value.ModuleName}+{pair.Value.StateMetadataName}")!,
                    probe, probe.GetSemanticModel(pair.Key))).ToArray();
                diagnostics.RemoveRange(before, diagnostics.Count - before);
                if (SameTypeArguments(typeArguments, rebound) && SameSlotTypes(local, contracts))
                    break;
                typeArguments = rebound;
                local = contracts;
            }
        }
        var trees = ProjectTrees();
        var localComponents = local.Select(contract => new JoltLocalComponent(contract.Name, contract.Path,
            contract.Properties.Select(property => new JoltLocalProperty(property.Name, property.Type, property.Required, property.Source)).ToArray(),
            contract.Slots.Select(slot => slot.Name).ToArray())
        {
            Namespace = namespacesByPath[contract.Path],
            HasReferenceContract = contract.ReferenceType is not null,
            Models = contract.LocalModels.Select(model =>
                new JoltLocalModel(model.Property, contract.FindModel(model.Property)!.Type)).ToArray(),
        }).ToArray();

        Dictionary<SyntaxTree, Projection> ProjectTrees(bool probeTypeArguments = false)
        {
            var result = new Dictionary<SyntaxTree, Projection>();
            foreach (var projection in projections)
            {
                var imports = projection.Imports.Select(import =>
                    import.NpmNamespace is { } npmNamespace && !import.IsNpmValue ? npmNamespace : import.Code).ToHashSet(StringComparer.Ordinal);
                var components = local.Where(component => IsVisible(projection, namespacesByPath[component.Path], imports))
                    .Concat(vue.Where(component => IsVisible(projection, component.Npm!.LocalNamespace!, imports)))
                    .Concat(npm.Where(component => imports.Contains(NpmNames.NamespaceFor(component.Npm!.ModuleSpecifier)))).ToArray();
                result.Add(projection.Build(components, ParseOptions, typeArguments?.GetValueOrDefault(projection), probeTypeArguments), projection);
            }
            return result;
        }

        compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(trees.Keys.Concat(sharedTrees));
        var reported = new HashSet<(string Id, Location Location, string Message)>();
        foreach (var diagnostic in compilation.GetDiagnostics(cancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            var location = diagnostic.Location;
            if (location.SourceTree is { } tree && trees.TryGetValue(tree, out var projection))
                location = Map(projection, location);
            var message = diagnostic.GetMessage();
            if (reported.Add((diagnostic.Id, location, message)))
                diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, location, message));
        }
        var documents = trees.Select(pair =>
        {
            var projection = pair.Value;
            var model = compilation.GetSemanticModel(pair.Key);
            projection.CheckLiterals(model);
            projection.CheckBindings(model);
            projection.CheckSlotOutlets(model);
            var module = compilation.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}")!;
            var state = module.GetTypeMembers(projection.StateName).Single();
            projection.CheckReferences(model, state);
            projection.CheckSetup(model, state);
            var imports = projection.Imports.Select(import => import.Code).ToHashSet(StringComparer.Ordinal);
            var visible = localComponents.Where(component => IsVisible(projection, component.Namespace, imports)).ToArray();
            return new BoundDocument(projection, new JoltProjectedDocument(projection.Document, model, projection.Spans, visible)
            {
                MarkupBindings = projection.MarkupBindings,
                SlotMemberOrigins = projection.SlotMemberOrigins,
                ExternalSlotMemberOrigins = projection.ExternalSlotMemberOrigins,
                Imports = projection.Imports.Select(import => import.Code).ToArray(),
                VueComponents = options.VueComponents.Where(component => IsVisible(projection, component.LocalNamespace!, imports)).ToArray(),
            }, module,
                local.Single(contract => contract.Path == projection.Document.Path));
        }).ToList();
        var routerModule = options.Router
            ? JoltRouter.Build(documents.Select(document => (document.Projection, document.Contract)).ToArray(), diagnostics)
            : null;
        return (documents, diagnostics, localComponents, valueProjection?.Bind(compilation), routerModule);
    }

    private static bool SameTypeArguments(Dictionary<Projection, IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding>> previous,
        Dictionary<Projection, IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding>> current) =>
        previous.All(document => document.Value.Count == current[document.Key].Count &&
            document.Value.All(binding => current[document.Key].TryGetValue(binding.Key, out var candidate) &&
                binding.Value.Error == candidate.Error && binding.Value.ErrorSource == candidate.ErrorSource &&
                (binding.Value.Arguments ?? []).SequenceEqual(candidate.Arguments ?? [])));

    private static bool SameSlotTypes(ComponentContract[] previous, ComponentContract[] current) =>
        previous.Zip(current).All(pair => pair.First.Slots.Zip(pair.Second.Slots).All(slot =>
            (slot.First.Scope ?? []).Select(member => member.Type).SequenceEqual((slot.Second.Scope ?? []).Select(member => member.Type))));

    internal static bool IsVisible(Projection projection, string componentNamespace, HashSet<string> imports)
        => componentNamespace == projection.Namespace ||
            projection.Namespace.StartsWith(componentNamespace + ".", StringComparison.Ordinal) ||
            imports.Contains(componentNamespace);

    internal static Diagnostic Error(string id, Location location, string message) => CreateDiagnostic(id, DiagnosticSeverity.Error, location, message);

    private static Diagnostic CreateDiagnostic(string id, DiagnosticSeverity severity, Location location, string message)
        => Diagnostic.Create(new DiagnosticDescriptor(id, "Jolt compilation", "{0}", "Jolt", severity, true), location, message);

    private static Location Map(Projection projection, Location location)
        => location.IsInSource ? projection.MapLocation(location.SourceSpan) : projection.Location(default);

    private sealed record BoundDocument(Projection Projection, JoltProjectedDocument Public, INamedTypeSymbol Module, ComponentContract Contract);
}
