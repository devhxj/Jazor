using Jazor.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jolt.Compiler;

public static partial class JoltProjectCompiler
{
    internal sealed class DevSnapshot
    {
        private sealed record Document(string Path, string Name, string Namespace, SyntaxTree Tree,
            JoltImport[] InheritedImports, ComponentContract[] VisibleContracts);

        private readonly Document[] _documents;
        private readonly NpmValueProjection? _values;
        internal CSharpCompilation Compilation { get; }

        private DevSnapshot(CSharpCompilation compilation, Document[] documents, NpmValueProjection? values) =>
            (Compilation, _documents, _values) = (compilation, documents, values);

        internal static DevSnapshot Create(CSharpCompilation compilation, Dictionary<SyntaxTree, Projection> trees, NpmValueProjection? values)
        {
            // Render projection reads property type names. Retaining their Roslyn symbols
            // or a bound Projection would keep the declaration compilation alive. Local
            // scoped slots, which would also carry symbols, already require a full bind.
            var locals = new Dictionary<string, ComponentContract>(StringComparer.Ordinal);
            ComponentContract RenderContract(ComponentContract contract)
            {
                if (contract.Npm is not null)
                    return contract;
                if (!locals.TryGetValue(contract.Path, out var shape))
                    locals.Add(contract.Path, shape = contract with
                    {
                        Properties = contract.Properties.Select(property => property with { LocalType = null }).ToArray(),
                    });
                return shape;
            }

            var documents = trees.Select(pair =>
            {
                var projection = pair.Value;
                return new Document(projection.Document.Path, projection.Name, projection.Namespace, pair.Key,
                    projection.Imports.Where(import => import.Document != projection.Document).ToArray(),
                    projection.VisibleComponents.Select(RenderContract).ToArray());
            }).ToArray();
            return new(compilation, documents, values);
        }

        internal async Task<(DevSnapshot Snapshot, JoltCompileResult Result)?> TryUpdateAsync(JoltSource source,
            JoltCompileOptions options, JoltCompileResult previous, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Documents and modules both follow the successful build's source order.
            int index = Array.FindIndex(_documents, item => item.Path == source.Path);
            if (index < 0)
                return null;
            for (int i = 0; i < previous.Modules.Count; i++)
                if (i != index && JoltDevCompilation.ReferencesSource(previous.Modules[i], source.Path))
                    return null;

            var parsed = JoltProjectImports.ParseDocument(source);
            if (parsed.Diagnostics.Any(diagnostic => !HandledByRouter(options, parsed, diagnostic)))
                return null;
            var document = _documents[index];
            var diagnostics = new List<Diagnostic>();
            var imports = document.InheritedImports.Concat(parsed.Nodes.OfType<JoltUsing>().Select(directive => new JoltImport(parsed, directive)))
                .DistinctBy(import => import.Code).ToArray();
            var projection = new Projection(parsed, document.Name, document.Namespace, imports, diagnostics);
            var newTree = projection.Build(document.VisibleContracts, ParseOptions);
            if (WithoutRenderBody(document.Tree) is not { } before || WithoutRenderBody(newTree) is not { } after ||
                !before.IsEquivalentTo(after))
                return null;

            var next = Compilation.ReplaceSyntaxTree(document.Tree, newTree);
            if (next.GetSemanticModel(newTree).GetDiagnostics(cancellationToken: cancellationToken)
                .Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
                return null;
            var state = next.GetTypeByMetadataName($"{projection.Namespace}.{projection.ModuleName}+{projection.StateMetadataName}")!;
            var contract = ComponentContract.FromLocal(projection, state, next);
            var bound = BindDocument(next, newTree, projection, contract, options, _values, diagnostics, cancellationToken);
            if (diagnostics.Count != 0)
                return null;
            var module = await EmitDocumentAsync(bound, options, _values?.Bind(next), diagnostics, cancellationToken).ConfigureAwait(false);
            if (module is null)
                return null;

            // Only a literal inside a rendered element has changed. Route directives,
            // layout selection, slot outlets and parameter declarations are outside it.
            var modules = previous.Modules.ToArray();
            modules[index] = module;
            var documents = (Document[])_documents.Clone();
            documents[index] = document with { Tree = newTree };
            return (new(next, documents, _values), new(modules, diagnostics) { RouterModule = previous.RouterModule });

            SyntaxNode? WithoutRenderBody(SyntaxTree tree)
            {
                var root = (CompilationUnitSyntax)tree.GetRoot(cancellationToken);
                var renders = root.Members.OfType<FileScopedNamespaceDeclarationSyntax>()
                    .Where(ns => ns.Name.ToString() == projection.Namespace)
                    .SelectMany(ns => ns.Members.OfType<ClassDeclarationSyntax>())
                    .Where(module => module.Identifier.ValueText == projection.ModuleName)
                    .SelectMany(module => module.Members.OfType<ClassDeclarationSyntax>())
                    .Where(stateClass => stateClass.Identifier.ValueText == projection.StateName)
                    .SelectMany(stateClass => stateClass.Members.OfType<MethodDeclarationSyntax>())
                    .Where(method => method.Identifier.ValueText == Projection.Render && method.ParameterList.Parameters.Count == 0)
                    .Take(2).ToArray();
                return renders is [{ Body: { } body }] ? root.ReplaceNode(body, SyntaxFactory.Block()) : null;
            }
        }
    }
}
