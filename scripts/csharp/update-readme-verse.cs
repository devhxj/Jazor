#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Linq;

const string sourceUrl = "https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate";
const string startMarker = "<!-- today-verse:start -->";
const string endMarker = "<!-- today-verse:end -->";

using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
var document = XDocument.Parse(await client.GetStringAsync(sourceUrl));
XNamespace svgNamespace = "http://www.w3.org/2000/svg";
var verse = document.Descendants(svgNamespace + "text").Single().Value.Trim();
if (string.IsNullOrWhiteSpace(verse))
    throw new InvalidDataException("The Today's Verse response contains no verse text.");

// GitHub cannot execute the provider's SDK, and its image proxy cannot load the
// original endpoint. Keep that source, but publish its sentence as ordinary text.
var paragraph = $"<p><a href=\"https://www.jinrishici.com/\">{WebUtility.HtmlEncode(verse)}</a></p>";
var updates = new List<(string Path, string Content)>();
foreach (var path in new[] { "README.md", "README_CN.md" })
{
    var content = await File.ReadAllTextAsync(path);
    var start = content.IndexOf(startMarker, StringComparison.Ordinal);
    var end = content.IndexOf(endMarker, StringComparison.Ordinal);
    if (start < 0 || end < start)
        throw new InvalidDataException($"The Today's Verse block is missing from {path}.");

    var newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    var block = string.Join(newline, startMarker, paragraph, endMarker);
    updates.Add((path, content[..start] + block + content[(end + endMarker.Length)..]));
}

// Prepare both translations before writing, so source/format errors change neither.
foreach (var update in updates)
    await File.WriteAllTextAsync(update.Path, update.Content, new UTF8Encoding(false));

Console.WriteLine($"Today's Verse: {verse}");

if (!args.Contains("--commit", StringComparer.Ordinal))
    return;

await RequireGitSuccessAsync("add", "README.md", "README_CN.md");
var diffExitCode = await RunGitAsync("diff", "--cached", "--quiet");
if (diffExitCode == 0)
    return;
if (diffExitCode != 1)
    throw new InvalidOperationException($"git diff exited with code {diffExitCode}.");

await RequireGitSuccessAsync("config", "user.name", "github-actions[bot]");
await RequireGitSuccessAsync("config", "user.email", "41898282+github-actions[bot]@users.noreply.github.com");
await RequireGitSuccessAsync("commit", "-m", "docs(readme): refresh today's verse");
await RequireGitSuccessAsync("push", "origin", "HEAD:main");

static async Task RequireGitSuccessAsync(params string[] arguments)
{
    var exitCode = await RunGitAsync(arguments);
    if (exitCode != 0)
        throw new InvalidOperationException($"git {arguments[0]} exited with code {exitCode}.");
}

static async Task<int> RunGitAsync(params string[] arguments)
{
    var startInfo = new ProcessStartInfo("git") { UseShellExecute = false };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = Process.Start(startInfo)!;
    await process.WaitForExitAsync();
    return process.ExitCode;
}
