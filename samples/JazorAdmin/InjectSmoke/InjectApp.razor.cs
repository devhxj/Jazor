using ECMAScript;
using Jazor.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using static ECMAScript.Vue3;

namespace JazorAdmin.InjectSmoke;

[ECMAScriptModule("./components/inject/app")]
public sealed class InjectApp : ComponentBase, IVueComponent
{
    private int actionCount;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PageContainer>(0);
        builder.AddComponentParameter(1, nameof(PageContainer.Title), "Injected administration page");
        builder.AddComponentParameter(2, nameof(PageContainer.Subtitle), "Assembly-level admin container replacement");
        builder.AddComponentParameter(3, nameof(PageContainer.BreadcrumbItems), new AdminBreadcrumbItem[]
        {
            new() { Key = "home", Title = "Home", Href = "/" },
            new() { Key = "inject", Title = "Inject smoke", Disabled = true }
        });
        builder.AddComponentParameter(4, nameof(PageContainer.Actions), new AdminPageAction[]
        {
            new()
            {
                Key = "verify",
                Text = "Verify injection",
                Kind = AdminPageActionKind.Primary,
                Click = EventCallback.Factory.Create(this, VerifyInjection)
            }
        });
        builder.AddComponentParameter(5, nameof(PageContainer.Extra),
            (RenderFragment)(extraBuilder =>
            {
                extraBuilder.OpenElement(0, "span");
                extraBuilder.AddAttribute(1, "data-inject-slot", "extra");
                extraBuilder.AddContent(2, "Extra slot preserved");
                extraBuilder.CloseElement();
            }));
        builder.AddComponentParameter(6, nameof(PageContainer.ChildContent),
            (RenderFragment)(contentBuilder =>
            {
                contentBuilder.OpenElement(0, "p");
                contentBuilder.AddAttribute(1, "data-inject-slot", "content");
                contentBuilder.AddContent(2, "Default content preserved");
                contentBuilder.CloseElement();
                contentBuilder.OpenElement(3, "output");
                contentBuilder.AddAttribute(4, "data-inject-count", true);
                contentBuilder.AddContent(5, actionCount);
                contentBuilder.CloseElement();
            }));
        builder.CloseComponent();
    }

    private void VerifyInjection()
        => actionCount++;
}
