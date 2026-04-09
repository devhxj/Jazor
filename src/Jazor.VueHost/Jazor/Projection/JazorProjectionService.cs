using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.VirtualDocuments.Models;

namespace Jazor.VueHost.Jazor.Projection;

internal sealed class JazorProjectionService
{
    private const string TemplateOpenTag = "<template>";
    private const string TemplateCloseTag = "</template>";
    private const string CodeCommentMarker = "Original @code block retained for bridge diagnostics:";
    private readonly JazorVueParser _parser = new();
    private readonly JazorVueCompiler _compiler = new();

    public ValueTask<IReadOnlyList<VirtualDocument>> ProjectAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(Array.Empty<VirtualDocument>());
        }

        var parsedDocument = _parser.Parse(document.DocumentPath, document.Text);
        var compilation = _compiler.Compile(parsedDocument);

        var vueProjectedPath = "virtual:" + document.DocumentPath + ".g.vue";
        var csharpProjectedPath = "virtual:" + document.DocumentPath + ".g.cs";

        IReadOnlyList<VirtualDocument> virtualDocuments =
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    vueProjectedPath,
                    VirtualDocumentKind.Vue),
                compilation.GeneratedVueText,
                CreateVueProjectionMap(document.DocumentPath, parsedDocument, compilation.GeneratedVueText),
                document.Version),
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    csharpProjectedPath,
                    VirtualDocumentKind.CSharp),
                compilation.GeneratedExternalDeclarationsText,
                new ProjectionMap(
                    document.DocumentPath,
                    csharpProjectedPath,
                    Array.Empty<ProjectionSegment>()),
                document.Version)
        ];

        return ValueTask.FromResult(virtualDocuments);
    }

    private static ProjectionMap CreateVueProjectionMap(
        string sourceDocumentPath,
        JazorVueDocument document,
        string generatedVueText)
    {
        var segments = new List<ProjectionSegment>();

        if (TryFindGeneratedTemplateContentStart(generatedVueText, document.Template, out var generatedTemplateStart)
            && document.TemplateStartIndex >= 0
            && document.TemplateLength > 0)
        {
            segments.Add(new ProjectionSegment(
                document.TemplateStartIndex,
                document.TemplateLength,
                generatedTemplateStart,
                document.Template.Length));
        }

        if (document.CodeStartIndex >= 0
            && document.CodeLength > 0
            && !document.Code.Contains("*/", StringComparison.Ordinal)
            && TryFindGeneratedCodeCommentStart(generatedVueText, document.Code, out var generatedCodeStart))
        {
            segments.Add(new ProjectionSegment(
                document.CodeStartIndex,
                document.CodeLength,
                generatedCodeStart,
                document.Code.Length));
        }

        return new ProjectionMap(
            sourceDocumentPath,
            "virtual:" + sourceDocumentPath + ".g.vue",
            segments);
    }

    private static bool TryFindGeneratedTemplateContentStart(
        string generatedVueText,
        string templateText,
        out int generatedStart)
    {
        generatedStart = -1;
        if (string.IsNullOrEmpty(templateText))
        {
            return false;
        }

        var templateTagIndex = generatedVueText.LastIndexOf(TemplateOpenTag, StringComparison.OrdinalIgnoreCase);
        if (templateTagIndex < 0)
        {
            return false;
        }

        var searchStart = templateTagIndex + TemplateOpenTag.Length;
        generatedStart = generatedVueText.IndexOf(templateText, searchStart, StringComparison.Ordinal);
        if (generatedStart < 0)
        {
            return false;
        }

        var closeTagIndex = generatedVueText.IndexOf(TemplateCloseTag, generatedStart, StringComparison.OrdinalIgnoreCase);
        return closeTagIndex >= generatedStart;
    }

    private static bool TryFindGeneratedCodeCommentStart(
        string generatedVueText,
        string codeText,
        out int generatedStart)
    {
        generatedStart = -1;
        if (string.IsNullOrEmpty(codeText))
        {
            return false;
        }

        var commentMarkerIndex = generatedVueText.IndexOf(CodeCommentMarker, StringComparison.Ordinal);
        if (commentMarkerIndex < 0)
        {
            return false;
        }

        generatedStart = generatedVueText.IndexOf(codeText, commentMarkerIndex + CodeCommentMarker.Length, StringComparison.Ordinal);
        return generatedStart >= 0;
    }
}
