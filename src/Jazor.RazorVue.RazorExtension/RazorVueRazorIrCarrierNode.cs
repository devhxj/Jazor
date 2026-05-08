using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Jazor.RazorVue.RazorExtension;

internal sealed class RazorVueRazorIrCarrierNode(
    string documentPath,
    string importsJson,
    string documentText) : ExtensionIntermediateNode
{
    public override IntermediateNodeCollection Children { get; } = new();

    public string DocumentPath { get; } = documentPath;

    public string ImportsJson { get; } = importsJson;

    public string DocumentText { get; } = documentText;

    public override void WriteNode(CodeTarget target, CodeRenderingContext context)
    {
        var extension = target.GetExtension<RazorVueRazorIrCodeTargetExtension>();
        if (extension is null)
        {
            ReportMissingCodeTargetExtension<RazorVueRazorIrCodeTargetExtension>(context);
            return;
        }

        extension.WriteCarrierAttribute(context, this);
    }

    public override void Accept(IntermediateNodeVisitor visitor)
        => visitor.VisitExtension(this);
}
