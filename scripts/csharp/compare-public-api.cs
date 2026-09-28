#!/usr/bin/env dotnet run

using System.Text;

var currentPath = GetOption("--current") ?? throw new ArgumentException("Missing --current <path>.");
var baselinePath = GetOption("--baseline");
var allowMissingBaseline = args.Contains("--allow-missing-baseline", StringComparer.Ordinal);
var current = ReadSnapshot(currentPath);
var outputPath = GetOption("--output");

if (baselinePath is null || !File.Exists(baselinePath))
{
    var missingReport = $"# Public API compatibility report\n\nCurrent: `{currentPath}`\n\n- Current entries: {current.Count}\n- Current top-level entries: {current.TopLevelCount}\n- Current member entries: {current.MemberCount}\n- Baseline entries: unavailable\n- Compatibility: not established\n\nThe candidate snapshot must be retained as the next baseline before 1.0 freeze.\n";
    WriteReport(missingReport, outputPath);
    Console.WriteLine($"Public API baseline missing; current snapshot contains {current.Count} entries.");
    if (!allowMissingBaseline)
        throw new InvalidOperationException("Public API baseline is required. Generate and retain a candidate baseline, or pass --allow-missing-baseline only when creating one locally.");
    return;
}

var baseline = ReadSnapshot(baselinePath);
var removed = baseline.Entries.Except(current.Entries, StringComparer.Ordinal).OrderBy(static line => line, StringComparer.Ordinal).ToArray();
var added = current.Entries.Except(baseline.Entries, StringComparer.Ordinal).OrderBy(static line => line, StringComparer.Ordinal).ToArray();

var report = new StringBuilder();
report.AppendLine("# Public API compatibility report");
report.AppendLine();
report.AppendLine($"Baseline: `{baselinePath}`");
report.AppendLine($"Current: `{currentPath}`");
report.AppendLine();
report.AppendLine($"- Baseline entries: {baseline.Count}");
report.AppendLine($"- Baseline top-level entries: {baseline.TopLevelCount}");
report.AppendLine($"- Baseline member entries: {baseline.MemberCount}");
report.AppendLine($"- Current entries: {current.Count}");
report.AppendLine($"- Current top-level entries: {current.TopLevelCount}");
report.AppendLine($"- Current member entries: {current.MemberCount}");
report.AppendLine($"- Added entries: {added.Length}");
report.AppendLine($"- Removed entries: {removed.Length}");

AppendSection(report, "Added", added);
AppendSection(report, "Removed", removed);

WriteReport(report.ToString(), outputPath);

if (removed.Length > 0)
    throw new InvalidOperationException($"Public API compatibility check failed: {removed.Length} entries were removed or changed.");

static Snapshot ReadSnapshot(string path)
{
    var entries = File.ReadLines(path)
        .Where(static line => line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("  - ", StringComparison.Ordinal))
        .ToHashSet(StringComparer.Ordinal);
    var topLevelCount = entries.Count(static line => line.StartsWith("- ", StringComparison.Ordinal));
    var memberCount = entries.Count(static line => line.StartsWith("  - ", StringComparison.Ordinal));
    return new Snapshot(entries, topLevelCount, memberCount);
}

static void AppendSection(StringBuilder builder, string title, IReadOnlyCollection<string> entries)
{
    builder.AppendLine();
    builder.AppendLine($"## {title}");
    if (entries.Count == 0)
    {
        builder.AppendLine("- None");
        return;
    }
    foreach (var entry in entries)
        builder.AppendLine($"- `{entry}`");
}

static void WriteReport(string report, string? outputPath)
{
    if (outputPath is null)
    {
        Console.Write(report);
        return;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
    File.WriteAllText(outputPath, report, Encoding.UTF8);
    Console.WriteLine($"Wrote compatibility report: {Path.GetFullPath(outputPath)}");
}

string? GetOption(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

sealed record Snapshot(HashSet<string> Entries, int TopLevelCount, int MemberCount)
{
    public int Count => Entries.Count;
}
