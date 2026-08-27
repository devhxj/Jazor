namespace Jazor.CLR;

// Vue-specific behavior for these members remains in RazorVue's current-component
// hook. This module owns only the Roslyn-derived whitelist surface, so it must not
// create an otherwise empty CLR runtime artifact.
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ComponentBase")]
public static class ComponentBaseModule
{
	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ComponentBase.ComponentBase()")]
	public extern static Object _ba3b159e29672378();

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ComponentBase.RendererInfo.get")]
	public extern static Object _74b8b217bee1b4da(Object instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ComponentBase.Assets.get")]
	public extern static Object _8988f4779348e944(Object instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ComponentBase.AssignedRenderMode.get")]
	public extern static Object? _a83579883446a00c(Object instance);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]
	public extern static void _0e72578e1fb96fd0(Object instance, Object builder);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.OnInitialized()")]
	public extern static void _b64294aa17e29e64(Object instance);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.OnInitializedAsync()")]
	public extern static global::System.Threading.Tasks.Task _18dd23b6c04800ea(Object instance);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.OnParametersSet()")]
	public extern static void _a237aa5266b5fb03(Object instance);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.OnParametersSetAsync()")]
	public extern static global::System.Threading.Tasks.Task _46f59cf83ce9bda6(Object instance);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.ComponentBase.StateHasChanged()")]
	public extern static void _b6eac8380b912a53(Object instance);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.ShouldRender()")]
	public extern static bool _ef7a91a10e210262(Object instance);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(bool)")]
	public extern static void _af7102e892f6af27(Object instance, bool firstRender);

	[Jazor(Op.Discard ,"virtual Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(bool)")]
	public extern static global::System.Threading.Tasks.Task _077ed9619eaa5fae(Object instance, bool firstRender);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Action)")]
	public extern static global::System.Threading.Tasks.Task _9aaa75f07e6ff83e(Object instance, global::System.Action workItem);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Func<System.Threading.Tasks.Task>)")]
	public extern static global::System.Threading.Tasks.Task _8c80b94d95adc123(Object instance, global::System.Func<global::System.Threading.Tasks.Task> workItem);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ComponentBase.DispatchExceptionAsync(System.Exception)")]
	public extern static global::System.Threading.Tasks.Task _ef319c365e5d0770(Object instance, global::System.Exception exception);

	[Jazor(Op.Allowed ,"virtual Microsoft.AspNetCore.Components.ComponentBase.SetParametersAsync(Microsoft.AspNetCore.Components.ParameterView)")]
	public extern static global::System.Threading.Tasks.Task _04587a67ed4e8384(Object instance, Object parameters);
}
