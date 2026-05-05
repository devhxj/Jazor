using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Artifacts;

internal sealed record RazorVueSourceOrigin(
    RazorVueOriginKind OriginKind,
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    RazorVueMappingQuality MappingQuality,
    RazorVueOriginProvenance Provenance)
{
    public static RazorVueSourceOrigin FromLocation(Location location, RazorVueOriginKind originKind)
    {
        if (location is null)
            throw new ArgumentNullException(nameof(location));

        var lineSpan = location.GetLineSpan();
        return new RazorVueSourceOrigin(
            OriginKind: originKind,
            SourceFilePath: lineSpan.Path ?? string.Empty,
            SourceSpanStart: location.SourceSpan.Start,
            SourceSpanLength: location.SourceSpan.Length,
            StartLine: lineSpan.StartLinePosition.Line + 1,
            StartColumn: lineSpan.StartLinePosition.Character + 1,
            GeneratedFilePath: lineSpan.Path,
            GeneratedSpanStart: location.SourceSpan.Start,
            GeneratedSpanLength: location.SourceSpan.Length,
            MappingQuality: RazorVueMappingQuality.MappedFromGenerated,
            Provenance: RazorVueOriginProvenance.GeneratedSyntaxLocation);
    }
}

internal enum RazorVueOriginKind
{
    Component,
    Descriptor,
    Template,
    Logic,
    GeneratedRender,
    Style,
    CustomBlock
}

internal enum RazorVueMappingQuality
{
    ExactSource,
    MappedFromGenerated,
    GeneratedOnly
}

internal enum RazorVueOriginProvenance
{
    RazorSourceMap,
    GeneratedSyntaxLocation,
    GeneratedFallback
}
