using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.VirtualDocuments.Models;

namespace Jazor.VueHost.Jazor.Projection;

internal sealed class JazorProjectionService
{
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
        var sourceLength = document.Text.Length;

        IReadOnlyList<VirtualDocument> virtualDocuments =
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    vueProjectedPath,
                    VirtualDocumentKind.Vue),
                compilation.GeneratedVueText,
                ProjectionMap.CreateWholeDocument(
                    document.DocumentPath,
                    vueProjectedPath,
                    sourceLength,
                    compilation.GeneratedVueText.Length),
                document.Version),
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    document.DocumentPath,
                    csharpProjectedPath,
                    VirtualDocumentKind.CSharp),
                compilation.GeneratedExternalDeclarationsText,
                ProjectionMap.CreateWholeDocument(
                    document.DocumentPath,
                    csharpProjectedPath,
                    sourceLength,
                    compilation.GeneratedExternalDeclarationsText.Length),
                document.Version)
        ];

        return ValueTask.FromResult(virtualDocuments);
    }
}
