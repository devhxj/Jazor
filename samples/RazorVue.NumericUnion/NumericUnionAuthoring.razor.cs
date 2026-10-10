using ECMAScript;
using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace Demo.Pages;

[ECMAScriptModule("./components/numeric-union-authoring")]
public partial class NumericUnionAuthoring : ComponentBase, IVueComponent
{
    private Number Page { get; set; } = 1;
    private int BusinessPage { get; set; } = 1;
    private VueBooleanStringNumberObjectArrayableValue? Selected { get; set; } = 32;
    private VueBooleanStringNumberObjectArrayableValue? SelectedMany { get; set; } = new double[] { 32d };
    private string? Id { get; set; } = "9007199254740993";

    private void PageChanged(Number value)
    {
        Page = value;
        // Keep the host Number until the business model needs an integer.
        BusinessPage = (int)value;
    }
}
