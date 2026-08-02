using ECMAScript.Contract;

namespace Jazor.RazorVue.RazorSdk.Catalog;

/// <summary>
/// Declares the ComponentBase update-dispatch surface implemented by the
/// RazorVue current-component host.
/// </summary>
/// <remarks>
/// ComponentBase is not emitted as a JavaScript base class. These declarations
/// admit only the members that CurrentComponentSemanticWalkerHost projects to
/// setup-scoped runtime functions, keeping analyzer and lowering contracts aligned.
/// </remarks>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ComponentBase")]
public static class ComponentBaseCatalog
{
    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ComponentBase.StateHasChanged()")]
    public extern static void _b6eac8380b912a53(object instance);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Action)")]
    public extern static System.Threading.Tasks.Task _9aaa75f07e6ff83e(
        object instance,
        System.Action workItem);

    [Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Func<System.Threading.Tasks.Task>)")]
    public extern static System.Threading.Tasks.Task _8c80b94d95adc123(
        object instance,
        System.Func<System.Threading.Tasks.Task> workItem);
}
