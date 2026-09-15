using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using TreeSitter;
using File = System.IO.File;

/// <summary>
/// Documentation is a separate frozen input: npm declarations remain the type contract.
/// Match comments by source file and declaration/member path, never by a global member name.
/// </summary>
internal sealed partial class TDesignDocumentation
{
    private const string Revision = "018b90352b184fb93f57dca62db83d80d486a14f";
    private const string Version = "1.20.7";
    private readonly Dictionary<string, string> _comments;

    public TDesignDocumentation(string snapshotRoot)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(snapshotRoot, "documentation.json")));
        _comments = document.RootElement.GetProperty("comments").EnumerateObject()
            .ToDictionary(static item => item.Name, static item => item.Value.GetString()!, StringComparer.Ordinal);
    }

    public string? GetSummary(string sourcePath, string name)
        => _comments.TryGetValue(sourcePath + "#" + name, out var comment) ? ReadComment(comment) : null;

    public string Annotate(string sourcePath, string source, Language language)
    {
        using var parser = new Parser(language);
        using var tree = parser.Parse(source)!;
        var insertions = new List<(int Index, string Comment)>();
        Visit(tree.RootNode, string.Empty, (node, path, existingComment) =>
        {
            if (node.Type == "property_signature" && existingComment is null &&
                _comments.TryGetValue(sourcePath + "#" + path, out var comment))
            {
                insertions.Add(((int)node.StartIndex, comment + "\n"));
            }
        });
        var builder = new StringBuilder(source);
        foreach (var insertion in insertions.OrderByDescending(static insertion => insertion.Index))
            builder.Insert(insertion.Index, insertion.Comment);
        return builder.ToString();
    }

    public static void Run(string[] args)
    {
        if (args is not [var archivePath])
            throw new ArgumentException("Usage: tdesign documentation <upstream-source.tar.gz>");
        var repoRoot = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(repoRoot, "Jazor.slnx")))
            repoRoot = Directory.GetParent(repoRoot)?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        var snapshotRoot = Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "tdesign-vue-next", Version);
        var comments = new SortedDictionary<string, string>(StringComparer.Ordinal);
        using var language = new Language("TypeScript");
        using var stream = File.OpenRead(archivePath);
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        using var archive = new TarReader(gzip);
        var prefix = $"tdesign-vue-next-{Revision}/packages/components/";
        while (archive.GetNextEntry() is { } entry)
        {
            if (entry.DataStream is null || !entry.Name.StartsWith(prefix, StringComparison.Ordinal) ||
                !entry.Name.EndsWith(".ts", StringComparison.Ordinal))
                continue;
            var sourcePath = "es/" + entry.Name[prefix.Length..^3] + ".d.ts";
            if (!File.Exists(Path.Combine(snapshotRoot, sourcePath)))
                continue;
            using var reader = new StreamReader(entry.DataStream, leaveOpen: true);
            using var parser = new Parser(language);
            using var tree = parser.Parse(reader.ReadToEnd())!;
            Visit(tree.RootNode, string.Empty, (_, path, comment) =>
            {
                if (comment is not null && ReadComment(comment) is not null)
                    comments[sourcePath + "#" + path] = comment.Replace("\r\n", "\n", StringComparison.Ordinal);
            });
        }

        if (comments.Count == 0)
            throw new InvalidOperationException($"Expected the TDesign {Version} source archive at commit {Revision}.");
        var json = JsonSerializer.Serialize(new
        {
            version = Version,
            repository = "https://github.com/Tencent/tdesign-vue-next",
            revision = Revision,
            license = "MIT",
            comments
        }, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        File.WriteAllText(Path.Combine(snapshotRoot, "documentation.json"), json + "\n", new UTF8Encoding(false));
        Console.WriteLine($"Frozen {comments.Count} upstream documentation entries for TDesign {Version}.");
    }

    private static void Visit(Node parent, string path, Action<Node, string, string?> visit)
    {
        string? comment = null;
        foreach (var child in parent.NamedChildren)
        {
            if (child.Type == "comment")
            {
                comment = child.Text.StartsWith("/**", StringComparison.Ordinal) ? child.Text : null;
                continue;
            }
            if (child.Type == "export_statement")
            {
                // JSDoc precedes export, while the declaration itself is its child.
                var declaration = child.NamedChildren.FirstOrDefault(static node =>
                    node.Type is "interface_declaration" or "type_alias_declaration" or "class_declaration");
                if (declaration is not null)
                    VisitDeclaration(declaration, path, comment, visit);
                comment = null;
                continue;
            }
            VisitDeclaration(child, path, comment, visit);
            comment = null;
        }
    }

    private static void VisitDeclaration(Node node, string path, string? comment, Action<Node, string, string?> visit)
    {
        if (node.Type is "interface_declaration" or "type_alias_declaration" or "class_declaration" or "property_signature")
        {
            var name = node.GetChildForField("name")?.Text.Trim('\'', '"');
            if (name is not null)
            {
                path = path.Length == 0 ? name : path + "." + name;
                visit(node, path, comment);
            }
        }
        Visit(node, path, visit);
    }

    public static string? ReadComment(string comment)
    {
        var lines = comment.Trim().TrimStart('/').TrimStart('*').TrimEnd('/').TrimEnd('*')
            .Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n')
            .Select(static line => Regex.Replace(line.Trim(), @"^\*\s?", string.Empty));
        var summary = new List<string>();
        foreach (var line in lines)
        {
            if (line.StartsWith("@default ", StringComparison.Ordinal))
                summary.Add("默认值：" + line[9..]);
            else if (line.StartsWith("@deprecated", StringComparison.Ordinal))
                summary.Add("已弃用：" + line[11..].Trim());
            else if (!line.StartsWith('@'))
                summary.Add(line);
        }
        var text = string.Join("\n", summary).Trim();
        return text.Length == 0 ? null : text;
    }
}
