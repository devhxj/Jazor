using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Jazor.RazorVue.RazorExtension;

internal sealed class RazorVueRazorIrPass : IntermediateNodePassBase, IRazorEngineFeature
{
    public override int Order => 1;

    protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
    {
        if (codeDocument is null)
            throw new ArgumentNullException(nameof(codeDocument));
        if (documentNode is null)
            throw new ArgumentNullException(nameof(documentNode));

        if (!string.Equals(codeDocument.GetFileKind(), RazorFileKind.Component, StringComparison.Ordinal))
            return;

        var primaryClass = DocumentIntermediateNodeExtensions.FindPrimaryClass(documentNode);
        if (primaryClass is null)
            return;

        var source = codeDocument.Source;
        if (source is null || string.IsNullOrWhiteSpace(source.FilePath))
            return;

        primaryClass.Children.Insert(0, new RazorVueRazorIrCarrierNode(
            source.FilePath,
            SerializeImports(codeDocument.Imports),
            source.GetText()?.ToString() ?? string.Empty));
    }

    public RazorEngine Engine { get; set; } = default!;

    private static string SerializeImports(IReadOnlyList<RazorSourceDocument> imports)
    {
        if (imports.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(imports
            .Where(static import => !string.IsNullOrWhiteSpace(import.FilePath))
            .Select(static import => new RazorVueImportPayload(
                import.FilePath!,
                import.GetText()?.ToString() ?? string.Empty))
            .ToArray());
    }

    private sealed record RazorVueImportPayload(string Path, string Text);
}
