using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using TreeSitter;
using File = System.IO.File;

/// <summary>Freezes original documentation independently of runtime/type snapshots.</summary>
internal static class BindingDocumentationSnapshot
{
    internal sealed record Entry(string Library, string Scope, string Member, string Text, string Source);

    public static void Run(string[] args)
    {
        if (args is not [var vue])
            throw new ArgumentException("Usage: documentation snapshot <vue-3.5.42.tar.gz>");
        var entries = new List<Entry>();
        using var language = new Language("TypeScript");
        var manual = File.ReadAllText(Path.Combine("src", "ECMAScript.Vue.Generator", "documentation", "vue", "built-in-components.md"));
        foreach (Match code in Regex.Matches(manual, @"\x60{3}ts\r?\n([\s\S]*?)\x60{3}"))
        {
            using var parser = new Parser(language);
            using var tree = parser.Parse(code.Groups[1].Value)!;
            TypeScriptDocumentation.Visit(tree.RootNode, "", (_, name, comment) =>
            {
                if (comment is null || TypeScriptDocumentation.ReadComment(comment) is not { } prose) return;
                var parts = name.Split('.', 2);
                entries.Add(new("ECMAScript.Vue", parts[0], parts.Length == 2 ? parts[1] : "", prose, "vuejs/docs:src/api/built-in-components.md"));
            });
        }
        ReadArchive(vue, "core-3.5.42/", (path, text) =>
        {
            if (!path.StartsWith("packages/") || !path.Contains("/src/") || !path.EndsWith(".ts")) return;
            using var parser = new Parser(language);
            using var tree = parser.Parse(text)!;
            TypeScriptDocumentation.Visit(tree.RootNode, "", (_, name, comment) =>
            {
                if (comment is null || TypeScriptDocumentation.ReadComment(comment) is not { } prose) return;
                var parts = name.Split('.', 2);
                entries.Add(new("ECMAScript.Vue", parts[0], parts.Length == 2 ? parts[1] : "", prose, path));
            });
        });
        var output = Path.Combine("src", "ECMAScript.Vue.Generator", "documentation", "originals.json");
        File.WriteAllText(output, JsonSerializer.Serialize(new
        {
            sources = new[] {
                new { library = "ECMAScript.Vue", version = "3.5.42", repository = "https://github.com/vuejs/core", license = "MIT" }
            },
            entries = entries.Distinct().OrderBy(e => e.Library, StringComparer.Ordinal).ThenBy(e => e.Source, StringComparer.Ordinal)
                .ThenBy(e => e.Scope, StringComparer.Ordinal).ThenBy(e => e.Member, StringComparer.Ordinal)
        }, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) + "\n", new UTF8Encoding(false));
        Console.WriteLine($"Frozen {entries.Count} original API descriptions.");
    }

    private static void ReadArchive(string path, string prefix, Action<string, string> read)
    {
        using var gzip = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
        using var tar = new TarReader(gzip);
        var matched = false;
        while (tar.GetNextEntry() is { } entry)
        {
            if (entry.DataStream is null || !entry.Name.StartsWith(prefix, StringComparison.Ordinal)) continue;
            matched = true;
            if (!entry.Name.EndsWith(".md") && !entry.Name.EndsWith(".json") && !entry.Name.EndsWith(".ts")) continue;
            using var reader = new StreamReader(entry.DataStream, leaveOpen: true);
            read(entry.Name[prefix.Length..], reader.ReadToEnd());
        }
        if (!matched) throw new InvalidOperationException("Archive must contain " + prefix);
    }
}