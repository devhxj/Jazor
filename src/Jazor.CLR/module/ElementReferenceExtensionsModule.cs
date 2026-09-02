namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/ElementReferenceExtensionsModule.js")]
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ElementReferenceExtensions")]
public static class ElementReferenceExtensionsModule
{
	/// <summary>
	/// Focuses a Razor-captured DOM element and preserves the framework failure for a default or
	/// already-unmounted ElementReference. The browser call itself is synchronous, while the
	/// mapped ValueTask carrier remains a completed Promise after a successful focus.
	/// </summary>
	/// <remarks>
	/// Inline focus calls cannot distinguish a missing ref from a browser TypeError. Keep this
	/// check in the CLR runtime so all RazorVue consumers receive the same actionable contract.
	/// 未挂载的 @ref 必须保留 framework 的明确错误，而不是泄漏浏览器的 null.focus() TypeError。
	/// </remarks>
	[Jazor(Op.Import, "static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference)", "focusAsync")]
	public static global::System.Threading.Tasks.ValueTask _645a86fda0f9964f(HTMLElement elementReference)
	{
		EnsureConfigured(elementReference);
		elementReference.Focus();
		return global::System.Threading.Tasks.ValueTask.CompletedTask;
	}

	[Jazor(Op.Import, "static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference, bool)", "focusAsyncWithOptions")]
	public static global::System.Threading.Tasks.ValueTask _14e4fbd895589589(HTMLElement elementReference, bool preventScroll)
	{
		EnsureConfigured(elementReference);
		// FocusOptions' generated record would also emit focusVisible: false. The CLR bool overload
		// only owns preventScroll, so preserve the browser payload shape through Reflect.
		var focus = ECMAScript.Reflect.Get(elementReference, "focus");
		ECMAScript.Reflect.Apply(focus!, elementReference, [new { preventScroll }]);
		return global::System.Threading.Tasks.ValueTask.CompletedTask;
	}

	private static void EnsureConfigured(HTMLElement elementReference)
	{
		if (elementReference is null)
			throw new Error("InvalidOperationException: ElementReference has not been configured correctly.");
	}
}
