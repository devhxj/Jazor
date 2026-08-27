namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ElementReferenceExtensions")]
public static class ElementReferenceExtensionsModule
{
	[Jazor(Op.Inline ,"static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference)", "Promise.resolve(__arg1.focus())")]
	public extern static global::System.Threading.Tasks.ValueTask _645a86fda0f9964f(HTMLElement elementReference);

	[Jazor(Op.Inline ,"static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference, bool)", "Promise.resolve(__arg1.focus({ preventScroll: __arg2 }))")]
	public extern static global::System.Threading.Tasks.ValueTask _14e4fbd895589589(HTMLElement elementReference, bool preventScroll);
}
