using Jolt.SourceMap;

namespace Jolt.Debug;

internal sealed class BreakpointManager(ISourceMapService sourceMapService)
{
    private readonly ISourceMapService _sourceMapService = sourceMapService ?? throw new ArgumentNullException(nameof(sourceMapService));

    public MappedBreakpoint? MapBreakpoint(string sourcePath, int sourceLine, int sourceColumn = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var generated = _sourceMapService.GeneratedPositionFor(sourcePath, sourceLine, sourceColumn);
        return generated is null
            ? null
            : new MappedBreakpoint(generated.GeneratedPath, generated.Line, generated.Column);
    }
}

internal sealed record MappedBreakpoint(
    string GeneratedPath,
    int GeneratedLine,
    int GeneratedColumn);
