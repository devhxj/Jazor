#!/usr/bin/env dotnet run

using System.Text;
using System.Text.Json;

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run --file scripts/csharp/update-jazor-style-css-grammars.cs -- <extracted @webref/css package directory>");
    Environment.ExitCode = 1;
    return;
}

var repoRoot = FindRepositoryRoot();
var packageRoot = Path.GetFullPath(args[0]);
var packageJsonPath = Path.Combine(packageRoot, "package.json");
using var packageDocument = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
var version = packageDocument.RootElement.GetProperty("version").GetString()
    ?? throw new InvalidDataException("The @webref/css package version is missing.");

var properties = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
foreach (var path in Directory.EnumerateFiles(packageRoot, "*.json").Order(StringComparer.Ordinal))
{
    if (string.Equals(path, packageJsonPath, StringComparison.OrdinalIgnoreCase))
        continue;

    using var document = JsonDocument.Parse(File.ReadAllText(path));
    if (!document.RootElement.TryGetProperty("properties", out var entries))
        continue;

    foreach (var entry in entries.EnumerateArray())
    {
        if (!entry.TryGetProperty("name", out var nameElement) ||
            !entry.TryGetProperty("value", out var valueElement) ||
            nameElement.GetString() is not { } name ||
            valueElement.GetString() is not { } grammar)
        {
            continue;
        }

        if (!properties.TryGetValue(name, out var grammars))
            properties.Add(name, grammars = new(StringComparer.Ordinal));
        grammars.Add(grammar);
    }
}

var outputPath = Path.Combine(repoRoot, "src", "Jazor.Style", "CssProperties.Webref.json");
using var stream = new MemoryStream();
using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
{
    writer.WriteStartObject();
    writer.WriteNumber("schemaVersion", 1);
    writer.WriteString("source", $"@webref/css@{version}");
    writer.WriteStartObject("properties");
    foreach (var (name, grammars) in properties)
    {
        writer.WriteStartArray(name);
        foreach (var grammar in grammars)
            writer.WriteStringValue(grammar);
        writer.WriteEndArray();
    }
    writer.WriteEndObject();
    writer.WriteEndObject();
}
var output = Encoding.UTF8.GetString(stream.ToArray()) + "\n";
File.WriteAllText(outputPath, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine($"Generated {properties.Count} Jazor.Style CSS grammar entries from @webref/css@{version}.");

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
        directory = directory.Parent;
    }
    throw new DirectoryNotFoundException("Repository root containing Jazor.slnx was not found.");
}
