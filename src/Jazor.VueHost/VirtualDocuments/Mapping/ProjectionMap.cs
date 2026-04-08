namespace Jazor.VueHost.VirtualDocuments.Mapping;

public sealed class ProjectionMap
{
    public ProjectionMap(
        string sourceDocumentPath,
        string projectedDocumentPath,
        IReadOnlyList<ProjectionSegment> segments)
    {
        SourceDocumentPath = sourceDocumentPath ?? throw new ArgumentNullException(nameof(sourceDocumentPath));
        ProjectedDocumentPath = projectedDocumentPath ?? throw new ArgumentNullException(nameof(projectedDocumentPath));
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public string SourceDocumentPath { get; }

    public string ProjectedDocumentPath { get; }

    public IReadOnlyList<ProjectionSegment> Segments { get; }

    public static ProjectionMap CreateWholeDocument(
        string sourceDocumentPath,
        string projectedDocumentPath,
        int sourceLength,
        int projectedLength)
        => new(
            sourceDocumentPath,
            projectedDocumentPath,
            [
                new ProjectionSegment(
                    OriginalStart: 0,
                    OriginalLength: sourceLength,
                    ProjectedStart: 0,
                    ProjectedLength: projectedLength)
            ]);
}
