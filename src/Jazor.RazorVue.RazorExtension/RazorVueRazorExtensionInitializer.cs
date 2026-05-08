using Microsoft.AspNetCore.Razor.Language;

[assembly: ProvideRazorExtensionInitializer("RazorVueRazorIrCarrier", typeof(Jazor.RazorVue.RazorExtension.RazorVueRazorExtensionInitializer))]

namespace Jazor.RazorVue.RazorExtension;

public sealed class RazorVueRazorExtensionInitializer : RazorExtensionInitializer
{
    public override void Initialize(RazorProjectEngineBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Features.Add(new RazorVueRazorIrTargetExtensionFeature());
        builder.Features.Add(new RazorVueRazorIrPass());
    }
}
