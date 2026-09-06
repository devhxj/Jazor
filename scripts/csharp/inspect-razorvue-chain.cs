#!/usr/bin/env dotnet run
#:property NoWarn=IL2026;IL3050

using System.Text.Json;

try
{
var options = ChainOptions.Parse(args);
var source = RequireFile(options.Source, "source .razor");
var generated = RequireFile(options.Generated, "generated C#");
var artifact = RequireFile(options.Artifact, "render-function artifact");
var map = options.Map is null ? null : RequireFile(options.Map, "source map");

var sourceText = File.ReadAllText(source);
var generatedText = File.ReadAllText(generated);
var artifactText = File.ReadAllText(artifact);
var sourceMap = map is null ? null : ReadSourceMap(map);
var normalizedSource = NormalizePath(source);
var mappedSource = sourceMap?.Sources.Any(candidate =>
    PathEndsWith(normalizedSource, NormalizePath(candidate))) == true;
var hasMapReference = artifactText.Contains("sourceMappingURL=", StringComparison.Ordinal);

var report = new ChainReport(
    NormalizePath(source),
    new FileReport(NormalizePath(generated), CountLines(generatedText),
        generatedText.Contains("BuildRenderTree", StringComparison.Ordinal)),
    new ArtifactReport(NormalizePath(artifact), CountLines(artifactText), hasMapReference),
    sourceMap is null
        ? null
        : new SourceMapReport(
            NormalizePath(map!),
            sourceMap.Sources,
            sourceMap.SourcesContentCount,
            mappedSource));

if (options.Json)
{
    Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
}
else
{
    Console.WriteLine("RazorVue authoring chain");
    Console.WriteLine($"  source:   {report.Source}");
    Console.WriteLine($"  generated:{report.Generated.Path} ({report.Generated.Lines} lines, BuildRenderTree={report.Generated.HasBuildRenderTree})");
    Console.WriteLine($"  artifact: {report.Artifact.Path} ({report.Artifact.Lines} lines, map reference={report.Artifact.HasSourceMapReference})");
    if (report.SourceMap is { } mapReport)
    {
        Console.WriteLine($"  map:      {mapReport.Path} ({mapReport.Sources.Count} sources, mapped source={mapReport.MappedSource})");
        Console.WriteLine($"  source content entries: {mapReport.SourcesContentCount}");
    }
}

if (sourceMap is not null && !mappedSource)
    throw new InvalidOperationException("The source map does not contain the supplied .razor source path.");
if (map is not null && !hasMapReference)
    throw new InvalidOperationException("The render-function artifact does not contain a sourceMappingURL reference.");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

static string RequireFile(string? path, string description)
{
    if (string.IsNullOrWhiteSpace(path))
        throw new InvalidOperationException($"Missing {description} path. Use --help for usage.");
    var fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
        throw new FileNotFoundException($"The {description} file was not found.", fullPath);
    return fullPath;
}

static int CountLines(string text)
    => text.Length == 0 ? 0 : text.Count(static character => character == '\n') + 1;

static string NormalizePath(string path)
    => path.Replace('\\', '/');

static bool PathEndsWith(string path, string suffix)
    => path.Equals(suffix, StringComparison.OrdinalIgnoreCase) ||
       path.EndsWith('/' + suffix.TrimStart('/'), StringComparison.OrdinalIgnoreCase);

static SourceMapData ReadSourceMap(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    var root = document.RootElement;
    var sources = root.TryGetProperty("sources", out var sourceArray)
        ? sourceArray.EnumerateArray()
            .Where(static item => item.ValueKind == JsonValueKind.String)
            .Select(static item => item.GetString()!)
            .Select(NormalizePath)
            .ToArray()
        : Array.Empty<string>();
    var contentCount = root.TryGetProperty("sourcesContent", out var contentArray)
        ? contentArray.GetArrayLength()
        : 0;
    return new SourceMapData(sources, contentCount);
}

internal sealed record ChainReport(
    string Source,
    FileReport Generated,
    ArtifactReport Artifact,
    SourceMapReport? SourceMap);

internal sealed record FileReport(string Path, int Lines, bool HasBuildRenderTree);

internal sealed record ArtifactReport(string Path, int Lines, bool HasSourceMapReference);

internal sealed record SourceMapReport(
    string Path,
    IReadOnlyList<string> Sources,
    int SourcesContentCount,
    bool MappedSource);

internal sealed record SourceMapData(IReadOnlyList<string> Sources, int SourcesContentCount);

internal sealed record ChainOptions(
    string? Source,
    string? Generated,
    string? Artifact,
    string? Map,
    bool Json)
{
    public static ChainOptions Parse(string[] args)
    {
        string? source = null;
        string? generated = null;
        string? artifact = null;
        string? map = null;
        var json = false;
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--source": source = ReadValue(args, ref index); break;
                case "--generated": generated = ReadValue(args, ref index); break;
                case "--artifact": artifact = ReadValue(args, ref index); break;
                case "--map": map = ReadValue(args, ref index); break;
                case "--json": json = true; break;
                case "--help":
                    Console.WriteLine("Usage: dotnet run --file scripts/csharp/inspect-razorvue-chain.cs -- --source page.razor --generated page.razor.g.cs --artifact page.mjs [--map page.mjs.map] [--json]");
                    Environment.Exit(0);
                    break;
                default: throw new InvalidOperationException("Unknown argument: " + args[index]);
            }
        }

        return new ChainOptions(source, generated, artifact, map, json);
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (++index >= args.Length)
            throw new InvalidOperationException("Missing value for " + args[index - 1]);
        return args[index];
    }
}
