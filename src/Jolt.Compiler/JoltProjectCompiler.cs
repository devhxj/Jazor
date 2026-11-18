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
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);
    private static readonly SyntaxTree Authoring = CSharpSyntaxTree.ParseText("""
        namespace JoltAuthoring;
        [System.AttributeUsage(System.AttributeTargets.Property)]
        internal sealed class ParameterAttribute : System.Attribute { }
        [System.AttributeUsage(System.AttributeTargets.Property)]
        internal sealed class EditorRequiredAttribute : System.Attribute { }
        """, ParseOptions);

    private static readonly Lazy<ImmutableArray<MetadataReference>> PlatformReferences = new(() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray());

    public static JoltAnalysis Analyze(IEnumerable<JoltSource> sources, JoltCompileOptions? options = null, CancellationToken cancellationToken = default)
    {
        var analysis = Build(sources, options ?? new(), cancellationToken);
        return new JoltAnalysis(analysis.Documents.Select(document => document.Public).ToArray(), analysis.Diagnostics)
        {
            LocalComponents = analysis.LocalComponents,
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
            var host = new VueLowering(projection, document.Contract);
            var optionsForModule = new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: symbol => !symbol.Name.StartsWith(Projection.IntrinsicPrefix, StringComparison.Ordinal) &&
                    !symbol.Name.StartsWith("__JoltScope", StringComparison.Ordinal),
                Host: host,
                RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties);
            try
            {
                var converter = new AstConverter(document.Module, document.Public.SemanticModel, optionsForModule);
                var module = await converter.Convert(cancellationToken).ConfigureAwait(false);
                if (module is null)
                    throw new InvalidOperationException("Jazor did not produce a component state module.");
                var state = document.Module.GetTypeMembers(projection.StateName).Single();
                modules.Add(host.Emit(module, Util.GetConfigOrSymbolName(state.GetMembers(Projection.Render).Single())));
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
            : new(modules, analysis.Diagnostics);
    }

    private static (List<BoundDocument> Documents, List<Diagnostic> Diagnostics, JoltLocalComponent[] LocalComponents) Build(
        IEnumerable<JoltSource> sources, JoltCompileOptions options, CancellationToken cancellationToken)
    {
        var diagnostics = new List<Diagnostic>();
        var projections = new List<Projection>();
        var names = new HashSet<(string Namespace, string Name)>();
        if (SyntaxFactory.ParseName(options.RootNamespace).ContainsDiagnostics || string.IsNullOrWhiteSpace(options.RootNamespace))
        {
            diagnostics.Add(Error("JOLT1013", Location.None, $"Invalid component namespace '{options.RootNamespace}'."));
            return ([], diagnostics, []);
        }
        foreach (var source in sources.OrderBy(source => source.Path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = JoltParser.Parse(source.Content, source.Path);
            var parts = source.Path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            string name = Path.GetFileNameWithoutExtension(parts.LastOrDefault() ?? "");
            string componentNamespace = string.Join('.', parts.SkipLast(1).Prepend(options.RootNamespace));
            var projection = new Projection(document, name, componentNamespace, diagnostics);
            foreach (var extra in document.Nodes.OfType<JoltTypeParameter>().Skip(1))
                diagnostics.Add(Error("JOLT1020", projection.Location(extra.Span), "A Jolt component can declare one type parameter."));
            foreach (var diagnostic in document.Diagnostics)
                diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, projection.Location(diagnostic.Span), diagnostic.Message));
            if (!SyntaxFacts.IsValidIdentifier(name) || parts.SkipLast(1).Any(part => !SyntaxFacts.IsValidIdentifier(part)))
                diagnostics.Add(Error("JOLT1011", projection.Location(default), $"Component path '{source.Path}' must contain valid C# identifiers."));
            else if (!names.Add((componentNamespace, name)))
                diagnostics.Add(Error("JOLT1012", projection.Location(default), $"Component '{componentNamespace}.{name}' is declared in more than one file."));
            else
                projections.Add(projection);
        }

        var references = options.References is null ? PlatformReferences.Value : PlatformReferences.Value.AddRange(options.References);
        var namespacesByPath = projections.ToDictionary(projection => projection.Document.Path, projection => projection.Namespace, StringComparer.Ordinal);
        var compilation = CSharpCompilation.Create("JoltComponents", projections.Select(projection => projection.Build(null, ParseOptions)).Append(Authoring),
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var local = projections.Select(projection => ComponentContract.FromLocal(projection,
            compilation.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}+{projection.StateMetadataName}")!)).ToArray();
        var npm = (options.NpmComponents ?? []).Select(ComponentContract.FromNpm).ToArray();
        Dictionary<Projection, IReadOnlyDictionary<int, string?>>? typeArguments = null;
        // Bind authored outlet values before the final render projection so consumer slot
        // lambdas can share the compiler's actual C# types, including foreach locals.
        if (local.Any(component => component.Slots.Any(slot => slot.Scope is { Count: > 0 })))
        {
            int diagnosticsBeforeProbe = diagnostics.Count;
            var probeTrees = ProjectTrees();
            var probe = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(probeTrees.Keys.Append(Authoring));
            local = probeTrees.Select(pair => ComponentContract.FromLocal(pair.Value,
                probe.GetTypeByMetadataName($"{pair.Value.Namespace}.{pair.Value.ModuleName}+{pair.Value.StateMetadataName}")!,
                probe.GetSemanticModel(pair.Key))).ToArray();
            diagnostics.RemoveRange(diagnosticsBeforeProbe, diagnostics.Count - diagnosticsBeforeProbe);
        }
        if (local.Concat(npm).Any(component => component.TypeParameter is not null))
        {
            var before = diagnostics.Count;
            var typeTrees = ProjectTrees();
            var probe = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(typeTrees.Keys.Append(Authoring));
            typeArguments = typeTrees.ToDictionary(pair => pair.Value, pair => pair.Value.BindTypeArguments(probe.GetSemanticModel(pair.Key)));
            diagnostics.RemoveRange(before, diagnostics.Count - before);
        }
        var trees = ProjectTrees();
        var localComponents = local.Select(contract => new JoltLocalComponent(contract.Name, contract.Path,
            contract.Properties.Select(property => new JoltLocalProperty(property.Name, property.Type, property.Required, property.Source)).ToArray(),
            contract.Slots.Select(slot => slot.Name).ToArray())
        {
            Namespace = namespacesByPath[contract.Path],
        }).ToArray();

        Dictionary<SyntaxTree, Projection> ProjectTrees()
        {
            var result = new Dictionary<SyntaxTree, Projection>();
            foreach (var projection in projections)
            {
                var imports = projection.Document.Nodes.OfType<JoltUsing>()
                    .Select(import => import.Code.Trim().TrimEnd(';')).ToHashSet(StringComparer.Ordinal);
                var components = local.Where(component => IsVisible(projection, namespacesByPath[component.Path], imports))
                    .Concat(npm.Where(component => imports.Contains(NpmNames.NamespaceFor(component.Npm!.ModuleSpecifier)))).ToArray();
                result.Add(projection.Build(components, ParseOptions, typeArguments?.GetValueOrDefault(projection)), projection);
            }
            return result;
        }

        compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(trees.Keys.Append(Authoring));
        foreach (var diagnostic in compilation.GetDiagnostics(cancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            var location = diagnostic.Location;
            if (location.SourceTree is { } tree && trees.TryGetValue(tree, out var projection))
                location = Map(projection, location);
            diagnostics.Add(CreateDiagnostic(diagnostic.Id, diagnostic.Severity, location, diagnostic.GetMessage()));
        }
        var documents = trees.Select(pair =>
        {
            var projection = pair.Value;
            var model = compilation.GetSemanticModel(pair.Key);
            projection.CheckLiterals(model);
            var module = compilation.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}")!;
            var imports = projection.Document.Nodes.OfType<JoltUsing>()
                .Select(import => import.Code.Trim().TrimEnd(';')).ToHashSet(StringComparer.Ordinal);
            var visible = localComponents.Where(component => IsVisible(projection, component.Namespace, imports)).ToArray();
            return new BoundDocument(projection, new JoltProjectedDocument(projection.Document, model, projection.Spans, visible)
            {
                MarkupBindings = projection.MarkupBindings,
            }, module,
                local.Single(contract => contract.Path == projection.Document.Path));
        }).ToList();
        return (documents, diagnostics, localComponents);
    }

    private static bool IsVisible(Projection projection, string componentNamespace, HashSet<string> imports)
        => componentNamespace == projection.Namespace ||
            projection.Namespace.StartsWith(componentNamespace + ".", StringComparison.Ordinal) ||
            imports.Contains(componentNamespace);

    internal static Diagnostic Error(string id, Location location, string message) => CreateDiagnostic(id, DiagnosticSeverity.Error, location, message);

    private static Diagnostic CreateDiagnostic(string id, DiagnosticSeverity severity, Location location, string message)
        => Diagnostic.Create(new DiagnosticDescriptor(id, "Jolt compilation", "{0}", "Jolt", severity, true), location, message);

    private static Location Map(Projection projection, Location location)
        => projection.Location(location.IsInSource ? projection.Map(location.SourceSpan) ?? default : default);

    private sealed record BoundDocument(Projection Projection, JoltProjectedDocument Public, INamedTypeSymbol Module, ComponentContract Contract);
}
