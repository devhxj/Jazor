#!/usr/bin/env dotnet run

using System.Globalization;
using System.Xml.Linq;

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: dotnet run --file analyze-coverage-gaps.cs -- <cobertura.xml> [--top N] [--detail]");
    return 1;
}

var path = args[0];
var top = 15;
var detail = false;
for (var i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--top":
            if (i + 1 >= args.Length || !int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out top) || top < 1)
            {
                Console.Error.WriteLine("--top requires a positive integer.");
                return 1;
            }
            break;
        case "--detail":
            detail = true;
            break;
        default:
            Console.Error.WriteLine($"unknown option: {args[i]}");
            return 1;
    }
}

var root = XDocument.Load(path).Root ?? throw new InvalidOperationException("Cobertura document has no root element.");
var linesCovered = ParseInt(root, "lines-covered");
var linesValid = ParseInt(root, "lines-valid");
var branchesCovered = ParseInt(root, "branches-covered");
var branchesValid = ParseInt(root, "branches-valid");

Console.WriteLine($"# {path}");
Console.WriteLine($"lines:    {linesCovered}/{linesValid} ({Rate(linesCovered, linesValid):P2})");
Console.WriteLine($"branches: {branchesCovered}/{branchesValid} ({Rate(branchesCovered, branchesValid):P2})");

var files = new Dictionary<string, FileCoverage>(StringComparer.Ordinal);
foreach (var classElement in root.Descendants().Where(e => e.Name.LocalName == "class"))
{
    var fileName = classElement.Attribute("filename")?.Value ?? "?";
    if (!files.TryGetValue(fileName, out var file))
        files.Add(fileName, file = new FileCoverage());

    foreach (var line in classElement.Descendants().Where(e => e.Name.LocalName == "line"))
    {
        var number = ParseInt(line, "number");
        var hits = ParseInt(line, "hits");
        var branch = string.Equals(line.Attribute("branch")?.Value, "True", StringComparison.OrdinalIgnoreCase);
        var (covered, total) = ParseConditionCoverage(line.Attribute("condition-coverage")?.Value);
        var current = file.Lines.TryGetValue(number, out var existing)
            ? existing
            : new LineCoverage();

        current.AnyHit |= hits > 0;
        current.MissedBranches = Math.Max(current.MissedBranches, branch ? Math.Max(0, total - covered) : 0);
        current.TotalBranches = Math.Max(current.TotalBranches, branch ? total : 0);
        file.Lines[number] = current;
    }

    foreach (var method in classElement.Descendants().Where(e => e.Name.LocalName == "method"))
    {
        var methodLines = method.Descendants().Where(e => e.Name.LocalName == "line").ToList();
        var missed = methodLines.Where(line => ParseInt(line, "hits") == 0)
            .Select(line => ParseInt(line, "number"))
            .Distinct()
            .OrderBy(number => number)
            .ToArray();
        if (missed.Length != 0)
        {
            file.MissedMethods.Add(new MissedMethod(
                method.Attribute("name")?.Value ?? "?",
                method.Attribute("signature")?.Value ?? "",
                missed));
        }
    }
}

var rows = files.Select(pair =>
{
    var missedLines = pair.Value.Lines.Where(line => !line.Value.AnyHit).Select(line => line.Key).OrderBy(number => number).ToArray();
    var missedBranches = pair.Value.Lines.Sum(line => line.Value.MissedBranches);
    var branchLines = pair.Value.Lines
        .Where(line => line.Value.MissedBranches != 0)
        .Select(line => new BranchLine(line.Key, line.Value.MissedBranches, line.Value.TotalBranches))
        .OrderBy(line => line.Number)
        .ToArray();
    return new FileRow(pair.Key, missedLines, missedBranches, branchLines, pair.Value.MissedMethods);
}).ToArray();

Console.WriteLine($"files: {rows.Length}, missed lines (dedup): {rows.Sum(row => row.MissedLines.Length)}, missed branches (dedup): {rows.Sum(row => row.MissedBranches)}");
Console.WriteLine();
Console.WriteLine("## By missed branches (top)");
foreach (var row in rows.OrderByDescending(row => row.MissedBranches).ThenByDescending(row => row.MissedLines.Length).ThenBy(row => row.FileName, StringComparer.Ordinal).Take(top))
    Console.WriteLine($"{row.MissedBranches,4} missBr {row.MissedLines.Length,4} missLn  {row.FileName}");

Console.WriteLine();
Console.WriteLine("## By missed lines (top)");
foreach (var row in rows.OrderByDescending(row => row.MissedLines.Length).ThenByDescending(row => row.MissedBranches).ThenBy(row => row.FileName, StringComparer.Ordinal).Take(top))
    Console.WriteLine($"{row.MissedLines.Length,4} missLn {row.MissedBranches,4} missBr  {row.FileName}");

if (detail)
{
    Console.WriteLine();
    Console.WriteLine("## Detail (top by missed branches)");
    foreach (var row in rows.OrderByDescending(row => row.MissedBranches).ThenByDescending(row => row.MissedLines.Length).ThenBy(row => row.FileName, StringComparer.Ordinal).Take(top))
    {
        Console.WriteLine($"### {row.FileName}");
        Console.WriteLine($"  missed lines ({row.MissedLines.Length}): {string.Join(",", row.MissedLines)}");
        Console.WriteLine($"  missed branches ({row.MissedBranches}): {string.Join(",", row.BranchLines.Select(line => $"{line.Number}({line.Missed}/{line.Total})"))}");
        foreach (var method in row.MissedMethods.OrderBy(method => method.Name, StringComparer.Ordinal).ThenBy(method => method.Signature, StringComparer.Ordinal))
            Console.WriteLine($"  method {method.Name}{method.Signature}: {string.Join(",", method.Lines)}");
    }
}

return 0;

static int ParseInt(XElement element, string name)
    => int.TryParse(element.Attribute(name)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

static (int Covered, int Total) ParseConditionCoverage(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return (0, 0);
    var open = value.IndexOf('(');
    var close = value.IndexOf(')', open + 1);
    if (open < 0 || close <= open)
        return (0, 0);
    var parts = value[(open + 1)..close].Split('/');
    return parts.Length == 2 &&
           int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var covered) &&
           int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var total)
        ? (covered, total)
        : (0, 0);
}

static double Rate(int covered, int total) => total == 0 ? 1 : (double)covered / total;

sealed class FileCoverage
{
    public Dictionary<int, LineCoverage> Lines { get; } = new();
    public List<MissedMethod> MissedMethods { get; } = [];
}

sealed class LineCoverage
{
    public bool AnyHit { get; set; }
    public int MissedBranches { get; set; }
    public int TotalBranches { get; set; }
}

record FileRow(string FileName, int[] MissedLines, int MissedBranches, BranchLine[] BranchLines, List<MissedMethod> MissedMethods);
record BranchLine(int Number, int Missed, int Total);
record MissedMethod(string Name, string Signature, int[] Lines);
