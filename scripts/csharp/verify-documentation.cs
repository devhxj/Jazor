#!/usr/bin/env dotnet run
#:property PublishAot=false
#:package Markdig@*

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

// Parse Markdown so examples inside code blocks do not become false link failures.
// 上游文档和许可证保留原文；机器快照的原始签名不是 Markdown 导航。
var repoRoot = Directory.GetCurrentDirectory();
var reportPath = Path.Combine(repoRoot, "artifacts", "documentation", "report.json");
if (args.Length == 2 && args[0] == "--report") reportPath = Path.GetFullPath(args[1]);
else if (args.Length != 0) throw new ArgumentException("Usage: dotnet run --file scripts/csharp/verify-documentation.cs -- [--report PATH]");
var start = new ProcessStartInfo("git") { UseShellExecute = false, RedirectStandardOutput = true };
foreach (var argument in new[] { "ls-files", "--cached", "--others", "--exclude-standard", "-z", "--", "*.md" }) start.ArgumentList.Add(argument);
using var git = Process.Start(start)!;
var paths = (await git.StandardOutput.ReadToEndAsync()).Split('\0', StringSplitOptions.RemoveEmptyEntries)
    .Distinct(StringComparer.Ordinal).Where(File.Exists).Order(StringComparer.Ordinal).ToArray();
await git.WaitForExitAsync();
if (git.ExitCode != 0) return git.ExitCode;
var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseGenericAttributes()
    .UseAutoIdentifiers(AutoIdentifierOptions.GitHub).Build();
var documents = new Dictionary<string, (MarkdownDocument Document, HashSet<string> Anchors)>(StringComparer.OrdinalIgnoreCase);
var failures = new List<LinkFailure>();
var versionMatch = Regex.Match(File.ReadAllText("docs/04-roadmap/current-status.md"), @"releases/tag/v(?<version>[^\)\s]+)");
if (!versionMatch.Success) throw new InvalidOperationException("Current status must identify the published release tag.");
var currentVersion = versionMatch.Groups["version"].Value;
var links = 0;
var excluded = paths.Where(IsPreserved).ToArray();
foreach (var path in paths.Where(path => !IsPreserved(path)))
{
    CheckVersions(path);
    var document = ReadDocument(path).Document;
    foreach (var link in document.Descendants<LinkInline>())
        Check(path, link.Line + 1, link.Url);
    foreach (var html in document.Descendants<HtmlInline>())
        CheckHtml(path, html.Line + 1, html.Tag);
    foreach (var html in document.Descendants<HtmlBlock>())
        CheckHtml(path, html.Line + 1, html.Lines.ToString());
}
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(new
{
    CurrentVersion = currentVersion, Documents = paths.Length, Checked = paths.Length - excluded.Length, PreservedDocuments = excluded,
    LocalLinks = links, Failures = failures
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Documentation: {paths.Length} Markdown files; {paths.Length - excluded.Length} checked; {excluded.Length} generated/upstream files preserved; {links} local links.");
foreach (var failure in failures) Console.WriteLine($"{failure.File}:{failure.Line}: {failure.Message}: {failure.Target}");
Console.WriteLine($"Result: {failures.Count} documentation errors. Current release: {currentVersion}. Report: {reportPath}");
return failures.Count == 0 ? 0 : 1;

bool IsPreserved(string path) => path.EndsWith(".snapshot.md", StringComparison.Ordinal)
    || path.StartsWith("src/ECMAScript.Vue.Generator/documentation/", StringComparison.Ordinal);

void CheckVersions(string path)
{
    // Migration/history and analyzer release tables intentionally retain authored versions.
    // Upstream npm, SDK, LibNode and tool versions are independent of the NuGet release.
    if (path == "CHANGELOG.md" || path.StartsWith("docs/05-history/", StringComparison.Ordinal)
        || Path.GetFileName(path).StartsWith("AnalyzerReleases.", StringComparison.Ordinal)) return;
    var number = 0;
    foreach (var line in File.ReadLines(path))
    {
        number++;
        foreach (Match reference in Regex.Matches(line, "<PackageReference\\s+Include=\"(?:Jazor|ECMAScript)(?:\\.[^\"]+)?\"[^>]*\\bVersion=\"([^\"]+)\""))
            RequireCurrent(reference.Groups[1].Value, "Stale Jazor/ECMAScript installation version");
        foreach (Match command in Regex.Matches(line, @"\bdotnet add(?:\s+\S+)?\s+package\s+(?:Jazor|ECMAScript)(?:\.[\w.]+)?\s+--version\s+([\w.+-]+)"))
            RequireCurrent(command.Groups[1].Value, "Stale package install command");
        if (path is "README.md" or "README_CN.md")
        {
            foreach (Match intro in Regex.Matches(line, @"\bJazor\s+(\d+\.\d+\.\d+(?:-[\w.-]+)?)\b"))
                RequireCurrent(intro.Groups[1].Value, "Stale release description in README");
            foreach (Match badge in Regex.Matches(line, @"releases/tag/v([^\s\""\)]+)|[?&]tag=v([^&\""\s]+)"))
                RequireCurrent(badge.Groups[1].Success ? badge.Groups[1].Value : badge.Groups[2].Value, "Stale README release link/badge");
        }

        void RequireCurrent(string version, string message)
        {
            if (version != currentVersion)
                failures.Add(new(path, number, version, message + $"; expected {currentVersion}"));
        }
    }
}

(MarkdownDocument Document, HashSet<string> Anchors) ReadDocument(string path)
{
    var absolute = Path.GetFullPath(path);
    if (documents.TryGetValue(absolute, out var cached)) return cached;
    var content = File.ReadAllText(absolute);
    var document = Markdown.Parse(content, pipeline);
    var anchors = document.Descendants<HeadingBlock>().Select(heading => heading.GetAttributes().Id)
        .Where(id => id is not null).Select(id => id!).ToHashSet(StringComparer.Ordinal);
    foreach (var html in document.Descendants<HtmlInline>()) AddAnchors(html.Tag);
    foreach (var html in document.Descendants<HtmlBlock>()) AddAnchors(html.Lines.ToString());
    return documents[absolute] = (document, anchors);

    void AddAnchors(string html)
    {
        foreach (Match match in Regex.Matches(html, "<[^>]+\\b(?:id|name)=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase))
            anchors.Add(match.Groups[1].Value);
    }
}

void CheckHtml(string file, int line, string html)
{
    foreach (Match match in Regex.Matches(html, "\\b(?:href|src)=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase))
        Check(file, line, match.Groups[1].Value);
}

void Check(string file, int line, string? target)
{
    if (string.IsNullOrWhiteSpace(target) || target.StartsWith("//", StringComparison.Ordinal)
        || Regex.IsMatch(target, "^[a-zA-Z][a-zA-Z0-9+.-]*:")) return;
    links++;
    var parts = target.Split('#', 2);
    var relative = Uri.UnescapeDataString(parts[0].Split('?', 2)[0]);
    var destination = relative.Length == 0 ? Path.GetFullPath(file)
        : Path.GetFullPath(relative.StartsWith('/') ? Path.Combine(repoRoot, relative.TrimStart('/'))
            : Path.Combine(Path.GetDirectoryName(file)!, relative));
    if (!File.Exists(destination) && !Directory.Exists(destination))
    {
        failures.Add(new(file, line, target, "Missing target"));
        return;
    }
    if (parts.Length == 2 && parts[1].Length > 0 && Path.GetExtension(destination).Equals(".md", StringComparison.OrdinalIgnoreCase)
        && !ReadDocument(destination).Anchors.Contains(Uri.UnescapeDataString(parts[1])))
        failures.Add(new(file, line, target, "Missing heading/anchor"));
}

sealed record LinkFailure(string File, int Line, string Target, string Message);
