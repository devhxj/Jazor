using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Jazor.RazorVue.RazorExtension;

internal sealed class RazorVueRazorIrTargetExtensionFeature : RazorEngineFeatureBase, IRazorTargetExtensionFeature
{
    public ICollection<ICodeTargetExtension> TargetExtensions { get; } = new List<ICodeTargetExtension>
    {
        new RazorVueRazorIrCodeTargetExtension()
    };
}
