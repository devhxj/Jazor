using System.Runtime.CompilerServices;
using System.Text.Json;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal sealed class JoltDevCompilation
{
    private (JoltSource[] Sources, JoltCompileOptions Options, JoltCompileResult Result, JoltProjectCompiler.DevSnapshot? Snapshot)? _previous;
    internal JoltProjectCompiler.DevSnapshot? Snapshot => _previous?.Snapshot;

    internal async Task<JoltCompileResult> CompileAsync(IEnumerable<JoltSource> sources, JoltCompileOptions options,
        CancellationToken cancellationToken = default)
    {
        var current = sources.ToArray();
        if (_previous is { Snapshot: { } baseline } previous && SameOptions(previous.Options, options) && current.Length == previous.Sources.Length)
        {
            var changes = previous.Sources.Zip(current).Where(pair => pair.First != pair.Second).Take(2).ToArray();
            if (changes is [{ First: var before, Second: var after }] && before.Path == after.Path && IsStaticEdit(before, after) &&
                await baseline.TryUpdateAsync(after, options, previous.Result, cancellationToken).ConfigureAwait(false) is { } update)
            {
                _previous = (current, options, update.Result, update.Snapshot);
                return update.Result;
            }
        }

        JoltProjectCompiler.DevSnapshot? snapshot = null;
        var result = await JoltProjectCompiler.CompileAsync(current, options, cancellationToken, value => snapshot = value).ConfigureAwait(false);
        if (!result.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            _previous = (current, options, result, snapshot);
        return result;
    }

    private static bool SameOptions(JoltCompileOptions left, JoltCompileOptions right) =>
        left.RootNamespace == right.RootNamespace && left.Router == right.Router &&
        left.VueSourceRootRelative == right.VueSourceRootRelative &&
        (left.References ?? []).SequenceEqual(right.References ?? []) &&
        SameMetadata(left.NpmComponents ?? [], right.NpmComponents ?? []) &&
        SameMetadata(left.NpmValues, right.NpmValues) && SameMetadata(left.VueComponents, right.VueComponents) &&
        left.CSharpSources.SequenceEqual(right.CSharpSources);

    private static bool SameMetadata<T>(IReadOnlyList<T> left, IReadOnlyList<T> right) =>
        left.SequenceEqual(right) || JsonSerializer.Serialize(left) == JsonSerializer.Serialize(right);

    private static bool IsStaticEdit(JoltSource before, JoltSource after)
    {
        if (JoltProjectImports.IsImportsFile(after.Path)) return false;
        var start = 0;
        var oldEnd = before.Content.Length;
        var newEnd = after.Content.Length;
        while (start < oldEnd && start < newEnd && before.Content[start] == after.Content[start]) start++;
        while (oldEnd > start && newEnd > start && before.Content[oldEnd - 1] == after.Content[newEnd - 1])
        {
            oldEnd--;
            newEnd--;
        }
        var oldText = FindLiteral(before, TextSpan.FromBounds(start, oldEnd));
        var newText = FindLiteral(after, TextSpan.FromBounds(start, newEnd));
        // Both parsers must place the whole edit inside the same literal with
        // unchanged boundaries outside the edit. Reject whitespace-only nodes.
        return oldText is { } old && newText is { } next && old.Span.Start == next.Span.Start &&
            old.Span.End - oldEnd == next.Span.End - newEnd &&
            !string.IsNullOrWhiteSpace(old.Text) && !string.IsNullOrWhiteSpace(next.Text);
    }

    private static JoltText? FindLiteral(JoltSource source, TextSpan edit)
    {
        var document = JoltProjectImports.ParseDocument(source);
        foreach (var element in JoltTree.Elements(document.Nodes))
        {
            if (element.Name is "slot" or "template") continue;
            foreach (var attribute in element.Attributes)
                if (attribute.Kind == JoltAttributeKind.Markup && attribute.Name != "Context" &&
                    attribute.Value is [JoltText text] && text.Span.Contains(edit))
                    return text;
            foreach (var text in element.Children.OfType<JoltText>())
                if (text.Span.Contains(edit)) return text;
        }
        return null;
    }

    // A module's two maps are immutable. Index their source names once for as
    // long as that module is alive, including when edits switch between files.
    private static readonly ConditionalWeakTable<JoltModule, string[]> MapSources = new();

    internal static bool ReferencesSource(JoltModule module, string path)
    {
        var leaf = Path.GetFileName(path.Replace('\\', '/'));
        return MapSources.GetValue(module, static item =>
            Sources(item.SourceMap).Concat(Sources(item.TypeScriptSourceMap)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
            .Contains(leaf, StringComparer.OrdinalIgnoreCase);

        static IEnumerable<string> Sources(string? encoded)
        {
            if (encoded is null) yield break;
            using var map = JsonDocument.Parse(encoded);
            foreach (var source in map.RootElement.GetProperty("sources").EnumerateArray())
            {
                var name = Path.GetFileName(source.GetString()!.Replace('\\', '/'));
                yield return name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ? name[..^5] : name;
            }
        }
    }
}
