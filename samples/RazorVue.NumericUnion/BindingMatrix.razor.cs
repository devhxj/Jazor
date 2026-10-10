using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace Demo.Pages;

[ECMAScriptModule("./components/binding-matrix")]
public partial class BindingMatrix : ComponentBase, IVueComponent
{
    private int Extent { get; set; } = 32;
    private float Fraction { get; set; } = 1.5f;
    private VueBooleanStringNumberObjectArrayableValue? Selection { get; set; } = 0;
}
