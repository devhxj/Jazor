#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text.RegularExpressions;

var options = ReleaseNotesOptions.Parse(args);
var curatedNotes = TryReadCuratedReleaseNotes(options.Tag);
if (!string.IsNullOrWhiteSpace(curatedNotes))
{
    Console.WriteLine(curatedNotes);
    return;
}

var previousTag = string.IsNullOrWhiteSpace(options.PreviousTag)
    ? await ResolvePreviousTagAsync(options.Tag)
    : options.PreviousTag;

var range = string.IsNullOrWhiteSpace(previousTag)
    ? options.Tag
    : previousTag + ".." + options.Tag;

var subjects = await RunGitCaptureAsync("log", "--reverse", "--format=%s", range);
var parsedCommits = subjects
    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
    .Where(static subject => !string.IsNullOrWhiteSpace(subject))
    .Select(ParseCommitSubject)
    .ToArray();

var topScopes = parsedCommits
    .Where(static commit => !string.IsNullOrWhiteSpace(commit.Scope))
    .SelectMany(static commit => commit.Scope.Split(',', StringSplitOptions.RemoveEmptyEntries))
    .Select(static scope => scope.Trim())
    .Where(static scope => !string.IsNullOrWhiteSpace(scope))
    .GroupBy(static scope => scope, StringComparer.Ordinal)
    .OrderByDescending(static group => group.Count())
    .ThenByDescending(static group => group.Key, StringComparer.Ordinal)
    .Take(6)
    .Select(static group => $"`{group.Key}`")
    .ToArray();

var lines = new List<string>
{
    "## Summary",
    string.Empty
};

if (string.IsNullOrWhiteSpace(previousTag))
{
    lines.Add($"- {parsedCommits.Length} commits included in this release.");
}
else
{
    lines.Add($"- {parsedCommits.Length} commits since `{previousTag}`.");
}

if (topScopes.Length > 0)
{
    lines.Add("- Primary scopes: " + string.Join(", ", topScopes) + ".");
}

if (!string.IsNullOrWhiteSpace(options.Repository) && !string.IsNullOrWhiteSpace(previousTag))
{
    lines.Add($"- Compare: https://github.com/{options.Repository}/compare/{previousTag}...{options.Tag}");
}

foreach (var sectionType in new[] { "feat", "fix", "refactor", "test", "docs", "chore", "other" })
{
    var entries = parsedCommits.Where(commit => commit.Type == sectionType).ToArray();
    if (entries.Length == 0)
    {
        continue;
    }

    lines.Add(string.Empty);
    lines.Add("## " + GetSectionTitle(sectionType));
    lines.Add(string.Empty);

    foreach (var entry in entries)
    {
        lines.Add(string.IsNullOrWhiteSpace(entry.Scope)
            ? "- " + entry.Description
            : $"- **{entry.Scope}**: {entry.Description}");
    }
}

if (!string.IsNullOrWhiteSpace(options.Repository) && !string.IsNullOrWhiteSpace(previousTag))
{
    lines.Add(string.Empty);
    lines.Add("## Full Changelog");
    lines.Add(string.Empty);
    lines.Add($"https://github.com/{options.Repository}/compare/{previousTag}...{options.Tag}");
}

Console.WriteLine(string.Join(Environment.NewLine, lines));

static string? TryReadCuratedReleaseNotes(string tag)
{
    var version = tag.StartsWith('v', StringComparison.OrdinalIgnoreCase)
        ? tag[1..]
        : tag;
    var changelogPath = Path.Combine(Directory.GetCurrentDirectory(), "CHANGELOG.md");
    if (!File.Exists(changelogPath))
    {
        return null;
    }

    var lines = File.ReadAllLines(changelogPath);
    var heading = "### Jazor " + version;
    var start = Array.FindIndex(
        lines,
        line => string.Equals(line.Trim(), heading, StringComparison.OrdinalIgnoreCase));
    if (start < 0)
    {
        return null;
    }

    var selected = new List<string> { "## Jazor " + version };
    for (var index = start + 1; index < lines.Length; index++)
    {
        var line = lines[index];
        if (line.StartsWith("### ", StringComparison.Ordinal) || line.StartsWith("## ", StringComparison.Ordinal))
        {
            break;
        }

        selected.Add(line);
    }

    while (selected.Count > 1 && string.IsNullOrWhiteSpace(selected[1]))
    {
        selected.RemoveAt(1);
    }

    selected.Insert(1, string.Empty);

    while (selected.Count > 2 && string.IsNullOrWhiteSpace(selected[^1]))
    {
        selected.RemoveAt(selected.Count - 1);
    }

    return selected.Count <= 2
        ? null
        : string.Join(Environment.NewLine, selected);
}

static async Task<string?> ResolvePreviousTagAsync(string currentTag)
{
    var tags = await RunGitCaptureAsync("tag", "--sort=-version:refname");
    return tags
        .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(tag => !string.Equals(tag, currentTag, StringComparison.Ordinal));
}

static ParsedCommit ParseCommitSubject(string subject)
{
    var trimmed = subject.Trim();
    var match = Regex.Match(
        trimmed,
        @"^[^\p{L}\p{Nd}]*(?<type>feat|fix|refactor|test|docs|chore)(\((?<scope>[^)]+)\))?:\s*(?<desc>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    if (!match.Success)
    {
        return new ParsedCommit("other", string.Empty, trimmed, trimmed);
    }

    return new ParsedCommit(
        match.Groups["type"].Value.ToLowerInvariant(),
        match.Groups["scope"].Value.Trim(),
        match.Groups["desc"].Value.Trim(),
        trimmed);
}

static string GetSectionTitle(string type)
{
    return type switch
    {
        "feat" => "Features",
        "fix" => "Fixes",
        "refactor" => "Refactors",
        "test" => "Tests",
        "docs" => "Documentation",
        "chore" => "Chores",
        _ => "Other"
    };
}

static async Task<string> RunGitCaptureAsync(params string[] arguments)
{
    var startInfo = new ProcessStartInfo("git")
    {
        WorkingDirectory = Directory.GetCurrentDirectory(),
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = new Process { StartInfo = startInfo };
    process.Start();

    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    var output = await standardOutput;
    var error = await standardError;
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"git {string.Join(' ', arguments)} failed with exit code {process.ExitCode}.{Environment.NewLine}{error}".TrimEnd());
    }

    return output;
}

internal sealed record ReleaseNotesOptions(string Tag, string? PreviousTag, string? Repository)
{
    public static ReleaseNotesOptions Parse(IReadOnlyList<string> arguments)
    {
        string? tag = null;
        string? previousTag = null;
        string? repository = null;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--tag":
                case "-Tag":
                    tag = RequireValue(arguments, ref index, argument);
                    break;
                case "--previous-tag":
                case "-PreviousTag":
                    previousTag = RequireValue(arguments, ref index, argument);
                    break;
                case "--repository":
                case "-Repository":
                    repository = RequireValue(arguments, ref index, argument);
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + argument);
            }
        }

        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new InvalidOperationException("--tag is required.");
        }

        return new ReleaseNotesOptions(tag, previousTag, repository);
    }

    static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var nextIndex = index + 1;
        if (nextIndex >= arguments.Count)
        {
            throw new InvalidOperationException("Missing value for " + option + ".");
        }

        index = nextIndex;
        return arguments[index];
    }

    static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/release-notes.cs -- --tag <tag> [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --previous-tag <tag>");
        Console.WriteLine("  --repository <owner/repo>");
    }
}

internal sealed record ParsedCommit(string Type, string Scope, string Description, string Subject);
