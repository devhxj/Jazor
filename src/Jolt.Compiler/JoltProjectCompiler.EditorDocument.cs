using System.Runtime.CompilerServices;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jolt.Compiler;

public static partial class JoltProjectCompiler
{
    private static readonly ConditionalWeakTable<JoltSource, SyntaxTree> SharedSourceTrees = new();

    private static SyntaxTree ParseSharedSource(JoltSource source, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return SharedSourceTrees.GetValue(source, input =>
            CSharpSyntaxTree.ParseText(input.Content, ParseOptions, input.Path, cancellationToken: cancellationToken));
    }

    private static SyntaxTree[] ParseSharedSources(IReadOnlyList<JoltSource> sources, CancellationToken cancellationToken)
    {
        var seen = new HashSet<JoltSource>(ReferenceEqualityComparer.Instance);
        // Roslyn requires distinct tree instances even when an explicit caller
        // includes the same source object twice.
        return sources.Select(source => seen.Add(source) ? ParseSharedSource(source, cancellationToken) :
            CSharpSyntaxTree.ParseText(source.Content, ParseOptions, source.Path, cancellationToken: cancellationToken)).ToArray();
    }

    private static SyntaxTree[] ParseProjectSources(JoltCompileOptions options, CancellationToken cancellationToken,
        bool parallel = false)
    {
        if (options.ProjectDefines.Count == 0)
            return ParseSharedSources(options.ProjectSources, cancellationToken);
        var parseOptions = ParseOptions.WithPreprocessorSymbols(options.ProjectDefines);
        if (!parallel || options.ProjectSources.Count < 16)
            return options.ProjectSources.Select(Parse).ToArray();
        // Keep original tree order while parsing host files before the first editor bind.
        var trees = new SyntaxTree[options.ProjectSources.Count];
        Parallel.For(0, trees.Length, new ParallelOptions { CancellationToken = cancellationToken },
            index => trees[index] = Parse(options.ProjectSources[index]));
        return trees;

        SyntaxTree Parse(JoltSource source) => CSharpSyntaxTree.ParseText(
            source.Content, parseOptions, source.Path, cancellationToken: cancellationToken);
    }

    internal static JoltProjectedDocument? ProjectEditorDocument(JoltSource source, IReadOnlyList<JoltSource> sources,
        JoltCompileOptions options, CancellationToken cancellationToken)
    {
        if (JoltProjectImports.IsImportsFile(source.Path) ||
            string.IsNullOrWhiteSpace(options.RootNamespace) || SyntaxFactory.ParseName(options.RootNamespace).ContainsDiagnostics)
            return null;

        var sourcePath = Normalize(source.Path);
        var ancestors = sources.Where(candidate => JoltProjectImports.IsImportsFile(candidate.Path) &&
            sourcePath.StartsWith(Normalize(candidate.Path)[..^"_Imports.jazor".Length], StringComparison.Ordinal));
        var projectImports = JoltProjectImports.Parse(ancestors.Append(source), cancellationToken);
        var document = JoltProjectImports.ParseDocument(source);
        var imports = projectImports.ImportsFor(document).DistinctBy(import => import.Code).ToArray();
        if (imports.Any(import => import.IsNpmValue))
            return null;

        var parts = Parts(source.Path);
        var name = Path.GetFileNameWithoutExtension(parts.LastOrDefault() ?? "");
        if (!SyntaxFacts.IsValidIdentifier(name) || parts.SkipLast(1).Any(part => !SyntaxFacts.IsValidIdentifier(part)))
            return null;

        var componentNamespace = ComponentNamespace(parts);
        var generatedNames = new HashSet<string>(StringComparer.Ordinal);
        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (var other in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (JoltProjectImports.IsImportsFile(other.Path) || Normalize(other.Path) == sourcePath)
                continue;
            var otherParts = Parts(other.Path);
            var otherName = Path.GetFileNameWithoutExtension(otherParts.LastOrDefault() ?? "");
            if (!SyntaxFacts.IsValidIdentifier(otherName) || otherParts.SkipLast(1).Any(part => !SyntaxFacts.IsValidIdentifier(part)))
                continue;
            var otherNamespace = ComponentNamespace(otherParts);
            if (otherNamespace == componentNamespace && otherName == name)
                return null;
            if (MightDeclareExtension(other))
                return null;
            namespaces.Add(otherNamespace);
            generatedNames.Add("__Module" + otherName);
            generatedNames.Add(otherName + "Ref");
        }

        var projection = new Projection(document, name, componentNamespace, imports, []);
        var tree = projection.Build(null, ParseOptions);
        if (HasOmittedType(tree))
            return null;
        var references = options.References is null ? PlatformReferences.Value : PlatformReferences.Value.AddRange(options.References);
        var compilation = CSharpCompilation.Create("JoltComponents", references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        // Reference identities and source text can be loaded independently.
        var bindReferences = options.ProjectSources.Count >= 16
            ? Task.Run(() => compilation.GetSpecialType(SpecialType.System_Object), cancellationToken)
            : null;
        var projectTrees = ParseProjectSources(options, cancellationToken, parallel: true);
        var explicitTrees = ParseSharedSources(options.CSharpSources, cancellationToken);
        bindReferences?.GetAwaiter().GetResult();
        // Inspect the exact trees that will bind, including the host's active
        // preprocessor branches. Reusing them avoids parsing suspicious files twice.
        if (options.CSharpSources.Select((input, index) => (input, index))
                .Any(entry => MightNameOmittedType(entry.input.Content) && HasOmittedType(explicitTrees[entry.index])) ||
            options.ProjectSources.Select((input, index) => (input, index))
                .Any(entry => MightNameOmittedType(entry.input.Content) && HasOmittedType(projectTrees[entry.index])))
            return null;
        var sharedTrees = explicitTrees.Concat(projectTrees);
        var namespaceTree = CSharpSyntaxTree.ParseText(string.Join('\n', namespaces.Order(StringComparer.Ordinal)
            .Select(value => $"namespace {value} {{ }}")), ParseOptions, cancellationToken: cancellationToken);
        var compilationTrees = sharedTrees.Prepend(tree).Append(Authoring).Append(namespaceTree);
        if (options.NpmValues.Count != 0)
            compilationTrees = compilationTrees.Append(Jazor.Npm.NpmProjectionCache.LoadComponent(
                Jazor.Npm.NpmProjectionCache.ProjectionDirectory, [], options.NpmValues, ParseOptions, cancellationToken).Tree);
        return new(document, compilation.AddSyntaxTrees(compilationTrees).GetSemanticModel(tree), projection.Spans.ToArray(), []);

        // Avoid materializing Roslyn red nodes for shared sources that cannot name
        // a generated module or reference type. Escaped identifiers still need
        // token inspection because ValueText decodes their source spelling.
        bool MightNameOmittedType(string content) =>
            HasUnusualIdentifierSpelling(content) ||
            ((content.Contains("__Module", StringComparison.Ordinal) || content.Contains("Ref", StringComparison.Ordinal)) &&
                generatedNames.Any(name => content.Contains(name, StringComparison.Ordinal)));
        bool HasOmittedType(SyntaxTree input) => input.GetRoot(cancellationToken).DescendantTokens()
            .Any(token => token.IsKind(SyntaxKind.IdentifierToken) && generatedNames.Contains(token.ValueText));
        string ComponentNamespace(string[] path) => string.Join('.', path.SkipLast(1).Prepend(options.RootNamespace));
        static string Normalize(string path) => path.Replace('\\', '/');
        static string[] Parts(string path) => path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool MightDeclareExtension(JoltSource source)
    {
        // The text check only skips impossible files. Razor owns code-block
        // boundaries, including comments and inline templates.
        if (!source.Content.Contains("this", StringComparison.Ordinal) &&
            !source.Content.Contains("extension", StringComparison.Ordinal) &&
            !HasUnusualIdentifierSpelling(source.Content))
            return false;

        // Inline Razor templates can change C# parser recovery. Defer to the full
        // projection whenever an author block could declare a receiver.
        return JoltProjectImports.ParseDocument(source).Nodes.OfType<JoltCode>().Any(code =>
            code.Code.Contains("this", StringComparison.Ordinal) || code.Code.Contains("extension", StringComparison.Ordinal) ||
            HasUnusualIdentifierSpelling(code.Code));
    }

    private static bool HasUnusualIdentifierSpelling(string content) =>
        content.Contains("\\u", StringComparison.Ordinal) || content.Contains("\\U", StringComparison.Ordinal) ||
        content.Any(character => character >= '\u00ad' &&
            (char.GetUnicodeCategory(character) == System.Globalization.UnicodeCategory.Format || char.IsSurrogate(character)));
}
