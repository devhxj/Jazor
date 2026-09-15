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
        if (args is not [var elementPlus, var vuetify, var vue])
            throw new ArgumentException("Usage: documentation snapshot <element-plus-2.14.5.tar.gz> <vuetify-4.2.1.tar.gz> <vue-3.5.42.tar.gz>");
        var entries = new List<Entry>();
        ReadArchive(elementPlus, "element-plus-2.14.5/", (path, text) =>
        {
            if (path.StartsWith("docs/en-US/component/") && path.EndsWith(".md"))
                ReadTables(path, text, entries);
        });
        ReadArchive(vuetify, "vuetify-4.2.1/", (path, text) =>
        {
            if (!path.StartsWith("packages/api-generator/src/locale/en/") || !path.EndsWith(".json")) return;
            using var document = JsonDocument.Parse(text);
            var scope = Path.GetFileNameWithoutExtension(path);
            foreach (var section in document.RootElement.EnumerateObject())
            {
                if (section.Name == "description" && section.Value.ValueKind == JsonValueKind.String)
                    entries.Add(new("ECMAScript.Vuetify", scope, "", section.Value.GetString()!, path));
                if (section.Name is not ("props" or "events" or "slots" or "exposed") || section.Value.ValueKind != JsonValueKind.Object) continue;
                foreach (var prop in section.Value.EnumerateObject())
                    if (prop.Value.ValueKind == JsonValueKind.String)
                        entries.Add(new("ECMAScript.Vuetify", scope, section.Name + ":" + prop.Name, prop.Value.GetString()!, path));
            }
        });
        using var language = new Language("TypeScript");
        var manual = File.ReadAllText(Path.Combine("src", "ECMAScript.Vue.Generator", "documentation", "vue", "built-in-components.md"));
        foreach (Match code in Regex.Matches(manual, @"\x60{3}ts\r?\n([\s\S]*?)\x60{3}"))
        {
            using var parser = new Parser(language);
            using var tree = parser.Parse(code.Groups[1].Value)!;
            TDesignDocumentation.Visit(tree.RootNode, "", (_, name, comment) =>
            {
                if (comment is null || TDesignDocumentation.ReadComment(comment) is not { } prose) return;
                var parts = name.Split('.', 2);
                entries.Add(new("ECMAScript.Vue", parts[0], parts.Length == 2 ? parts[1] : "", prose, "vuejs/docs:src/api/built-in-components.md"));
            });
        }
        ReadArchive(vue, "core-3.5.42/", (path, text) =>
        {
            if (!path.StartsWith("packages/") || !path.Contains("/src/") || !path.EndsWith(".ts")) return;
            using var parser = new Parser(language);
            using var tree = parser.Parse(text)!;
            TDesignDocumentation.Visit(tree.RootNode, "", (_, name, comment) =>
            {
                if (comment is null || TDesignDocumentation.ReadComment(comment) is not { } prose) return;
                var parts = name.Split('.', 2);
                entries.Add(new("ECMAScript.Vue", parts[0], parts.Length == 2 ? parts[1] : "", prose, path));
            });
        });
        var output = Path.Combine("src", "ECMAScript.Vue.Generator", "documentation", "originals.json");
        File.WriteAllText(output, JsonSerializer.Serialize(new
        {
            sources = new[] {
                new { library = "ECMAScript.ElementPlus", version = "2.14.5", repository = "https://github.com/element-plus/element-plus", license = "MIT" },
                new { library = "ECMAScript.Vuetify", version = "4.2.1", repository = "https://github.com/vuetifyjs/vuetify", license = "MIT" },
                new { library = "ECMAScript.Vue", version = "3.5.42", repository = "https://github.com/vuejs/core", license = "MIT" }
            },
            entries = entries.Distinct().OrderBy(e => e.Library, StringComparer.Ordinal).ThenBy(e => e.Source, StringComparer.Ordinal)
                .ThenBy(e => e.Scope, StringComparer.Ordinal).ThenBy(e => e.Member, StringComparer.Ordinal)
        }, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) + "\n", new UTF8Encoding(false));
        Console.WriteLine($"Frozen {entries.Count} original API descriptions.");
    }

    private static void ReadTables(string path, string text, List<Entry> entries)
    {
        var heading = Path.GetFileNameWithoutExtension(path);
        string[]? headers = null;
        foreach (var line in text.Split('\n'))
        {
            if (line.StartsWith('#')) { heading = line.TrimStart('#').Trim(); headers = null; }
            if (!line.TrimStart().StartsWith('|')) continue;
            var cells = Regex.Split(line.Trim().Trim('|'), @"(?<!\\)\|").Select(c => c.Trim()).ToArray();
            if (cells.Any(c => c == "Description")) { headers = cells; continue; }
            if (headers is null || cells.Length != headers.Length || cells[0].StartsWith("--")) continue;
            var description = cells[Array.IndexOf(headers, "Description")];
            var member = Regex.Replace(cells[0], @"\s*\^\([^)]+\)", "").Trim(' ', (char)96);
            if (description is "" or "—" or "-") continue;
            var defaultIndex = Array.IndexOf(headers, "Default");
            if (defaultIndex >= 0 && cells[defaultIndex] is not ("" or "—" or "-"))
                description += "\nDefault: " + cells[defaultIndex];
            entries.Add(new("ECMAScript.ElementPlus", heading, member, description.Replace(@"\|", "|"), path));
        }
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