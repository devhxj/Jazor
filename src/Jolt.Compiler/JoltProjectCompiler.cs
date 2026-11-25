using Jazor.Npm;
using Jazor.Analyzer;
using Jazor.Compiler;
using Jolt.Syntax;
using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Jolt.Compiler;

public static partial class JoltProjectCompiler
{
    public static IReadOnlyDictionary<string, string> ProjectAuthorDeclarations(IEnumerable<JoltSource> sources,
        JoltCompileOptions options, CancellationToken cancellationToken = default)
    {
        var declarations = new Dictionary<string, string>(StringComparer.Ordinal);
        Build(sources, options, cancellationToken, declarations);
        return declarations;
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
        => Analyze(sources, options ?? new(), cancellationToken, diagnosticPaths: null);

    internal static JoltAnalysis Analyze(IEnumerable<JoltSource> sources, JoltCompileOptions options, CancellationToken cancellationToken,
        IReadOnlySet<string>? diagnosticPaths)
        => CreateAnalysis(Build(sources, options, cancellationToken, diagnosticPaths: diagnosticPaths), options);

    internal static IEnumerable<(JoltDocument Document, SemanticModel SemanticModel, IReadOnlyList<JoltProjectionSpan> Spans)>
        ProjectDocumentsForNpm(IEnumerable<JoltSource> sources, JoltCompileOptions options,
            IReadOnlySet<string> aliasPaths, CancellationToken cancellationToken) =>
        Build(sources, options, cancellationToken, aliasPaths: aliasPaths).Documents.Select(document =>
            (document.Projection.Document, document.Model, (IReadOnlyList<JoltProjectionSpan>)document.Projection.Spans));

    private static JoltAnalysis CreateAnalysis(BuildResult analysis, JoltCompileOptions options)
    {
        var locals = analysis.Documents.Select(document => document.ToLocalComponent()).ToArray();
        return new JoltAnalysis(analysis.Documents.Select(document => document.ToProjectedDocument(locals, options)).ToArray(), analysis.Diagnostics)
        {
            LocalComponents = locals,
            NpmValueDefinitions = analysis.NpmValues?.Definitions ?? new Dictionary<ISymbol, NpmDefinition>(SymbolEqualityComparer.Default),
        };
    }

    public static JoltCompileResult Compile(IEnumerable<JoltSource> sources, JoltCompileOptions? options = null, CancellationToken cancellationToken = default)
        => CompileAsync(sources, options, cancellationToken).GetAwaiter().GetResult();

    public static Task<JoltCompileResult> CompileAsync(IEnumerable<JoltSource> sources, JoltCompileOptions? options = null, CancellationToken cancellationToken = default)
        => CompileAsync(sources, options ?? new(), cancellationToken, null);

    internal static async Task<JoltCompileResult> CompileAsync(IEnumerable<JoltSource> sources, JoltCompileOptions options,
        CancellationToken cancellationToken, Action<DevSnapshot>? remember)
    {
        var analysis = Build(sources, options, cancellationToken, capture: remember is not null);
        if (analysis.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            return new([], analysis.Diagnostics);

        var modules = new List<JoltModule>();
        foreach (var document in analysis.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await EmitDocumentAsync(document, options, analysis.NpmValues, analysis.Diagnostics, cancellationToken).ConfigureAwait(false) is { } module)
                modules.Add(module);
        }

        if (analysis.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            return new([], analysis.Diagnostics);
        if (analysis.Snapshot is { } snapshot)
            remember?.Invoke(snapshot);
        return new(modules, analysis.Diagnostics) { RouterModule = analysis.RouterModule };
    }

    private static async Task<JoltModule?> EmitDocumentAsync(BoundDocument document, JoltCompileOptions options,
        NpmValueLowering? npmValues, List<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        var projection = document.Projection;
        var state = document.Module.GetTypeMembers(projection.StateName).Single();
        var host = new VueLowering(projection, document.Contract) { StateType = state, VueSourceRootRelative = options.VueSourceRootRelative };
        var optionsForModule = new AstConverterOptions(AstConverterProfile.Standard,
            MemberFilter: symbol => !symbol.Name.StartsWith(Projection.IntrinsicPrefix, StringComparison.Ordinal) &&
                !symbol.Name.StartsWith("__JoltScope", StringComparison.Ordinal),
            Host: npmValues is null ? host : new CompositeSemanticWalkerHost(host, npmValues),
            RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties);
        try
        {
            var module = await new AstConverter(document.Module, document.Model, optionsForModule).Convert(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Jazor did not produce a component state module.");
            return host.Emit(module, document.Module, Util.GetConfigOrSymbolName(state.GetMembers(Projection.Render).Single()),
                projection.ExposedMethods(state), projection.SetupMethod);
        }
        catch (OperationTransformationException error)
        {
            diagnostics.Add(Error("JOLT2001", Map(projection, error.SourceLocation), error.Message));
        }
        catch (SymbolTransformationException error)
        {
            diagnostics.Add(Error("JOLT2001", Map(projection, error.SourceLocation), error.Message));
        }
        return null;
    }

    private sealed record BuildResult(List<BoundDocument> Documents, List<Diagnostic> Diagnostics,
        NpmValueLowering? NpmValues, string? RouterModule, DevSnapshot? Snapshot = null);

    private static BuildResult Build(IEnumerable<JoltSource> sources, JoltCompileOptions options, CancellationToken cancellationToken,
        Dictionary<string, string>? declarations = null, bool capture = false, IReadOnlySet<string>? diagnosticPaths = null,
        Action<AnalysisState>? rememberAnalysis = null, IReadOnlySet<string>? aliasPaths = null)
    {
        var diagnostics = new List<Diagnostic>();
        var projections = new List<Projection>();
        var names = new HashSet<(string Namespace, string Name)>();
        if (SyntaxFactory.ParseName(options.RootNamespace).ContainsDiagnostics || string.IsNullOrWhiteSpace(options.RootNamespace))
        {
            diagnostics.Add(Error("JOLT1013", Location.None, $"Invalid component namespace '{options.RootNamespace}'."));
            return new([], diagnostics, null, null);
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
            AddSourceDiagnostics(projection, options, diagnostics);
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
            ? options.NpmValues.Count != 0
                ? NpmProjectionCache.LoadComponent(NpmProjectionCache.ProjectionDirectory, valueNamespaces, options.NpmValues, ParseOptions, cancellationToken)
                : new NpmValueProjection(valueNamespaces, options.NpmValues, ParseOptions)
            : null;
        var sharedSourceTrees = options.CSharpSources.Select(source => CSharpSyntaxTree.ParseText(
            source.Content, ParseOptions, source.Path, cancellationToken: cancellationToken)).ToArray();
        var sharedTrees = new List<SyntaxTree> { Authoring };
        sharedTrees.AddRange(sharedSourceTrees);
        if (valueProjection is not null)
            sharedTrees.Add(valueProjection.Tree);
        if (options.VueComponents.Count != 0)
            sharedTrees.Add(CSharpSyntaxTree.ParseText(string.Join('\n', options.VueComponents.Select(component => component.LocalNamespace)
                .Distinct(StringComparer.Ordinal).Select(ns => $"namespace {ns} {{ }}")), ParseOptions));
        var authorTrees = projections.ToDictionary(projection => projection, projection =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return projection.Build(null, ParseOptions);
        });
        var compilation = CSharpCompilation.Create("JoltComponents", authorTrees.Values.Concat(sharedTrees),
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var local = projections.Select(projection => ComponentContract.FromLocal(projection,
            compilation.Assembly.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}+{projection.StateMetadataName}")!, compilation)).ToArray();
        // The declaration pass already types literal and boolean outlet fields.
        var slotProbePaths = local.Where(component => component.Slots.Any(slot =>
            slot.Scope?.Any(member => member.LocalType is null) == true))
            .Select(component => component.Path).ToHashSet(StringComparer.Ordinal);
        bool needsSlotProbe = slotProbePaths.Count != 0;
        // Vue imports need component declarations only. Without expression-valued
        // outlet fields, the author trees already contain the whole public contract.
        if (declarations is not null && !needsSlotProbe)
        {
            WriteAuthorDeclarations(projections, local, compilation, declarations);
            return new([], diagnostics, null, null);
        }

        var npm = (options.NpmComponents ?? []).Select(ComponentContract.FromNpm).ToArray();
        var npmNamespaces = npm.Select(component =>
            (Namespace: NpmNames.NamespaceFor(component.Npm!.ModuleSpecifier), Component: component)).ToArray();
        var vue = options.VueComponents.Select(ComponentContract.FromNpm).ToArray();
        Dictionary<Projection, IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding>>? typeArguments = null;
        // Bind authored outlet values before the final render projection so consumer slot
        // lambdas can share the compiler's actual C# types, including foreach locals.
        if (needsSlotProbe)
        {
            int diagnosticsBeforeProbe = diagnostics.Count;
            // Outlet types use their author's render body. Keep the other files'
            // declarations available for references during the probe.
            var probeTrees = ProjectTrees(renderPaths: slotProbePaths);
            var probe = compilation;
            foreach (var (tree, projection) in probeTrees)
                probe = probe.ReplaceSyntaxTree(authorTrees[projection], tree);
            if (declarations is null)
            {
                // Render consumers only use the authored type names. Contracts
                // without outlet expressions have already been bound in this pass.
                var rebound = probeTrees.ToDictionary(pair => pair.Value.Document.Path, pair =>
                    ComponentContract.FromLocal(pair.Value,
                        probe.Assembly.GetTypeByMetadataName($"{pair.Value.Namespace}.{pair.Value.ModuleName}+{pair.Value.StateMetadataName}")!,
                        probe, probe.GetSemanticModel(pair.Key)), StringComparer.Ordinal);
                local = local.Select(contract => rebound.GetValueOrDefault(contract.Path) ?? contract).ToArray();
            }
            else
            {
                // The TypeScript declaration writer compares symbols directly,
                // so bind its contracts in one compilation.
                var probeTreesByProjection = probeTrees.ToDictionary(pair => pair.Value, pair => pair.Key);
                local = projections.Select(projection => ComponentContract.FromLocal(projection,
                    probe.Assembly.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}+{projection.StateMetadataName}")!,
                    probe, probeTreesByProjection.TryGetValue(projection, out var tree) ? probe.GetSemanticModel(tree) : null)).ToArray();
            }
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
                    probe.Assembly.GetTypeByMetadataName($"{pair.Value.Namespace}.{pair.Value.ModuleName}+{pair.Value.StateMetadataName}")!,
                    probe, probe.GetSemanticModel(pair.Key))).ToArray();
                diagnostics.RemoveRange(before, diagnostics.Count - before);
                if (SameTypeArguments(typeArguments, rebound) && SameSlotTypes(local, contracts))
                    break;
                typeArguments = rebound;
                local = contracts;
            }
        }
        var canCapture = capture && !needsSlotProbe && typeArguments is null &&
            local.All(contract => contract.Slots.All(slot => slot.Scope is not { Count: > 0 }));
        var trees = ProjectTrees(renderPaths: aliasPaths);
        Dictionary<SyntaxTree, Projection> ProjectTrees(bool probeTypeArguments = false, IReadOnlySet<string>? renderPaths = null)
        {
            var result = new Dictionary<SyntaxTree, Projection>();
            foreach (var projection in projections)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (renderPaths is not null && !renderPaths.Contains(projection.Document.Path))
                    continue;
                var imports = projection.Imports.Select(import =>
                    import.NpmNamespace is { } npmNamespace && !import.IsNpmValue ? npmNamespace : import.Code).ToHashSet(StringComparer.Ordinal);
                var components = local.Where(component => IsVisible(projection, namespacesByPath[component.Path], imports))
                    .Concat(vue.Where(component => IsVisible(projection, component.Npm!.LocalNamespace!, imports)))
                    .Concat(npmNamespaces.Where(entry => imports.Contains(entry.Namespace)).Select(entry => entry.Component)).ToArray();
                result.Add(projection.Build(components, ParseOptions, typeArguments?.GetValueOrDefault(projection), probeTypeArguments), projection);
            }
            return result;
        }

        if (aliasPaths is null)
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(trees.Keys.Concat(sharedTrees));
        else
        {
            // npm alias discovery reads only importing files. Other files still
            // supply their C# declarations and the slot probe runs independently.
            foreach (var (tree, projection) in trees)
                compilation = compilation.ReplaceSyntaxTree(authorTrees[projection], tree);
            var contractsByPath = local.ToDictionary(contract => contract.Path, StringComparer.Ordinal);
            var aliases = trees.Select(pair => BindDocument(compilation, pair.Key, pair.Value,
                contractsByPath[pair.Value.Document.Path], options, valueProjection, diagnostics, cancellationToken, validate: false)).ToList();
            return new(aliases, diagnostics, null, null);
        }
        if (declarations is not null)
        {
            WriteAuthorDeclarations(trees.Values, local, compilation, declarations);
            return new([], diagnostics, null, null);
        }

        var localByPath = local.ToDictionary(contract => contract.Path, StringComparer.Ordinal);
        var diagnosticTrees = diagnosticPaths is null ? trees.Keys.Concat(sharedSourceTrees) :
            trees.Where(pair => diagnosticPaths.Contains(pair.Value.Document.Path)).Select(pair => pair.Key);
        var models = diagnosticTrees.Select(tree => compilation.GetSemanticModel(tree)).ToArray();
        AddCompilerDiagnostics(diagnosticPaths is null
            ? compilation.GetDiagnostics(cancellationToken).AsEnumerable()
            : models.SelectMany(model => model.GetDiagnostics(cancellationToken: cancellationToken)), models, trees, diagnostics, cancellationToken);
        var documents = trees.Select(pair =>
        {
            return BindDocument(compilation, pair.Key, pair.Value, localByPath[pair.Value.Document.Path],
                options, valueProjection, diagnostics, cancellationToken, diagnosticPaths?.Contains(pair.Value.Document.Path) ?? true);
        }).ToList();
        var routerModule = options.Router
            ? JoltRouter.Build(documents.Select(document => (document.Projection, document.Contract)).ToArray(), diagnostics)
            : null;
        if (rememberAnalysis is not null && typeArguments is null)
            rememberAnalysis(new(compilation, authorTrees, documents, valueProjection));
        return new(documents, diagnostics, valueProjection?.Bind(compilation), routerModule,
            canCapture && diagnostics.Count == 0 ? DevSnapshot.Create(compilation, trees, valueProjection) : null);
    }

    private static void AddSourceDiagnostics(Projection projection, JoltCompileOptions options, List<Diagnostic> diagnostics)
    {
        var document = projection.Document;
        var parameters = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parameter in document.Nodes.OfType<JoltTypeParameter>())
            if (!parameters.Add(parameter.Name))
                diagnostics.Add(Error("JOLT1020", projection.Location(parameter.NameSpan), $"Type parameter '{parameter.Name}' is declared more than once."));
        foreach (var diagnostic in document.Diagnostics)
        {
            if (HandledByRouter(options, document, diagnostic))
                continue;
            diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, projection.Location(diagnostic.Span), diagnostic.Message));
        }
    }

    private static void AddCompilerDiagnostics(IEnumerable<Diagnostic> compilerDiagnostics, IEnumerable<SemanticModel> models,
        Dictionary<SyntaxTree, Projection> trees, List<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        var reported = new HashSet<(string Id, Location Location, string Message)>();
        foreach (var diagnostic in compilerDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Concat(models.SelectMany(model => AnalyzeJavaScriptNames(model, cancellationToken))))
        {
            var location = diagnostic.Location;
            if (location.SourceTree is { } tree && trees.TryGetValue(tree, out var projection))
                location = Map(projection, location);
            var message = diagnostic.GetMessage();
            if (reported.Add((diagnostic.Id, location, message)))
                diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, location, message));
        }
    }

    private static BoundDocument BindDocument(CSharpCompilation compilation, SyntaxTree tree, Projection projection,
        ComponentContract contract, JoltCompileOptions options,
        NpmValueProjection? valueProjection, List<Diagnostic> diagnostics, CancellationToken cancellationToken, bool validate = true)
    {
        var model = compilation.GetSemanticModel(tree);
        var module = compilation.Assembly.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}")!;
        var bound = new BoundDocument(projection, model, module, contract);
        if (!validate)
            return bound;
        if (valueProjection?.HasPropertyPaths == true || options.References is { Count: > 0 })
        {
            foreach (var syntax in tree.GetRoot(cancellationToken).DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (model.GetOperation(syntax, cancellationToken) is not IInvocationOperation invocation)
                    continue;
                foreach (var argument in invocation.Arguments)
                {
                    if (argument.Parameter is { } parameter && PropertyPathSelector.IsPathParameter(parameter) &&
                        PropertyPathSelector.GetError(argument) is { } error)
                        diagnostics.Add(Error("JOLT2001", Map(projection, argument.Value.Syntax.GetLocation()), error));
                }
            }
        }
        projection.CheckLiterals(model);
        projection.CheckRecordArrays(model);
        projection.CheckBindings(model);
        projection.CheckSlotOutlets(model);
        var state = module.GetTypeMembers(projection.StateName).Single();
        projection.CheckReferences(model, state);
        projection.CheckSetup(model, state);
        return bound;
    }

    private static bool HandledByRouter(JoltCompileOptions options, JoltDocument document, JoltDiagnostic diagnostic) =>
        options.Router && diagnostic.Id == "JOLT1022" &&
        document.Nodes.OfType<JoltRouteDirective>().Any(directive => directive.NameSpan == diagnostic.Span);

    private static void WriteAuthorDeclarations(IEnumerable<Projection> projections, ComponentContract[] contracts,
        CSharpCompilation compilation, Dictionary<string, string> declarations)
    {
        var byPath = contracts.ToDictionary(contract => contract.Path, StringComparer.Ordinal);
        foreach (var projection in projections)
        {
            var module = compilation.Assembly.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}")!;
            var state = module.GetTypeMembers(projection.StateName).Single();
            var path = projection.Document.Path;
            declarations.Add(path, JoltTypeScriptDeclaration.Emit(byPath[path], projection, module, state,
                projection.ExposedMethods(state)).Content);
        }
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

    private static IEnumerable<Diagnostic> AnalyzeJavaScriptNames(SemanticModel model, CancellationToken cancellationToken)
    {
        foreach (var attribute in model.SyntaxTree.GetRoot(cancellationToken).DescendantNodes().OfType<AttributeSyntax>())
        {
            if (JavaScriptNameAnalyzer.AnalyzeAttribute(model, attribute, cancellationToken) is { } diagnostic)
                yield return diagnostic;
        }
    }

    internal sealed record BoundDocument(Projection Projection, SemanticModel Model, INamedTypeSymbol Module, ComponentContract Contract)
    {
        internal JoltLocalComponent ToLocalComponent() => new(Contract.Name, Contract.Path,
            Contract.Properties.Select(property => new JoltLocalProperty(property.Name, property.Type, property.Required, property.Source)).ToArray(),
            Contract.Slots.Select(slot => slot.Name).ToArray())
        {
            Namespace = Projection.Namespace,
            HasReferenceContract = Contract.ReferenceType is not null,
            Models = Contract.LocalModels.Select(model => new JoltLocalModel(model.Property, Contract.FindModel(model.Property)!.Type)).ToArray(),
        };

        internal JoltProjectedDocument ToProjectedDocument(JoltLocalComponent[] locals, JoltCompileOptions options)
        {
            var imports = Projection.Imports.Select(import => import.Code).ToArray();
            var namespaces = imports.ToHashSet(StringComparer.Ordinal);
            return new(Projection.Document, Model, Projection.Spans, locals.Where(local => IsVisible(Projection, local.Namespace, namespaces)).ToArray())
            {
                MarkupBindings = Projection.MarkupBindings,
                SlotMemberOrigins = Projection.SlotMemberOrigins,
                ExternalSlotMemberOrigins = Projection.ExternalSlotMemberOrigins,
                Imports = imports,
                VueComponents = options.VueComponents.Where(component => IsVisible(Projection, component.LocalNamespace!, namespaces)).ToArray(),
            };
        }
    }
}
