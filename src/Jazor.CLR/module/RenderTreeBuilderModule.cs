namespace Jazor.CLR;

// Direct render lowering owns these operations. This module deliberately carries
// canonical CLR keys only; emitting a generic RenderTreeBuilder runtime would hide
// unsupported frame semantics behind an empty Object alias.
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder")]
public static class RenderTreeBuilderModule
{
	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenElement(int, string)")]
	public extern static void _a99396ac4ee7db1d(Object instance, Number sequence, string elementName);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.CloseElement()")]
	public extern static void _7c33027f486c1f64(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddMarkupContent(int, string)")]
	public extern static void _59a9e13643ad578f(Object instance, Number sequence, string? markupContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, string)")]
	public extern static void _a8cdb6707bee2069(Object instance, Number sequence, string? textContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, Microsoft.AspNetCore.Components.RenderFragment)")]
	public extern static void _66ad53b2026eb7bf(Object instance, Number sequence, Object? fragment);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent<TValue>(int, Microsoft.AspNetCore.Components.RenderFragment<TValue>, TValue)")]
	public extern static void _4068c010649a4089<TValue>(Object instance, Number sequence, Object? fragment, TValue value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, System.Nullable<Microsoft.AspNetCore.Components.MarkupString>)")]
	public extern static void _0bf55eb2abd10afa(Object instance, Number sequence, Object markupContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, Microsoft.AspNetCore.Components.MarkupString)")]
	public extern static void _bab463c37346650a(Object instance, Number sequence, Object markupContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, object)")]
	public extern static void _357745f35b9a4cf9(Object instance, Number sequence, object? textContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string)")]
	public extern static void _35a22d7e953b0738(Object instance, Number sequence, string name);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, bool)")]
	public extern static void _28ab41d7aa754702(Object instance, Number sequence, string name, bool value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, string)")]
	public extern static void _c7b3d5cf8ca501d4(Object instance, Number sequence, string name, string? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, System.MulticastDelegate)")]
	public extern static void _21a168361d20a062(Object instance, Number sequence, string name, global::System.MulticastDelegate? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, Microsoft.AspNetCore.Components.EventCallback)")]
	public extern static void _2c462c67cbec838a(Object instance, Number sequence, string name, Object value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute<TArgument>(int, string, Microsoft.AspNetCore.Components.EventCallback<TArgument>)")]
	public extern static void _ec14be09eef2690a<TArgument>(Object instance, Number sequence, string name, Object value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, object)")]
	public extern static void _7e12287ed24b8aa0(Object instance, Number sequence, string name, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame)")]
	public extern static void _66d38fc62ab2b16f(Object instance, Number sequence, Object frame);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddMultipleAttributes(int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>)")]
	public extern static void _8c69c7467f105245(Object instance, Number sequence, global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<string, object>>? attributes);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.SetUpdatesAttributeName(string)")]
	public extern static void _92e64559bb857d1a(Object instance, string updatesAttributeName);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.SetAttributeValue(int, object)")]
	public extern static void _3316fc4656366ed7(Object instance, Number sequence, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenComponent<TComponent>(int)")]
	public extern static void _c79a1a2386a24d3e<TComponent>(Object instance, Number sequence)
		where TComponent : notnull;

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenComponent(int, System.Type)")]
	public extern static void _ee0bdfd96493f93d(Object instance, Number sequence, global::System.Type componentType);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddComponentParameter(int, string, object)")]
	public extern static void _d0023a4c3aff7562(Object instance, Number sequence, string name, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.SetKey(object)")]
	public extern static void _12fcabfe12ab6c4c(Object instance, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.CloseComponent()")]
	public extern static void _865eb8996ae36c46(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddElementReferenceCapture(int, System.Action<Microsoft.AspNetCore.Components.ElementReference>)")]
	public extern static void _3d2b9fd20997e89a(Object instance, Number sequence, Object elementReferenceCaptureAction);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddComponentReferenceCapture(int, System.Action<object>)")]
	public extern static void _03f04af202d44724(Object instance, Number sequence, global::System.Action<object> componentReferenceCaptureAction);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddComponentRenderMode(Microsoft.AspNetCore.Components.IComponentRenderMode)")]
	public extern static void _09662114e59b38e4(Object instance, Object? renderMode);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddNamedEvent(string, string)")]
	public extern static void _dee6999521759d47(Object instance, string eventType, string assignedName);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenRegion(int)")]
	public extern static void _8a7759b8c5a13812(Object instance, Number sequence);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.CloseRegion()")]
	public extern static void _6961c81766554a16(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.Clear()")]
	public extern static void _daaa43a9c69e2ba5(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.GetFrames()")]
	public extern static Object _7f064d51b183a099(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.Dispose()")]
	public extern static void _52cefe0d72dff4ae(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.RenderTreeBuilder()")]
	public extern static Object _39cbaeec72650133();
}
