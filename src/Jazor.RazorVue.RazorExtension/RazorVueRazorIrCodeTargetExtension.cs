using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Jazor.RazorVue.RazorExtension;

internal sealed class RazorVueRazorIrCodeTargetExtension : ICodeTargetExtension
{
    public void WriteCarrierAttribute(CodeRenderingContext context, RazorVueRazorIrCarrierNode node)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        context.CodeWriter.WriteLine("[global::Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute(");
        context.CodeWriter.Write("    ");
        context.CodeWriter.WriteCSharpStringLiteral(node.DocumentPath);
        context.CodeWriter.WriteLine(",");
        context.CodeWriter.Write("    ");
        context.CodeWriter.WriteCSharpStringLiteral(node.ImportsJson);
        context.CodeWriter.WriteLine(",");
        context.CodeWriter.Write("    ");
        context.CodeWriter.WriteCSharpStringLiteral(node.DocumentText);
        context.CodeWriter.WriteLine(")]");
    }
}
