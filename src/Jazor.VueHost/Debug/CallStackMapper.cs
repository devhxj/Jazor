using Jazor.VueHost.SourceMap;

namespace Jazor.VueHost.Debug;

internal sealed class CallStackMapper(ISourceMapService sourceMapService)
{
    private readonly ISourceMapService _sourceMapService = sourceMapService ?? throw new ArgumentNullException(nameof(sourceMapService));

    public IReadOnlyList<DapStackFrame> MapCallStack(IReadOnlyList<CdpCallFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);

        var mappedFrames = new List<DapStackFrame>(frames.Count);
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames[index];
            var original = string.IsNullOrWhiteSpace(frame.Location.Url)
                ? null
                : _sourceMapService.OriginalPositionFor(frame.Location.Url, frame.Location.LineNumber, frame.Location.ColumnNumber);
            var sourcePath = original?.SourcePath ?? frame.Location.Url;
            var sourceName = Path.GetFileName(sourcePath);
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                sourceName = sourcePath;
            }

            mappedFrames.Add(new DapStackFrame(
                Id: index + 1,
                Name: string.IsNullOrWhiteSpace(frame.FunctionName) ? "(anonymous)" : frame.FunctionName,
                Source: new DapSource(sourceName, sourcePath),
                Line: (original?.Line ?? frame.Location.LineNumber) + 1,
                Column: (original?.Column ?? frame.Location.ColumnNumber) + 1));
        }

        return mappedFrames;
    }
}

internal sealed record CdpLocation(
    string Url,
    int LineNumber,
    int ColumnNumber);

internal sealed record CdpCallFrame(
    string CallFrameId,
    string FunctionName,
    CdpLocation Location);

internal sealed record DapSource(
    string Name,
    string Path,
    int? SourceReference = null);

internal sealed record DapStackFrame(
    int Id,
    string Name,
    DapSource Source,
    int Line,
    int Column);
