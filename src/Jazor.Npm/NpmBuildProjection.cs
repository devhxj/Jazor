using System.Text.Json;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.Npm;

internal static class NpmBuildProjection
{
    internal static async Task<int> RunGeneratorAsync(string request, string outputPath,
        CancellationToken cancellationToken)
    {
        using var input = new StringReader(request);
        var project = input.ReadLine()!;
        var store = input.ReadLine();
        var packages = NpmValueMetadataProvider.ListPackageNames(Path.GetDirectoryName(project)!);
        var requests = new List<NpmSourceRequest>();
        while (input.ReadLine() is { } ns)
        {
            var member = input.ReadLine();
            requests.Add(new(ns, member is { Length: > 0 } ? member : null, Location.None));
        }

        var failures = new List<NpmEditorProjectionError>();
        // Keep the diagnostics and partial source together, away from native tool logs.
        var source = await ResolveAsync(project, packages, requests, string.IsNullOrEmpty(store) ? null : store,
            (request, message) => failures.Add(new(request.NpmNamespace, request.MemberName, message)), cancellationToken, includeEditorLinks: true);
        var response = new NpmEditorProjectionResult(source, failures.ToArray());
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(response), cancellationToken);
        return 0;
    }

    internal static async Task<int> RunAsync(string inputsFile, TextWriter diagnostics, CancellationToken cancellationToken)
    {
        try
        {
            var inputs = File.ReadLines(inputsFile).Select(line => line.Split('|', 2))
                .ToLookup(parts => parts[0], parts => parts[1], StringComparer.Ordinal);
            var project = inputs["project"].Single();
            var directory = Path.GetDirectoryName(project)!;
            var output = inputs["output"].Single();
            var noImports = inputs["no-imports"].SingleOrDefault();
            if (noImports is not null)
                File.Delete(noImports);
            LanguageVersionFacts.TryParse(inputs["language"].Single(), out var language);
            var defines = inputs["define"].SelectMany(value => value.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries));
            var options = new CSharpParseOptions(language, preprocessorSymbols: defines);
            var trees = new List<SyntaxTree>();
            foreach (var path in inputs["source"])
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var stream = File.OpenRead(path);
                trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(stream), options, path, cancellationToken));
            }

            // Without the Npm root anywhere in active source there is no alias or fully
            // qualified name to resolve. Avoid loading the reference closure or starting Node.
            if (!trees.Any(tree => tree.GetRoot(cancellationToken).DescendantTokens()
                    .Any(token => token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == "Npm")))
            {
                File.Delete(output);
                if (noImports is not null)
                    await File.WriteAllTextAsync(noImports, string.Empty, cancellationToken);
                return 0;
            }

            var packages = NpmValueMetadataProvider.ListPackageNames(directory);
            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(project), trees,
                inputs["reference"].Select(ParseReference), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var requests = NpmSourceRequests.Discover(compilation, packages, cancellationToken);
            if (requests.Count == 0)
            {
                File.Delete(output);
                return 0;
            }

            var failed = false;
            var source = await ResolveAsync(project, packages, requests, inputs["store"].SingleOrDefault(), Report, cancellationToken);
            if (failed)
                return 1;
            if (File.Exists(output) && await File.ReadAllTextAsync(output, cancellationToken) == source)
                return 0;

            // CoreCompile reads this TFM/configuration's active file. A failed write
            // leaves the previous complete file available to concurrent design-time readers.
            var temporary = output + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporary, source, cancellationToken);
                File.Move(temporary, output, overwrite: true);
            }
            finally
            {
                File.Delete(temporary);
            }
            return 0;

            void Report(NpmSourceRequest request, string message)
            {
                var span = request.Location.GetMappedLineSpan();
                diagnostics.WriteLine($"{span.Path}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1}): error JAZOR: {message}");
                failed = true;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            diagnostics.WriteLine($"error JAZOR: {ex.Message}");
            return 1;
        }
    }

    internal static async Task<string> ResolveAsync(string project, IReadOnlyList<string> packages,
        IReadOnlyList<NpmSourceRequest> requests, string? store, Action<NpmSourceRequest, string> report,
        CancellationToken cancellationToken, bool includeEditorLinks = false)
    {
        var directory = Path.GetDirectoryName(project)!;
        var provider = new NpmValueMetadataProvider(store);
        var exports = new List<NpmValueExport>();
        var groups = requests.GroupBy(request => request.NpmNamespace, StringComparer.Ordinal).ToArray();
        foreach (var group in groups)
        {
            var site = group.First();
            try
            {
                var matches = await NpmExportSelection.FindModulesAsync(group.Key, packages,
                    package => provider.ListSpecifiersAsync(project, package, group.Key, cancellationToken), validatePackageRoot: true);
                if (matches.Count != 1)
                {
                    report(site, matches.Count == 0
                        ? $"No public npm export matches {group.Key} in {directory}."
                        : $"{group.Key} matches multiple npm specifiers: {string.Join(", ", matches)}.");
                    continue;
                }
                var module = matches[0];
                var members = group.Where(request => request.MemberName is not null).ToArray();
                if (members.Length == 0)
                    continue;
                var index = (await provider.ListValueExportsAsync(project, module, cancellationToken))
                    .ToLookup(NpmNames.ValueNameFor, StringComparer.Ordinal);
                // Some projected overloads acquire managed suffixes. Ask for a prefix only
                // as a candidate; its declaration must actually supply that overload.
                var selected = new List<(string Export, string? Suffix, NpmSourceRequest Site)>();
                foreach (var member in members)
                {
                    var name = member.MemberName!;
                    var matchesByName = NpmExportSelection.FindMembers(index, name);
                    if (matchesByName.Count > 1 && matchesByName[0].Suffix is null)
                        report(member, $"{name} matches multiple exports of {module}: {string.Join(", ", matchesByName.Select(match => match.Export))}.");
                    else
                        selected.AddRange(matchesByName.Select(match => (match.Export, match.Suffix, member)));
                }
                var byExport = selected.ToLookup(candidate => candidate.Export, StringComparer.Ordinal);
                var projections = await provider.ReadValueProjectionsAsync(project, module,
                    byExport.Select(items => items.Key).ToArray(), cancellationToken);
                foreach (var value in projections.Values)
                    if (byExport[value.ExportName].Any(item => NpmExportSelection.Matches(value, item.Suffix)))
                        exports.Add(value);
                foreach (var (name, error) in projections.Errors)
                    foreach (var item in byExport[name].Where(item => item.Suffix is null))
                        report(item.Site, $"{module} ({name}): {error}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                report(site, $"{group.Key}: {ex.Message}");
            }
        }
        var namespaces = groups.Select(group => group.Key).ToArray();
        return includeEditorLinks
            ? NpmBindings.GenerateEditorSource(directory, namespaces, exports, cancellationToken)
            : NpmBindings.GenerateCachedSource(directory, namespaces, exports, cancellationToken);
    }

    private static PortableExecutableReference ParseReference(string input)
    {
        var parts = input.Split('|', 2);
        var aliases = parts[0].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToImmutableArray();
        return MetadataReference.CreateFromFile(parts[1], new MetadataReferenceProperties(aliases: aliases));
    }
}
