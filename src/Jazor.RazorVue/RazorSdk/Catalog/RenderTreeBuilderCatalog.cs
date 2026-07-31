using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.RazorVue.RazorSdk.Catalog;

/// <summary>
/// 为 Razor Source Generator 产生的 RenderTreeBuilder 调用声明可进入编译域的成员。
/// </summary>
/// <remarks>
/// 这里是 RazorVue producer 侧允许列表，不实现 RenderTreeBuilder 本身；实际 Vue artifact
/// 投影由 RazorVue lowering host 负责。新增成员必须同步宿主实现和测试。
/// </remarks>
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder")]
public static class RenderTreeBuilderCatalog
{
	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenElement(int, string)")]
	public extern static void _a99396ac4ee7db1d(object instance, int sequence, string elementName);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.CloseElement()")]
	public extern static void _7c33027f486c1f64(object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddMarkupContent(int, string)")]
	public extern static void _59a9e13643ad578f(object instance, int sequence, string? markupContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, string)")]
	public extern static void _a8cdb6707bee2069(object instance, int sequence, string? textContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, Microsoft.AspNetCore.Components.RenderFragment)")]
	public extern static void _66ad53b2026eb7bf(object instance, int sequence, object fragment);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent<TValue>(int, Microsoft.AspNetCore.Components.RenderFragment<TValue>, TValue)")]
	public extern static void _4068c010649a4089<TValue>(object instance, int sequence, object fragment, object value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, System.Nullable<Microsoft.AspNetCore.Components.MarkupString>)")]
	public extern static void _0bf55eb2abd10afa(object instance, int sequence, object markupContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, Microsoft.AspNetCore.Components.MarkupString)")]
	public extern static void _bab463c37346650a(object instance, int sequence, object markupContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddContent(int, object)")]
	public extern static void _357745f35b9a4cf9(object instance, int sequence, object? textContent);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string)")]
	public extern static void _35a22d7e953b0738(object instance, int sequence, string name);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, bool)")]
	public extern static void _28ab41d7aa754702(object instance, int sequence, string name, bool value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, string)")]
	public extern static void _c7b3d5cf8ca501d4(object instance, int sequence, string name, string? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, System.MulticastDelegate)")]
	public extern static void _21a168361d20a062(object instance, int sequence, string name, object value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, Microsoft.AspNetCore.Components.EventCallback)")]
	public extern static void _2c462c67cbec838a(object instance, int sequence, string name, object value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute<TArgument>(int, string, Microsoft.AspNetCore.Components.EventCallback<TArgument>)")]
	public extern static void _ec14be09eef2690a<TArgument>(object instance, int sequence, string name, object value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, string, object)")]
	public extern static void _7e12287ed24b8aa0(object instance, int sequence, string name, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddAttribute(int, Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame)")]
	public extern static void _66d38fc62ab2b16f(object instance, int sequence, object frame);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddMultipleAttributes(int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>)")]
	public extern static void _8c69c7467f105245(object instance, int sequence, object attributes);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.SetUpdatesAttributeName(string)")]
	public extern static void _92e64559bb857d1a(object instance, string updatesAttributeName);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.SetAttributeValue(int, object)")]
	public extern static void _3316fc4656366ed7(object instance, int sequence, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenComponent<TComponent>(int)")]
	public extern static void _c79a1a2386a24d3e<TComponent>(object instance, int sequence);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenComponent(int, System.Type)")]
	public extern static void _ee0bdfd96493f93d(object instance, int sequence, object componentType);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddComponentParameter(int, string, object)")]
	public extern static void _d0023a4c3aff7562(object instance, int sequence, string name, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.SetKey(object)")]
	public extern static void _12fcabfe12ab6c4c(object instance, object? value);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.CloseComponent()")]
	public extern static void _865eb8996ae36c46(object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddElementReferenceCapture(int, System.Action<Microsoft.AspNetCore.Components.ElementReference>)")]
	public extern static void _3d2b9fd20997e89a(object instance, int sequence, object elementReferenceCaptureAction);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddComponentReferenceCapture(int, System.Action<object>)")]
	public extern static void _03f04af202d44724(object instance, int sequence, object componentReferenceCaptureAction);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddComponentRenderMode(Microsoft.AspNetCore.Components.IComponentRenderMode)")]
	public extern static void _09662114e59b38e4(object instance, object renderMode);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddNamedEvent(string, string)")]
	public extern static void _dee6999521759d47(object instance, string eventType, string assignedName);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.OpenRegion(int)")]
	public extern static void _8a7759b8c5a13812(object instance, int sequence);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.CloseRegion()")]
	public extern static void _6961c81766554a16(object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.Clear()")]
	public extern static void _daaa43a9c69e2ba5(object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.GetFrames()")]
	public extern static object _7f064d51b183a099(object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.Dispose()")]
	public extern static void _52cefe0d72dff4ae(object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.RenderTreeBuilder()")]
	public extern static object _39cbaeec72650133();
}
