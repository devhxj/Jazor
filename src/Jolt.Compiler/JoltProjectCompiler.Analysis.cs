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
        AnalysisState? previous, CancellationToken cancellationToken)
    {
        if (previous is not null &&
            previous.TryUpdate(sources, options, diagnosticPaths, cancellationToken) is { } updated)
            return updated;

        AnalysisState? state = null;
        var build = Build(sources, options, cancellationToken, diagnosticPaths: diagnosticPaths,
            rememberAnalysis: snapshot => state = snapshot);
        var analysis = CreateAnalysis(build, options);
        state?.Remember(sources, options, diagnosticPaths, analysis.Diagnostics);
        return (analysis, state);
    }

    internal sealed class AnalysisState
    {
        private readonly CSharpCompilation _compilation;
        private readonly Dictionary<Projection, SyntaxTree> _authorTrees;
        private readonly List<BoundDocument> _documents;
        private readonly NpmValueProjection? _valueProjection;
        private IReadOnlyList<JoltSource> _sources = [];
        private JoltCompileOptions _options = new();
        private IReadOnlySet<string> _diagnosticPaths = new HashSet<string>();
        private IReadOnlyList<Diagnostic> _diagnostics = [];

        internal AnalysisState(CSharpCompilation compilation, Dictionary<Projection, SyntaxTree> authorTrees,
            List<BoundDocument> documents, NpmValueProjection? valueProjection)
        {
            _compilation = compilation;
            _authorTrees = authorTrees;
            _documents = documents;
            _valueProjection = valueProjection;
        }

        internal void Remember(IReadOnlyList<JoltSource> sources, JoltCompileOptions options,
            IReadOnlySet<string> diagnosticPaths, IReadOnlyList<Diagnostic> diagnostics)
        {
            _sources = sources;
            _options = options;
            _diagnosticPaths = diagnosticPaths;
            _diagnostics = diagnostics;
        }

        internal (JoltAnalysis, AnalysisState)? TryUpdate(IReadOnlyList<JoltSource> sources, JoltCompileOptions options,
            IReadOnlySet<string> diagnosticPaths, CancellationToken cancellationToken)
        {
            if (sources.Count != _sources.Count || !_diagnosticPaths.SetEquals(diagnosticPaths) ||
                !SameOptions(_options, options))
                return null;

            var edits = _sources.Zip(sources).Where(pair => pair.First != pair.Second).Take(2).ToArray();
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
            if (_diagnostics.Any(diagnostic => JoltProjectImports.IsImportsFile(diagnostic.Location.GetMappedLineSpan().Path)))
                return null;
            var diagnostics = _diagnostics.Where(diagnostic => diagnostic.Location.GetMappedLineSpan().Path != after.Path).ToList();
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
            bool validate = diagnosticPaths.Contains(after.Path);
            if (validate)
                AddCompilerDiagnostics(model.GetDiagnostics(cancellationToken: cancellationToken), [model],
                    new() { [tree] = projection }, diagnostics, cancellationToken);
            var changed = BindDocument(compilation, tree, projection, contract, options, _valueProjection, diagnostics, cancellationToken, validate);

            // Bind each model to the same compilation so references/rename see
            // the new tree even when their source document itself did not change.
            var documents = _documents.Select((document, position) => position == index ? changed :
                document with { Model = compilation.GetSemanticModel(document.Model.SyntaxTree) }).ToList();
            var analysis = CreateAnalysis(new(documents, diagnostics, _valueProjection?.Bind(compilation), null), options);
            var authorTrees = new Dictionary<Projection, SyntaxTree>(_authorTrees);
            authorTrees.Remove(previous);
            authorTrees.Add(projection, authorTree);
            var next = new AnalysisState(compilation, authorTrees, documents, _valueProjection);
            next.Remember(sources, options, diagnosticPaths, diagnostics);
            return (analysis, next);
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
            left.CSharpSources.SequenceEqual(right.CSharpSources);
    }
}
