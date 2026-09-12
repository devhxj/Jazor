#!/usr/bin/env dotnet run

var tag = ReadRequiredOption(args, "--tag");
var version = tag.StartsWith('v', StringComparison.OrdinalIgnoreCase) ? tag[1..] : tag;
var changelogPath = Path.GetFullPath(ReadOption(args, "--changelog") ?? "CHANGELOG.md");
if (!File.Exists(changelogPath))
    throw new FileNotFoundException("CHANGELOG.md was not found.", changelogPath);

var lines = File.ReadAllLines(changelogPath);
var heading = "### Jazor " + version;
var headingIndex = Array.FindIndex(lines, line => string.Equals(line.Trim(), heading, StringComparison.Ordinal));
if (headingIndex < 0)
    throw new InvalidOperationException($"CHANGELOG.md must contain a curated '{heading}' section before publishing.");

var dateIndex = -1;
for (var index = headingIndex - 1; index >= 0; index--)
{
    if (lines[index].StartsWith("## ", StringComparison.Ordinal))
    {
        dateIndex = index;
        break;
    }
}

if (dateIndex < 0 || !DateOnly.TryParse(lines[dateIndex][3..].Trim(), out _))
    throw new InvalidOperationException($"CHANGELOG.md section '{heading}' must be under a dated '## YYYY-MM-DD' heading.");

var body = lines.Skip(headingIndex + 1)
    .TakeWhile(static line => !line.StartsWith("### ", StringComparison.Ordinal) && !line.StartsWith("## ", StringComparison.Ordinal))
    .ToArray();
if (!body.Any(static line => !string.IsNullOrWhiteSpace(line)))
    throw new InvalidOperationException($"CHANGELOG.md section '{heading}' must contain release notes.");

Console.WriteLine($"Release notes verified: {heading} ({lines[dateIndex][3..].Trim()})");

static string ReadRequiredOption(string[] arguments, string name)
    => ReadOption(arguments, name) ?? throw new InvalidOperationException($"Missing {name} value.");

static string? ReadOption(string[] arguments, string name)
{
    var index = Array.IndexOf(arguments, name);
    return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
}
