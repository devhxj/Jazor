using System.Collections.Immutable;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CSharp;

namespace JazorComponentProbe.Razor;

public sealed record RazorProbeResult(DocumentIntermediateNode Document, ImmutableArray<RazorDiagnostic> Diagnostics);

public static class RazorProbe
{
    public static RazorProbeResult Parse(string source, string path)
    {
        var capture = new CaptureDocumentPass();
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(Path.GetDirectoryName(Path.GetFullPath(path))!),
            builder =>
            {
                builder.ConfigureParserOptions(options =>
                {
                    options.UseRoslynTokenizer = true;
                    options.CSharpParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
                });
                builder.Features.Add(capture);

                // Component optimization resolves internal markup placeholders into public IR nodes.
                // End the pipeline before code generation so the adapter consumes Razor IR directly.
                var optimizer = builder.Phases.Single(phase => phase is IRazorOptimizationPhase);
                while (builder.Phases[^1] != optimizer)
                    builder.Phases.RemoveAt(builder.Phases.Count - 1);
            });
        engine.Process(RazorSourceDocument.Create(source, path), RazorFileKind.Component, [], TagHelperCollection.Empty);
        var document = capture.Document ?? throw new InvalidOperationException("Razor did not run the document classifier pass.");
        return new RazorProbeResult(document, document.GetAllDiagnostics());
    }

    private sealed class CaptureDocumentPass : IntermediateNodePassBase, IRazorDocumentClassifierPass
    {
        public DocumentIntermediateNode? Document { get; private set; }

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode, CancellationToken cancellationToken)
        {
            Document = documentNode;
        }
    }
}
