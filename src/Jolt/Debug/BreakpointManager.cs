using System.Text.Json;
using Jolt.SourceMap;

namespace Jolt.Debug;

internal sealed class BreakpointManager(ISourceMapService sourceMapService)
{
    private readonly ISourceMapService _sourceMapService = sourceMapService ?? throw new ArgumentNullException(nameof(sourceMapService));

    public MappedBreakpoint? MapBreakpoint(string sourcePath, int sourceLine, int sourceColumn = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var generated = _sourceMapService.GeneratedPositionFor(sourcePath, sourceLine, sourceColumn);
        if (generated is null && ShouldReportMissingSourcePath(sourcePath))
        {
            WriteBreakpointWarning(sourcePath, sourceLine, sourceColumn);
        }

        return generated is null
            ? null
            : new MappedBreakpoint(generated.GeneratedPath, generated.Line, generated.Column);
    }

    private static bool ShouldReportMissingSourcePath(string sourcePath)
    {
        try
        {
            return Path.IsPathRooted(sourcePath) && !File.Exists(sourcePath);
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or IOException
            or UnauthorizedAccessException
            or NotSupportedException)
        {
            return true;
        }
    }

    private static void WriteBreakpointWarning(string sourcePath, int sourceLine, int sourceColumn)
    {
        try
        {
            Console.Error.WriteLine(JsonSerializer.Serialize(new
            {
                eventType = "dapBreakpointSourcePathUnavailable",
                sourcePath,
                sourceLine,
                sourceColumn,
                timestamp = DateTimeOffset.UtcNow
            }));
        }
        catch (Exception)
        {
            // Observability must not affect breakpoint mapping behavior.
        }
    }
}

internal sealed record MappedBreakpoint(
    string GeneratedPath,
    int GeneratedLine,
    int GeneratedColumn);
