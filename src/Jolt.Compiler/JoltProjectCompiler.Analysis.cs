using Jazor.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jolt.Compiler;

public static partial class JoltProjectCompiler
{
    internal static (JoltAnalysis Analysis, AnalysisState? State) AnalyzeEditor(
        IReadOnlyList<JoltSource> sources, JoltCompileOptions options, IReadOnlySet<string> diagnosticPaths,
        AnalysisState? previous, CancellationToken cancellationToken, Action<JoltAnalysis>? authorDeclarationsReady = null)
    {
        if (previous is not null &&
            previous.TryUpdate(sources, options, diagnosticPaths, cancellationToken) is { } updated)
            return updated;

        AnalysisState? state = null;
        var build = Build(sources, options, cancellationToken, diagnosticPaths: diagnosticPaths,
            rememberAnalysis: snapshot => state = snapshot, authorDeclarationsReady: authorDeclarationsReady);
        var analysis = CreateAnalysis(build, options);
        state?.Remember(sources, options, diagnosticPaths, analysis);
        return (analysis, state);
    }

    internal sealed class AnalysisState
    {
        private readonly CSharpCompilation _compilation;
        private readonly Dictionary<Projection, SyntaxTree> _authorTrees;
        private readonly List<BoundDocument> _documents;
        private readonly NpmValueProjection? _valueProjection;
        private readonly IReadOnlyList<Diagnostic> _baseDiagnostics;
        private readonly bool _canUpdateSource;
        private readonly Dictionary<string, (List<Diagnostic> Compiler, List<Diagnostic> Validation)> _documentDiagnostics = new(StringComparer.Ordinal);
        private IReadOnlyList<JoltSource> _sources = [];
        private JoltCompileOptions _options = new();
        private IReadOnlySet<string> _diagnosticPaths = new HashSet<string>();
        private JoltAnalysis _analysis = null!;

        internal AnalysisState(CSharpCompilation compilation, Dictionary<Projection, SyntaxTree> authorTrees,
            List<BoundDocument> documents, NpmValueProjection? valueProjection,
            IReadOnlyList<Diagnostic> baseDiagnostics, bool canUpdateSource)
        {
            _compilation = compilation;
            _authorTrees = authorTrees;
            _documents = documents;
            _valueProjection = valueProjection;
            _baseDiagnostics = baseDiagnostics;
            _canUpdateSource = canUpdateSource;
        }

        internal void Remember(IReadOnlyList<JoltSource> sources, JoltCompileOptions options,
            IReadOnlySet<string> diagnosticPaths, JoltAnalysis analysis)
        {
            _sources = sources;
            _options = options;
            _diagnosticPaths = diagnosticPaths;
            _analysis = analysis;
        }

        internal (JoltAnalysis, AnalysisState)? TryUpdate(IReadOnlyList<JoltSource> sources, JoltCompileOptions options,
            IReadOnlySet<string> diagnosticPaths, CancellationToken cancellationToken)
        {
            if (sources.Count != _sources.Count || !SameOptions(_options, options))
                return null;

            var edits = _sources.Zip(sources).Where(pair => pair.First != pair.Second).Take(2).ToArray();
            if (edits.Length == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_diagnosticPaths.SetEquals(diagnosticPaths))
                    return (_analysis, this);
                var selected = new AnalysisState(_compilation, _authorTrees, _documents, _valueProjection, _baseDiagnostics, _canUpdateSource);
                var selectedDiagnostics = selected.CollectDiagnostics(options, diagnosticPaths, cancellationToken, this);
                // Opening a file changes its diagnostics. All symbols, origins and completion
                // contracts still belong to the same compilation and can be shared verbatim.
                var selectedAnalysis = _analysis with { Diagnostics = selectedDiagnostics };
                selected.Remember(sources, options, diagnosticPaths, selectedAnalysis);
                return (selectedAnalysis, selected);
            }
            if (!_canUpdateSource || !_diagnosticPaths.SetEquals(diagnosticPaths))
                return null;
            if (edits is not [{ First: var before, Second: var after }] || before.Path != after.Path ||
                JoltProjectImports.IsImportsFile(after.Path))
                return null;

            int index = _documents.FindIndex(document => document.Projection.Document.Path == after.Path);
            if (index < 0)
                return null;

            var previous = _documents[index].Projection;
            var parsed = JoltProjectImports.ParseDocument(after);
            // Slot types and their origins are part of a consumer's render projection.
            // Route directives participate in project-wide validation and diagnostics.
            if (_documents[index].Contract.Slots.Count != 0 || ComponentContract.SlotOutlets(parsed).Any() ||
                options.Router && (parsed.Nodes.OfType<JoltRouteDirective>().Any() ||
                    previous.Document.Nodes.OfType<JoltRouteDirective>().Any()))
                return null;

            var imports = previous.Imports.Where(import => import.Document != previous.Document)
                .Concat(parsed.Nodes.OfType<JoltUsing>().Select(directive => new JoltImport(parsed, directive)))
                .DistinctBy(import => import.Code).ToArray();
            if (!imports.Select(import => import.Code).SequenceEqual(previous.Imports.Select(import => import.Code)))
                return null;

            // Semantic errors in inherited directives map to _Imports.jazor. A
            // standalone edit cannot replace those publications by document path.
            if (_analysis.Diagnostics.Any(diagnostic => JoltProjectImports.IsImportsFile(diagnostic.Location.GetMappedLineSpan().Path)))
                return null;
            var diagnostics = _baseDiagnostics.Where(diagnostic => diagnostic.Location.GetMappedLineSpan().Path != after.Path).ToList();
            var projection = new Projection(parsed, previous.Name, previous.Namespace, imports, diagnostics);
            AddSourceDiagnostics(projection, options, diagnostics);
            var authorTree = projection.Build(null, ParseOptions);
            var previousAuthorTree = _authorTrees[previous];
            if (!HasSameAuthorDeclarations(previousAuthorTree, authorTree, cancellationToken))
                return null;

            var tree = projection.Build(previous.VisibleComponents, ParseOptions);
            var compilation = _compilation.ReplaceSyntaxTree(_documents[index].Model.SyntaxTree, tree);
            var model = compilation.GetSemanticModel(tree);
            var module = compilation.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}")!;
            var state = module.GetTypeMembers(projection.StateName).Single();
            var contract = ComponentContract.FromLocal(projection, state, compilation);
            var changed = new BoundDocument(projection, model, module, contract);

            // Bind each model to the same compilation so references/rename see
            // the new tree even when their source document itself did not change.
            var documents = _documents.Select((document, position) => position == index ? changed :
                document with
                {
                    Model = compilation.GetSemanticModel(document.Model.SyntaxTree),
                    Module = compilation.Assembly.GetTypeByMetadataName($"{document.Projection.Namespace}.{document.Projection.ModuleName}")!,
                }).ToList();
            var authorTrees = new Dictionary<Projection, SyntaxTree>(_authorTrees);
            authorTrees.Remove(previous);
            authorTrees.Add(projection, authorTree);
            var next = new AnalysisState(compilation, authorTrees, documents, _valueProjection, diagnostics.ToArray(), canUpdateSource: true);
            diagnostics = next.CollectDiagnostics(options, diagnosticPaths, cancellationToken, this, after.Path);
            var analysis = CreateAnalysis(new(documents, diagnostics, _valueProjection?.Bind(compilation), null), options,
                _analysis.LocalComponents, index);
            next.Remember(sources, options, diagnosticPaths, analysis);
            return (analysis, next);
        }

        internal List<Diagnostic> CollectDiagnostics(JoltCompileOptions options, IReadOnlySet<string> paths,
            CancellationToken cancellationToken, AnalysisState? previous = null, string? changedPath = null)
        {
            // Source and projection diagnostics belong to the binding. Semantic
            // validation depends on the documents selected by the editor.
            var diagnostics = _baseDiagnostics.ToList();
            // Carry closed-document entries forward only while their binding is identical.
            if (previous is not null)
                foreach (var (path, entry) in previous._documentDiagnostics)
                    if (changedPath is null || path != changedPath && paths.Contains(path))
                        _documentDiagnostics.Add(path, entry);

            var selected = new List<(List<Diagnostic> Compiler, List<Diagnostic> Validation)>();
            foreach (var document in _documents.Where(document => paths.Contains(document.Projection.Document.Path)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = document.Projection.Document.Path;
                if (!_documentDiagnostics.TryGetValue(path, out var entry))
                {
                    var model = document.Model;
                    entry = ([], []);
                    AddCompilerDiagnostics(model.GetDiagnostics(cancellationToken: cancellationToken), [model],
                        new() { [model.SyntaxTree] = document.Projection }, entry.Compiler, cancellationToken);
                    ValidateBoundDocument(document, options, _valueProjection, entry.Validation, cancellationToken, prepareLowering: false);
                    _documentDiagnostics.Add(path, entry);
                }
                selected.Add(entry);
            }
            // A bad inherited using can be reported by several selected models.
            // Deduplicate after mapping so its diagnostic stays on _Imports.jazor.
            diagnostics.AddRange(selected.SelectMany(entry => entry.Compiler)
                .DistinctBy(diagnostic => (diagnostic.Id, diagnostic.Location, diagnostic.GetMessage())));
            diagnostics.AddRange(selected.SelectMany(entry => entry.Validation));
            return diagnostics;
        }

        private static bool HasSameAuthorDeclarations(SyntaxTree previous, SyntaxTree current, CancellationToken cancellationToken)
        {
            if (previous.GetText(cancellationToken).ContentEquals(current.GetText(cancellationToken)))
                return true;
            if (!previous.IsEquivalentTo(current, topLevel: true))
                return false;
            var previousRoot = previous.GetRoot(cancellationToken);
            var currentRoot = current.GetRoot(cancellationToken);
            // Top-level equivalence tolerates executable bodies. Constant member
            // values and directive placement can affect binding in other files.
            return !HasAuthorDirectives(previousRoot) && !HasAuthorDirectives(currentRoot) &&
                Constants(previousRoot).SequenceEqual(Constants(currentRoot));

            static bool HasAuthorDirectives(SyntaxNode root) =>
                root.DescendantTrivia().Where(trivia => trivia.IsDirective).Skip(1).Any(); // Projection starts with #nullable enable.

            static IEnumerable<string> Constants(SyntaxNode root) =>
                root.DescendantNodes().OfType<FieldDeclarationSyntax>()
                    .Where(field => field.Modifiers.Any(SyntaxKind.ConstKeyword)).Select(field => field.ToString());
        }

        private static bool SameOptions(JoltCompileOptions left, JoltCompileOptions right) =>
            left.RootNamespace == right.RootNamespace && left.Router == right.Router &&
            (left.References ?? []).SequenceEqual(right.References ?? []) &&
            (left.NpmComponents ?? []).SequenceEqual(right.NpmComponents ?? []) &&
            left.NpmValues.SequenceEqual(right.NpmValues) && left.VueComponents.SequenceEqual(right.VueComponents) &&
            left.CSharpSources.SequenceEqual(right.CSharpSources) &&
            left.ProjectSources.SequenceEqual(right.ProjectSources) &&
            left.ProjectDefines.SequenceEqual(right.ProjectDefines);
    }
}
