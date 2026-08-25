using System.Threading.Tasks;
using ECMAScript;
using ECMAScript.Contract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Blazor;

/// <summary>
/// Maps Blazor <see cref="ElementReference"/> operations to the DOM element captured by
/// RazorVue's render emitter. The alias is intentionally a native element carrier: an
/// element reference has no standalone CLR object to materialize in the browser.
/// </summary>
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.ElementReference", "HTMLElement")]
internal static class ElementReferenceExtensions
{
    [Jazor(
        Op.Inline,
        "static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference)",
        "Promise.resolve(__arg1.focus())")]
    internal static ValueTask FocusAsync(this ElementReference elementReference)
        => default;

    [Jazor(
        Op.Inline,
        "static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference, bool)",
        "Promise.resolve(__arg1.focus({ preventScroll: __arg2 }))")]
    internal static ValueTask FocusAsync(this ElementReference elementReference, bool preventScroll)
        => default;
}
