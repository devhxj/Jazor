using System.Text.Json;

namespace Jazor.VueHost.Build;

/// <summary>
/// Post-processes esbuild output to collect chunk/asset information.
/// </summary>
internal sealed class AssetProcessor
{
    private readonly BuildContext _context;

    public AssetProcessor(BuildContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Processes esbuild metafile to extract chunk and asset information.
    /// </summary>
    /// <param name="esbuildResult">The esbuild execution result.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Processed assets with chunk and CSS information.</returns>
    public Task<ProcessedAssets> ProcessAsync(EsbuildResult esbuildResult, CancellationToken ct)
    {
        if (!esbuildResult.Success || esbuildResult.MetafileJson is null)
        {
            return Task.FromResult(new ProcessedAssets
            {
                Chunks = [],
                CssAssets = [],
                StaticAssets = [],
                TotalSize = 0
            });
        }

        var chunks = new List<ChunkInfo>();
        var cssAssets = new List<AssetInfo>();
        long totalSize = 0;

        using var metafile = JsonDocument.Parse(esbuildResult.MetafileJson);
        var outputs = metafile.RootElement.GetProperty("outputs");

        foreach (var output in outputs.EnumerateObject())
        {
            ct.ThrowIfCancellationRequested();

            var outputPath = output.Name;
            var outputInfo = output.Value;

            if (!outputInfo.TryGetProperty("bytes", out var bytesElement))
                continue;

            var size = bytesElement.GetInt64();
            totalSize += size;

            var fileName = Path.GetFileName(outputPath);

            // Check if this is an entry point
            var isEntry = IsEntryPoint(outputInfo);

            // Extract imports
            var imports = ExtractImports(outputInfo);

            // Check for source map
            var sourceMapPath = ExtractSourceMapPath(outputPath, outputInfo);

            if (outputPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                // JavaScript chunk
                var chunkInfo = new ChunkInfo
                {
                    FileName = fileName,
                    FilePath = outputPath,
                    Size = size,
                    IsEntry = isEntry,
                    IsDynamic = IsDynamicChunk(outputInfo),
                    Imports = imports,
                    SourceMapPath = sourceMapPath
                };

                chunks.Add(chunkInfo);

                // Check chunk size warning
                if (size > _context.Options.ChunkSizeWarningLimit)
                {
                    _context.Diagnostics.Add(new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Warning,
                        Message = $"Chunk {fileName} is {FormatSize(size)} " +
                                  $"(exceeds {_context.Options.ChunkSizeWarningLimit / 1024}KB limit)",
                        File = outputPath
                    });
                }
            }
            else if (outputPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                // CSS asset
                var cssAsset = new AssetInfo
                {
                    FileName = fileName,
                    FilePath = outputPath,
                    Size = size,
                    SourceMapPath = sourceMapPath
                };

                cssAssets.Add(cssAsset);
            }
        }

        return Task.FromResult(new ProcessedAssets
        {
            Chunks = chunks,
            CssAssets = cssAssets,
            StaticAssets = [], // Static assets are handled separately
            TotalSize = totalSize
        });
    }

    /// <summary>
    /// Determines if an output is an entry point.
    /// </summary>
    private static bool IsEntryPoint(JsonElement outputInfo)
    {
        // An entry point is a chunk that has inputs from the src/ directory
        // and is not imported by any other chunk
        if (!outputInfo.TryGetProperty("inputs", out var inputs))
            return false;

        foreach (var input in inputs.EnumerateObject())
        {
            var inputPath = input.Name;
            // Entry points typically come from src/ directory
            if (inputPath.StartsWith("src/", StringComparison.OrdinalIgnoreCase) ||
                inputPath.StartsWith("src\\", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a chunk is dynamically imported.
    /// </summary>
    private static bool IsDynamicChunk(JsonElement outputInfo)
    {
        // Dynamic chunks are typically not entry points and are imported dynamically
        // This is a simplified heuristic
        return !IsEntryPoint(outputInfo);
    }

    /// <summary>
    /// Extracts import paths from output info.
    /// </summary>
    private static IReadOnlyList<string> ExtractImports(JsonElement outputInfo)
    {
        var imports = new List<string>();

        if (outputInfo.TryGetProperty("imports", out var importsElement))
        {
            foreach (var import in importsElement.EnumerateArray())
            {
                if (import.TryGetProperty("path", out var pathElement))
                {
                    imports.Add(pathElement.GetString() ?? string.Empty);
                }
            }
        }

        return imports;
    }

    /// <summary>
    /// Extracts source map path from output info.
    /// </summary>
    private static string? ExtractSourceMapPath(string outputPath, JsonElement outputInfo)
    {
        // Check if a .map file exists alongside the output
        var mapPath = outputPath + ".map";
        if (outputInfo.TryGetProperty("sourcemap", out var sourcemap) && sourcemap.GetBoolean())
        {
            return mapPath;
        }

        return null;
    }

    /// <summary>
    /// Formats a byte size as a human-readable string.
    /// </summary>
    private static string FormatSize(long bytes)
    {
        const int KB = 1024;
        const int MB = 1024 * KB;

        return bytes switch
        {
            < KB => $"{bytes} B",
            < MB => $"{bytes / KB} KB",
            _ => $"{bytes / MB} MB"
        };
    }
}

/// <summary>
/// Processed assets result containing chunks, CSS, and static assets.
/// </summary>
internal sealed class ProcessedAssets
{
    public required IReadOnlyList<ChunkInfo> Chunks { get; init; }
    public required IReadOnlyList<AssetInfo> CssAssets { get; init; }
    public required IReadOnlyList<AssetInfo> StaticAssets { get; init; }
    public long TotalSize { get; init; }
}
