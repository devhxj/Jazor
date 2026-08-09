using ECMAScript;
using ECMAScript.TDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace JazorAdmin;

// Razor cannot choose between the generated generic and non-generic TDesign bindings.
// These bridge components preserve the official controls while selecting their generic type in C#.
[ECMAScriptModule("./components/starter-form")]
public sealed class StarterForm : ComponentBase, IVueComponent
{
    [Parameter]
    public TFormLabelAlignValue? LabelAlign { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TForm<TJsonObject>>(0);
        builder.AddComponentParameter(1, nameof(TForm<TJsonObject>.LabelAlign), LabelAlign);
        builder.AddComponentParameter(2, nameof(TContentComponentBase.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

[ECMAScriptModule("./components/starter-radio-group")]
public sealed class StarterRadioGroup : ComponentBase, IVueComponent
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public TSizeEnum? Size { get; set; }

    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public EventCallback<string> OnChange { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TRadioGroup<string>>(0);
        builder.AddComponentParameter(1, nameof(TRadioGroup<string>.Value), Value);
        builder.AddComponentParameter(2, nameof(TRadioGroup<string>.Size), Size);
        builder.AddComponentParameter(3, nameof(TContentComponentBase.CssClass), CssClass);
        builder.AddComponentParameter(4, nameof(TRadioGroup<string>.OnChange),
            EventCallback.Factory.Create<string>(this, HandleChangeAsync));
        builder.AddComponentParameter(5, nameof(TContentComponentBase.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    private Task HandleChangeAsync(string value) => OnChange.InvokeAsync(value);
}

[ECMAScriptModule("./components/starter-radio-button")]
public sealed class StarterRadioButton : ComponentBase, IVueComponent
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string? LabelValue { get; set; }

    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TRadioButton<string>>(0);
        builder.AddComponentParameter(1, nameof(TRadioButton<string>.Value), Value);
        builder.AddComponentParameter(2, nameof(TRadioButton<string>.LabelValue), LabelValue);
        builder.AddComponentParameter(3, nameof(TContentComponentBase.CssClass), CssClass);
        builder.AddComponentParameter(4, nameof(TContentComponentBase.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

[ECMAScriptModule("./components/starter-toggle")]
public sealed class StarterToggle : ComponentBase, IVueComponent
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> OnChange { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TSwitch<bool>>(0);
        builder.AddComponentParameter(1, nameof(TSwitch<bool>.Value), Value);
        builder.AddComponentParameter(2, nameof(TSwitch<bool>.OnChange),
            EventCallback.Factory.Create<bool>(this, HandleChangeAsync));
        builder.CloseComponent();
    }

    private Task HandleChangeAsync(bool value) => OnChange.InvokeAsync(value);
}
