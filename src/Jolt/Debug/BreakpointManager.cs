using System.Text.Json;
using Jolt.SourceMap;

namespace Jolt.Debug;

internal sealed class BreakpointManager(
    ISourceMapService sourceMapService,
    Action<string>? warningSink = null)
{
    private readonly ISourceMapService _sourceMapService = sourceMapService ?? throw new ArgumentNullException(nameof(sourceMapService));
    private readonly Action<string>? _warningSink = warningSink;

    public MappedBreakpoint? MapBreakpoint(string sourcePath, int sourceLine, int sourceColumn = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var generated = _sourceMapService.GeneratedPositionFor(sourcePath, sourceLine, sourceColumn);
        if (generated is null && ShouldReportMissingSourcePath(sourcePath))
        {
            WriteBreakpointWarning(_warningSink, sourcePath, sourceLine, sourceColumn);
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

    private static void WriteBreakpointWarning(
        Action<string>? warningSink,
        string sourcePath,
        int sourceLine,
        int sourceColumn)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                eventType = "dapBreakpointSourcePathUnavailable",
                sourcePath,
                sourceLine,
                sourceColumn,
                timestamp = DateTimeOffset.UtcNow
            });

            if (warningSink is not null)
            {
                warningSink(payload);
                return;
            }

            Console.Error.WriteLine(payload);
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
