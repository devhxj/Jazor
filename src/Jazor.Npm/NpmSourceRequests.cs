using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.Npm;

public sealed record NpmSourceRequest(string NpmNamespace, string? MemberName, Location Location, bool IsUnqualified = false);

public static class NpmSourceRequests
{
    public static IReadOnlyList<NpmSourceRequest> Discover(CSharpCompilation compilation,
        IEnumerable<string> packages, CancellationToken cancellationToken = default)
    {
        var prefixes = packages.Select(NpmNames.NamespaceFor).Distinct(StringComparer.Ordinal).ToArray();
        if (prefixes.Length == 0)
            return [];

        var roots = GetRoots(compilation, cancellationToken);
        var probe = CreateProbe(compilation, roots, prefixes, cancellationToken, out var markers, out var candidates);
        if (probe is null)
            return [];

        var usings = roots.SelectMany(GetUsings).ToArray();
        var aliasNames = new HashSet<string>(usings.Where(directive => directive.Alias is not null)
            .Select(directive => directive.Alias!.Name.Identifier.ValueText), StringComparer.Ordinal);
        var staticImports = usings.Where(directive => directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
            .Select(directive => (Directive: directive, Namespace: StaticImport(directive)))
            .Where(import => import.Namespace is not null)
            .ToDictionary(import => import.Directive, import => import.Namespace);
        var globalImports = staticImports.Where(item => item.Key.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
            .Select(item => item.Value).ToArray();
        var results = new List<NpmSourceRequest>();
        foreach (var root in roots)
        {
            var model = probe.GetSemanticModel(root.SyntaxTree);
            foreach (var directive in GetUsings(root))
            {
                var ns = directive.Name is null ? null : MarkerNamespace(model.GetSymbolInfo(directive.Name, cancellationToken).Symbol);
                if (ns is not null)
                    results.Add(new(ns, null, directive.GetLocation()));
            }
            foreach (var identifier in root.DescendantNodes().OfType<SimpleNameSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = identifier.Identifier.ValueText;
                SyntaxNode? receiver = identifier.Parent switch
                {
                    MemberAccessExpressionSyntax member when member.Name == identifier => member.Expression,
                    QualifiedNameSyntax qualified when qualified.Right == identifier => qualified.Left,
                    _ => null,
                };
                if (receiver is not null)
                {
                    if (!CouldBeExports(receiver, aliasNames))
                        continue;
                    var ns = MarkerNamespace(model.GetSymbolInfo(receiver, cancellationToken).Symbol, requireExports: true);
                    if (ns is not null)
                        results.Add(new(ns, name, identifier.GetLocation()));
                    continue;
                }

                if (staticImports.Count == 0 ||
                    identifier.Parent is MemberAccessExpressionSyntax or QualifiedNameSyntax or AliasQualifiedNameSyntax or NameColonSyntax or NameEqualsSyntax ||
                    identifier.Ancestors().Any(node => node is UsingDirectiveSyntax))
                    continue;
                var scopedUsings = identifier.Ancestors().SelectMany(node => node switch
                {
                    CompilationUnitSyntax unit => unit.Usings,
                    BaseNamespaceDeclarationSyntax ns => ns.Usings,
                    _ => default,
                });
                var namespaces = globalImports
                    .Concat(scopedUsings.Where(directive => !directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                        .Select(directive => staticImports.TryGetValue(directive, out var imported) ? imported : null))
                    .OfType<string>().Distinct(StringComparer.Ordinal).ToArray();
                if (namespaces.Length == 0)
                    continue;
                var symbol = model.GetSymbolInfo(identifier, cancellationToken);
                if (symbol.Symbol is not null || symbol.CandidateSymbols.Length != 0)
                    continue;
                foreach (var ns in namespaces)
                    results.Add(new(ns, name, identifier.GetLocation(), IsUnqualified: true));
            }
        }
        return results;

        string? MarkerNamespace(ISymbol? symbol, bool requireExports = false)
        {
            if (symbol is INamedTypeSymbol { Name: "Exports" } type &&
                type.Locations.Any(location => location.SourceTree == markers))
                return type.ContainingNamespace.ToDisplayString();
            if (!requireExports && symbol is INamespaceSymbol ns && candidates.Contains(ns.ToDisplayString()))
                return ns.ToDisplayString();
            return null;
        }

        string? StaticImport(UsingDirectiveSyntax directive) =>
            directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) && directive.Name is not null
                ? MarkerNamespace(probe.GetSemanticModel(directive.SyntaxTree).GetSymbolInfo(directive.Name, cancellationToken).Symbol, requireExports: true)
                : null;
    }

    internal static string? FindCompletionNamespace(CSharpCompilation compilation, ExpressionSyntax receiver,
        CancellationToken cancellationToken)
    {
        if (receiver is not (NameSyntax or MemberAccessExpressionSyntax))
            return null;
        var symbol = compilation.GetSemanticModel(receiver.SyntaxTree).GetSymbolInfo(receiver, cancellationToken);
        if (symbol.Symbol is { } bound && bound is not IErrorTypeSymbol)
        {
            // Roslyn already has the live projection after an earlier use. For a local, a user
            // type or a referenced DLL, built-in completion owns the receiver and its exact members.
            if (bound is INamedTypeSymbol { Name: "Exports" } type && type.Locations.Any(location =>
                string.Equals(Path.GetFileName(location.SourceTree?.FilePath), EditorSourceHint, StringComparison.Ordinal)))
                return type.ContainingNamespace.ToDisplayString();
            return null;
        }
        if (symbol.CandidateSymbols.Length > 0)
            return null;

        var probe = CreateProbe(compilation, GetRoots(compilation, cancellationToken), null, cancellationToken, out var markers, out _, receiver);
        return probe?.GetSemanticModel(receiver.SyntaxTree).GetSymbolInfo(receiver, cancellationToken).Symbol
            is INamedTypeSymbol { Name: "Exports" } marker && marker.Locations.Any(location => location.SourceTree == markers)
                ? marker.ContainingNamespace.ToDisplayString() : null;
    }

    internal const string EditorSourceHint = "Jazor.Npm.Editor.g.cs";

    private static SyntaxNode[] GetRoots(CSharpCompilation compilation, CancellationToken cancellationToken) =>
        compilation.SyntaxTrees.Where(tree => tree.Options.Kind == SourceCodeKind.Regular && !IsGeneratedCatalog(tree.FilePath))
            .Select(tree => tree.GetRoot(cancellationToken)).ToArray();

    private static bool IsGeneratedCatalog(string path)
    {
        const string catalog = "Jazor.Compiler/Jazor.Compiler.ESGenerator/Jazor.Generated.ModuleCatalog.g.cs";
        if (!path.EndsWith("Jazor.Generated.ModuleCatalog.g.cs", StringComparison.Ordinal))
            return false;
        var normalized = path.Replace('\\', '/');
        return normalized == catalog || normalized.EndsWith("/" + catalog, StringComparison.Ordinal);
    }

    private static bool CouldBeExports(SyntaxNode receiver, HashSet<string> aliases) => receiver switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText == "Exports" || aliases.Contains(identifier.Identifier.ValueText),
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText == "Exports",
        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText == "Exports",
        AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText == "Exports",
        _ => false,
    };

    private static IEnumerable<UsingDirectiveSyntax> GetUsings(SyntaxNode root) =>
        root.DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).OfType<UsingDirectiveSyntax>();

    private static CSharpCompilation? CreateProbe(CSharpCompilation compilation, SyntaxNode[] roots, string[]? prefixes,
        CancellationToken cancellationToken, out SyntaxTree? markers, out HashSet<string> candidates, ExpressionSyntax? receiver = null)
    {
        var usings = roots.SelectMany(GetUsings).ToArray();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var aliases = usings.Where(directive => directive.Alias is not null && directive.Name is not null)
            .ToLookup(directive => directive.Alias!.Name.Identifier.ValueText, directive => directive.Name!,
                StringComparer.Ordinal);
        bool IsPackageNamespace(string value) => prefixes is null ? value.StartsWith("Npm.", StringComparison.Ordinal) : prefixes.Any(prefix =>
            value == prefix || value.StartsWith(prefix + ".", StringComparison.Ordinal));

        // Collect marker candidates lexically, then make every decision about scope, aliases and
        // shadowing through Roslyn. A marker carries no members and cannot satisfy actual calls.
        var importedNamespaces = usings.Where(directive => directive.Name is not null && directive.Alias is null &&
                !directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
            .SelectMany(directive => Expand(directive.Name!))
            .Where(value => value == "Npm" || value.StartsWith("Npm.", StringComparison.Ordinal))
            .Concat(new[] { "Npm" }).Distinct(StringComparer.Ordinal).ToArray();
        var nodes = usings.Select(directive => directive.Name).OfType<SyntaxNode>();
        nodes = receiver is null
            ? nodes.Concat(roots.SelectMany(root => root.DescendantNodes()).Where(node =>
                node is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Exports" } ||
                node is QualifiedNameSyntax { Right.Identifier.ValueText: "Exports" } ||
                node is AliasQualifiedNameSyntax { Name.Identifier.ValueText: "Exports" }))
            : nodes.Concat(new[] { receiver });
        foreach (var node in nodes)
        foreach (var name in Expand(node).SelectMany(name => name.StartsWith("Npm.", StringComparison.Ordinal)
                     ? new[] { name }
                     : importedNamespaces.Select(prefix => prefix + "." + name).Concat(new[] { name })))
        {
            var index = name.IndexOf(".Exports", StringComparison.Ordinal);
            if (index >= 0 && (index + 8 == name.Length || name[index + 8] == '.'))
                AddCandidate(name.Substring(0, index));
            else if (node.Parent is UsingDirectiveSyntax)
                AddCandidate(name);
        }
        candidates = names;
        if (names.Count == 0)
        {
            markers = null;
            return null;
        }

        var source = string.Join("\n", names.OrderBy(name => name, StringComparer.Ordinal)
            .Select(ns => $"namespace {string.Join(".", ns.Split('.').Select(part => "@" + part))} {{ public static class Exports {{ }} }}"));
        // Roslyn requires every tree in this compilation to use the consumer's language version.
        markers = CSharpSyntaxTree.ParseText(source, (CSharpParseOptions)roots[0].SyntaxTree.Options,
            cancellationToken: cancellationToken);
        return compilation.AddSyntaxTrees(markers);

        void AddCandidate(string ns)
        {
            if (!IsPackageNamespace(ns) || names.Contains(ns))
                return;
            var parts = ns.Split('.');
            if (parts.All(SyntaxFacts.IsValidIdentifier) && !HasVisibleExports(compilation.GlobalNamespace, parts))
                names.Add(ns);
        }

        IEnumerable<string> Expand(SyntaxNode node, HashSet<string>? visited = null)
        {
            if (DottedName(node) is not { } name)
                yield break;
            if (name.StartsWith("global::", StringComparison.Ordinal))
            {
                yield return name.Substring(8);
                yield break;
            }
            yield return name;
            var separator = name.IndexOf('.');
            var first = separator < 0 ? name : name.Substring(0, separator);
            if (!aliases.Contains(first))
                yield break;
            visited ??= new(StringComparer.Ordinal);
            if (!visited.Add(first))
                yield break;
            foreach (var target in aliases[first].SelectMany(target => Expand(target, visited)))
                if (target == "Npm" || target.StartsWith("Npm.", StringComparison.Ordinal))
                    yield return separator < 0 ? target : target + name.Substring(separator);
            visited.Remove(first);
        }
    }

    internal static bool HasVisibleExports(INamespaceSymbol root, IEnumerable<string> parts)
    {
        // GlobalNamespace follows C# reference aliases. Metadata-name lookup also sees
        // extern-only assemblies. Leave conflicting types and namespace names to Roslyn.
        INamespaceSymbol? current = root;
        foreach (var part in parts)
        {
            if (!current.GetTypeMembers(part, arity: 0).IsEmpty)
                return true;
            current = current.GetMembers(part).OfType<INamespaceSymbol>().FirstOrDefault();
            if (current is null)
                return false;
        }
        return current.GetMembers("Exports").Any(member => member is not INamedTypeSymbol type || type.Arity == 0);
    }

    private static string? DottedName(SyntaxNode node) => node switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        QualifiedNameSyntax name when DottedName(name.Left) is { } left => left + "." + name.Right.Identifier.ValueText,
        MemberAccessExpressionSyntax member when DottedName(member.Expression) is { } left => left + "." + member.Name.Identifier.ValueText,
        AliasQualifiedNameSyntax alias => alias.Alias.Identifier.ValueText == "global"
            ? "global::" + alias.Name.Identifier.ValueText
            : alias.Alias.Identifier.ValueText + "." + alias.Name.Identifier.ValueText,
        _ => null,
    };
}
